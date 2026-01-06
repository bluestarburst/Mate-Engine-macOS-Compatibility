using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug; // Disambiguate Unity Debug

/// <summary>
/// Post-build script that automatically configures macOS app for always-on-top functionality
/// Runs after every macOS build and:
/// 1. Adds LSUIElement=true to Info.plist (makes app a background utility)
/// 2. Adds NSHighResolutionCapable=true for high-DPI support
/// 3. Verifies the changes were applied
/// </summary>
public class PostBuildMacOSConfig
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.StandaloneOSX)
        {
       return; // Only process macOS builds
        }

        Debug.Log("=== Post-Build: Configuring macOS app for always-on-top ===");

        // Determine the app bundle path
        // If pathToBuiltProject ends in .app, it's the bundle itself
        // Otherwise it might be a folder containing the app
        string appBundlePath = pathToBuiltProject;
        
        if (!appBundlePath.EndsWith(".app"))
        {
            // Assume the built app is directly in the provided path
            // Try to find the .app folder
            if (Directory.Exists(pathToBuiltProject) && !pathToBuiltProject.EndsWith(".app"))
            {
                // pathToBuiltProject is likely a folder containing the .app
                // Use it as-is
                appBundlePath = pathToBuiltProject;
            }
        }

        Debug.Log($"App bundle path: {appBundlePath}");

        // Path to the add_lsuielement.sh script
        string scriptPath = Path.Combine(Application.dataPath, 
            "MATE ENGINE - Scripts", "Plugins", "MacOS", "add_lsuielement.sh");

        if (!File.Exists(scriptPath))
        {
            Debug.LogWarning($"Post-build script not found at: {scriptPath}");
            Debug.LogWarning("Skipping automatic LSUIElement configuration.");
            return;
        }

        Debug.Log($"Running configuration script: {scriptPath}");

        // Run the add_lsuielement.sh script
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{scriptPath}\" \"{appBundlePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Debug.Log("✓ macOS app configuration successful!");
                    Debug.Log(output);
                }
                else
                {
                    Debug.LogError($"✗ macOS app configuration failed (exit code: {process.ExitCode})");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to run post-build configuration: {ex.Message}");
        }

        // Create entitlements to explicitly disable sandbox (important for window tracking)
        try
        {
            string entitlementsPath = Path.Combine(appBundlePath, "Contents", "Resources", "app.entitlements");
            string entitlementsDir = Path.GetDirectoryName(entitlementsPath);
            if (!Directory.Exists(entitlementsDir)) Directory.CreateDirectory(entitlementsDir);

            const string entitlementsContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>com.apple.security.app-sandbox</key>
    <false/>
    <key>com.apple.security.files.user-selected.read-write</key>
    <true/>
</dict>
</plist>";

            File.WriteAllText(entitlementsPath, entitlementsContent);
            Debug.Log($"✓ Entitlements written to disable sandbox: {entitlementsPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to write entitlements: {ex.Message}");
        }

        Debug.Log("=== Post-Build Configuration Complete ===");
    }
}
