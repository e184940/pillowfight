using UnityEngine;
using UnityEngine.UI;

public class NPCHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public EnemyNpc enemyNpc;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public bool rotateToCamera = true;
    
    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (enemyNpc == null)
        {
            enemyNpc = GetComponentInParent<EnemyNpc>();
        }
        
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
        
        if (healthSlider != null && enemyNpc != null)
        {
            healthSlider.maxValue = enemyNpc.maxHealth;
            healthSlider.value = enemyNpc.maxHealth;
        }
    }

    void LateUpdate()
    {
        if (enemyNpc == null) return;
        
        transform.position = enemyNpc.transform.position + offset;
        
        if (rotateToCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
        
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        if (healthSlider == null || enemyNpc == null) return;
        
        healthSlider.value = enemyNpc.currentHealth;
        
        if (enemyNpc.currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    private float GetCurrentHealth()
    {
        return enemyNpc.currentHealth;
    }
}

