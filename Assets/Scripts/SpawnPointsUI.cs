using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsUI : MonoBehaviour
{

    [Header("Circle settings")] 
    [SerializeField] private float radius = 15f;
    [SerializeField] private int nrOfspawnPoints = 10;
    [SerializeField] private float heightOffset = 2f;

    [Header("Random variation")] 
    [SerializeField] private float randomRadiusVariation = 2f;
    [SerializeField] private float randomHeightVariation = 1f;

    private List<Vector3> spawnPositions = new List<Vector3>();

    private void Awake()
    {
        GenerateSpawnpoints();
    }

    void GenerateSpawnpoints()
    {
        spawnPositions.Clear();

        float angleStep = 360f / nrOfspawnPoints;

        for (int i = 0; i < nrOfspawnPoints; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            float y = heightOffset;

            x += Random.Range(-randomRadiusVariation, randomRadiusVariation);
            z += Random.Range(-randomRadiusVariation, randomRadiusVariation);
            y += Random.Range(0, randomHeightVariation);

            Vector3 position = transform.position + new Vector3(x, y, z);
            spawnPositions.Add(position);
        }
        
        Debug.Log($"CircleSpawnPoints: Generated {spawnPositions.Count} spawn points");
    }

    public Vector3 GetRandomSpawnPoint()
    {
        if (spawnPositions.Count == 0)
        {
            Debug.LogWarning("CircleSpawnPoints: No spawn points generated");
            return transform.position;
        }

        return spawnPositions[Random.Range(0, spawnPositions.Count)];
    }

    public Vector3 GetSpawnPoint(int index)
    {
        if (index < 0 || index >= spawnPositions.Count)
        {
            Debug.LogWarning($"CircleSpawnPoints: Invalid spawn point index {index}");
            return transform.position;
        }

        return spawnPositions[index];
    }

    public int GetSpawnPointCount()
    {
        return spawnPositions.Count;
    }
    
    private void OnDrawGizmos()
    {
        // Draw circle preview in editor
        Gizmos.color = Color.yellow;
        
        int previewPoints = nrOfspawnPoints;
        float angleStep = 360f / previewPoints;

        for (int i = 0; i < previewPoints; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 point1 = transform.position + new Vector3(
                Mathf.Cos(angle1) * radius,
                heightOffset,
                Mathf.Sin(angle1) * radius
            );

            Vector3 point2 = transform.position + new Vector3(
                Mathf.Cos(angle2) * radius,
                heightOffset,
                Mathf.Sin(angle2) * radius
            );

            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawWireSphere(point1, 0.5f);
        }

        // Draw generated spawn points in Play mode
        if (Application.isPlaying && spawnPositions.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (var pos in spawnPositions)
            {
                Gizmos.DrawWireSphere(pos, 1f);
            }
        }
    }
}
