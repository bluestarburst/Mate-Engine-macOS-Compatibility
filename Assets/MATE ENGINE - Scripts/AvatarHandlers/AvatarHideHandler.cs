using UnityEngine;
using System;
using MateEngine.Platform;

/// <summary>
/// Handles avatar hiding at screen edges with snapping behavior
/// Now uses platform abstraction layer for cross-platform support
/// </summary>
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
    
    // Platform services
    private IWindowService windowService;
    private IScreenService screenService;

    Transform leftHand;
    Transform rightHand;
    Camera cam;

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
        // Initialize platform services
        windowService = PlatformServiceLocator.GetWindowService();
        screenService = PlatformServiceLocator.GetScreenService();
        unityHWND = windowService.GetMainWindowHandle();
        
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
        if (unityHWND == IntPtr.Zero || animator == null || controller == null) return;

        if (controller.isDragging && !wasDragging)
        {
            PlatformRect wr = windowService.GetWindowRect();
            Vector2 cp = screenService.GetCursorPosition();
            
            windowW = Math.Max(1, wr.Width);
            windowH = Math.Max(1, wr.Height);
            cursorOffsetY = (int)cp.y - wr.top;
            smoothingActive = false;
            velX = velY = 0f;
        }

        if (controller.isDragging)
        {
            Vector2 cp = screenService.GetCursorPosition();
            PlatformRect wrCur = windowService.GetWindowRect();
            if (wrCur.Width == 0 || wrCur.Height == 0) { wasDragging = controller.isDragging; return; }
            
            ScreenInfo mon = GetCurrentMonitorRect(cp);

            int anchorLeftDesk = GetAnchorDesktopX(Side.Left);
            int anchorRightDesk = GetAnchorDesktopX(Side.Right);
            if (anchorLeftDesk < 0) anchorLeftDesk = wrCur.left + Math.Max(1, wrCur.Width / 2);
            if (anchorRightDesk < 0) anchorRightDesk = wrCur.left + Math.Max(1, wrCur.Width / 2);

            bool nearLeft = anchorLeftDesk - (int)mon.bounds.x <= Math.Max(1, snapThresholdPx);
            bool nearRight = (int)(mon.bounds.x + mon.bounds.width) - anchorRightDesk <= Math.Max(1, snapThresholdPx);

            if (snappedSide == Side.None)
            {
                if (nearLeft) SnapTo(Side.Left, cp, mon);
                else if (nearRight) SnapTo(Side.Right, cp, mon);
            }
            else
            {
                if (Time.unscaledTime >= snappedAt + unsnapGraceTime)
                {
                    if (snappedSide == Side.Left && (cp.x - mon.bounds.x) > Math.Max(1, unsnapThresholdPx)) Unsnap();
                    else if (snappedSide == Side.Right && (mon.bounds.x + mon.bounds.width - cp.x) > Math.Max(1, unsnapThresholdPx)) Unsnap();
                }
            }

            if (snappedSide != Side.None)
            {
                PlatformRect wr2 = windowService.GetWindowRect();
                if (wr2.Width == 0 || wr2.Height == 0) { wasDragging = controller.isDragging; return; }
                
                ScreenInfo monNow = GetCurrentMonitorRect(cp);

                int anchorDesk = GetAnchorDesktopX(snappedSide);
                if (anchorDesk < 0) anchorDesk = wr2.left + Math.Max(1, wr2.Width / 2);
                int anchorWinX = Mathf.Clamp(anchorDesk - wr2.left, 0, Math.Max(1, wr2.Width));

                int desiredAnchorDesk = snappedSide == Side.Left ? (int)(monNow.bounds.x + edgeInsetPx) : (int)(monNow.bounds.x + monNow.bounds.width - edgeInsetPx);
                int tx = desiredAnchorDesk - anchorWinX;

                int ty = (int)cp.y - cursorOffsetY;

                MoveSmooth(wr2.left, wr2.top, tx, ty, wr2.Width, wr2.Height);
                if (keepTopmostWhileSnapped) SetTopMost(true);
            }
        }
        else
        {
            if (snappedSide != Side.None)
            {
                PlatformRect wr = windowService.GetWindowRect();
                if (wr.Width == 0 || wr.Height == 0) return;
                
                ScreenInfo mon = GetMonitorFromWindow(unityHWND);

                int anchorDesk = GetAnchorDesktopX(snappedSide);
                if (anchorDesk < 0) anchorDesk = wr.left + Math.Max(1, wr.Width / 2);
                int anchorWinX = Mathf.Clamp(anchorDesk - wr.left, 0, Math.Max(1, wr.Width));

                int desiredAnchorDesk = snappedSide == Side.Left ? (int)(mon.bounds.x + edgeInsetPx) : (int)(mon.bounds.x + mon.bounds.width - edgeInsetPx);
                int tx = desiredAnchorDesk - anchorWinX;

                int ty = wr.top;

                MoveSmooth(wr.left, wr.top, tx, ty, wr.Width, wr.Height);
                if (keepTopmostWhileSnapped) SetTopMost(true);
            }
        }

        wasDragging = controller.isDragging;
    }

    int GetAnchorDesktopX(Side side)
    {
        Transform t = side == Side.Left ? leftHand : rightHand;
        if (t == null || cam == null) return -1;
        
        PlatformRect uCli = GetUnityClientRect();
        if (uCli.Width == 0 || uCli.Height == 0) return -1;

        Vector3 sp = cam.WorldToScreenPoint(t.position);
        if (sp.z < 0.01f) return -1;

        float clientW = Mathf.Max(1f, uCli.Width);
        float pxW = Mathf.Max(1, cam.pixelWidth);
        float sx = Mathf.Clamp(sp.x, 0, cam.pixelWidth) * (clientW / pxW);
        int desktopX = uCli.left + Mathf.RoundToInt(sx);
        return desktopX;
    }

    void SnapTo(Side side, Vector2 cp, ScreenInfo mon)
    {
        PlatformRect wr = windowService.GetWindowRect();
        if (wr.Width == 0 || wr.Height == 0) return;

        windowW = Math.Max(1, wr.Width);
        windowH = Math.Max(1, wr.Height);
        cursorOffsetY = (int)cp.y - wr.top;
        snappedSide = side;
        SetHide(side == Side.Left, side == Side.Right);

        int anchorDesk = GetAnchorDesktopX(side);
        if (anchorDesk < 0) anchorDesk = wr.left + Math.Max(1, wr.Width / 2);
        int anchorWinX = Mathf.Clamp(anchorDesk - wr.left, 0, Math.Max(1, wr.Width));

        int desiredAnchorDesk = side == Side.Left ? (int)(mon.bounds.x + edgeInsetPx) : (int)(mon.bounds.x + mon.bounds.width - edgeInsetPx);
        int tx = desiredAnchorDesk - anchorWinX;

        int ty = (int)cp.y - cursorOffsetY;

        windowService.MoveWindow(unityHWND, tx, ty, windowW, windowH, true);
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
            if (curX != targetX || curY != targetY) windowService.MoveWindow(unityHWND, targetX, targetY, w, h, true);
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
        if (ix != curX || iy != curY) windowService.MoveWindow(unityHWND, ix, iy, w, h, true);
    }

    ScreenInfo GetCurrentMonitorRect(Vector2 cp)
    {
        return screenService.GetScreenAtPoint(cp);
    }

    ScreenInfo GetMonitorFromWindow(IntPtr hwnd)
    {
        return screenService.GetScreenContainingWindow(hwnd);
    }

    PlatformRect GetUnityClientRect()
    {
        PlatformRect client = windowService.GetClientRect();
        if (client.Width == 0 || client.Height == 0) return new PlatformRect();
        
        PlatformPoint p = windowService.ClientToScreen(unityHWND, new PlatformPoint(0, 0));
        return new PlatformRect(p.x, p.y, p.x + client.right, p.y + client.bottom);
    }

    void SetTopMost(bool on)
    {
        windowService.SetAlwaysOnTop(on);
    }
}