using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Main UI manager - health bar, pause menu, etc
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;
    private Image healthBarFill;
    private Health playerHealth;
    private GameObject pauseMenuPanel;
    private bool isPaused = false;
    
    void Start()
    {
        CreateUI();
        Debug.Log("UIManager: UI created");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
        
        // Try to find player health if we don't have it yet
        if (playerHealth == null)
        {
            FindPlayerHealth();
        }
        
        UpdateHealthBar();
    }
    
    void CreateUI()
    {
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }
        
        // Create small cylindrical health bar
        GameObject healthContainer = new GameObject("HealthContainer");
        healthContainer.transform.parent = mainCanvas.transform;
        
        RectTransform containerRect = healthContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.pivot = new Vector2(0, 0);
        containerRect.anchoredPosition = new Vector2(30, 30);
        containerRect.sizeDelta = new Vector2(120, 10);
        
        // Outer capsule background
        GameObject healthBgObj = new GameObject("HealthBarBg");
        healthBgObj.transform.parent = healthContainer.transform;
        Image healthBgImage = healthBgObj.AddComponent<Image>();
        healthBgImage.sprite = CreateRoundedSprite();
        healthBgImage.type = Image.Type.Sliced;
        healthBgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        RectTransform healthBgRect = healthBgObj.GetComponent<RectTransform>();
        healthBgRect.anchorMin = Vector2.zero;
        healthBgRect.anchorMax = Vector2.one;
        healthBgRect.offsetMin = Vector2.zero;
        healthBgRect.offsetMax = Vector2.zero;
        
        // Health bar fill with rounded sprite
        GameObject healthFillObj = new GameObject("HealthBarFill");
        healthFillObj.transform.parent = healthBgObj.transform;
        healthBarFill = healthFillObj.AddComponent<Image>();
        healthBarFill.sprite = CreateRoundedSprite();
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarFill.fillAmount = 1f;
        healthBarFill.color = new Color(0.9f, 0.15f, 0.15f, 1f);
        
        RectTransform healthFillRect = healthFillObj.GetComponent<RectTransform>();
        healthFillRect.anchorMin = new Vector2(0, 0);
        healthFillRect.anchorMax = new Vector2(1, 1);
        healthFillRect.offsetMin = new Vector2(1, 1);
        healthFillRect.offsetMax = new Vector2(-1, -1);
        
        // Create pause menu
        CreatePauseMenu();
    }
    
    void CreatePauseMenu()
    {
        pauseMenuPanel = new GameObject("PauseMenuPanel");
        pauseMenuPanel.transform.parent = mainCanvas.transform;
        
        Image panelImage = pauseMenuPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = pauseMenuPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Pause title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.parent = pauseMenuPanel.transform;
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyle.Bold;
        titleText.text = "PAUSED";
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 100);
        titleRect.sizeDelta = new Vector2(400, 100);
        
        // Resume button
        GameObject resumeObj = CreateButton("Resume Button", pauseMenuPanel.transform, new Vector2(0, 0), "Resume");
        Button resumeBtn = resumeObj.GetComponent<Button>();
        resumeBtn.onClick.AddListener(TogglePauseMenu);
        
        // Quit button
        GameObject quitObj = CreateButton("Quit Button", pauseMenuPanel.transform, new Vector2(0, -80), "Quit");
        Button quitBtn = quitObj.GetComponent<Button>();
        quitBtn.onClick.AddListener(QuitGame);
        
        pauseMenuPanel.SetActive(false);
    }
    
    GameObject CreateButton(string name, Transform parent, Vector2 position, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.parent = parent;
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.5f, 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(200, 60);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.parent = buttonObj.transform;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 28;
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
    
    void FindPlayerHealth()
    {
        playerHealth = FindObjectOfType<Health>();
        if (playerHealth != null)
        {
            Debug.Log("UIManager: Found player health component");
        }
        else
        {
            Debug.LogWarning("UIManager: Could not find Health component yet");
        }
    }
    
    void UpdateHealthBar()
    {
        if (playerHealth != null && healthBarFill != null)
        {
            float healthPercent = playerHealth.GetHealthPercent();
            healthBarFill.fillAmount = healthPercent;
        }
        else if (healthBarFill == null)
        {
            Debug.LogError("UIManager: healthBarFill is null!");
        }
    }
    
    Sprite CreateRoundedSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = (x - 16f) / 16f;
                float dy = (y - 16f) / 16f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                pixels[y * 32 + x] = distance < 1f ? Color.white : Color.clear;
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(15, 15, 15, 15));
    }
    
    void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }
    
    void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
