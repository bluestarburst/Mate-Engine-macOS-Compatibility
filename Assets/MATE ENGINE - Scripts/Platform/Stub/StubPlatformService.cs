namespace MateEngine.Platform.Stub
{
    public class StubPlatformService : IPlatformService
    {
        public string PlatformName => "Unsupported Platform";
        
        public bool IsSupported(PlatformFeature feature) => false;
    }
}
