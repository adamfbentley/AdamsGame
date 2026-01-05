using UnityEngine;

/// <summary>
/// Attach this to an enemy GameObject to automatically configure enemy components
/// </summary>
public class SetupEnemy : MonoBehaviour
{
    [SerializeField] private Vector3 enemyScale = new Vector3(0.5f, 1f, 0.5f);
    [SerializeField] private Color enemyColor = new Color(0.8f, 0.2f, 0.2f); // Red
    
    // Awake() runs BEFORE Start() - ensures enemy components exist before game starts
    void Awake()
    {
        // Create visual capsule if no renderer exists
        if (GetComponent<MeshRenderer>() == null)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.parent = transform;
            capsule.transform.localPosition = Vector3.zero;
            capsule.transform.localScale = enemyScale;
            
            // Remove default capsule collider (we use CharacterController)
            Collider col = capsule.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            
            // Apply color
            Renderer renderer = capsule.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = enemyColor;
                renderer.material = mat;
            }
        }
        
        // Add CharacterController
        if (GetComponent<CharacterController>() == null)
        {
            CharacterController cc = gameObject.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.25f;
            cc.center = new Vector3(0, 0, 0);
        }
        
        // Add enemy components
        if (GetComponent<EnemyAI>() == null) gameObject.AddComponent<EnemyAI>();
        if (GetComponent<Health>() == null) gameObject.AddComponent<Health>();
        
        // Set initial position if at origin
        if (transform.position == Vector3.zero)
        {
            transform.position = new Vector3(5, 1, 0);
        }
        
        Debug.Log("✓ Enemy setup complete");
    }
}
