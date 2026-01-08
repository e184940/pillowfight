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
    // private float timeBetweenWaves = 15f;

    [Header("NPC settings")]
    public GameObject npcPrefab;
    public int npcSpawnWave = 5;
    public int npcsPerWave = 1;

    [Header("Spawn Settings")]
    public float spawnRadius = 10f; 
    public float minHeight = 0f; 
    public float maxHeight = 5f; 
    
    private int currentWave = 0;
    
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
            yield return new WaitForSeconds(2f);
            StartNewWave();
        }
    }

    private void StartNewWave()
    {
        currentWave++;

        int cannonsToSpawn = startingCannons + (cannonsPerWave * (currentWave - 1));

        Debug.Log($"WaveManager: Starting new wave {currentWave} with {cannonsToSpawn} cannons");

        // Spawn rundt hver spawn point
        for (int i = 0; i < cannonsToSpawn; i++)
        {
            // Velg tilfeldig spawn point (returnerer Vector3)
            Vector3 randomSpawnPoint = spawnPoints.GetRandomSpawnPoint();
            SpawnCannon(randomSpawnPoint);
        }

        if (currentWave >= npcSpawnWave)
        {
            int npcsToSpawn = (currentWave - npcSpawnWave + 1) * npcsPerWave;
            for (int i = 0; i < npcsToSpawn; i++)
            {
                SpawnNPC();
            }
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

       Debug.Log($"WaveManager: Spawned cannon at {spawnPos} (height: {randomHeight:F1})");
   }

   private void SpawnNPC()
   {
       if (npcPrefab == null)
       {
           Debug.LogWarning("WaveManager: NPC Prefab not assigned!");
           return;
       }

       Vector3 randomSpawnPoint = spawnPoints.GetRandomSpawnPoint();
       
       float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
       float x = Mathf.Cos(angle) * spawnRadius;
       float z = Mathf.Sin(angle) * spawnRadius;

       Vector3 spawnPos = randomSpawnPoint + new Vector3(x, 0, z);
       float randomHeight = Random.Range(minHeight, maxHeight);
       spawnPos.y = randomSpawnPoint.y + randomHeight;

       GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity, cannonParent);
       
       Debug.Log($"WaveManager: Spawned NPC at {spawnPos}");
   }
   
}
