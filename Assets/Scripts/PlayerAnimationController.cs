using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerController playerController;
    
    [Header("Animation Settings")]
    public float runSpeedThreshold = 0.1f;
    public float fallSpeedThreshold = -3f; // Økt fra -1 til -3 - kun ved større fall
    public float transitionTime = 0.05f; // Veldig kort transition (nesten immediate)
    
    private bool wasGrounded = true;
    private bool isJumping = false;
    private string currentState = "idle";
    
    void Start()
    {
        // Auto-find components if not assigned
        if (animator == null)
        {
            // Look for Animator in children (character model should be child of player)
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("No Animator found! Make sure character model is a child of player.");
            }
            else
            {
                Debug.Log($"Animator found: {animator.gameObject.name}");
                
                // Check if animator has a controller
                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Animator has NO Controller assigned! Assign 'char_AC' controller in Inspector.");
                }
                else
                {
                    Debug.Log($"Animator controller: {animator.runtimeAnimatorController.name}");
                    
                    // List all parameters
                    Debug.Log("Available animator parameters:");
                    foreach (var param in animator.parameters)
                    {
                        Debug.Log($"  - {param.name} ({param.type})");
                    }
                }
            }
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("No PlayerController found on this GameObject!");
            }
            else
            {
                Debug.Log("PlayerController found!");
            }
        }
    }
    
    void Update()
    {
        if (animator == null || playerController == null)
            return;
        
        UpdateAnimationState();
    }
    
    void UpdateAnimationState()
    {
        // Get player state from PlayerController
        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        bool isGrounded = playerController.isGrounded;
        float horizontalSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        float verticalSpeed = rb.linearVelocity.y;
        
        // Debug info every 60 frames (once per second at 60fps)
        if (Time.frameCount % 60 == 0)
        {
            // Commented out to reduce console spam
            // Debug.Log($"Player State - Grounded: {isGrounded}, HSpeed: {horizontalSpeed:F2}, VSpeed: {verticalSpeed:F2}");
        }
        
        // Detect jump start
        if (isGrounded && !wasGrounded)
        {
            // Just landed
            isJumping = false;
            // Removed debug log to prevent spam
        }
        else if (!isGrounded && wasGrounded && verticalSpeed > 0.5f)
        {
            // Just jumped
            isJumping = true;
            SetAnimationState("jump");
        }
        
        // Update animation based on current state
        if (!isGrounded)
        {
            // In air
            if (verticalSpeed < fallSpeedThreshold)
            {
                // Falling fast enough - trigger fall animation
                // Dette gjelder både når man går av platform OG ved nedturen fra stort hopp
                if (!isJumping || verticalSpeed < fallSpeedThreshold)
                {
                    SetAnimationState("fall");
                }
            }
            else if (isJumping && verticalSpeed > 0)
            {
                // Jumping up - already set, keep it
            }
        }
        else
        {
            // On ground
            if (horizontalSpeed > runSpeedThreshold)
            {
                // Moving
                SetAnimationState("run");
            }
            else
            {
                // Standing still
                SetAnimationState("idle");
            }
        }
        
        wasGrounded = isGrounded;
    }
    
    void SetAnimationState(string newState)
    {
        if (currentState == newState)
            return;
        
        // Don't interrupt jump animation too early
        if (currentState == "jump" && newState == "run")
        {
            // Let jump animation finish naturally (it auto-transitions)
            return;
        }
        
        // Use CrossFade for immediate response instead of triggers
        string stateName = GetStateName(newState);
        if (!string.IsNullOrEmpty(stateName))
        {
            animator.CrossFade(stateName, transitionTime); // Nesten immediate (0.05s)
            currentState = newState;
            
            // Debug logging - commented out to reduce spam
            // Debug.Log($"Animation crossfade to: {stateName} (transition: {transitionTime}s)");
        }
        else
        {
            Debug.LogWarning($"Animation state '{newState}' not found in animator!");
        }
    }
    
    // Map trigger names to actual animation state names
    string GetStateName(string trigger)
    {
        switch (trigger)
        {
            case "idle": return "Idle";
            case "run": return "running";
            case "jump": return "jump";
            case "fall": return "fall";
            default: return null;
        }
    }
    
    // Public methods to trigger win/lose animations
    public void PlayWinAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("blend feeling", 1f); // 1 = Win
            animator.SetTrigger("feel");
            currentState = "feel";
            Debug.Log("Playing WIN animation");
        }
    }
    
    public void PlayLoseAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("blend feeling", 0f); // 0 = Lose
            animator.SetTrigger("feel");
            currentState = "feel";
            Debug.Log("Playing LOSE animation");
        }
    }
    
    public void PlayGetUpAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("getup");
            Debug.Log("Playing GET UP animation");
        }
    }
}

