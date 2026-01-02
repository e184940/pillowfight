using UnityEngine;

/// <summary>
/// Pillow projectile - skytes fra kanoner
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Pillow : MonoBehaviour
{
    [Header("Pillow Settings")]
    public float lifetime = 5f; // Hvor lenge puten eksisterer før den forsvinner
    public float pushForce = 10f; // Hvor hardt puten dytter spilleren
    public bool destroyOnHit = true; // Ødelegges ved treff
    
    [Header("Effects")]
    public GameObject hitEffect; // Partikkel-effekt ved treff (optional)
    
    private Rigidbody rb;
    private float spawnTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
        
        // Sett physics properties
        rb.useGravity = true;
        rb.mass = 0.5f;
        rb.linearDamping = 0.1f;
    }
    
    void Update()
    {
        // Destroy etter lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Sjekk om vi traff spilleren
        if (collision.gameObject.CompareTag("Player"))
        {
            // Dytt spilleren
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                pushDirection.y = 0.5f; // Litt opp også
                playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                
                Debug.Log($"Pillow hit player! Push force: {pushForce}");
            }
            
            // Spawn hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy pillow
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        // Traff noe annet (vegg, platform)
        else
        {
            // Spawn hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Destroy etter et lite "bounce"
            if (destroyOnHit)
            {
                Destroy(gameObject, 0.5f); // Litt delay for bounce
            }
        }
    }
}

