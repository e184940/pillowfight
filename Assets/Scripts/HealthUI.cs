using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro support

/// <summary>
/// Enkel Health Bar UI
/// Viser spillerens health som en bar
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Slider healthSlider; // Unity UI Slider
    public TMP_Text healthText; // TextMeshPro Text (optional)
    
    [Header("Colors")]
    public Image fillImage; // Health bar fill image
    public Color healthyColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f; // 30% health = red
    
    void Awake()
    {
        Debug.Log("HealthUI: Awake called - initializing...");
        
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"HealthUI: Auto-found PlayerHealth on {playerHealth.gameObject.name}");
            }
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("HealthUI: No PlayerHealth found! Health bar will not work.");
            return;
        }
        
        if (healthSlider == null)
        {
            Debug.LogError("HealthUI: Health Slider NOT assigned! Assign it in Inspector!");
            return;
        }
        else
        {
            Debug.Log($"HealthUI: Health Slider found: {healthSlider.name}");
        }
        
        if (fillImage == null)
        {
            Debug.LogWarning("HealthUI: Fill Image not assigned - trying to find it...");
            Transform fillTransform = healthSlider.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                fillImage = fillTransform.GetComponent<Image>();
                Debug.Log("HealthUI: Auto-found Fill Image");
            }
        }
        
        playerHealth.OnHealthChanged += UpdateHealthUI;
        playerHealth.OnDeath += OnPlayerDeath;
        
        Debug.Log("HealthUI: Setup complete and subscribed to events");
    }
    
    void Start()
    {
        // Initial update (wait for PlayerHealth.Start() to set currentHealth)
        if (playerHealth != null)
        {
            UpdateHealthUI(playerHealth.currentHealth, playerHealth.maxHealth);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }
    
    void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        Debug.Log($"HealthUI: Updating UI - Health: {currentHealth}/{maxHealth}");
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            Debug.Log($"HealthUI: Slider updated to {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.LogWarning("HealthUI: Cannot update slider - healthSlider is null!");
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
        
        // Update color based on health percentage
        if (fillImage != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            fillImage.color = healthPercentage <= lowHealthThreshold ? lowHealthColor : healthyColor;
            Debug.Log($"HealthUI: Fill color updated - Health %: {healthPercentage:P0}");
        }
        else
        {
            Debug.LogWarning("HealthUI: Cannot update fill color - fillImage is null!");
        }
    }
    
    void OnPlayerDeath()
    {
        Debug.Log("HealthUI: Player died - show game over screen");
        // TODO: Show Game Over UI
    }
}

