using UnityEngine;

/// <summary>
/// Debug helper to verify MacOSWindowTracker is receiving valid data from macOS.
/// If you see all zeros, you likely need Screen Recording permission.
/// 
/// Usage: Attach to any GameObject in the scene and check the Console.
/// </summary>
public class MacTrackerDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableLogging = true;
    public int logEveryNFrames = 60;
    
    [Header("Permission Helper")]
    [Tooltip("Click to trigger first-time permission prompt")]
    public bool requestPermissionOnStart = false;

    [Header("Runtime Info (Read Only)")]
    [SerializeField] private string lastDockRect = "";
    [SerializeField] private string lastActiveWindow = "";
    [SerializeField] private bool hasScreenRecordingPermission = false;

    void Start()
    {
#if UNITY_STANDALONE_OSX
        if (requestPermissionOnStart)
        {
            Debug.Log("[MacTracker] Requesting Screen Recording permission on start...");
            MacOSWindowHelper.RequestScreenRecordingPermission();
        }
#endif
    }

    void Update()
    {
#if UNITY_STANDALONE_OSX
        if (!enableLogging) return;

        // Only check every N frames to avoid log spam
        if (Time.frameCount % logEveryNFrames == 0)
        {
            Rect dock = MacOSWindowTracker.GetDockRect();
            Rect active = MacOSWindowTracker.GetActiveWindowRect();

            lastDockRect = $"({dock.x:F0}, {dock.y:F0}, w:{dock.width:F0}, h:{dock.height:F0})";
            lastActiveWindow = $"({active.x:F0}, {active.y:F0}, w:{active.width:F0}, h:{active.height:F0})";

            // Check if we're getting valid data
            hasScreenRecordingPermission = (dock != Rect.zero || active != Rect.zero);

            if (dock == Rect.zero && active == Rect.zero)
            {
                Debug.LogWarning("[MacTracker] ⚠️ Both Dock and Active Window are ZERO!\n" +
                    "This likely means Screen Recording permission is DENIED.\n" +
                    "Go to: System Settings > Privacy & Security > Screen Recording\n" +
                    "Add your app and RESTART it.");
            }
            else
            {
                Debug.Log($"[MacTracker] ✅ Dock: {lastDockRect} | Active Win: {lastActiveWindow}");

                // Additional validation
                if (dock.width > 0 && dock.height < 10)
                {
                    Debug.Log("[MacTracker] Dock is auto-hidden (height < 10px)");
                }

                if (active.width > 0)
                {
                    Debug.Log($"[MacTracker] Active window detected: {active.width:F0}x{active.height:F0}");
                }
            }
        }
#else
        if (Time.frameCount % logEveryNFrames == 0)
        {
            Debug.LogWarning("[MacTracker] This debug script only works on macOS builds.");
        }
#endif
    }

    // Manual test buttons (useful in Inspector during Play Mode)
    [ContextMenu("Test Dock Query")]
    void TestDockQuery()
    {
#if UNITY_STANDALONE_OSX
        Rect dock = MacOSWindowTracker.GetDockRect();
        Debug.Log($"[Manual Test] Dock Rect: {dock}");
        
        if (dock == Rect.zero)
            Debug.LogError("Dock query returned ZERO. Check Screen Recording permission!");
        else
            Debug.Log($"Dock found at bottom of screen: {dock.height:F0}px tall");
#endif
    }

    [ContextMenu("Test Active Window Query")]
    void TestActiveWindowQuery()
    {
#if UNITY_STANDALONE_OSX
        Rect active = MacOSWindowTracker.GetActiveWindowRect();
        Debug.Log($"[Manual Test] Active Window Rect: {active}");
        
        if (active == Rect.zero)
            Debug.LogError("Active window query returned ZERO. Check Screen Recording permission!");
        else
            Debug.Log($"Active window found: {active.width:F0}x{active.height:F0}");
#endif
    }

    [ContextMenu("Check Coordinate Conversion")]
    void TestCoordinateConversion()
    {
#if UNITY_STANDALONE_OSX
        Rect dock = MacOSWindowTracker.GetDockRect();
        if (dock == Rect.zero)
        {
            Debug.LogError("Cannot test conversion - no dock data. Check permissions!");
            return;
        }

        float screenHeight = Screen.currentResolution.height;
        Debug.Log($"[Coordinate Test]\n" +
            $"Screen Height: {screenHeight}px\n" +
            $"Dock Rect (Unity Space): {dock}\n" +
            $"Dock should be at BOTTOM of screen\n" +
            $"Expected Y range: 0 to ~100px\n" +
            $"Actual Y: {dock.y:F0}px\n" +
            $"Status: {(dock.y < 100 ? "✅ Correct" : "❌ Wrong - should be near 0")}");
#endif
    }

    [ContextMenu("Request Screen Recording Permission")]
    void RequestPermission()
    {
#if UNITY_STANDALONE_OSX
        Debug.Log("[MacTracker] Triggering Screen Recording permission request...");
        Debug.Log("If this is the FIRST time, you should see a system prompt.");
        Debug.Log("If you've already denied it, use 'Open System Settings' instead.");
        MacOSWindowHelper.RequestScreenRecordingPermission();
#endif
    }

    [ContextMenu("Open System Settings (Screen Recording)")]
    void OpenSettings()
    {
#if UNITY_STANDALONE_OSX
        Debug.Log("[MacTracker] Opening System Settings...");
        Debug.Log("Navigate to: Privacy & Security > Screen Recording");
        Debug.Log("Then enable permission for this app and RESTART the app.");
        MacOSWindowHelper.OpenScreenRecordingSettings();
#else
        Debug.LogWarning("[MacTracker] This function only works on macOS");
#endif
    }
}
