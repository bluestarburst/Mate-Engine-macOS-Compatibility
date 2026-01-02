using UnityEngine;
using System;
using System.Diagnostics;
using MateEngine.Platform;

public class AvatarHideHandler : MonoBehaviour
{
    public int snapThresholdPx = 12;
    public int unsnapThresholdPx = 24;
    public int edgeInsetPx = 0;
    public bool enableSmoothing = true;
    [Range(0.01f, 0.5f)] public float smoothingTime = 0.10f;
    public float smoothingMaxSpeed = 6000f;
    public bool keepTopmostWhileSnapped = true;
    public float unsnapGraceTime = 0.12f;

    Animator animator;
    AvatarAnimatorController controller;
    IntPtr unityHWND;

    Transform leftHand;
    Transform rightHand;
    Camera cam;

    // Platform services
    IWindowService windowService;
    IScreenService screenService;

    enum Side { None, Left, Right }
    Side snappedSide = Side.None;

    int cursorOffsetY;
    int windowW, windowH;
    float velX, velY;
    bool smoothingActive;
    bool wasDragging;
    float snappedAt;

    void Start()
    {
        // Get platform services
        windowService = PlatformServiceLocator.WindowService;
        screenService = PlatformServiceLocator.ScreenService;

        // Get window handle - only works on Windows
        if (windowService != null)
        {
            unityHWND = windowService.GetMainWindowHandle();
        }

        animator = GetComponent<Animator>();
        controller = GetComponent<AvatarAnimatorController>();
        if (animator != null && animator.isHuman && animator.avatar != null)
        {
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
        cam = Camera.main;
        if (cam == null) cam = FindObjectOfType<Camera>();
    }

    void OnDisable()
    {
        SetHide(false, false);
        snappedSide = Side.None;
    }

    void Update()
    {
        // Only proceed on Windows or if services are available
        if (unityHWND == IntPtr.Zero || animator == null || controller == null || windowService == null || screenService == null) return;

        if (controller.isDragging && !wasDragging)
        {
            if (windowService.GetWindowRect(unityHWND, out WindowRect wr) && screenService.GetCursorPosition(out Vector2Int cp))
            {
                windowW = Math.Max(1, (int)wr.Width);
                windowH = Math.Max(1, (int)wr.Height);
                cursorOffsetY = (int)(cp.y - wr.Top);
                smoothingActive = false;
                velX = velY = 0f;
            }
        }

        if (controller.isDragging)
        {
            if (!screenService.GetCursorPosition(out Vector2Int cp)) { wasDragging = controller.isDragging; return; }
            if (!windowService.GetWindowRect(unityHWND, out WindowRect wrCur)) { wasDragging = controller.isDragging; return; }
            Rect mon = GetCurrentMonitorRect(cp);

            int anchorLeftDesk = GetAnchorDesktopX(Side.Left);
            int anchorRightDesk = GetAnchorDesktopX(Side.Right);
            if (anchorLeftDesk < 0) anchorLeftDesk = (int)wrCur.Left + Math.Max(1, (int)wrCur.Width / 2);
            if (anchorRightDesk < 0) anchorRightDesk = (int)wrCur.Left + Math.Max(1, (int)wrCur.Width / 2);

            bool nearLeft = anchorLeftDesk - (int)mon.x <= Math.Max(1, snapThresholdPx);
            bool nearRight = (int)(mon.x + mon.width) - anchorRightDesk <= Math.Max(1, snapThresholdPx);

            if (snappedSide == Side.None)
            {
                if (nearLeft) SnapTo(Side.Left, cp, mon);
                else if (nearRight) SnapTo(Side.Right, cp, mon);
            }
            else
            {
                if (Time.unscaledTime >= snappedAt + unsnapGraceTime)
                {
                    if (snappedSide == Side.Left && (cp.x - (int)mon.x) > Math.Max(1, unsnapThresholdPx)) Unsnap();
                    else if (snappedSide == Side.Right && ((int)(mon.x + mon.width) - cp.x) > Math.Max(1, unsnapThresholdPx)) Unsnap();
                }
            }

            if (snappedSide != Side.None)
            {
                if (!windowService.GetWindowRect(unityHWND, out WindowRect wr2)) { wasDragging = controller.isDragging; return; }
                Rect monNow = GetCurrentMonitorRect(cp);

                int anchorDesk = GetAnchorDesktopX(snappedSide);
                if (anchorDesk < 0) anchorDesk = (int)wr2.Left + Math.Max(1, (int)wr2.Width / 2);
                int anchorWinX = Mathf.Clamp(anchorDesk - (int)wr2.Left, 0, Math.Max(1, (int)wr2.Width));

                int desiredAnchorDesk = snappedSide == Side.Left ? (int)monNow.x + edgeInsetPx : (int)(monNow.x + monNow.width) - edgeInsetPx;
                int tx = desiredAnchorDesk - anchorWinX;

                int ty = cp.y - cursorOffsetY;

                MoveSmooth((int)wr2.Left, (int)wr2.Top, tx, ty, (int)wr2.Width, (int)wr2.Height);
                if (keepTopmostWhileSnapped) SetTopMost(true);
            }
        }
        else
        {
            if (snappedSide != Side.None)
            {
                if (!windowService.GetWindowRect(unityHWND, out WindowRect wr)) return;
                Rect mon = GetMonitorFromWindow(unityHWND);

                int anchorDesk = GetAnchorDesktopX(snappedSide);
                if (anchorDesk < 0) anchorDesk = (int)wr.Left + Math.Max(1, (int)wr.Width / 2);
                int anchorWinX = Mathf.Clamp(anchorDesk - (int)wr.Left, 0, Math.Max(1, (int)wr.Width));

                int desiredAnchorDesk = snappedSide == Side.Left ? (int)mon.x + edgeInsetPx : (int)(mon.x + mon.width) - edgeInsetPx;
                int tx = desiredAnchorDesk - anchorWinX;

                int ty = (int)wr.Top;

                MoveSmooth((int)wr.Left, (int)wr.Top, tx, ty, (int)wr.Width, (int)wr.Height);
                if (keepTopmostWhileSnapped) SetTopMost(true);
            }
        }

        wasDragging = controller.isDragging;
    }

    int GetAnchorDesktopX(Side side)
    {
        Transform t = side == Side.Left ? leftHand : rightHand;
        if (t == null || cam == null) return -1;
        if (!GetUnityClientRect(out Rect uCli)) return -1;

        Vector3 sp = cam.WorldToScreenPoint(t.position);
        if (sp.z < 0.01f) return -1;

        float clientW = Mathf.Max(1f, uCli.width);
        float pxW = Mathf.Max(1, cam.pixelWidth);
        float sx = Mathf.Clamp(sp.x, 0, cam.pixelWidth) * (clientW / pxW);
        int desktopX = (int)uCli.x + Mathf.RoundToInt(sx);
        return desktopX;
    }

    void SnapTo(Side side, Vector2Int cp, Rect mon)
    {
        if (!windowService.GetWindowRect(unityHWND, out WindowRect wr)) return;

        windowW = Math.Max(1, (int)wr.Width);
        windowH = Math.Max(1, (int)wr.Height);
        cursorOffsetY = cp.y - (int)wr.Top;
        snappedSide = side;
        SetHide(side == Side.Left, side == Side.Right);

        int anchorDesk = GetAnchorDesktopX(side);
        if (anchorDesk < 0) anchorDesk = (int)wr.Left + Math.Max(1, (int)wr.Width / 2);
        int anchorWinX = Mathf.Clamp(anchorDesk - (int)wr.Left, 0, Math.Max(1, (int)wr.Width));

        int desiredAnchorDesk = side == Side.Left ? (int)mon.x + edgeInsetPx : (int)(mon.x + mon.width) - edgeInsetPx;
        int tx = desiredAnchorDesk - anchorWinX;

        int ty = cp.y - cursorOffsetY;

        windowService.SetWindowPosition(unityHWND, tx, ty, windowW, windowH, SetWindowFlags.NoZOrder);
        smoothingActive = enableSmoothing;
        velX = velY = 0f;
        snappedAt = Time.unscaledTime;
        if (keepTopmostWhileSnapped) SetTopMost(true);
    }

    void Unsnap()
    {
        snappedSide = Side.None;
        SetHide(false, false);
        smoothingActive = false;
        velX = velY = 0f;
        SetTopMost(false);
    }

    void SetHide(bool left, bool right)
    {
        animator.SetBool("HideLeft", left);
        animator.SetBool("HideRight", right);
    }

    void MoveSmooth(int curX, int curY, int targetX, int targetY, int w, int h)
    {
        if (!enableSmoothing || !smoothingActive)
        {
            if (curX != targetX || curY != targetY) 
                windowService.SetWindowPosition(unityHWND, targetX, targetY, w, h, SetWindowFlags.NoZOrder);
            return;
        }
        float dt = Time.unscaledDeltaTime;
        float nx = Mathf.SmoothDamp(curX, targetX, ref velX, smoothingTime, smoothingMaxSpeed, dt);
        float ny = Mathf.SmoothDamp(curY, targetY, ref velY, smoothingTime, smoothingMaxSpeed, dt);
        int ix = Mathf.RoundToInt(nx);
        int iy = Mathf.RoundToInt(ny);
        if (Mathf.Abs(targetX - ix) <= 1 && Mathf.Abs(targetY - iy) <= 1)
        {
            ix = targetX; iy = targetY; smoothingActive = false; velX = velY = 0f;
        }
        if (ix != curX || iy != curY) 
            windowService.SetWindowPosition(unityHWND, ix, iy, w, h, SetWindowFlags.NoZOrder);
    }

    Rect GetCurrentMonitorRect(Vector2Int cp)
    {
        MonitorInfo mon = screenService.GetMonitorFromPoint(cp);
        return mon.MonitorArea;
    }

    Rect GetMonitorFromWindow(IntPtr hwnd)
    {
        MonitorInfo mon = screenService.GetMonitorFromWindow(hwnd);
        return mon.MonitorArea;
    }

    bool GetUnityClientRect(out Rect r)
    {
        r = new Rect();
        if (!windowService.GetClientRect(unityHWND, out WindowRect client)) return false;
        
        // ClientToScreen equivalent - convert client (0,0) to screen coordinates
        Vector2Int screenPos = new Vector2Int(0, 0);
        // Since we don't have a direct ClientToScreen, we use GetWindowRect and assume top-left alignment
        if (!windowService.GetWindowRect(unityHWND, out WindowRect winRect)) return false;
        
        r = new Rect((int)winRect.Left, (int)winRect.Top, (int)client.Width, (int)client.Height);
        return true;
    }

    void SetTopMost(bool on)
    {
        windowService.SetTopMost(unityHWND, on);
    }
}
