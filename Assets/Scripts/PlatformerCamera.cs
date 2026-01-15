using UnityEngine;

/// <summary>
/// Simple Smooth Follow Camera
/// </summary>
public class PlatformerCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Settings")]
    public float distance = 6.0f;
    public float height = 3.0f;

    public float heightDamping = 0.05f; // Mye raskere (var 0.2)
    public float rotationDamping = 0.05f; // Mye raskere (var 0.2)
    public Vector3 lookOffset = new Vector3(0, 1, 0);

    [Header("Input")]
    public float mouseSensitivity = 5.0f;

    private LayerMask obstacleLayer; // For enkel kollisjonssjekk hvis vi trenger det senere, men holder det rent nå.

    // Velocity referanser for SmoothDamp
    private float rotationVelocity;
    private float heightVelocity;
    
    // Mouse control
    private float mouseXOffset = 0f;
    private float mouseYOffset = 0f;

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void Update()
    {
        // Les mus-input for å rotere kamera rundt spilleren
        mouseXOffset += Input.GetAxis("Mouse X") * mouseSensitivity;
        
        // Juster høyde med musen (Mouse Y)
        mouseYOffset -= Input.GetAxis("Mouse Y") * (mouseSensitivity * 0.2f);
        mouseYOffset = Mathf.Clamp(mouseYOffset, -2.0f, 4.0f); // Begrens hvor høyt/lavt man kan se
    }

    void LateUpdate()
    {
        if (!target) return;

        // Ønsket rotasjon er Spillerens rotasjon + Musens offset
        float wantedRotationAngle = target.eulerAngles.y + mouseXOffset;
        
        // Ønsket høyde er Spillerens høyde + Base høyde + Musens offset
        float wantedHeight = target.position.y + height + mouseYOffset;

        // Nåværende rotasjon og høyde
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Demp rotasjonen med SmoothDampAngle
        currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref rotationVelocity, rotationDamping);

        // Demp høyden med SmoothDamp
        currentHeight = Mathf.SmoothDamp(currentHeight, wantedHeight, ref heightVelocity, heightDamping);

        // Konverter vinkel til rotasjon
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Sett posisjon: Start ved player, gå bakover (distance) basert på vår dempede rotasjon
        // Det er viktig at vi bruker 'rotationDamping' som er lav nok til at kameraet henger etter
        Vector3 pos = target.position;
        pos -= currentRotation * Vector3.forward * distance;
        
        // Sett høyden manuelt
        pos.y = currentHeight;
        
        // Oppdater kameraets posisjon
        transform.position = pos;

        // Sørg for at kameraet alltid ser mot spillerens posisjon pluss offset
        transform.LookAt(target.position + lookOffset);
    }
}
