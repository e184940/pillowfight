using UnityEngine;

/// <summary>
/// Pillow projectile - skytes fra kanoner
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Pillow : MonoBehaviour
{
    [Header("Pillow Settings")]
    public float lifetime = 10f; // Hvor lenge puten eksisterer før den forsvinner (økt fra 5)
    public float pushForce = 10f; // Hvor hardt puten dytter spilleren
    public int damage = 10; // Hvor mye damage puten gjør
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
         if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                pushDirection.y = 0.5f;
                
                playerRb.AddForce(pushDirection * pushForce, ForceMode.VelocityChange);
                playerRb.angularVelocity = Vector3.zero;
            }
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyNPC enemyNpc = collision.gameObject.GetComponent<EnemyNPC>();
            if (enemyNpc != null)
            {
                enemyNpc.TakeDamage(damage);
            }
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            if (destroyOnHit)
            {
                Destroy(gameObject, 0.5f);
            }
        }
    }
}

