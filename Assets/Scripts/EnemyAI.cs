using UnityEngine;

/// <summary>
/// Basic Enemy AI - Step 1: Detection only
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2.5f;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    
    private Transform player;
    private bool playerDetected = false;
    private Renderer enemyRenderer;
    private Material enemyMaterial;
    private CharacterController controller;
    
    void Start()
    {
        // Get components
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyMaterial = enemyRenderer.material;
        }
        
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError($"{gameObject.name}: No CharacterController found!");
        }
        
        // Find player in scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"<color=green>{gameObject.name}: Found player at {player.position}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>{gameObject.name}: No player found with 'Player' tag!</color>");
        }
    }
    
    void Update()
    {
        if (player == null)
            return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            if (!playerDetected)
            {
                playerDetected = true;
                // Change color to yellow when detected
                if (enemyMaterial != null)
                    enemyMaterial.color = Color.yellow;
                Debug.Log($"<color=yellow>{gameObject.name}: PLAYER DETECTED at {distanceToPlayer:F1} units!</color>");
            }
            
            // Face the player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0; // Keep on horizontal plane
            
            if (directionToPlayer.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            
            // Move towards player if outside attack range
            if (distanceToPlayer > attackRange && controller != null)
            {
                Vector3 moveDirection = directionToPlayer * moveSpeed;
                
                // Apply simple gravity if not grounded
                if (!controller.isGrounded)
                {
                    moveDirection.y = -9.81f;
                }
                
                controller.Move(moveDirection * Time.deltaTime);
            }
            else if (distanceToPlayer <= attackRange)
            {
                // Within attack range - turn green (attack placeholder)
                if (enemyMaterial != null)
                    enemyMaterial.color = Color.green;
            }
        }
        else
        {
            if (playerDetected)
            {
                playerDetected = false;
                // Change back to red when lost
                if (enemyMaterial != null)
                    enemyMaterial.color = new Color(0.8f, 0.2f, 0.2f);
                Debug.Log($"<color=red>{gameObject.name}: Player out of range</color>");
            }
        }
    }
    
    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
