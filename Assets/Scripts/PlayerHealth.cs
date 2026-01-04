using UnityEngine;
using System;

/// <summary>
/// Player Health System
/// Håndterer damage, death og health UI
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Invincibility")]
    public float invincibilityDuration = 1f; // Sekunder etter treff hvor spilleren er immun
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;
    
    [Header("Visual Feedback")]
    public Renderer playerRenderer; // For blink effect (auto-finnes hvis ikke satt)
    public float blinkInterval = 0.1f; // Hvor raskt spilleren blinker ved invincibility
    private float blinkTimer = 0f;
    private bool isVisible = true;
    
    // Events
    public event Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Auto-finn renderer hvis ikke satt
        if (playerRenderer == null)
        {
            // Søk i children (Character model)
            playerRenderer = GetComponentInChildren<Renderer>();
            
            if (playerRenderer == null)
            {
                // Søk på denne GameObjectet selv
                playerRenderer = GetComponent<Renderer>();
            }
            
            if (playerRenderer != null)
            {
                Debug.Log($"PlayerHealth: Auto-found renderer on {playerRenderer.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("PlayerHealth: No renderer found - blink effect disabled");
            }
        }
        
        Debug.Log($"PlayerHealth initialized: {currentHealth}/{maxHealth} HP");
    }
    
    void Update()
    {
        // Håndter invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                SetVisible(true); // Sørg for at spilleren er synlig
            }
            else
            {
                // Blink effect
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0;
                    isVisible = !isVisible;
                    SetVisible(isVisible);
                }
            }
        }
    }
    
    /// <summary>
    /// Ta damage fra pute eller annen kilde
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            // Debug.Log("Player is invincible - damage ignored");
            return;
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Ikke gå under 0
        
        // Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        // Trigger event for UI update
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Start invincibility
        if (currentHealth > 0)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
        
        // Sjekk om spilleren er død
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Heal spilleren
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Ikke gå over max
        
        Debug.Log($"Player healed {amount} HP! Health: {currentHealth}/{maxHealth}");
        
        // Trigger event for UI update
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Spilleren dør
    /// </summary>
    void Die()
    {
        Debug.Log("Player died!");
        
        // Trigger death event
        OnDeath?.Invoke();
        
        // Disable player controls
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // TODO: Game Over screen, restart option, etc.
    }
    
    /// <summary>
    /// Sett synlighet for blink effect
    /// </summary>
    void SetVisible(bool visible)
    {
        if (playerRenderer != null)
        {
            playerRenderer.enabled = visible;
        }
    }
    
    /// <summary>
    /// Reset health til full (for restart)
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        invincibilityTimer = 0;
        SetVisible(true);
        
        // Enable player controls
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("Player health reset");
    }
}

