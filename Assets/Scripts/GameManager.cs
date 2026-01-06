using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Difficulty levels
    public enum Difficulty { Easy, Normal, Hard }
    public Difficulty SelectedDifficulty { get; set; } = Difficulty.Normal;
    
    // Selected hat
    public int SelectedHatIndex { get; set; } = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Instance created");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Static metode for å finne eller opprette GameManager
    public static GameManager FindOrCreate()
    {
        if (Instance != null)
            return Instance;

        // Prøv å finne eksisterende GameManager
        Instance = FindFirstObjectByType<GameManager>();
        
        if (Instance != null)
        {
            DontDestroyOnLoad(Instance.gameObject);
            Debug.Log("GameManager: Found existing instance");
            return Instance;
        }

        // Opprett ny hvis den ikke finnes
        GameObject go = new GameObject("GameManager");
        Instance = go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
        Debug.Log("GameManager: Auto-created new instance");
        
        return Instance;
    }

    // Difficulty modifiers
    public float GetPlayerHealthMultiplier()
    {
        return SelectedDifficulty switch
        {
            Difficulty.Easy => 1.5f,   // 50% mer helse
            Difficulty.Normal => 1f,   // Normal
            Difficulty.Hard => 0.5f,   // 50% mindre helse
            _ => 1f
        };
    }

    public float GetEnemyDamageMultiplier()
    {
        return SelectedDifficulty switch
        {
            Difficulty.Easy => 0.7f,   // 30% mindre damage
            Difficulty.Normal => 1f,   // Normal
            Difficulty.Hard => 1.5f,   // 50% mer damage
            _ => 1f
        };
    }

    public float GetWaveSpeedMultiplier()
    {
        return SelectedDifficulty switch
        {
            Difficulty.Easy => 0.7f,   // 30% tregere waves
            Difficulty.Normal => 1f,   // Normal
            Difficulty.Hard => 1.3f,   // 30% raskere waves
            _ => 1f
        };
    }
}

