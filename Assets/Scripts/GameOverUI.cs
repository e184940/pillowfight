using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro support

/// <summary>
/// Game Over Screen
/// Vises når spilleren dør
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    public GameObject gameOverPanel; // Panel som vises ved game over
    public TMP_Text gameOverText; // "GAME OVER" tekst (TextMeshPro)
    public TMP_Text scoreText; // Viser score/tid overlevd (TextMeshPro)
    public UnityEngine.UI.Button restartButton;
    public UnityEngine.UI.Button quitButton;
    
    [Header("Settings")]
    public bool pauseGameOnDeath = true; // Pause spillet ved death
    
    private PlayerHealth playerHealth;
    private float survivalTime = 0f;
    private bool isDead = false;
    
    void Start()
    {
        // Finn PlayerHealth
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("GameOverUI: No PlayerHealth found!");
            return;
        }
        
        Debug.Log($"GameOverUI: Found PlayerHealth on {playerHealth.gameObject.name}");
        
        // Subscribe to death event
        playerHealth.OnDeath += ShowGameOver;
        Debug.Log("GameOverUI: Subscribed to OnDeath event");
        
        // Skjul game over panel ved start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("GameOverUI: Game Over Panel hidden at start");
        }
        else
        {
            Debug.LogError("GameOverUI: Game Over Panel not assigned! Drag GameOverPanel to this field in Inspector.");
        }
        
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogWarning("GameOverUI: Restart Button not assigned");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        Debug.Log("GameOverUI: Ready");
    }
    
    void Update()
    {
        // Tell overlevelsestid (bruker unscaledDeltaTime for å fungere selv om spillet pauses)
        if (!isDead)
        {
            survivalTime += Time.unscaledDeltaTime;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= ShowGameOver;
        }
    }
    
    void ShowGameOver()
    {
        isDead = true;
        
        Debug.Log($"GameOverUI: Showing game over screen. Survived: {survivalTime:F1}s");
        
        // Vis game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Oppdater score text
        if (scoreText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            string timeText = $"You survived: {minutes:00}:{seconds:00}";
            scoreText.text = timeText;
            Debug.Log($"GameOverUI: Score text set to '{timeText}'");
        }
        else
        {
            Debug.LogWarning("GameOverUI: Score Text is null! Cannot update survival time.");
        }
        
        // Pause spillet
        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
            Debug.Log("GameOverUI: Game paused");
        }
        
        // Vis cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void RestartGame()
    {
        Debug.Log("GameOverUI: Restarting game...");
        
        // Unpause
        Time.timeScale = 1f;
        
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Debug.Log("GameOverUI: Quitting game...");
        
        // Unpause først (hvis vi går til main menu)
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

