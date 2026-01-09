using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    [Header("References")] 
    [SerializeField] private GameObject cannonPrefab;
    [SerializeField] private SpawnPointsUI spawnPoints;
    [SerializeField] private Transform cannonParent;
    [SerializeField] private Transform npcParent; // separate parent for NPCs to avoid parenting under UI/Canvas

    [Header("Wave settings")] 
    [SerializeField] private int startingCannons = 1;
    [SerializeField] private int cannonsPerWave = 1;
    [SerializeField] private float timeBetweenWaves = 30f;

    [Header("NPC settings")]
    public GameObject npcPrefab;
    public int npcSpawnWave = 5;
    public int npcsPerWave = 1;

    [Header("Spawn Settings")]
    public float spawnRadius = 10f; 
    public float minHeight = 0f; 
    public float maxHeight = 5f; 
    
    private int currentWave = 0;
    private List<GameObject> activeCannons = new List<GameObject>();
    private List<GameObject> activeNPCs = new List<GameObject>();
    
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

        // Auto-create NPC parent if not assigned (place at root to avoid UI parenting issues)
        if (npcParent == null)
        {
            GameObject npcParentGO = new GameObject("NPCs");
            npcParent = npcParentGO.transform;
            Debug.Log("WaveManager: Auto-created NPCs parent");
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

        // ØDELEGG ALLE GAMLE KANONER OG NPCs
        DestroyOldEnemies();

        int cannonsToSpawn = startingCannons + (cannonsPerWave * (currentWave - 1));

        Debug.Log($"WaveManager: Starting wave {currentWave} with {cannonsToSpawn} cannons");

        for (int i = 0; i < cannonsToSpawn; i++)
        {
            Vector3 randomSpawnPoint = spawnPoints.GetRandomSpawnPoint();
            SpawnCannon(randomSpawnPoint);
        }

        if (currentWave >= npcSpawnWave)
        {
            int npcsToSpawn = (currentWave - npcSpawnWave + 1) * npcsPerWave;
            Debug.Log($"WaveManager: Wave {currentWave} >= {npcSpawnWave} - spawning {npcsToSpawn} NPCs");
            
            if (npcPrefab == null)
            {
                Debug.LogError("WaveManager: NPC Prefab is NULL! Assign it in Inspector!");
            }
            
            for (int i = 0; i < npcsToSpawn; i++)
            {
                SpawnNPC();
            }
        }
        else
        {
            Debug.Log($"WaveManager: Wave {currentWave} < {npcSpawnWave} - no NPCs spawning yet");
        }

        Debug.Log($"WaveManager: Wave {currentWave} complete - {activeCannons.Count} cannons, {activeNPCs.Count} NPCs");
    }
    
    private void DestroyOldEnemies()
    {
        // Fjern alle gamle kanoner
        foreach (GameObject cannon in activeCannons)
        {
            if (cannon != null)
            {
                Destroy(cannon);
            }
        }
        activeCannons.Clear();
        
        // Fjern alle gamle NPCs
        foreach (GameObject npc in activeNPCs)
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        activeNPCs.Clear();
    }

   private void SpawnCannon(Vector3 spawnPointPos)
   {
       if (cannonPrefab == null)
           return;

       float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
       float x = Mathf.Cos(angle) * spawnRadius;
       float z = Mathf.Sin(angle) * spawnRadius;

       Vector3 spawnPos = spawnPointPos + new Vector3(x, 0, z);

       float randomHeight = Random.Range(minHeight, maxHeight);
       spawnPos.y = spawnPointPos.y + randomHeight;

       GameObject cannon = Instantiate(cannonPrefab, spawnPos, Quaternion.identity, cannonParent);
       activeCannons.Add(cannon);

       // Diagnostics: check visibility issues
       LogSpawnDiagnostics(cannon, "Cannon");

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

       // Instantiate without parent first to avoid inheriting unwanted parent scale (common problem when parent is UI)
       GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
       // Ensure the instance is active and scaled properly
       if (!npc.activeSelf) npc.SetActive(true);
       npc.transform.localScale = Vector3.one;
       // Parent under dedicated npcParent (world position stays so it keeps spawnPos)
       if (npcParent != null)
           npc.transform.SetParent(npcParent, true);

       activeNPCs.Add(npc);
       
       Debug.Log($"WaveManager: Spawned NPC at {spawnPos}");

       // Diagnostics: provide details to help find invisible NPCs
       LogSpawnDiagnostics(npc, "NPC");
   }
   
   // New helper - logs renderer/bounds/scale info and attempts a safe fix for zero-scale parents
   private void LogSpawnDiagnostics(GameObject go, string tag)
   {
       if (go == null)
       {
           Debug.LogWarning($"WaveManager: {tag} instance is null after Instantiate");
           return;
       }

       Debug.Log($"WaveManager Diagnostics: {tag} active={go.activeSelf}, position={go.transform.position}, localScale={go.transform.localScale}, lossyScale={go.transform.lossyScale}");

       // If parent has zero scale anywhere, try to correct (common mistake when assigning UI object as parent)
       if (Mathf.Approximately(go.transform.lossyScale.x, 0f) || Mathf.Approximately(go.transform.lossyScale.y, 0f) || Mathf.Approximately(go.transform.lossyScale.z, 0f))
       {
           Debug.LogWarning($"WaveManager Diagnostics: Detected zero scale on spawned {tag} (lossyScale={go.transform.lossyScale}). Setting localScale=Vector3.one to force visibility. NOTE: fix prefab/parent scale in Editor.");
           go.transform.localScale = Vector3.one;
       }

       // Log renderer info
       Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
       if (rends == null || rends.Length == 0)
       {
           Debug.LogWarning($"WaveManager Diagnostics: No Renderer found on spawned {tag}. It may be invisible or use SkinnedMeshRenderer inside nested prefab.");
       }
       else
       {
           Debug.Log($"WaveManager Diagnostics: Found {rends.Length} renderers on {tag}.");
           foreach (var r in rends)
           {
               Debug.Log($" - Renderer: {r.gameObject.name}, enabled={r.enabled}, bounds.center={r.bounds.center}, bounds.extents={r.bounds.extents}, worldPos={r.transform.position}");
           }

           // If found renderers but they're far from spawn position, notify
           Vector3 avgCenter = Vector3.zero;
           foreach (var r in rends) avgCenter += r.bounds.center;
           avgCenter /= rends.Length;

           float verticalOffset = Mathf.Abs(avgCenter.y - go.transform.position.y);
           if (verticalOffset > 5f)
           {
               Debug.LogWarning($"WaveManager Diagnostics: Visual center for {tag} is {verticalOffset:F2} units away from spawn position. Open prefab and fix pivot/mesh offsets. avgCenter={avgCenter}");
           }
       }

       // Also log parent chain scales
       Transform t = go.transform;
       string chain = "";
       while (t != null)
       {
           chain += $"/{t.name}(localScale={t.localScale},lossy={t.lossyScale})";
           t = t.parent;
       }
       Debug.Log($"WaveManager Diagnostics: Transform chain: {chain}");
   }

}
