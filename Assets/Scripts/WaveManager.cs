using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    [Header("References")] 
    [SerializeField] private GameObject cannonPrefab;
    [SerializeField] private SpawnPointsUI spawnPoints;
    [SerializeField] private Transform cannonParent;

    [Header("Wave settings")] 
    [SerializeField] private int startingCannons = 1;
    [SerializeField] private int cannonsPerWave = 1;
    [SerializeField] private float timeBetweenWaves = 15f;

    [Header("Spawn Settings")]
    public float spawnRadius = 10f; 
    public float minHeight = 0f; 
    public float maxHeight = 5f; 
    
    private int currentWave = 0;
    private int totalCannonsSpawned = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cannonPrefab == null)
        {
            Debug.LogError("WaveManager: Cannon Prefab not assigned");
            return;
        }
        
        if (spawnPoints == null)
        {
            Debug.LogError("WaveManager: Spawn Points not assigned");
            return;
        }

        // Auto-create parent if not assigned
        if (cannonParent == null)
        {
            GameObject parent = new GameObject("Cannons");
            cannonParent = parent.transform;
            Debug.Log("WaveManager: Auto-created Cannons parent");
        }

        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(2f);
        StartNewWave();

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartNewWave();
        }
    }

    private void StartNewWave()
    {
        currentWave++;

        int cannonsToSpawn = (currentWave == 1) ? startingCannons : cannonsPerWave;

        Debug.Log($"WaveManager: Starting new wave {currentWave} with {cannonsToSpawn} cannons");

        // Spawn rundt hver spawn point
        for (int i = 0; i < cannonsToSpawn; i++)
        {
            // Velg tilfeldig spawn point (returnerer Vector3)
            Vector3 randomSpawnPoint = spawnPoints.GetRandomSpawnPoint();
            SpawnCannon(randomSpawnPoint);
        }

        Debug.Log($"WaveManager: Wave {currentWave} done. Spawned {cannonsToSpawn} cannons");
    }

   private void SpawnCannon(Vector3 spawnPointPos)
   {
       if (cannonPrefab == null)
           return;

       // Spawn PÅ OMKRETSEN (alltid på kanten av sirkelen)
       float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
       float x = Mathf.Cos(angle) * spawnRadius;
       float z = Mathf.Sin(angle) * spawnRadius;

       Vector3 spawnPos = spawnPointPos + new Vector3(x, 0, z);

       // Random høyde OVER omkretsen
       float randomHeight = Random.Range(minHeight, maxHeight);
       spawnPos.y = spawnPointPos.y + randomHeight;

       // Spawn kanon
       GameObject cannon = Instantiate(cannonPrefab, spawnPos, Quaternion.identity, cannonParent);
       totalCannonsSpawned++;

       Debug.Log($"WaveManager: Spawned cannon at {spawnPos} (height: {randomHeight:F1})");
   }
}
