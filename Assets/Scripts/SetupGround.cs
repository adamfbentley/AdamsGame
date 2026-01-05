using UnityEngine;

/// <summary>
/// Attach this to an empty GameObject to create a ground plane
/// </summary>
public class SetupGround : MonoBehaviour
{
    [SerializeField] private Vector3 planeScale = new Vector3(10, 1, 10);
    
    // Awake() runs BEFORE Start() - creates ground before other objects spawn
    void Awake()
    {
        // Create ground plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "GroundPlane";
        plane.transform.parent = transform;
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = planeScale;
        
        // Optional: Add material
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.5f, 0.3f); // Greenish ground
            renderer.material = mat;
        }
        
        Debug.Log("✓ Ground created");
    }
}
