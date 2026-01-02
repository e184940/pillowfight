using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;        // GameObject that we should follow
    public float distance = 10.0f;  // how far back?
    public float maxDriftRange = 5.0f; // how far are we allowed to drift from the target position
    public float angleX = 20.0f;    // angle to pitch up on top of the target
    public float angleY = 0.0f;     // angle to yaw around the target
    
    // Min/Max verdier for sikkerhet
    public float minDistance = 1.0f;
    public float maxDistance = 50.0f;
    
    private Transform m_transform_cache;
    private Transform myTransform
    {
        get
        {
            if (m_transform_cache == null)
            {
                m_transform_cache = transform;
            }
            return m_transform_cache;
        }
    }
    
    void Start()
    {
        // Sikre at vi har gyldige verdier ved oppstart
        ValidateValues();
        
        // Hvis target ikke er satt, prøv å finn spilleren
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (target == null)
            {
                Debug.LogError("SmoothFollow: No target assigned and no GameObject with 'Player' tag found!");
            }
        }
    }
    
    void OnValidate()
    {
        ValidateValues();
        
        if (target != null)
        {
            try
            {
                Vector3 targetPos = GetTargetPos();
                // Sjekk for NaN verdier før tilordning
                if (IsValidPosition(targetPos))
                {
                    myTransform.position = targetPos;
                    myTransform.LookAt(target);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("SmoothFollow OnValidate error: " + e.Message);
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("SmoothFollow: Target is null, cannot follow");
            return;
        }
        
        try
        {
            Vector3 targetPos = GetTargetPos();
            
            // Sjekk for NaN eller ugyldige verdier
            if (!IsValidPosition(targetPos) || !IsValidPosition(myTransform.position))
            {
                Debug.LogWarning("SmoothFollow: Invalid position detected, resetting camera");
                ResetCameraPosition();
                return;
            }
            
            // Beregn avstand og sørg for at den er innenfor gyldige grenser
            float currentDistance = Vector3.Distance(myTransform.position, targetPos);
            
            // Bruk en glattere interpolasjon
            float t = Mathf.Clamp01(currentDistance / Mathf.Max(maxDriftRange, 0.1f));
            float smoothSpeed = Mathf.Lerp(0.5f, 2.0f, t) * Time.deltaTime;
            
            // Glatt kamera posisjon
            myTransform.position = Vector3.Lerp(myTransform.position, targetPos, smoothSpeed);
            
            // Se på target
            if (IsValidPosition(target.position))
            {
                myTransform.LookAt(target);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("SmoothFollow LateUpdate error: " + e.Message);
            ResetCameraPosition();
        }
    }
    
    private Vector3 GetTargetPos()
    {
        if (target == null)
        {
            return myTransform.position;
        }
        
        // Sikre gyldige vinkler
        float safeAngleX = float.IsNaN(angleX) ? 20.0f : Mathf.Clamp(angleX, -89, 89);
        float safeAngleY = float.IsNaN(angleY) ? 0.0f : angleY % 360;
        float safeDistance = float.IsNaN(distance) ? 10.0f : Mathf.Clamp(distance, minDistance, maxDistance);
        
        // Beregn posisjon
        Vector3 offset = new Vector3(0, 0, -safeDistance);
        Quaternion rotation = Quaternion.Euler(safeAngleX, safeAngleY, 0);
        Vector3 rotatedOffset = rotation * offset;
        
        // Returner posisjon relativt til target
        return target.position + (target.rotation * rotatedOffset);
    }
    
    private void ValidateValues()
    {
        // Sikre at alle verdier er gyldige
        if (float.IsNaN(distance) || float.IsInfinity(distance))
            distance = 10.0f;
        if (float.IsNaN(maxDriftRange) || float.IsInfinity(maxDriftRange))
            maxDriftRange = 5.0f;
        if (float.IsNaN(angleX) || float.IsInfinity(angleX))
            angleX = 20.0f;
        if (float.IsNaN(angleY) || float.IsInfinity(angleY))
            angleY = 0.0f;
        
        // Begrens verdier
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        maxDriftRange = Mathf.Max(0.1f, maxDriftRange);
        angleX = Mathf.Clamp(angleX, -89, 89);
    }
    
    private bool IsValidPosition(Vector3 position)
    {
        // Sjekk for NaN eller uendelige verdier
        return !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
               !float.IsInfinity(position.x) && !float.IsInfinity(position.y) && !float.IsInfinity(position.z);
    }
    
    private void ResetCameraPosition()
    {
        if (target != null && IsValidPosition(target.position))
        {
            try
            {
                // Reset til en sikker posisjon
                myTransform.position = target.position + new Vector3(0, 2, -5);
                myTransform.LookAt(target);
                
                // Reset kamera variabler
                angleX = 20.0f;
                angleY = 0.0f;
                distance = 10.0f;
            }
            catch
            {
                // Fallback til verdiene fra Start
                myTransform.position = new Vector3(0, 1, 0);
                myTransform.rotation = Quaternion.identity;
            }
        }
        else
        {
            // Hvis target er ugyldig, sett til standard posisjon
            myTransform.position = new Vector3(0, 1, 0);
            myTransform.rotation = Quaternion.identity;
        }
    }
    
    void OnDrawGizmos()
    {
        if (target != null && IsValidPosition(myTransform.position) && IsValidPosition(target.position))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(myTransform.position, target.position);
            
            // Vis også target posisjonen
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetTargetPos(), 0.5f);
        }
    }
}