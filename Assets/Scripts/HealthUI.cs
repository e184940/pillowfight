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
    
    void Start()
    {
        // Auto-find PlayerHealth hvis ikke satt
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
        
        // Validate references
        if (healthSlider == null)
        {
            Debug.LogWarning("HealthUI: Health Slider not assigned! Drag HealthBar slider to this field.");
        }
        
        if (fillImage == null)
        {
            Debug.LogWarning("HealthUI: Fill Image not assigned! Drag Fill image to this field.");
        }
        
        // Subscribe to health change event
        playerHealth.OnHealthChanged += UpdateHealthUI;
        playerHealth.OnDeath += OnPlayerDeath;
        
        Debug.Log("HealthUI: Subscribed to PlayerHealth events");
        
        // Initial update
        UpdateHealthUI(playerHealth.currentHealth, playerHealth.maxHealth);
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

