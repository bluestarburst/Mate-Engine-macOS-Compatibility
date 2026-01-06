using UnityEngine;

public class DebugVisualizer : MonoBehaviour
{
    private RECT _lastActiveDesktop;
    public float characterBoxSize = 40f;

    private void OnGUI()
    {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        // Get active window in DESKTOP coordinates (CGWindowList: top-left origin, POINTS)
        RECT activeDesktop = MacOSWindowTracker.GetActiveWindowRectDesktop();

        if (activeDesktop.Left != 0 || activeDesktop.Top != 0 || activeDesktop.Right != 0 || activeDesktop.Bottom != 0)
        {
            _lastActiveDesktop = activeDesktop;
        }

        // Get Unity window bounds in CGWindowList coordinates (top-left origin, POINTS)
        var unityBounds = MacOSWindowHelper.GetUnityWindowBounds();
        if (!unityBounds.isValid)
        {
            GUI.color = Color.white;
            GUILayout.Label("Unity window bounds invalid!");
            return;
        }

        // All coordinates are now in POINTS (CGWindowList uses points, not device pixels)
        float scale = MacOSWindowHelper.GetUnityBackingScale();

        // Get active window position and size (in points)
        float activeLeft = _lastActiveDesktop.Left;
        float activeTop = _lastActiveDesktop.Top;
        float activeW = _lastActiveDesktop.Right - _lastActiveDesktop.Left;
        float activeH = _lastActiveDesktop.Bottom - _lastActiveDesktop.Top;

        // Get Unity window position and size (in points)
        float unityLeft = unityBounds.x;
        float unityTop = unityBounds.y;
        float unityW = unityBounds.width;
        float unityH = unityBounds.height;

        // Unity's Screen.width/height is the CLIENT AREA in logical pixels
        // For a borderless window, this should match the window frame size
        int screenW = Screen.width;
        int screenH = Screen.height;

        // Calculate ratio to handle any difference between window frame and client area
        float ratioX = (float)screenW / unityW;
        float ratioY = (float)screenH / unityH;

        // Calculate active window position RELATIVE to Unity window (in points, top-left origin)
        float relX = activeLeft - unityLeft;
        float relY = activeTop - unityTop;

        // Convert to GUI coordinates:
        // GUI uses Screen coordinates which may differ from window frame
        // Apply ratio to map from window frame space to Screen space
        float guiX = relX * ratioX;
        float guiY = relY * ratioY;
        float guiW = activeW * ratioX;
        float guiH = activeH * ratioY;

        // Draw active window overlay (green)
        GUI.color = new Color(0f, 1f, 0f, 0.35f);
        GUI.DrawTexture(new Rect(guiX, guiY, guiW, guiH), Texture2D.whiteTexture);

        // Draw character position indicator (blue box)
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 charWorldPos = transform.position;
            Vector3 charScreenPos = cam.WorldToScreenPoint(charWorldPos);
            if (charScreenPos.z > 0)
            {
                GUI.color = new Color(0f, 0.5f, 1f, 0.7f); // Blue
                float halfSize = characterBoxSize * 0.5f;
                // Screen coords use bottom-left origin, GUI uses top-left, so flip Y
                GUI.DrawTexture(new Rect(charScreenPos.x - halfSize, Screen.height - charScreenPos.y - halfSize, characterBoxSize, characterBoxSize), Texture2D.whiteTexture);
            }
        }

        GUI.color = Color.white;
        GUILayout.Label($"Screen: {screenW}x{screenH}");
        GUILayout.Label($"Unity frame: {unityW:F0}x{unityH:F0} pts at ({unityLeft:F0},{unityTop:F0})");
        GUILayout.Label($"Ratio: {ratioX:F3} x {ratioY:F3}");
        GUILayout.Label($"Active: ({activeLeft:F0},{activeTop:F0}) {activeW:F0}x{activeH:F0} pts");
        GUILayout.Label($"Relative: ({relX:F0},{relY:F0})");
        GUILayout.Label($"GUI: ({guiX:F0},{guiY:F0}) {guiW:F0}x{guiH:F0}");
        GUILayout.Label($"Scale: {scale:F2}x");
#else
        GUI.color = Color.white;
        GUILayout.Label("Debug visualizer only works on macOS builds");
#endif
    }
}
