using UnityEngine;

/// <summary>
/// Attach this to Main Camera to automatically configure third person camera
/// </summary>
public class SetupCamera : MonoBehaviour
{
    // Awake() runs BEFORE Start() - ensures camera is ready before other scripts
    void Awake()
    {
        // Ensure tag is set
        gameObject.tag = "MainCamera";
        
        // Add Camera component if missing
        if (GetComponent<Camera>() == null)
        {
            gameObject.AddComponent<Camera>();
        }
        
        // Add AudioListener if missing
        if (GetComponent<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
        
        // Add ThirdPersonCamera component if missing
        if (GetComponent<ThirdPersonCamera>() == null)
        {
            gameObject.AddComponent<ThirdPersonCamera>();
        }
        
        // Set initial position
        if (transform.position == Vector3.zero)
        {
            transform.position = new Vector3(0, 1.2f, -8f);
        }
        
        Debug.Log("✓ Camera setup complete");
    }
}
