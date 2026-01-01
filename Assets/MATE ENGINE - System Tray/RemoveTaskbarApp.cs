using System;
using UnityEngine;
using MateEngine.Platform;

/// <summary>
/// Manages hiding/showing the application in the taskbar (Windows) or dock (macOS)
/// Now uses platform abstraction layer for cross-platform support
/// </summary>
public class RemoveTaskbarApp : MonoBehaviour
{
    private IWindowService _windowService;
    private bool _isHidden = true;
    
    public bool IsHidden => _isHidden;

    void Start()
    {
        _windowService = PlatformServiceLocator.GetWindowService();
        
        // Hide from taskbar/dock by default
        _windowService.HideFromDock();
        _isHidden = true;
    }

    /// <summary>
    /// Toggle between hidden and visible in taskbar/dock
    /// </summary>
    public void ToggleAppMode()
    {
        if (_isHidden)
        {
            _windowService.ShowInDock();
            _isHidden = false;
        }
        else
        {
            _windowService.HideFromDock();
            _isHidden = true;
        }
    }
}
