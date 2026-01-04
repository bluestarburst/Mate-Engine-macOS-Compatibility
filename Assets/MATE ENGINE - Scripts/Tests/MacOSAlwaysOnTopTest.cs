using UnityEngine;

/// <summary>
/// Test/demo script for macOS always-on-top functionality
/// Attach this to a GameObject to test the macOS window helper
/// </summary>
public class MacOSAlwaysOnTopTest : MonoBehaviour
{
    [Header("Test Controls")]
    [Tooltip("Enable always-on-top over fullscreen apps")]
    public bool enableAlwaysOnTop = true;
    
    [Tooltip("Use aggressive screen-saver level instead of floating level")]
    public bool useScreenSaverLevel = false;
    
    [Tooltip("Show on all desktops/spaces")]
    public bool visibleOnAllSpaces = true;
    
    [Header("Status")]
    [SerializeField] private int currentWindowLevel = 0;
    [SerializeField] private bool canAppearOverFullscreen = false;
    
    private bool lastEnabledState = false;
    private bool lastScreenSaverMode = false;
    private bool lastVisibleOnAllSpaces = false;

    void Start()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        Debug.Log("MacOSAlwaysOnTopTest: Starting on macOS");
        ApplySettings();
        UpdateStatus();
#else
        Debug.LogWarning("MacOSAlwaysOnTopTest: This test only works on macOS");
        enabled = false;
#endif
    }

    void Update()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        // Check if settings changed
        if (enableAlwaysOnTop != lastEnabledState ||
            useScreenSaverLevel != lastScreenSaverMode ||
            visibleOnAllSpaces != lastVisibleOnAllSpaces)
        {
            ApplySettings();
            lastEnabledState = enableAlwaysOnTop;
            lastScreenSaverMode = useScreenSaverLevel;
            lastVisibleOnAllSpaces = visibleOnAllSpaces;
        }
        
        // Update status every second
        if (Time.frameCount % 60 == 0)
        {
            UpdateStatus();
        }
        
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.F1))
        {
            enableAlwaysOnTop = !enableAlwaysOnTop;
            Debug.Log($"MacOSAlwaysOnTopTest: Toggled always-on-top to {enableAlwaysOnTop}");
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            useScreenSaverLevel = !useScreenSaverLevel;
            Debug.Log($"MacOSAlwaysOnTopTest: Toggled screen-saver level to {useScreenSaverLevel}");
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            visibleOnAllSpaces = !visibleOnAllSpaces;
            Debug.Log($"MacOSAlwaysOnTopTest: Toggled visible on all spaces to {visibleOnAllSpaces}");
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            PrintStatus();
        }
#endif
    }

    void ApplySettings()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        if (enableAlwaysOnTop)
        {
            if (useScreenSaverLevel)
            {
                MacOSWindowHelper.EnableAlwaysOnTopScreenSaverLevel(true);
                Debug.Log("MacOSAlwaysOnTopTest: Enabled screen-saver level always-on-top");
            }
            else
            {
                MacOSWindowHelper.EnableAlwaysOnTopOverFullscreen(true);
                Debug.Log("MacOSAlwaysOnTopTest: Enabled floating level always-on-top");
            }
        }
        else
        {
            MacOSWindowHelper.EnableAlwaysOnTopOverFullscreen(false);
            Debug.Log("MacOSAlwaysOnTopTest: Disabled always-on-top");
        }
        
        MacOSWindowHelper.SetVisibleOnAllSpaces(visibleOnAllSpaces);
        Debug.Log($"MacOSAlwaysOnTopTest: Set visible on all spaces to {visibleOnAllSpaces}");
#endif
    }

    void UpdateStatus()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        currentWindowLevel = MacOSWindowHelper.GetWindowLevel();
        canAppearOverFullscreen = MacOSWindowHelper.CanAppearOverFullscreen();
#endif
    }

    void PrintStatus()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        UpdateStatus();
        Debug.Log("=== macOS Window Status ===");
        Debug.Log($"Window Level: {currentWindowLevel}");
        Debug.Log($"Can Appear Over Fullscreen: {canAppearOverFullscreen}");
        Debug.Log($"Always On Top: {enableAlwaysOnTop}");
        Debug.Log($"Screen Saver Level: {useScreenSaverLevel}");
        Debug.Log($"Visible On All Spaces: {visibleOnAllSpaces}");
        Debug.Log("==========================");
        
        // Provide level interpretation
        string levelName = "Unknown";
        if (currentWindowLevel == MacOSWindowHelper.NSNormalWindowLevel) levelName = "Normal";
        else if (currentWindowLevel == MacOSWindowHelper.NSFloatingWindowLevel) levelName = "Floating";
        else if (currentWindowLevel == MacOSWindowHelper.NSModalPanelWindowLevel) levelName = "Modal";
        else if (currentWindowLevel == MacOSWindowHelper.NSScreenSaverWindowLevel) levelName = "ScreenSaver";
        
        Debug.Log($"Current level ({currentWindowLevel}) is: {levelName}");
#endif
    }

    void OnGUI()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("macOS Always-On-Top Test", GUI.skin.box);
        GUILayout.Space(10);
        
        GUILayout.Label("Keyboard Shortcuts:");
        GUILayout.Label("F1 - Toggle Always On Top");
        GUILayout.Label("F2 - Toggle Screen Saver Level");
        GUILayout.Label("F3 - Toggle Visible On All Spaces");
        GUILayout.Label("F4 - Print Status to Console");
        GUILayout.Space(10);
        
        GUILayout.Label($"Window Level: {currentWindowLevel}");
        GUILayout.Label($"Can Appear Over Fullscreen: {canAppearOverFullscreen}");
        GUILayout.Space(10);
        
        GUILayout.Label("Instructions:");
        GUILayout.Label("1. Make another app fullscreen (e.g., Safari)");
        GUILayout.Label("2. This Unity window should stay visible");
        GUILayout.Label("3. Use F1-F3 to test different modes");
        
        GUILayout.EndArea();
#endif
    }

    void OnDestroy()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        // Restore normal window behavior on exit
        MacOSWindowHelper.EnableAlwaysOnTopOverFullscreen(false);
        Debug.Log("MacOSAlwaysOnTopTest: Restored normal window behavior");
#endif
    }
}
