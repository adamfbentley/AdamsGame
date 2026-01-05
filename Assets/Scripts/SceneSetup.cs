using UnityEngine;

/// <summary>
/// Auto-setup system - configures entire game on play
/// Just add this to an empty GameObject and press Play
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Character Assets (Optional)")]
    [SerializeField] private GameObject characterModelPrefab;
    [SerializeField] private RuntimeAnimatorController animatorController;
    
    void Awake()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    public void SetupScene()
    {
        Debug.Log("========== AUTO SCENE SETUP ==========");
        
        // Create ground
        CreateGround();
        
        // Create/configure player
        SetupPlayer();
        
        // Create test enemy
        CreateTestEnemy();
        
        // Ensure GameManager exists
        EnsureGameManager();
        
        Debug.Log("========== SETUP COMPLETE ==========");
        Debug.Log("Game ready! Controls: WASD=Move, Shift=Run, Space=Jump, Mouse1=Attack, ESC=Pause");
    }
    
    private void CreateGround()
    {
        if (GameObject.Find("Ground") != null)
            return;
            
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);
        
        // Setup physics
        Collider col = ground.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        ground.AddComponent<BoxCollider>();
        
        // Material
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.4f, 0.4f, 0.4f);
            renderer.material = mat;
        }
        
        Debug.Log("✓ Ground plane created");
    }
    
    private void SetupPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            player.tag = "Player"; // Tag for enemy detection
            
            // Spawn at terrain height if terrain exists
            Vector3 spawnPos = new Vector3(0, 1, 0);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                float terrainHeight = terrain.SampleHeight(spawnPos) + terrain.transform.position.y;
                spawnPos.y = terrainHeight + 1f; // 1 unit above terrain
            }
            
            player.transform.position = spawnPos;
        }
        
        // CharacterController
        if (player.GetComponent<CharacterController>() == null)
        {
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 0.9f, 0);
        }
        
        // Core components
        if (player.GetComponent<EnhancedPlayerController>() == null) player.AddComponent<EnhancedPlayerController>();
        if (player.GetComponent<Health>() == null) player.AddComponent<Health>();
        if (player.GetComponent<CombatSystem>() == null) player.AddComponent<CombatSystem>();
        if (player.GetComponent<TargetingSystem>() == null) player.AddComponent<TargetingSystem>();
        
        // Weapon system
        if (player.GetComponent<WeaponSystem>() == null)
        {
            WeaponSystem weaponSystem = player.AddComponent<WeaponSystem>();
            // Load the axe prefab and assign it
            GameObject axePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Blink/Art/Weapons/LowPoly/FreeRPGWeapons/_PREFABS/Axe1H_Basic.prefab");
            if (axePrefab != null)
            {
                // Use reflection to set the private field since it's serialized
                System.Reflection.FieldInfo weaponField = typeof(WeaponSystem).GetField("weaponPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (weaponField != null)
                {
                    weaponField.SetValue(weaponSystem, axePrefab);
                    Debug.Log("✓ Axe1H_Basic assigned to WeaponSystem");
                }
            }
        }
        
        // Third-Person Camera - Independent object, not parented
        GameObject cameraObj = GameObject.Find("Main Camera");
        if (cameraObj == null)
        {
            cameraObj = new GameObject("Main Camera");
            cameraObj.transform.position = new Vector3(0, 1.2f, -3f);
            
            Camera cam = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cam.tag = "MainCamera";
            
            cameraObj.AddComponent<ThirdPersonCamera>();
            Debug.Log("✓ Camera created (independent)");
        }
        else
        {
            // Camera exists - make sure it has ThirdPersonCamera component
            if (cameraObj.GetComponent<ThirdPersonCamera>() == null)
            {
                cameraObj.AddComponent<ThirdPersonCamera>();
                Debug.Log("✓ Camera found, ThirdPersonCamera added");
            }
        }
        
        // Character model
        if (player.GetComponentInChildren<MeshRenderer>() == null || characterModelPrefab != null)
        {
            SetupCharacterModel(player);
        }
        
        Debug.Log("✓ Player configured with all components");
    }
    
    private void SetupCharacterModel(GameObject player)
    {
        // Remove placeholder if exists
        Transform existingModel = player.transform.Find("Model");
        if (existingModel != null)
            DestroyImmediate(existingModel.gameObject);
        
        if (characterModelPrefab != null)
        {
            // Use assigned model
            GameObject model = Instantiate(characterModelPrefab, player.transform);
            model.name = "Model";
            
            // Remove colliders
            foreach (Collider col in model.GetComponentsInChildren<Collider>())
                DestroyImmediate(col);
            
            // Setup animator on model
            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.AddComponent<Animator>();
            
            if (animatorController != null)
                animator.runtimeAnimatorController = animatorController;
            
            animator.applyRootMotion = false;
            Debug.Log("✓ Character model loaded");
        }
        else
        {
            // Create placeholder
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Model";
            capsule.transform.parent = player.transform;
            capsule.transform.localPosition = Vector3.zero;
            capsule.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            
            Collider col = capsule.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            
            Renderer renderer = capsule.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.8f, 0.2f, 0.2f);
                renderer.material = mat;
            }
            
            // Add animator to capsule
            Animator animator = capsule.AddComponent<Animator>();
            if (animatorController != null)
                animator.runtimeAnimatorController = animatorController;
            
            animator.applyRootMotion = false;
            
            Debug.Log("✓ Placeholder character created (assign model in inspector)");
        }
    }
    
    private void CreateNPCs()
    {
        // Disabled until enemy system is implemented
        // CreateNPC("NPC_Guard", new Vector3(5, 1, 0), "Guard");
        // CreateNPC("NPC_Merchant", new Vector3(-5, 1, 0), "Merchant");
    }
    
    /* Commented out - will be replaced with proper enemy AI system
    private void CreateNPC(string name, Vector3 position, string displayName)
    {
        if (GameObject.Find(name) != null)
            return;
        
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = name;
        npc.transform.position = position;
        npc.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        
        // Remove collider
        Collider col = npc.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        
        // Add components
        CharacterController cc = npc.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        npc.AddComponent<NPC>();
        npc.AddComponent<Health>();
        
        // Material
        Renderer renderer = npc.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.8f, 0.2f);
            renderer.material = mat;
        }
        
        Debug.Log($"✓ NPC created: {displayName}");
    }
    */
    
    private void CreateTestEnemy()
    {
        // Check if already exists
        if (GameObject.Find("TestEnemy") != null)
        {
            Debug.Log("Test enemy already exists");
            return;
        }
        
        // Create a simple test enemy
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "TestEnemy";
        // enemy.tag = "Enemy"; // Tag not set up yet - will add later
        enemy.layer = LayerMask.NameToLayer("Default");
        enemy.transform.position = new Vector3(5, 1, 0); // y=1 for proper ground placement
        enemy.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        
        // Remove default collider
        Collider col = enemy.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);
        
        // Add CharacterController - match capsule dimensions
        CharacterController cc = enemy.AddComponent<CharacterController>();
        cc.height = 2f; // Match capsule height
        cc.radius = 0.25f; // Match scaled radius (0.5 * 0.5)
        cc.center = new Vector3(0, 0, 0); // Center at transform origin
        
        // Add enemy components
        enemy.AddComponent<EnemyAI>();
        enemy.AddComponent<Health>();
        
        // Red material so we can see it
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.8f, 0.2f, 0.2f); // Red
            renderer.material = mat;
        }
        
        Debug.Log("✓ Test enemy created at (5, 1, 0)");
    }
    
    private void EnsureGameManager()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            Debug.Log("GameManager already exists");
            return;
        }
        
        gm = new GameObject("GameManager");
        gm.AddComponent<UIManager>();
        Debug.Log("✓ GameManager and UI initialized");
    }
}
