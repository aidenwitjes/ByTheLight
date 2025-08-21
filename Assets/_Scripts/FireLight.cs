using UnityEngine;
using UnityEngine.Rendering.Universal; // Needed for Light2D

public class FireLight : MonoBehaviour
{
    [Header("Fire State")]
    [SerializeField] private bool isOn = false;
    [SerializeField] private Light2D fireLight;
    [SerializeField] private Animator animator;

    [Header("Fire Animation States")]
    [SerializeField] private AnimationClip onAnimationClip;
    [SerializeField] private AnimationClip offAnimationClip;

    [Header("Flame Flicker Settings")]
    [SerializeField] private float intensityBase = 1f;
    [SerializeField] private float intensityVariation = 0.2f;
    [SerializeField] private float flickerSpeed = 2f;

    [Header("Light Radius Settings")]
    [SerializeField] private float radiusBase = 5f;
    [SerializeField] private float radiusVariation = 0.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 3f; // units per second

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;
    private float noiseSeed;
    private float targetIntensity = 0f;
    private float targetRadius = 0f;

    // Public property for other components to check fire state
    public bool IsOn => isOn;

    private void Awake()
    {
        if (fireLight == null)
            fireLight = GetComponentInChildren<Light2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        noiseSeed = Random.Range(0f, 100f);
        UpdateFireState();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleFire();
        }

        // Linear fade toward target values
        if (fireLight != null)
        {
            // Linear interpolation for consistent speed
            fireLight.intensity = Mathf.MoveTowards(fireLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
            fireLight.pointLightOuterRadius = Mathf.MoveTowards(fireLight.pointLightOuterRadius, targetRadius, Time.deltaTime * fadeSpeed);
        }

        // Add flicker only if fire is "on"
        if (isOn)
        {
            AnimateFlame();
        }
    }

    private void AnimateFlame()
    {
        if (fireLight == null) return;

        // Use same noise for both intensity and radius for synchronized flicker
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseSeed);
        float intensityFlicker = Mathf.Lerp(-intensityVariation, intensityVariation, noise);
        float radiusFlicker = Mathf.Lerp(-radiusVariation, radiusVariation, noise);

        targetIntensity = intensityBase + intensityFlicker;
        targetRadius = radiusBase + radiusFlicker;
    }

    public void ToggleFire()
    {
        isOn = !isOn;
        UpdateFireState();
    }

    // Public method for other components to light the fire
    public void LightFire()
    {
        if (!isOn)
        {
            isOn = true;
            UpdateFireState();
        }
    }

    // Public method for other components to extinguish the fire
    public void ExtinguishFire()
    {
        if (isOn)
        {
            isOn = false;
            UpdateFireState();
        }
    }

    // Public method to set fire state directly
    public void SetFireState(bool state)
    {
        if (isOn != state)
        {
            isOn = state;
            UpdateFireState();
        }
    }

    private void UpdateFireState()
    {
        if (animator != null)
        {
            if (isOn && onAnimationClip != null)
                animator.Play(onAnimationClip.name);
            else if (!isOn && offAnimationClip != null)
                animator.Play(offAnimationClip.name);
        }

        if (!isOn)
        {
            // Fade to 0 when turning off
            targetIntensity = 0f;
            targetRadius = 0f;
        }
        else
        {
            // Set starting targets when turning on (AnimateFlame will modulate further)
            targetIntensity = intensityBase;
            targetRadius = radiusBase;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    // Optional: Method to get current light intensity (useful for other systems)
    public float GetCurrentIntensity()
    {
        return fireLight != null ? fireLight.intensity : 0f;
    }

    // Optional: Method to get current light radius (useful for other systems)
    public float GetCurrentRadius()
    {
        return fireLight != null ? fireLight.pointLightOuterRadius : 0f;
    }
}