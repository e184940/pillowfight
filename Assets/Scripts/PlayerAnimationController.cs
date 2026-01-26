using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    // Use a generic Component so the script compiles even if the Animation package is missing.
    // In the inspector drag the Animator component (or the GameObject that has it) into this field.
    public Component animatorComponent;
    public PlayerController playerController;

    // Reflection caches
    private Type _animatorType;
    private MethodInfo _stringToHashMethod;
    private MethodInfo _hasStateMethod;
    private List<MethodInfo> _crossFadeMethods = new List<MethodInfo>();
    private PropertyInfo _runtimeControllerProp;
    private PropertyInfo _parametersProp;

    [Header("Animation Settings")]
    public float runSpeedThreshold = 0.1f;
    // Hysteresis thresholds to prevent snapping
    public float runEnterThreshold = 0.25f; // speed required to enter running
    public float runExitThreshold = 0.12f; // speed below which we exit running
    public float fallSpeedThreshold = -3f; // only trigger fall on larger falls
    public float transitionTime = 0.05f; // very short transition

    [Tooltip("Minimum seconds between animation state switches to avoid snapping")]
    public float minAnimationSwitchInterval = 0.12f;

    [Tooltip("Short lock after landing to avoid immediate state flips")]
    public float landingLockDuration = 0.12f;

    [Tooltip("If true the script requires an Animator Controller with the mapped states.")]
    public bool requireAnimatorController = true;

    [Tooltip("If true the script will log attempts to change animation states (useful to debug snapping)")]
    public bool verboseLogs = false;

    private bool wasGrounded = true;
    private bool isJumping = false;
    private string currentState = "idle";

    // internal flag set at runtime to mark that the animator + controller is usable
    private bool animationAvailable = false;

    private Dictionary<string, float> _lastWarningTime = new Dictionary<string, float>();
    private const float WarningThrottleSeconds = 2f;
    private float _lastAnimationSwitchTime = -10f;
    private float _lastLandTime = -10f;

    // Smoothing factor for horizontal speed (0..1). Higher = smoother (less responsive).
    [Range(0f, 1f)]
    public float speedSmoothing = 0.2f;

    private float _smoothedHorizontalSpeed = 0f;

    void Start()
    {
        // Auto-find PlayerController if not assigned
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        // Initialize reflection if animatorComponent assigned
        if (animatorComponent == null)
        {
            // Avoid using the Animator type directly (it may not exist if the Animation package is missing).
            Component tryFind = null;
            foreach (var c in GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                if (c.GetType().Name == "Animator")
                {
                    tryFind = c;
                    break;
                }
            }
            if (tryFind != null)
            {
                animatorComponent = tryFind;
            }
        }

        InitAnimatorReflection();

        if (_animatorType == null)
        {
            Debug.LogWarning("PlayerAnimationController: Animator/Animation package not available or Animator component not assigned. Animations disabled.");
            animationAvailable = false;
            return;
        }

        // Check runtimeAnimatorController via reflection
        var controller = _runtimeControllerProp?.GetValue(animatorComponent, null);
        if (controller == null)
        {
            animationAvailable = false;
            if (requireAnimatorController)
            {
                Debug.LogError("PlayerAnimationController: Animator has NO Controller assigned! Create an Animator Controller and assign it to the Animator component.");
            }
            else
            {
                Debug.Log("PlayerAnimationController: No controller but requireAnimatorController=false — continuing without animations.");
            }
            return;
        }

        animationAvailable = true;

        // List parameters (debug)
        if (_parametersProp != null)
        {
            try
            {
                var parameters = _parametersProp.GetValue(animatorComponent, null) as Array;
                if (parameters != null)
                {
                    Debug.Log("Animator parameters (reflection):");
                    foreach (var p in parameters)
                    {
                        var nameProp = p.GetType().GetProperty("name");
                        var typeProp = p.GetType().GetProperty("type");
                        string n = nameProp?.GetValue(p, null) as string;
                        object t = typeProp?.GetValue(p, null);
                        Debug.Log($"  - {n} (type={t})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to list animator parameters: " + ex.Message);
            }
        }

        Debug.Log("PlayerAnimationController: Animation reflection initialized and controller present.");
    }

    void InitAnimatorReflection()
    {
        if (animatorComponent == null)
            return;

        _animatorType = animatorComponent.GetType();
        // static int StringToHash(string)
        _stringToHashMethod = _animatorType.GetMethod("StringToHash", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        // bool HasState(int layerIndex, int stateID)
        _hasStateMethod = _animatorType.GetMethod("HasState", BindingFlags.Public | BindingFlags.Instance);
        // collect CrossFade overloads that have an int as first parameter or string as first parameter
        foreach (var m in _animatorType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (m.Name == "CrossFade")
            {
                var ps = m.GetParameters();
                if (ps.Length >= 1 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(string)))
                {
                    _crossFadeMethods.Add(m);
                }
            }
        }

        _runtimeControllerProp = _animatorType.GetProperty("runtimeAnimatorController", BindingFlags.Public | BindingFlags.Instance);
        _parametersProp = _animatorType.GetProperty("parameters", BindingFlags.Public | BindingFlags.Instance);
    }

    void Update()
    {
        if (playerController == null)
            return;

        if (!animationAvailable && requireAnimatorController)
            return;

        UpdateAnimationState();
    }

    void UpdateAnimationState()
    {
        if (playerController == null)
            return;

        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        bool isGrounded = playerController.isGrounded;
        Vector2 horiz = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        float rawHorizontalSpeed = horiz.magnitude;
        // simple low-pass to avoid wobble around threshold
        _smoothedHorizontalSpeed = Mathf.Lerp(_smoothedHorizontalSpeed, rawHorizontalSpeed, 1f - speedSmoothing);
        float horizontalSpeed = _smoothedHorizontalSpeed;
        float verticalSpeed = rb.linearVelocity.y;
        float now = Time.realtimeSinceStartup;

        // Landing / jumping detection
        if (isGrounded && !wasGrounded)
        {
            // Just landed
            isJumping = false;
            _lastLandTime = now;
            // don't force a state here, allow the stable-state logic below to decide
        }
        else if (!isGrounded && wasGrounded && verticalSpeed > 0.5f)
        {
            // Just jumped
            isJumping = true;
            // Immediately prefer jump animation when leaving ground upward
            TryChangeAnimationState("jump", now, horizontalSpeed, verticalSpeed);
            // update last switch time to avoid immediate flips
            _lastAnimationSwitchTime = now;
            wasGrounded = isGrounded;
            return; // we've set jump; wait for next frame for other logic
        }

        // Determine desired state with hysteresis
        string desiredState = currentState;

        if (!isGrounded)
        {
            // airborne: prefer fall if falling fast enough, otherwise keep jump if we were jumping
            if (verticalSpeed < fallSpeedThreshold)
            {
                desiredState = "fall";
            }
            else if (isJumping && verticalSpeed > 0f)
            {
                desiredState = "jump";
            }
        }
        else
        {
            // grounded: run/idle with hysteresis
            if (currentState == "run")
            {
                if (horizontalSpeed < runExitThreshold)
                    desiredState = "idle";
                else
                    desiredState = "run";
            }
            else
            {
                if (horizontalSpeed >= runEnterThreshold)
                    desiredState = "run";
                else
                    desiredState = "idle";
            }
        }

        // Prevent rapid toggles: don't switch if within min interval or immediately after landing
        if (desiredState != currentState)
        {
            bool landingLocked = (now - _lastLandTime) < landingLockDuration;
            if ((now - _lastAnimationSwitchTime) >= minAnimationSwitchInterval && !landingLocked)
            {
                TryChangeAnimationState(desiredState, now, horizontalSpeed, verticalSpeed);
            }
        }

        wasGrounded = isGrounded;
    }

    void TryChangeAnimationState(string newState, float now, float horizontalSpeed = 0f, float verticalSpeed = 0f)
    {
        if (verboseLogs)
        {
            Debug.Log($"PlayerAnimationController: Attempting state change -> {newState} at {now:F2}s (hSpeed={horizontalSpeed:F2}, vSpeed={verticalSpeed:F2})");
        }
        SafeSetAnimationState(newState);
        _lastAnimationSwitchTime = now;
        currentState = newState;
    }

    void SafeSetAnimationState(string newState)
    {
        if (!animationAvailable)
            return;

        if (animatorComponent == null)
            return;

        SetAnimationState(newState);
    }

    void SetAnimationState(string newState)
    {
        if (currentState == newState)
            return;

        if (currentState == "jump" && newState == "run")
            return;

        string stateName = GetStateName(newState);
        if (string.IsNullOrEmpty(stateName))
        {
            Debug.LogWarning($"PlayerAnimationController: No mapped state name for '{newState}'");
            return;
        }

        int hash = ComputeStringToHash(stateName);
        if (hash == int.MinValue)
        {
            Debug.LogWarning("PlayerAnimationController: Could not compute state hash. Aborting animation change.");
            return;
        }

        bool hasState = false;
        if (_hasStateMethod != null)
        {
            try
            {
                hasState = (bool)_hasStateMethod.Invoke(animatorComponent, new object[] { 0, hash });
            }
            catch { hasState = false; }
        }

        if (!hasState)
        {
            Debug.LogWarning($"PlayerAnimationController: Animator does not contain a state named '{stateName}' in layer 0.");
            return;
        }

        // CrossFade via reflection
        // Prefer Play(string) / Play(int) if available as they're simpler across Unity versions.
        try
        {
            var playStr = _animatorType?.GetMethod("Play", new Type[] { typeof(string) });
            if (playStr != null)
            {
                playStr.Invoke(animatorComponent, new object[] { stateName });
                currentState = newState;
                return;
            }

            var playInt = _animatorType?.GetMethod("Play", new Type[] { typeof(int) });
            if (playInt != null)
            {
                playInt.Invoke(animatorComponent, new object[] { hash });
                currentState = newState;
                return;
            }
        }
        catch (Exception ex)
        {
            ThrottledWarning(stateName, "Animator.Play fallback failed: " + ex.Message);
        }

        if (_crossFadeMethods.Count == 0)
        {
            ThrottledWarning(stateName, "No CrossFade overloads found via reflection.");
        }

        // Try each CrossFade overload and attempt to map parameter names to sensible defaults
        foreach (var method in _crossFadeMethods)
        {
            var ps = method.GetParameters();
            object[] args = new object[ps.Length];
            bool mapped = true;
            for (int i = 0; i < ps.Length; i++)
            {
                var pi = ps[i];
                var pType = pi.ParameterType;
                string pName = pi.Name?.ToLower() ?? string.Empty;

                if (pType == typeof(int))
                {
                    // layer index or similar: default -1
                    if (pName.Contains("layer")) args[i] = -1;
                    else args[i] = 0;
                }
                else if (pType == typeof(float))
                {
                    // transition duration vs normalized time
                    if (pName.Contains("trans") || pName.Contains("duration")) args[i] = transitionTime;
                    else args[i] = 0f;
                }
                else if (pType == typeof(string))
                {
                    // sometimes CrossFade has a string first param
                    args[i] = stateName;
                }
                else if (pType == typeof(bool))
                {
                    args[i] = false;
                }
                else
                {
                    // unknown type — try null (for reference types) or default for value types
                    if (pType.IsValueType) args[i] = Activator.CreateInstance(pType);
                    else args[i] = null;
                }
            }

            try
            {
                method.Invoke(animatorComponent, args);
                currentState = newState;
                return; // success
            }
            catch (TargetInvocationException tie)
            {
                string msg = tie.InnerException != null ? tie.InnerException.Message : tie.Message;
                ThrottledWarning(stateName, $"CrossFade overload failed ({method.GetParameters().Length} params): {msg}");
                continue; // try next overload
            }
            catch (Exception ex)
            {
                ThrottledWarning(stateName, $"CrossFade overload invocation failed ({method.GetParameters().Length} params): {ex.Message}");
                continue;
            }
        }

        // Fallbacks if CrossFade failed or wasn't available: try Play(int), Play(string), or SetTrigger(stateName)
        try
        {
            // Try Play(int)
            var playInt = _animatorType?.GetMethod("Play", new Type[] { typeof(int) });
            if (playInt != null)
            {
                playInt.Invoke(animatorComponent, new object[] { hash });
                currentState = newState;
                return;
            }

            // Try Play(string)
            var playStr = _animatorType?.GetMethod("Play", new Type[] { typeof(string) });
            if (playStr != null)
            {
                playStr.Invoke(animatorComponent, new object[] { stateName });
                currentState = newState;
                return;
            }

            // As last resort, try SetTrigger with the state name (some controllers use triggers)
            var setTrigger = _animatorType?.GetMethod("SetTrigger", new Type[] { typeof(string) });
            if (setTrigger != null)
            {
                setTrigger.Invoke(animatorComponent, new object[] { stateName });
                currentState = newState;
                return;
            }
        }
        catch (Exception fallbackEx)
        {
            Debug.LogWarning("PlayerAnimationController: Fallback animation invoke failed: " + fallbackEx.Message);
        }

        Debug.LogWarning("PlayerAnimationController: Unable to play animation state '" + stateName + "' via reflection.");
    }

    int ComputeStringToHash(string stateName)
    {
        if (_stringToHashMethod != null)
        {
            try
            {
                object res = _stringToHashMethod.Invoke(null, new object[] { stateName });
                if (res is int i) return i;
            }
            catch { }
        }

        // Fallback: use managed hash (not identical to Animator.StringToHash but usable as fallback)
        return stateName.GetHashCode();
    }

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

    bool HasParam(string paramName)
    {
        if (animatorComponent == null || _parametersProp == null)
            return false;

        try
        {
            var parameters = _parametersProp.GetValue(animatorComponent, null) as Array;
            if (parameters == null) return false;
            foreach (var p in parameters)
            {
                var nameProp = p.GetType().GetProperty("name");
                string n = nameProp?.GetValue(p, null) as string;
                if (n == paramName) return true;
            }
        }
        catch { }

        return false;
    }

    public void PlayWinAnimation()
    {
        if (!animationAvailable) return;
        if (HasParam("blend feeling")) InvokeAnimatorSetFloat("blend feeling", 1f);
        if (HasParam("feel")) InvokeAnimatorSetTrigger("feel");
        currentState = "feel";
    }

    public void PlayLoseAnimation()
    {
        if (!animationAvailable) return;
        if (HasParam("blend feeling")) InvokeAnimatorSetFloat("blend feeling", 0f);
        if (HasParam("feel")) InvokeAnimatorSetTrigger("feel");
        currentState = "feel";
    }

    public void PlayGetUpAnimation()
    {
        if (!animationAvailable) return;
        if (HasParam("getup")) InvokeAnimatorSetTrigger("getup");
    }

    void InvokeAnimatorSetFloat(string paramName, float value)
    {
        if (animatorComponent == null) return;
        var method = _animatorType?.GetMethod("SetFloat", new Type[] { typeof(string), typeof(float) });
        if (method != null) method.Invoke(animatorComponent, new object[] { paramName, value });
    }

    void InvokeAnimatorSetTrigger(string paramName)
    {
        if (animatorComponent == null) return;
        var method = _animatorType?.GetMethod("SetTrigger", new Type[] { typeof(string) });
        if (method != null) method.Invoke(animatorComponent, new object[] { paramName });
    }

    void ThrottledWarning(string key, string message)
    {
        float now = Time.realtimeSinceStartup;
        if (_lastWarningTime.TryGetValue(key, out float last) && now - last < WarningThrottleSeconds)
            return;
        _lastWarningTime[key] = now;
        Debug.LogWarning("PlayerAnimationController: " + message);
    }
}
