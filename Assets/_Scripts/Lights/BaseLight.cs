using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class BaseLight : MonoBehaviour, IInteractable
{
    [Header("Light State")]
    [SerializeField] protected bool isOn = false;
    [SerializeField] protected Light2D light2D;
    [SerializeField] protected Animator animator;

    [Header("Light Animation States")]
    [SerializeField] protected AnimationClip onAnimationClip;
    [SerializeField] protected AnimationClip offAnimationClip;

    [Header("Light Settings")]
    [SerializeField] protected float intensityBase = 1f;
    [SerializeField] protected float radiusBase = 5f;

    [Header("Fade Settings")]
    [SerializeField] protected float fadeSpeed = 3f;

    protected bool playerInRange = false;
    protected float targetIntensity = 0f;
    protected float targetRadius = 0f;

    // Public properties
    public bool IsOn => isOn;

    protected virtual void Awake()
    {
        if (light2D == null)
            light2D = GetComponentInChildren<Light2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        UpdateLightState();
    }

    protected virtual void Update()
    {
        // Handle fading
        if (light2D != null)
        {
            light2D.intensity = Mathf.MoveTowards(light2D.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
            light2D.pointLightOuterRadius = Mathf.MoveTowards(light2D.pointLightOuterRadius, targetRadius, Time.deltaTime * fadeSpeed);
        }

        // Override in derived classes for specific lighting effects
        UpdateLightEffect();
    }

    // Abstract method for specific light effects (flicker, pulse, etc.)
    protected abstract void UpdateLightEffect();

    // IInteractable implementation
    public virtual void Interact()
    {
        ToggleLight();
    }

    public virtual void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }

    // Public methods for external control
    public virtual void ToggleLight()
    {
        isOn = !isOn;
        UpdateLightState();
    }

    public virtual void TurnOn()
    {
        if (!isOn)
        {
            isOn = true;
            UpdateLightState();
        }
    }

    public virtual void TurnOff()
    {
        if (isOn)
        {
            isOn = false;
            UpdateLightState();
        }
    }

    public virtual void SetLightState(bool state)
    {
        if (isOn != state)
        {
            isOn = state;
            UpdateLightState();
        }
    }

    protected virtual void UpdateLightState()
    {
        // Handle animation
        if (animator != null)
        {
            if (isOn && onAnimationClip != null)
                animator.Play(onAnimationClip.name);
            else if (!isOn && offAnimationClip != null)
                animator.Play(offAnimationClip.name);
        }

        // Set base targets
        if (!isOn)
        {
            targetIntensity = 0f;
            targetRadius = 0f;
        }
        else
        {
            targetIntensity = intensityBase;
            targetRadius = radiusBase;
        }
    }

    // Utility methods
    public float GetCurrentIntensity() => light2D?.intensity ?? 0f;
    public float GetCurrentRadius() => light2D?.pointLightOuterRadius ?? 0f;
}