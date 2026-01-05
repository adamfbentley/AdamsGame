// using directive: Imports Unity engine classes
using UnityEngine;

/// <summary>
/// Health system for player and enemies - tracks HP, damage, and death
/// </summary>
public class Health : MonoBehaviour
    {
        // [SerializeField]: Makes private fields editable in Unity Inspector
        // Without this, private fields are hidden
        [SerializeField] private float maxHealth = 100f; // Max HP value (f = float literal)
        
        // Private variables: Only accessible within this class
        private float currentHealth;  // Current HP (changes during gameplay)
        private bool isDead = false;  // Flag to prevent multiple death triggers
        
        // Start(): Called once when object is created
        void Start()
        {
            // Initialize current health to maximum at spawn
            currentHealth = maxHealth;
        }
        
        // PUBLIC METHOD: Other scripts can call this (e.g., CombatSystem calls TakeDamage)
        // Parameter: 'float damage' - amount of HP to subtract
        public void TakeDamage(float damage)
        {
            // Early exit if already dead (prevents negative health)
            if (isDead) return;
            
            // -= operator: Shorthand for currentHealth = currentHealth - damage
            currentHealth -= damage;
            
            // String interpolation: $"" allows embedding variables with {}
            Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");
            
            // <= operator: Less than or equal to
            if (currentHealth <= 0)
            {
                Die(); // Call death function
            }
        }
        
        // PRIVATE METHOD: Only this class can call Die()
        void Die()
        {
            isDead = true; // Set flag to prevent double deaths
            
            // Try to get Animator component from this object or children
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) // Null check: only proceed if animator exists
            {
                // Trigger death animation
                animator.SetTrigger("Death");
            }
            
            // Destroy(gameObject, delay): Remove this GameObject after 2 seconds
            // Delay allows death animation to play
            Destroy(gameObject, 2f);
        }
        
        // PROPERTY METHODS: Provide read-only access to private data
        // => is EXPRESSION-BODIED MEMBER syntax (shorthand for simple returns)
        // Equivalent to: public float GetHealthPercent() { return currentHealth / maxHealth; }
        public float GetHealthPercent() => currentHealth / maxHealth; // Returns 0.0 to 1.0 (percentage)
        public float GetCurrentHealth() => currentHealth;              // Returns exact HP value
        public float GetMaxHealth() => maxHealth;                      // Returns max HP value
        public bool IsDead() => isDead;                                 // Returns death status
    }
