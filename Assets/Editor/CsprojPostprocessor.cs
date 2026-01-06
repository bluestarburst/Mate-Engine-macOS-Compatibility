using UnityEditor;

public class CsprojPostprocessor : AssetPostprocessor
{
    public static string OnGeneratedCSProject(string path, string content)
    {
        // Note: System.Windows.Forms is handled by link.xml for IL2CPP builds
        // and by platform-specific code guards (#if UNITY_STANDALONE_WIN) for runtime
        // No unconditional reference needed here
        return content;
    }
}
