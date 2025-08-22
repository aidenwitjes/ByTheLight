// FireLight with flickering effect, matchbox requirement, and lifetime feature
using UnityEngine;
using System.Collections;

public class FireLight : BaseLight, IRequirementInteractable
{
    [Header("Fire Requirements")]
    [SerializeField] private bool requiresMatchbox = true;
    [SerializeField] private string requirementMessage = "Need a matchbox to light this fire";

    [Header("Flame Flicker Settings")]
    [SerializeField] private float intensityVariation = 0.2f;
    [SerializeField] private float radiusVariation = 0.5f;
    [SerializeField] private float flickerSpeed = 2f;

    [Header("Lifetime Settings")]
    [SerializeField] private bool hasLifetime = false;
    [SerializeField] private float lifetimeDuration = 60f; // Duration in seconds

    private float noiseSeed;
    private Coroutine lifetimeCoroutine;

    protected override void Awake()
    {
        base.Awake();
        noiseSeed = Random.Range(0f, 100f);
    }

    public bool CanInteract()
    {
        if (!isOn && requiresMatchbox)
        {
            // Check if player has matchbox in inventory
            return PlayerInventory.Instance?.HasItem("Matchbox") ?? false;
        }
        return true; // Can always extinguish fire
    }

    public string GetRequirementMessage()
    {
        return requirementMessage;
    }

    public override void Interact()
    {
        if (CanInteract())
        {
            bool wasOn = isOn;
            base.Interact();

            // Handle lifetime logic
            if (isOn && !wasOn && hasLifetime)
            {
                // Fire was just lit, start lifetime countdown
                StartLifetimeCountdown();
            }
            else if (!isOn && lifetimeCoroutine != null)
            {
                // Fire was manually extinguished, stop countdown
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }
        }
        else
        {
            // Show requirement message to player
            // This could be handled by a UI system or notification system
            Debug.Log(GetRequirementMessage());
            // You might want to trigger a UI popup or sound effect here
        }
    }

    private void StartLifetimeCountdown()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
    }

    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetimeDuration);

        // Automatically extinguish the fire as if manually interacted with
        if (isOn)
        {
            isOn = false;
            UpdateLightState();
            lifetimeCoroutine = null;
        }
    }

    // Public method to disable lifetime (used by puzzle system)
    public void DisableLifetime()
    {
        hasLifetime = false;
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }

    // Public method to re-enable lifetime
    public void EnableLifetime(float duration)
    {
        hasLifetime = true;
        lifetimeDuration = duration;
        if (isOn)
        {
            StartLifetimeCountdown();
        }
    }

    protected override void UpdateLightEffect()
    {
        if (isOn)
        {
            AnimateFlame();
        }
    }

    private void AnimateFlame()
    {
        if (light2D == null) return;

        // Use Perlin noise for smooth flickering
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseSeed);
        float intensityFlicker = Mathf.Lerp(-intensityVariation, intensityVariation, noise);
        float radiusFlicker = Mathf.Lerp(-radiusVariation, radiusVariation, noise);

        targetIntensity = intensityBase + intensityFlicker;
        targetRadius = radiusBase + radiusFlicker;
    }

    private void OnDisable()
    {
        // Clean up coroutine if the object is disabled
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }
}