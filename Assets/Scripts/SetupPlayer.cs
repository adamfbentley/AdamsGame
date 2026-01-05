using UnityEngine;

/// <summary>
/// Attach this to the Player GameObject to automatically configure all components
/// </summary>
public class SetupPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject characterModelPrefab; // Drag character.fbx here
    [SerializeField] private RuntimeAnimatorController animatorController; // Drag PlayerAnimator here
    
    // Awake() runs BEFORE Start() - ensures components exist before other scripts need them
    void Awake()
    {
        // Ensure Player tag is set
        gameObject.tag = "Player";
        
        // CharacterController
        if (GetComponent<CharacterController>() == null)
        {
            CharacterController cc = gameObject.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 0.9f, 0);
        }
        
        // Core components
        if (GetComponent<EnhancedPlayerController>() == null) gameObject.AddComponent<EnhancedPlayerController>();
        if (GetComponent<Health>() == null) gameObject.AddComponent<Health>();
        if (GetComponent<CombatSystem>() == null) gameObject.AddComponent<CombatSystem>();
        if (GetComponent<TargetingSystem>() == null) gameObject.AddComponent<TargetingSystem>();
        
        // Weapon system (without auto-assigning weapon)
        if (GetComponent<WeaponSystem>() == null)
        {
            gameObject.AddComponent<WeaponSystem>();
        }
        
        // Character model
        if (GetComponentInChildren<MeshRenderer>() == null && characterModelPrefab != null)
        {
            SetupCharacterModel();
        }
        
        Debug.Log("✓ Player setup complete");
    }
    
    void SetupCharacterModel()
    {
        // Remove existing model if any
        Transform existingModel = transform.Find("Model");
        if (existingModel != null)
            DestroyImmediate(existingModel.gameObject);
        
        if (characterModelPrefab != null)
        {
            // Instantiate character model as child
            GameObject model = Instantiate(characterModelPrefab, transform);
            model.name = "Model";
            
            // Remove colliders from model
            foreach (Collider col in model.GetComponentsInChildren<Collider>())
                DestroyImmediate(col);
            
            // Check for materials and report status
            SkinnedMeshRenderer skinnedRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
            MeshRenderer meshRenderer = model.GetComponentInChildren<MeshRenderer>();
            
            Renderer renderer = skinnedRenderer != null ? (Renderer)skinnedRenderer : (Renderer)meshRenderer;
            
            if (renderer != null)
            {
                if (renderer.sharedMaterial == null)
                {
                    Debug.LogError("❌ Character model has NO material assigned!");
                    Debug.LogError("FIX: In Unity, select the model file → Inspector → Materials tab → Extract Materials");
                }
                else if (renderer.sharedMaterial.mainTexture == null)
                {
                    Debug.LogWarning("⚠ Material exists but NO TEXTURE assigned!");
                    Debug.LogWarning($"Material: {renderer.sharedMaterial.name}");
                    Debug.LogWarning("FIX: Select the material → Drag texture to Albedo slot");
                }
                else
                {
                    Debug.Log($"✓ Material OK: {renderer.sharedMaterial.name}, Texture: {renderer.sharedMaterial.mainTexture.name}");
                }
            }
            
            // Setup animator ON THE MODEL (not Player parent)
            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.AddComponent<Animator>();
            
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log($"✓ Animator controller assigned: {animatorController.name}");
            }
            else
            {
                Debug.LogError("❌ NO Animator Controller assigned! Drag PlayerAnimator to SetupPlayer component!");
            }
            
            animator.applyRootMotion = false;
            
            Debug.Log("✓ Character model loaded");
        }
    }
}
