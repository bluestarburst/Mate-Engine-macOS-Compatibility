using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform
{
    public interface ISystemTrayService
    {
        bool IsSupported { get; }
        bool Initialize(string appName, string tooltip, Texture2D icon, Func<List<(string, Action)>> buildMenu);
        bool UpdateIcon(Texture2D icon);
        bool UpdateTooltip(string tooltip);
        bool Remove();
        
        event Action OnLeftClick;
        event Action OnRightClick;
    }
}
