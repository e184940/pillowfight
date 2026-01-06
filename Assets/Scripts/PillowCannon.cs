using UnityEngine;

public class PillowCannon : MonoBehaviour
{
    [Header("References")]
    public GameObject pillowPrefab;
    public Transform firePoint;
    
    [Header("Shooting Settings")]
    public float shootForce = 25f;
    public float fireRate = 0.8f;
    public bool autoFire = true;
    
    [Header("Targeting")]
    public bool aimAtPlayer = true;
    public Transform target;
    public float randomSpread = 2f;
    public float aimSpeed = 5f;
    public float maxPitchAngle = 45f;
    
    [Header("Visual")]
    public GameObject muzzleFlash;
    public AudioClip fireSound;
    
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
            
            if (direction != Vector3.zero)
            {
                // For standard Cylinder (Y-akse er forward):
                // Vi må bruke Quaternion som peker Y-aksen mot målet
                
                // Beregn pitch for å begrense
                float horizontalDistance = new Vector3(direction.x, 0, direction.z).magnitude;
                float pitchAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;
                
                // Begrens pitch
                pitchAngle = Mathf.Clamp(pitchAngle, -maxPitchAngle, maxPitchAngle);
                
                // Rekonstruer direction med begrenset pitch
                if (Mathf.Abs(pitchAngle) == maxPitchAngle)
                {
                    float pitchRad = pitchAngle * Mathf.Deg2Rad;
                    Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
                    direction = horizontalDir * Mathf.Cos(pitchRad) + Vector3.up * Mathf.Sin(pitchRad);
                }
                
                // Bruk Quaternion.LookRotation men med Y-akse som "up"
                // For Cylinder: Y-akse skal peke mot målet
                Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);
                
                // Smooth rotasjon
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * aimSpeed);
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
        
        // Calculate direction - sikt mot spilleren hvis aimAtPlayer er på
        Vector3 direction;
        if (aimAtPlayer && target != null)
        {
            // Sikt direkte mot spilleren (med spread)
            direction = (target.position - firePoint.position).normalized;
        }
        else
        {
            // Bruk kanon sin forward direction
            direction = firePoint.forward;
        }
        
        // Add random spread
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

