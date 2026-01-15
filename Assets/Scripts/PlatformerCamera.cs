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
    public float distance = 6f; // Litt avstand for oversikt
    public float height = 4f; // Høyde for oversikt
    public float smoothSpeed = 0.2f; // Smooth follow
    
    [Header("Rotation")]
    public bool rotateWithPlayer = false; // SKAL IKKE ROTERE MED PLAYER (FIXED)
    public bool lookAtPlayer = false; // Skal kamera se på spilleren? (False = Fast rotasjon)
    public float rotationDamping = 0.5f; 
    public float lookSmooth = 10f; 
    public Vector3 lookOffset = new Vector3(0, 1, 0); 
    
    [Header("Collision Avoidance")]
    public bool avoidObstacles = true;
    public LayerMask obstacleLayer;
    public float minDistance = 2f; // Nærmeste kamera kan komme
    public float collisionSmoothing = 0.3f; // Smooth zoom ved collision (høyere = glattere)
    public float collisionBuffer = 0.8f; // Buffer mellom kamera og vegg
    public float zoomDeadZone = 0.5f; // Ignorer små zoom-endringer
    
    private float currentDistance;
    // Current height is managed by desiredPosition calculation directly
    private Vector3 currentVelocity;
    private float distanceVelocity;
    private float targetDistance;
    
    // Rotation smoothing
    private float currentRotationAngle;
    private float rotationVelocity;
    
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
        targetDistance = distance;

        if (target != null)
        {
            // Start med nåværende vinkel hvis vi ikke roterer med spiller
            if (!rotateWithPlayer)
            {
                // Beregn vinkel fra kamera til spiller ved start
                Vector3 toCam = transform.position - target.position;
                toCam.y = 0;
                if (toCam.sqrMagnitude > 0)
                {
                    currentRotationAngle = Quaternion.LookRotation(-toCam).eulerAngles.y;
                }
                else
                {
                   currentRotationAngle = 0;
                }
            }
            else
            {
                currentRotationAngle = target.eulerAngles.y;
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Beregn rotasjon
        float wantedRotationAngle = currentRotationAngle;
        
        if (rotateWithPlayer)
        {
            wantedRotationAngle = target.eulerAngles.y;
            // Smooth rotation damping only when following player rotation
            currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref rotationVelocity, rotationDamping);
        }
        else
        {
             // Keep calculated angle fixed or adjust manually if needed
             // currentRotationAngle stays constant
        }
        
        Quaternion rotation = Quaternion.Euler(0, currentRotationAngle, 0);
        
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
        
        // Dead zone
        if (Mathf.Abs(newTargetDistance - targetDistance) > zoomDeadZone)
        {
            targetDistance = newTargetDistance;
        }
        
        // Smooth zoom
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, collisionSmoothing);
        
        // Oppdater posisjon
        direction = rotation * new Vector3(0, 0, -1); // Recalculate direction in case angle changed
        desiredPosition = target.position + direction * currentDistance + Vector3.up * height;
        
        // Smooth follow position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);
        transform.position = smoothedPosition;
        
        // Look at target - ONLY if enabled
        if (lookAtPlayer)
        {
            Vector3 lookAtPoint = target.position + lookOffset;
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSmooth);
        }
        // Else: beholder sin opprinnelige rotasjon (Fixed)
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
