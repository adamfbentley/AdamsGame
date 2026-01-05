using UnityEngine;

/// <summary>
/// Click-to-target system like WoW - click enemies to select them
/// </summary>
public class TargetingSystem : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float maxTargetDistance = 50f;
    [SerializeField] private KeyCode targetKey = KeyCode.Tab; // Tab to cycle targets
    
    private GameObject currentTarget;
    private Camera mainCamera;
    private GameObject targetIndicator;
    
    void Start()
    {
        mainCamera = Camera.main;
        CreateTargetIndicator();
    }
    
    void Update()
    {
        // Left-click to target
        if (Input.GetMouseButtonDown(0))
        {
            TryTargetUnderMouse();
        }
        
        // Tab to cycle targets (future feature)
        if (Input.GetKeyDown(targetKey))
        {
            CycleTarget();
        }
        
        // Update target indicator position
        UpdateTargetIndicator();
        
        // Clear target if it's destroyed
        if (currentTarget != null && currentTarget.GetComponent<Health>() != null)
        {
            if (currentTarget.GetComponent<Health>().IsDead())
            {
                ClearTarget();
            }
        }
    }
    
    void TryTargetUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxTargetDistance))
        {
            // Check if we hit something with Health (enemy or NPC)
            Health health = hit.collider.GetComponent<Health>();
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            
            if (health != null && enemy != null)
            {
                SetTarget(hit.collider.gameObject);
            }
        }
    }
    
    void SetTarget(GameObject target)
    {
        // Clear old target
        if (currentTarget != null)
        {
            // Remove old highlight
            Renderer oldRenderer = currentTarget.GetComponent<Renderer>();
            if (oldRenderer != null)
            {
                oldRenderer.material.SetFloat("_Outline", 0f);
            }
        }
        
        currentTarget = target;
        
        if (currentTarget != null)
        {
            Health health = currentTarget.GetComponent<Health>();
            string targetName = currentTarget.name;
            float healthPercent = health != null ? health.GetHealthPercent() * 100f : 0f;
            
            Debug.Log($"<color=cyan>TARGETED: {targetName} ({healthPercent:F0}% HP)</color>");
        }
    }
    
    void ClearTarget()
    {
        if (currentTarget != null)
        {
            Debug.Log($"<color=grey>Target cleared: {currentTarget.name}</color>");
        }
        currentTarget = null;
    }
    
    void CycleTarget()
    {
        // Find all enemies with EnemyAI component
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        
        if (enemies.Length == 0)
        {
            ClearTarget();
            return;
        }
        
        // Find current target index
        int currentIndex = -1;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].gameObject == currentTarget)
            {
                currentIndex = i;
                break;
            }
        }
        
        // Cycle to next target
        int nextIndex = (currentIndex + 1) % enemies.Length;
        SetTarget(enemies[nextIndex].gameObject);
    }
    
    void CreateTargetIndicator()
    {
        // Create a hollow ring using LineRenderer
        targetIndicator = new GameObject("TargetIndicator");
        LineRenderer lineRenderer = targetIndicator.AddComponent<LineRenderer>();
        
        // Configure line renderer for a circle
        int segments = 40;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        
        // Line appearance
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 0.9f, 0.2f, 0.8f); // Subtle yellow
        lineRenderer.endColor = new Color(1f, 0.9f, 0.2f, 0.8f);
        
        // Create circle points
        float radius = 0.6f;
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            angle += 360f / segments;
        }
        
        targetIndicator.SetActive(false);
    }
    
    void UpdateTargetIndicator()
    {
        if (currentTarget != null && !currentTarget.GetComponent<Health>().IsDead())
        {
            targetIndicator.SetActive(true);
            
            // Position at target's feet
            Vector3 targetPos = currentTarget.transform.position;
            targetPos.y = 0.05f; // Just above ground
            targetIndicator.transform.position = targetPos;
            
            // Slow rotation for subtle animation
            targetIndicator.transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
        else
        {
            targetIndicator.SetActive(false);
        }
    }
    
    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }
    
    public bool HasTarget()
    {
        return currentTarget != null;
    }
}
