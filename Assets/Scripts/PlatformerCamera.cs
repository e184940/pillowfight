using UnityEngine;

/// <summary>
/// Third-person platformer camera
/// Roterer rundt spilleren og følger smooth
/// </summary>
public class PlatformerCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player transform
    
    [Header("Camera Position")]
    public float distance = 8f; // Avstand fra spiller
    public float height = 3f; // Høyde over spiller
    public float smoothSpeed = 0.125f; // Hvor smooth kamera følger (0.01-1)
    
    [Header("Rotation")]
    public bool rotateWithPlayer = true; // Roter rundt spilleren
    public float rotationSpeed = 5f; // Hvor raskt kamera roterer
    public Vector3 lookOffset = new Vector3(0, 1, 0); // Hvor på spilleren (bryst-høyde)
    
    [Header("Collision Avoidance")]
    public bool avoidObstacles = true;
    public LayerMask obstacleLayer;
    public float minDistance = 2f; // Nærmeste kamera kan komme
    public float collisionSmoothing = 0.3f; // Smooth zoom ved collision (høyere = glattere)
    public float collisionBuffer = 0.8f; // Buffer mellom kamera og vegg
    public float zoomDeadZone = 0.5f; // Ignorer små zoom-endringer
    
    private float currentDistance;
    private float currentHeight;
    private Vector3 currentVelocity;
    private float distanceVelocity;
    private float targetDistance;
    
    void Start()
    {
        // Auto-finn Player hvis ikke satt
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("PlatformerCamera: Found Player target");
            }
            else
            {
                Debug.LogError("PlatformerCamera: No target! Tag Player with 'Player' tag or assign manually.");
            }
        }
        
        // Sett obstacleLayer til Default + Ground hvis ikke satt
        if (obstacleLayer == 0)
        {
            obstacleLayer = LayerMask.GetMask("Default", "Ground");
        }
        
        // Initialiser current values
        currentDistance = distance;
        currentHeight = height;
        targetDistance = distance;
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Beregn rotasjon basert på spillerens forward direction
        Quaternion rotation;
        if (rotateWithPlayer)
        {
            // Roter rundt spilleren basert på hvor spilleren ser
            rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
        }
        else
        {
            // Hold fast rotasjon
            rotation = Quaternion.Euler(0, 0, 0);
        }
        
        // Beregn ønsket posisjon (bak spilleren basert på rotasjon)
        Vector3 direction = rotation * new Vector3(0, 0, -1); // Bak spilleren
        Vector3 desiredPosition = target.position + direction * distance + Vector3.up * height;
        
        // Default: bruk full distance
        float newTargetDistance = distance;
        
        // Collision detection - zoom inn hvis vegg er i veien
        if (avoidObstacles)
        {
            Vector3 rayDirection = desiredPosition - target.position;
            float checkDistance = rayDirection.magnitude;
            
            RaycastHit hit;
            if (Physics.SphereCast(target.position + lookOffset, 0.3f, rayDirection.normalized, out hit, checkDistance, obstacleLayer))
            {
                // Vegg i veien - reduser avstand med buffer
                newTargetDistance = Mathf.Max(hit.distance - collisionBuffer, minDistance);
            }
        }
        
        // Dead zone - ignorer små endringer for å unngå "breathing"
        if (Mathf.Abs(newTargetDistance - targetDistance) > zoomDeadZone)
        {
            targetDistance = newTargetDistance;
        }
        
        // Smooth zoom ved collision
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, collisionSmoothing);
        
        // Oppdater posisjon med smooth distance
        direction = rotation * new Vector3(0, 0, -1);
        desiredPosition = target.position + direction * currentDistance + Vector3.up * height;
        
        // Smooth follow position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);
        transform.position = smoothedPosition;
        
        // Look at target
        Vector3 lookAtPoint = target.position + lookOffset;
        transform.LookAt(lookAtPoint);
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (target == null)
            return;
        
        // Beregn hvor kameraet vil være
        Quaternion rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
        Vector3 direction = rotation * new Vector3(0, 0, -1);
        Vector3 desiredPos = target.position + direction * distance + Vector3.up * height;
        
        // Vis ønsket kamera-posisjon
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(desiredPos, 0.3f);
        
        // Vis linje fra target til kamera
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(target.position, desiredPos);
        
        // Vis look-at punkt
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position + lookOffset, 0.2f);
        
        // Vis collision raycast
        if (avoidObstacles)
        {
            Gizmos.color = Color.red;
            Vector3 rayDir = desiredPos - target.position;
            Gizmos.DrawRay(target.position, rayDir.normalized * distance);
        }
    }
}

