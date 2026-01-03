using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enkel Health Bar UI
/// Viser spillerens health som en bar
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Slider healthSlider; // Unity UI Slider
    public Text healthText; // Unity UI Text (optional)
    
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
        }
        
        if (playerHealth == null)
        {
            Debug.LogError("HealthUI: No PlayerHealth found!");
            return;
        }
        
        // Subscribe to health change event
        playerHealth.OnHealthChanged += UpdateHealthUI;
        playerHealth.OnDeath += OnPlayerDeath;
        
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
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
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
        }
    }
    
    void OnPlayerDeath()
    {
        Debug.Log("HealthUI: Player died - show game over screen");
        // TODO: Show Game Over UI
    }
}

