// FireLight with flickering effect and matchbox requirement
using UnityEngine;

public class FireLight : BaseLight, IRequirementInteractable
{
    [Header("Fire Requirements")]
    [SerializeField] private bool requiresMatchbox = true;
    [SerializeField] private string requirementMessage = "Need a matchbox to light this fire";

    [Header("Flame Flicker Settings")]
    [SerializeField] private float intensityVariation = 0.2f;
    [SerializeField] private float radiusVariation = 0.5f;
    [SerializeField] private float flickerSpeed = 2f;

    private float noiseSeed;

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
            base.Interact();
        }
        else
        {
            // Show requirement message to player
            // This could be handled by a UI system or notification system
            Debug.Log(GetRequirementMessage());
            // You might want to trigger a UI popup or sound effect here
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
}