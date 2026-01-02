using UnityEngine;

public class UnusePlayerController : MonoBehaviour
{
    // ground movement
    private Rigidbody rb;
    public float speed = 5f;
    private float moveSideways;
    private float moveForward;

    // jump
    public float jumpForce = 10f;

    // multiplies gravity when falling/ascending to peak of jump
    public float descendFactor = 2.5f;
    public float ascendFactor = 2f;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance = 1.1f;

    // camera roation
    public float mouseSensitivity = 3f;
    private float verticalRotation;
    private Transform cameraTransform;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerHeight = GetComponent<BoxCollider>().size.y * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        moveSideways = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();

        isGrounded = Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
    }

    void MovePlayer()
    {
        Vector3 movement = (transform.right * moveSideways + transform.forward * moveForward).normalized;
        Vector3 targetVelocity = movement * speed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;

        if (isGrounded && moveSideways == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void Jump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * descendFactor * Time.fixedDeltaTime;
        }
        else
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * ascendFactor * Time.fixedDeltaTime;
        }
    }

    void RotateCamera()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}