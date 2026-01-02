using UnityEngine;

/// <summary>
/// Pillow Cannon - skyter puter mot spilleren
/// </summary>
public class PillowCannon : MonoBehaviour
{
    [Header("References")]
    public GameObject pillowPrefab; // Pute-prefab som skytes
    public Transform firePoint; // Hvor puten spawner (ende av kanon)
    
    [Header("Shooting Settings")]
    public float shootForce = 15f; // Hvor fort puten skytes
    public float fireRate = 2f; // Skudd per sekund
    public bool autoFire = true; // Skyt automatisk
    
    [Header("Targeting")]
    public bool aimAtPlayer = true; // Sikt mot spilleren
    public Transform target; // Målpunkt (player)
    public float randomSpread = 5f; // Tilfeldighet i sikting (grader)
    
    [Header("Visual")]
    public GameObject muzzleFlash; // Effekt når kanon skyter (optional)
    public AudioClip fireSound; // Lyd når kanon skyter (optional)
    
    private float nextFireTime;
    private AudioSource audioSource;
    
    void Start()
    {
        // Auto-finn spilleren hvis ikke satt
        if (target == null && aimAtPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"PillowCannon: Found player target");
            }
        }
        
        // Opprett firePoint hvis ikke satt
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.forward * 1f;
            firePoint = fp.transform;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && fireSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        nextFireTime = Time.time + 1f / fireRate;
    }
    
    void Update()
    {
        if (autoFire && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }
        
        // Roter mot target hvis aim er på
        if (aimAtPlayer && target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0; // Hold horisontalt (ikke sikt opp/ned)
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
    }
    
    public void Fire()
    {
        if (pillowPrefab == null)
        {
            Debug.LogError("PillowCannon: No pillow prefab assigned!");
            return;
        }
        
        // Spawn pillow
        GameObject pillow = Instantiate(pillowPrefab, firePoint.position, firePoint.rotation);
        
        // Calculate direction with random spread
        Vector3 direction = firePoint.forward;
        if (randomSpread > 0)
        {
            direction = Quaternion.Euler(
                Random.Range(-randomSpread, randomSpread),
                Random.Range(-randomSpread, randomSpread),
                0
            ) * direction;
        }
        
        // Add force to pillow
        Rigidbody rb = pillow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * shootForce;
        }
        
        // Muzzle flash effect
        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
            Destroy(flash, 1f);
        }
        
        // Play sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        Debug.Log($"PillowCannon fired! Direction: {direction}, Force: {shootForce}");
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (firePoint == null)
            return;
        
        // Vis skuddretning
        Gizmos.color = Color.red;
        Gizmos.DrawRay(firePoint.position, firePoint.forward * 5f);
        
        // Vis spread cone
        if (randomSpread > 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 spreadDirection1 = Quaternion.Euler(randomSpread, randomSpread, 0) * firePoint.forward;
            Vector3 spreadDirection2 = Quaternion.Euler(-randomSpread, -randomSpread, 0) * firePoint.forward;
            Gizmos.DrawRay(firePoint.position, spreadDirection1 * 5f);
            Gizmos.DrawRay(firePoint.position, spreadDirection2 * 5f);
        }
    }
}

