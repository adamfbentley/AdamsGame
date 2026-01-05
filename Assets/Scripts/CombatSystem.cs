// C# LANGUAGE: using directive imports code libraries (like #include in C++)
// This imports Unity's game engine code so we can use GameObjects, Components, etc.
using UnityEngine;

/// <summary>
/// Combat system - handles player attacks, damage calculation, and knockback physics
/// C# XML DOCUMENTATION: Three slashes (///) create documentation that shows in Unity's Inspector
/// </summary>
// CLASS DEFINITION: 'public' means other scripts can access this class
// 'class' declares a new type/blueprint for objects
// ': MonoBehaviour' means this inherits from Unity's base component class (enables Unity lifecycle)
public class CombatSystem : MonoBehaviour
    {
        // SERIALIZED FIELDS: [SerializeField] makes private variables visible in Unity Inspector
        // This lets you adjust values in Unity without recompiling code
        // 'private' means only this class can access the variable
        // 'float' is a decimal number type (32-bit floating point)
        [SerializeField] private float attackCooldown = 0.5f; // Seconds between attacks (f suffix = float literal)
        [SerializeField] private float attackRange = 3.5f;     // Distance player can hit enemies (in Unity units)
        [SerializeField] private float attackDamage = 35f;     // How much health damage each hit deals
        [SerializeField] private float knockbackForce = 15f;   // How far enemies are pushed back
        [SerializeField] private LayerMask enemyLayer;         // LayerMask filters which objects can be hit
        
        [Header("Heavy Attack Settings")]
        [SerializeField] private float heavyAttackCooldown = 1.0f;  // Longer cooldown for heavy attack
        [SerializeField] private float heavyAttackRange = 4.0f;     // Slightly longer range
        [SerializeField] private float heavyAttackDamage = 60f;     // More damage than light attack
        [SerializeField] private float heavyKnockbackForce = 25f;   // Stronger knockback
        
        // PRIVATE VARIABLES: Only accessible within this class
        private float lastAttackTime = 0f;                     // Timestamp of last attack (for cooldown)
        private float lastHeavyAttackTime = 0f;                // Timestamp of last heavy attack
        private Animator animator;                              // Reference to animation controller component
        private CharacterController controller;                 // Reference to movement physics component
        private EnhancedPlayerController playerController;      // Reference to player movement script
        
        // UNITY LIFECYCLE: Start() is called once when the GameObject spawns
        // Use this for initialization/setup code
        void Start()
        {
            // GetComponentInChildren<T>() searches this GameObject and all children for component of type T
            // <Animator> is a GENERIC TYPE PARAMETER (like templates in C++)
            animator = GetComponentInChildren<Animator>();
            
            // GetComponent<T>() searches only this GameObject for component of type T
            controller = GetComponent<CharacterController>();
            playerController = GetComponent<EnhancedPlayerController>();
            
            // DEBUG: Verify components were found
            if (animator == null)
                Debug.LogError("❌ CombatSystem: Animator NOT FOUND! Attacks won't work!");
            else
                Debug.Log($"✓ CombatSystem: Animator found on {animator.gameObject.name}");
            
            if (controller == null)
                Debug.LogError("❌ CombatSystem: CharacterController NOT FOUND!");
            else
                Debug.Log("✓ CombatSystem: CharacterController found");
        }
        
        // UNITY LIFECYCLE: Update() is called every frame (~60 times per second)
        // Use this for input handling and continuous checks
        void Update()
        {
            // INPUT: Input.GetMouseButtonDown(0) returns TRUE the first frame the left mouse button is pressed
            // 0=left button, 1=right button, 2=middle button
            // && is the LOGICAL AND operator (both conditions must be TRUE)
            // controller.isGrounded checks if player is touching the ground (prevents air attacks)
            if (Input.GetMouseButtonDown(0) && controller.isGrounded)
            {
                Debug.Log("🗡 Light attack input detected!");
                // METHOD CALL: Executes the TryAttack function defined below
                TryAttack();
            }
            
            // KEYBOARD INPUT: Check for "1" key press for heavy attack
            // Input.GetKeyDown() returns TRUE the first frame a key is pressed
            // KeyCode.Alpha1 is the number "1" key on top row of keyboard
            if (Input.GetKeyDown(KeyCode.Alpha1) && controller.isGrounded)
            {
                Debug.Log("⚔ Heavy attack input detected!");
                // Call heavy attack method
                TryHeavyAttack();
            }
        }
        
        // METHOD DEFINITION: 'void' means this function doesn't return a value
        // Functions organize code into reusable blocks
        void TryAttack()
        {
            // COOLDOWN CHECK: Time.time is total seconds since game started
            // If (current time - last attack time) is less than cooldown, attack is blocked
            // This prevents spam clicking attacks
            if (Time.time - lastAttackTime < attackCooldown)
            {
                Debug.Log("⏱ Attack on cooldown");
                return; // 'return' exits the function early
            }
            
            // Update timestamp to current time for next cooldown check
            lastAttackTime = Time.time;
            
            Debug.Log("✓ Executing light attack!");
            
            // NULL CHECK: 'null' means the reference points to nothing
            // Always check if components exist before using them to prevent crashes
            if (animator != null)
            {
                // SetTrigger() activates an animation transition in Unity's Animator
                // "Attack" is a STRING LITERAL (text in quotes) matching the parameter name in Animator
                animator.SetTrigger("Attack");
                Debug.Log("✓ Attack animation triggered");
            }
            else
            {
                Debug.LogError("❌ Cannot play attack animation - Animator is null!");
            }
            
            // PHYSICS QUERY: Check for enemies within attack radius
            // Collider[] is an ARRAY (list) of Collider components
            // Physics.OverlapSphere() returns all colliders touching an invisible sphere
            Collider[] hitEnemies = Physics.OverlapSphere(
                // PARAMETER 1: Center position of detection sphere
                // transform.position is this GameObject's world position (Vector3)
                // transform.forward is direction character faces (Vector3, magnitude 1)
                // * 1.5f multiplies the direction vector (moves sphere forward)
                // + operator adds vectors (moves sphere 1.5 units in front of player)
                transform.position + transform.forward * 1.5f, 
                attackRange,  // PARAMETER 2: Radius of the detection sphere
                enemyLayer    // PARAMETER 3: Only detect objects on this layer (filters results)
            );
            
            // FOREACH LOOP: Iterates through each element in the array
            // 'Collider enemy' declares a variable to hold each collider one at a time
            // 'in hitEnemies' specifies the array to loop through
            foreach (Collider enemy in hitEnemies)
            {
                // Try to get the Health component from the enemy GameObject
                Health health = enemy.GetComponent<Health>();
                if (health != null) // Check if enemy has a Health component
                {
                    // METHOD CALL with parameter: Tells the enemy to reduce health
                    health.TakeDamage(attackDamage);
                    
                    // STRING INTERPOLATION: $"text {variable}" inserts variable values into strings
                    // Debug.Log() prints to Unity's Console window (useful for testing)
                    Debug.Log($"HIT! {enemy.name} took {attackDamage} damage!");
                    
                    // KNOCKBACK PHYSICS: Push enemy away from player
                    // Vector3 is a 3D point/direction (x, y, z coordinates)
                    // Subtract player position from enemy position = direction from player to enemy
                    // .normalized converts vector to length 1 while keeping direction (unit vector)
                    Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    knockbackDir.y = 0; // Zero out vertical component (only push horizontally)
                    
                    // Try to get enemy's CharacterController (handles movement)
                    CharacterController enemyController = enemy.GetComponent<CharacterController>();
                    if (enemyController != null)
                    {
                        // Move() applies motion to CharacterController
                        // Multiply direction by force, then by Time.deltaTime for frame-rate independence
                        // Time.deltaTime = time since last frame (makes movement smooth on any framerate)
                        enemyController.Move(knockbackDir * knockbackForce * Time.deltaTime);
                    }
                }
            }
        }
        
        // HEAVY ATTACK METHOD: Similar to TryAttack but with different parameters
        // Uses separate cooldown timer and different damage/range values
        void TryHeavyAttack()
        {
            // Check heavy attack cooldown (longer than light attack)
            if (Time.time - lastHeavyAttackTime < heavyAttackCooldown)
                return;
            
            // Update heavy attack timestamp
            lastHeavyAttackTime = Time.time;
            
            // Trigger heavy attack animation
            // NOTE: You must create "HeavyAttack" trigger in Animator controller
            if (animator != null)
            {
                animator.SetTrigger("HeavyAttack");
            }
            
            // Detect enemies with heavy attack range
            Collider[] hitEnemies = Physics.OverlapSphere(
                transform.position + transform.forward * 1.5f, 
                heavyAttackRange,  // Use heavy attack range (slightly larger)
                enemyLayer
            );
            
            // Apply heavy attack damage and knockback
            foreach (Collider enemy in hitEnemies)
            {
                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    // Deal heavy attack damage (more than light attack)
                    health.TakeDamage(heavyAttackDamage);
                    Debug.Log($"HEAVY HIT! {enemy.name} took {heavyAttackDamage} damage!");
                    
                    // Apply stronger knockback
                    Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    knockbackDir.y = 0;
                    
                    CharacterController enemyController = enemy.GetComponent<CharacterController>();
                    if (enemyController != null)
                    {
                        // Use heavy knockback force (stronger push)
                        enemyController.Move(knockbackDir * heavyKnockbackForce * Time.deltaTime);
                    }
                }
            }
        }
        
        // UNITY DEBUG: OnDrawGizmosSelected() draws debug visuals in Scene view when object is selected
        // Gizmos only appear in editor, not in built game
        void OnDrawGizmosSelected()
        {
            // Set gizmo drawing color to red
            Gizmos.color = Color.red;
            
            // DrawWireSphere() shows the attack detection sphere in Scene view
            // Helps visualize exactly where attacks will hit
            Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, attackRange);
        }
    }
