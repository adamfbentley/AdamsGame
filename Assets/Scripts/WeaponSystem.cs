// using directive: Import Unity's core classes
using UnityEngine;

/// <summary>
/// Attaches a weapon prefab to the character's left hand bone
/// Allows position/rotation adjustment for proper grip
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    // [Header]: Creates a label in Inspector to organize fields
    [Header("Weapon Settings")]
    [SerializeField] private GameObject weaponPrefab;      // Reference to weapon model (drag prefab here)
    [SerializeField] private bool showWeaponAtStart = true; // Should weapon appear immediately?
    
    [Header("Grip Adjustment")]
    // Vector3: 3D coordinate/direction (x, y, z)
    // These adjust weapon position/rotation in the hand
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 0, 0); // Move weapon (units)
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 0); // Rotate weapon (degrees)
    [SerializeField] private Vector3 scaleOffset = new Vector3(1, 1, 1);    // Resize weapon (multipliers)
    
    // Private variables: Internal state
    private GameObject currentWeapon; // Instance of equipped weapon in scene
    private Transform leftHandBone;   // Reference to hand bone in character skeleton
    
    // Start(): Unity lifecycle method - called once on spawn
    void Start()
    {
        // Call helper method to locate hand bone
        FindLeftHandBone();
        
        // Logical AND (&&): Both conditions must be true
        // Equip weapon if enabled AND prefab is assigned
        if (showWeaponAtStart && weaponPrefab != null)
        {
            EquipWeapon();
        }
    }
    
    // Helper method: Searches character hierarchy for left hand bone
    void FindLeftHandBone()
    {
        // GetComponentsInChildren: Returns array of ALL Transform components
        // This includes every bone, mesh, and child object
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        // foreach: Loop through each element in array
        // 'Transform child' creates a variable holding current element
        foreach (Transform child in allChildren)
        {
            // .Contains(): String method checking if substring exists
            // Searches for "LeftHand" in bone name (case sensitive)
            if (child.name.Contains("LeftHand"))
            {
                leftHandBone = child; // Store reference
                Debug.Log($"[WeaponSystem] Found left hand bone: {child.name}");
                return; // Exit method early (found what we need)
            }
        }
        
        // If loop completes without return, hand wasn't found
        Debug.LogError("[WeaponSystem] Could not find LeftHand bone in character hierarchy!");
    }
    
    // PUBLIC METHOD: Other scripts can call this to equip weapon
    public void EquipWeapon()
    {
        // Guard clauses: Check conditions before proceeding
        // Returns early if requirements aren't met
        if (leftHandBone == null)
        {
            Debug.LogError("[WeaponSystem] Cannot equip weapon - left hand bone not found!");
            return; // Exit method
        }
        
        if (weaponPrefab == null)
        {
            Debug.LogError("[WeaponSystem] Cannot equip weapon - no weapon prefab assigned!");
            return;
        }
        
        // Remove old weapon if exists (prevents stacking multiple weapons)
        if (currentWeapon != null)
        {
            // Destroy(): Removes GameObject from scene
            Destroy(currentWeapon);
        }
        
        // Instantiate(): Creates a copy of a prefab in the scene
        // Parameter 1: prefab to copy
        // Parameter 2: parent transform (weapon becomes child of hand bone)
        // This parents weapon to hand, so it moves with animations
        currentWeapon = Instantiate(weaponPrefab, leftHandBone);
        
        // Adjust weapon transform relative to hand bone
        // .transform accesses the GameObject's Transform component
        // .localPosition: Position relative to parent (not world position)
        currentWeapon.transform.localPosition = positionOffset;
        
        // Quaternion: Represents 3D rotations (complex math)
        // Quaternion.Euler(): Converts degrees (x,y,z) to Quaternion
        // .localRotation: Rotation relative to parent
        currentWeapon.transform.localRotation = Quaternion.Euler(rotationOffset);
        
        // .localScale: Size multiplier relative to parent (1,1,1 = normal size)
        currentWeapon.transform.localScale = scaleOffset;
        
        Debug.Log($"[WeaponSystem] Equipped weapon: {weaponPrefab.name}");
    }
    
    // PUBLIC METHOD: Removes equipped weapon
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);      // Remove from scene
            currentWeapon = null;         // Clear reference
            Debug.Log("[WeaponSystem] Unequipped weapon");
        }
    }
    
    // PUBLIC METHOD: Show/hide weapon without destroying it
    // Useful for sheathing/unsheathing
    public void SetWeaponVisible(bool visible)
    {
        if (currentWeapon != null)
        {
            // SetActive(): Enables/disables GameObject
            // false = invisible and inactive, true = visible and active
            currentWeapon.SetActive(visible);
        }
    }
}
