namespace MateEngine.Platform.Windows
{
    public class WindowsPlatformService : IPlatformService
    {
        public string PlatformName => "Windows";
        
        public bool IsSupported(PlatformFeature feature)
        {
            // All features are supported on Windows
            return true;
        }
    }
}
