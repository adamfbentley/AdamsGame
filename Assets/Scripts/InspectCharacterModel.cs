using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility to inspect character model hierarchy and find bones/weapons
/// </summary>
public class InspectCharacterModel : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Inspect Character Model")]
    static void InspectModel()
    {
        // Load the character model
        GameObject characterModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/character.fbx");
        
        if (characterModel == null)
        {
            Debug.LogError("Could not find character.fbx in Assets folder!");
            return;
        }
        
        Debug.Log("========== CHARACTER MODEL INSPECTION ==========");
        Debug.Log($"Model: {characterModel.name}");
        Debug.Log("");
        
        // Recursively inspect all children
        InspectTransform(characterModel.transform, 0);
        
        Debug.Log("========== INSPECTION COMPLETE ==========");
    }
    
    static void InspectTransform(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        // Check what components this object has
        string info = $"{indent}├─ {t.name}";
        
        // Check for mesh
        if (t.GetComponent<MeshFilter>() != null || t.GetComponent<SkinnedMeshRenderer>() != null)
        {
            info += " [MESH]";
        }
        
        // Look for weapon-related names
        string nameLower = t.name.ToLower();
        if (nameLower.Contains("sword") || nameLower.Contains("weapon") || 
            nameLower.Contains("blade") || nameLower.Contains("knife") ||
            nameLower.Contains("dagger") || nameLower.Contains("axe"))
        {
            info += " <-- WEAPON?";
        }
        
        // Look for bone names (hands, hips, spine)
        if (nameLower.Contains("hand") || nameLower.Contains("finger") ||
            nameLower.Contains("hip") || nameLower.Contains("spine") ||
            nameLower.Contains("pelvis") || nameLower.Contains("thigh"))
        {
            info += " [BONE]";
        }
        
        Debug.Log(info);
        
        // Inspect children
        for (int i = 0; i < t.childCount; i++)
        {
            InspectTransform(t.GetChild(i), depth + 1);
        }
    }
#endif
}
