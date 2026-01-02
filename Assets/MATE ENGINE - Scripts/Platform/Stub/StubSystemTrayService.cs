using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubSystemTrayService : ISystemTrayService
    {
        public bool IsSupported => false;
        public bool Initialize(string appName, string tooltip, Texture2D icon, Func<List<(string, Action)>> buildMenu) => false;
        public bool UpdateIcon(Texture2D icon) => false;
        public bool UpdateTooltip(string tooltip) => false;
        public bool Remove() => false;
        
        public event Action OnLeftClick;
        public event Action OnRightClick;
    }
}
