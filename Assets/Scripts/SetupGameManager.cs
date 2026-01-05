using UnityEngine;

/// <summary>
/// Attach this to GameManager GameObject to setup UI system
/// </summary>
public class SetupGameManager : MonoBehaviour
{
    // Awake() runs BEFORE Start() - ensures GameManager exists before UI needs it
    void Awake()
    {
        // Add UIManager component
        if (GetComponent<UIManager>() == null)
        {
            gameObject.AddComponent<UIManager>();
        }
        
        Debug.Log("✓ GameManager setup complete");
    }
}
