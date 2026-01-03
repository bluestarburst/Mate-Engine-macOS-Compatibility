using UnityEngine;
using UnityEngine.Scripting;
using UniHumanoid;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// Forces the IL2CPP/AOT compiler to generate native code for VRM-related classes.
/// Attach this script to any GameObject in your starting scene (e.g., Main Camera).
/// The public fields force the compiler to include these types even if they're only
/// accessed via Reflection at runtime.
/// </summary>
[Preserve]
public class VRM_AOT_Fix : MonoBehaviour
{
    // REMOVED public fields to prevent triggering the static initializer on background threads.
    // References are now forced in Start() and ForceAOTCompilation() below.

    /// <summary>
    /// Called automatically before scene load to ensure types are registered.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    [Preserve]
    private static void ForceAOTCompilation()
    {
        // Force the compiler to see these types
        Type[] typesToPreserve = new Type[]
        {
            typeof(BoneLimit),
            typeof(AvatarDescription),
            typeof(List<BoneLimit>),
            typeof(Dictionary<HumanBodyBones, string>),
            typeof(Dictionary<string, HumanBodyBones>),
        };

        // Touch HumanTrait to force its static data to be included
        try
        {
            int boneCount = HumanTrait.BoneCount;
            // Do NOT call HumanTrait.BoneName here - let BoneLimit.cachedProperties handle it lazily
            if (boneCount > 0)
            {
                // Force Enum.Parse path used by BoneLimit static initializer
                var parsed = Enum.Parse(typeof(HumanBodyBones), "Hips");
                if (parsed != null && typesToPreserve != null)
                {
                    Debug.Log("[VRM_AOT_Fix] AOT types registered successfully");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[VRM_AOT_Fix] Initialization warning (may be normal): {e.Message}");
        }
    }

    [Preserve]
    private void Awake()
    {
        // Create a local list to ensure the generic type is generated
        var boneList = new List<BoneLimit>();

        // Create explicit usage of BoneLimit to force AOT code generation
        BoneLimit dummy = new BoneLimit
        {
            humanBone = HumanBodyBones.Hips,
            boneName = "Hips",
            useDefaultValues = true,
            min = Vector3.zero,
            max = Vector3.zero,
            center = Vector3.zero,
            axisLength = 0
        };

        // Force the struct to be fully used
        boneList.Add(dummy);
        boneList.Clear();

        // Log to confirm the fix is active
        Debug.Log("[VRM_AOT_Fix] AOT preservation active");
    }

    /// <summary>
    /// Additional forced references to ensure all related types are compiled.
    /// This method is never called but forces the compiler to generate code.
    /// </summary>
    [Preserve]
    private void ForceTypeGeneration()
    {
        // BoneLimit operations
        BoneLimit limit = new BoneLimit();
        HumanBone humanBone = limit.ToHumanBone();

        // Dictionary operations used in static initializer
        var dict1 = new Dictionary<HumanBodyBones, string>();
        var dict2 = new Dictionary<string, HumanBodyBones>();
        dict1[HumanBodyBones.Hips] = "Hips";
        dict2["Hips"] = HumanBodyBones.Hips;

        // LINQ operations
        var list = new List<BoneLimit> { limit };
        var array = list.ToArray();
        var selected = list.Select(x => x.boneName);
        var dict = list.ToDictionary(x => x.humanBone, x => x.boneName);

        // Prevent optimization
        if (humanBone.boneName == null && dict1.Count < 0 && dict2.Count < 0 &&
            array.Length < 0 && selected == null && dict.Count < 0)
        {
            Debug.Log("Never prints");
        }
    }
}
