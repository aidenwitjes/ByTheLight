// Flashlight that requires the actual flashlight item to use
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour, IRequirementInteractable
{
    [Header("Flashlight Requirements")]
    [SerializeField] private bool requiresFlashlight = true;
    [SerializeField] private string requirementMessage = "Need to find a flashlight first";

    [Header("Flashlight References")]
    public Transform flashlightTransform;
    public SpriteRenderer playerSprite;
    public Light2D flashlightLight;
    public GameObject flashlightSprite;

    [Header("Flashlight Settings")]
    public bool isFlashlightOn = false; // Start off since we need the item first
    public KeyCode toggleKey = KeyCode.F;

    [Header("Upgrades")]
    public bool hasAdjustableLens = false;
    public bool hasAdvancedBulb = false;

    [Header("Base Values (Before Upgrades)")]
    public float baseIntensity = 1f;
    public float baseOuterRadius = 8f;
    public float baseInnerAngle = 30f;
    public float baseOuterAngle = 60f;
    public float baseFalloff = 1f;

    [Header("Advanced Bulb Buffs")]
    public float bulbIntensityBoost = 0.3f;
    public float bulbRadiusBoost = 3f;
    public float bulbFalloff = 0.9f;

    [Header("Lens Adjustment Settings")]
    public float scrollSensitivity = 0.05f;
    private float lensT = 0.5f;

    [Header("Lens Focused Modifiers")]
    public float focusIntensityAdd = 0.4f;
    public float focusRadiusAdd = 2f;
    public float focusInnerAngle = 15f;
    public float focusOuterAngle = 35f;

    [Header("Lens Wide Modifiers")]
    public float wideIntensitySub = 0.3f;
    public float wideRadiusSub = 2f;
    public float wideInnerAngle = 55f;
    public float wideOuterAngle = 90f;

    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip toggleOnSound;
    public AudioClip toggleOffSound;

    [Header("Interaction")]
    private bool playerInRange = false;

    private bool lastFrameFlashlightState;
    private bool lastHasAdjustableLens;
    private bool lastHasAdvancedBulb;
    private bool hasFlashlightItem = false;

    void Start()
    {
        CheckForFlashlightItem();
        ApplyFlashlightSettings();
        lastFrameFlashlightState = isFlashlightOn;
        lastHasAdjustableLens = hasAdjustableLens;
        lastHasAdvancedBulb = hasAdvancedBulb;
    }

    void Update()
    {
        // Check if player has flashlight item
        CheckForFlashlightItem();

        // Only allow flashlight usage if player has the item
        if (hasFlashlightItem)
        {
            HandleFlashlightToggle();

            if (isFlashlightOn)
            {
                HandleFlashlightAiming();
                HandlePlayerFlipping();

                if (hasAdjustableLens)
                    HandleLensAdjustment();
            }
        }
        else
        {
            // Force flashlight off if player doesn't have item
            if (isFlashlightOn)
            {
                isFlashlightOn = false;
                UpdateFlashlightVisuals();
            }
        }

        CheckForUpgradeChanges();

        if (lastFrameFlashlightState != isFlashlightOn)
        {
            UpdateFlashlightVisuals();
            PlayToggleSound();
            lastFrameFlashlightState = isFlashlightOn;
        }
    }

    private void CheckForFlashlightItem()
    {
        if (PlayerInventory.Instance != null)
        {
            hasFlashlightItem = PlayerInventory.Instance.HasItem("Flashlight");
        }
    }

    // IRequirementInteractable implementation
    public bool CanInteract()
    {
        if (requiresFlashlight)
        {
            return PlayerInventory.Instance?.HasItem("Flashlight") ?? false;
        }
        return true;
    }

    public string GetRequirementMessage()
    {
        return requirementMessage;
    }

    public void Interact()
    {
        if (CanInteract())
        {
            ToggleFlashlight();
        }
        else
        {
            Debug.Log(GetRequirementMessage());
            // You could show UI message here instead
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }

    // Public method to toggle flashlight (can be called externally)
    public void ToggleFlashlight()
    {
        if (hasFlashlightItem)
        {
            isFlashlightOn = !isFlashlightOn;
        }
        else
        {
            Debug.Log(GetRequirementMessage()); // Add this!
                                                // Or trigger UI notification here
        }
    }

    private void CheckForUpgradeChanges()
    {
        if (lastHasAdjustableLens != hasAdjustableLens || lastHasAdvancedBulb != hasAdvancedBulb)
        {
            ApplyFlashlightSettings();
            lastHasAdjustableLens = hasAdjustableLens;
            lastHasAdvancedBulb = hasAdvancedBulb;
        }
    }

    private void HandleFlashlightToggle()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleFlashlight();
    }

    private void HandleFlashlightAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 dir = mousePos - flashlightTransform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        flashlightTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandlePlayerFlipping()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        playerSprite.flipX = mousePos.x <= playerSprite.transform.position.x;
    }

    private void HandleLensAdjustment()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            lensT = Mathf.Clamp01(lensT - scroll * scrollSensitivity);
            ApplyFlashlightSettings();
        }
    }

    private void ApplyFlashlightSettings()
    {
        // Start from base
        float intensity = baseIntensity;
        float radius = baseOuterRadius;
        float innerAngle = baseInnerAngle;
        float outerAngle = baseOuterAngle;
        float falloff = baseFalloff;

        // Apply bulb upgrade (permanent buff)
        if (hasAdvancedBulb)
        {
            intensity += bulbIntensityBoost;
            radius += bulbRadiusBoost;
            falloff = bulbFalloff;
        }

        // Apply lens adjustment (additive/subtractive)
        if (hasAdjustableLens)
        {
            // Calculate focused values (base + additions)
            float focusedIntensity = intensity + focusIntensityAdd;
            float focusedRadius = radius + focusRadiusAdd;

            // Calculate wide values (base - subtractions)
            float wideIntensity = intensity - wideIntensitySub;
            float wideRadius = radius - wideRadiusSub;

            // Lerp between focused and wide
            intensity = Mathf.Lerp(focusedIntensity, wideIntensity, lensT);
            radius = Mathf.Lerp(focusedRadius, wideRadius, lensT);
            innerAngle = Mathf.Lerp(focusInnerAngle, wideInnerAngle, lensT);
            outerAngle = Mathf.Lerp(focusOuterAngle, wideOuterAngle, lensT);
        }

        // Assign to light
        flashlightLight.intensity = intensity;
        flashlightLight.pointLightOuterRadius = radius;
        flashlightLight.pointLightInnerAngle = innerAngle;
        flashlightLight.pointLightOuterAngle = outerAngle;
        flashlightLight.falloffIntensity = falloff;
    }

    private void UpdateFlashlightVisuals()
    {
        if (flashlightLight != null)
            flashlightLight.enabled = isFlashlightOn;
        if (flashlightSprite != null)
            flashlightSprite.SetActive(isFlashlightOn);
    }

    private void PlayToggleSound()
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = isFlashlightOn ? toggleOnSound : toggleOffSound;
            if (clipToPlay != null)
                audioSource.PlayOneShot(clipToPlay);
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying && flashlightLight != null)
        {
            ApplyFlashlightSettings();
        }
    }
}