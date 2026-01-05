using UnityEngine;

/// <summary>
/// Attach this to a CombatManager GameObject to manage global combat settings
/// Note: Individual CombatSystem components on Player still handle attacks
/// This manages shared combat rules and settings
/// </summary>
public class SetupCombatManager : MonoBehaviour
{
    [Header("Global Combat Settings")]
    [SerializeField] private float globalDamageMultiplier = 1.0f;
    [SerializeField] private bool showDamageNumbers = true;
    
    void Start()
    {
        Debug.Log("✓ CombatManager setup complete");
    }
    
    // PUBLIC METHOD: Other scripts can call this to get damage multiplier
    public float GetDamageMultiplier()
    {
        return globalDamageMultiplier;
    }
    
    // PUBLIC METHOD: Check if damage numbers should be displayed
    public bool ShouldShowDamageNumbers()
    {
        return showDamageNumbers;
    }
}
