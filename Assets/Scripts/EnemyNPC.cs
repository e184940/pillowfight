using UnityEngine;

public class EnemyNPC : MonoBehaviour
{
    [Header("Movement")]
    public float npcSpeed = 3f;
    public float stopDistance = 10f;
    public float rotationSpeed = 10f;

    [Header("Shooting settings")]
    public GameObject pillowPrefab;
    public Transform firePoint;
    public float shootForce = 20f;
    public float shootRange = 15f;
    public float shootInterval = 3f;
    private float nextShootTime;
    
    [Header("Health")]
    public float maxHealth = 25f;
    [HideInInspector] public float currentHealth;

    private Transform player;
    
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("EnemyNpc: Player not found!");
        }
        
        currentHealth = maxHealth;
        
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 1f, 0.5f);
            firePoint = fp.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Alltid roter mot spilleren (raskere)
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Beveg kun hvis for langt unna
        if (distanceToPlayer > stopDistance)
        {
            transform.position += direction * npcSpeed * Time.deltaTime;
        }

        if (distanceToPlayer <= shootRange && Time.time >= nextShootTime)
        {
            ShootPillow();
            nextShootTime = Time.time + shootInterval;
        }
    }

    void ShootPillow()
    {
        if (pillowPrefab == null || firePoint == null) return;

        GameObject pillow = Instantiate(pillowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = pillow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 shootDirection = (player.position - firePoint.position).normalized;
            rb.linearVelocity = shootDirection * shootForce;
        }
        
        Destroy(pillow, 5f);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }


}