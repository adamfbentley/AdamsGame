using UnityEngine;

/// <summary>
/// Dark Souls-style third person camera
/// Right mouse button rotates camera - WASD movement responds to camera direction
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private float followDistance = 8f;
    [SerializeField] private float followHeight = 1.2f;
    [SerializeField] private float mouseSensitivity = 3f;
    
    [Header("Zoom")]
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float zoomSpeed = 2f;
    
    private Transform playerTarget;
    public float rotationX = 0f;
    public float rotationY = 25f;
    
    void Start()
    {
        // Find player - back to following parent since we sync positions now
        playerTarget = FindObjectOfType<EnhancedPlayerController>()?.transform;
        if (playerTarget == null)
            playerTarget = GameObject.Find("Player")?.transform;
        
        if (playerTarget == null)
        {
            Debug.LogError("[ThirdPersonCamera] Player not found!");
            enabled = false;
            return;
        }
        
        // Initialize camera behind player
        Vector3 playerPos = playerTarget.position;
        transform.position = playerPos + new Vector3(0, followHeight, -followDistance);
        rotationX = 0f; // Face forward
        rotationY = 25f; // Look slightly down;
    }
    
    void LateUpdate()
    {
        if (playerTarget == null) return;
        
        // Mouse wheel zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            followDistance -= scrollInput * zoomSpeed;
            followDistance = Mathf.Clamp(followDistance, minDistance, maxDistance);
        }
        
        // Right mouse button rotates camera
        if (Input.GetMouseButton(1))
        {
            // Lock cursor while rotating
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationY = Mathf.Clamp(rotationY, -30f, 60f);
        }
        else
        {
            // Release cursor when not rotating
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Position camera behind player based on rotation
        Vector3 targetLookAt = playerTarget.position + Vector3.up * followHeight;
        
        // Calculate offset using rotation angles
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        Vector3 offset = rotation * (Vector3.back * followDistance);
        
        // Lock camera directly to player position (no smoothing)
        transform.position = targetLookAt + offset;
        
        // Always look at player
        transform.LookAt(targetLookAt);
    }
    
    /// <summary>Get the camera's horizontal forward direction (y=0)</summary>
    public Vector3 GetCameraForward()
    {
        // Use rotationX only for horizontal direction
        Vector3 forward = Quaternion.Euler(0, rotationX, 0) * Vector3.forward;
        return forward;
    }
    
    /// <summary>Get the camera's horizontal right direction (y=0)</summary>
    public Vector3 GetCameraRight()
    {
        // Use rotationX only for horizontal direction
        Vector3 right = Quaternion.Euler(0, rotationX, 0) * Vector3.right;
        return right;
    }
}
