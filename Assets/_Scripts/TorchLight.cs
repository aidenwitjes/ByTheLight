using UnityEngine;
using UnityEngine.Rendering.Universal; // Needed for Light2D

public class TorchLight : MonoBehaviour
{
    [Header("Torch State")]
    [SerializeField] private bool isOn = false;
    [SerializeField] private Light2D torchLight;
    [SerializeField] private Animator animator;

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

    private void Awake()
    {
        if (torchLight == null)
            torchLight = GetComponentInChildren<Light2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        noiseSeed = Random.Range(0f, 100f);
        UpdateTorchState();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleTorch();
        }

        // Linear fade toward target values
        if (torchLight != null)
        {
            // Linear interpolation for consistent speed
            torchLight.intensity = Mathf.MoveTowards(torchLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
            torchLight.pointLightOuterRadius = Mathf.MoveTowards(torchLight.pointLightOuterRadius, targetRadius, Time.deltaTime * fadeSpeed);
        }

        // Add flicker only if torch is "on"
        if (isOn)
        {
            AnimateFlame();
        }
    }

    private void AnimateFlame()
    {
        if (torchLight == null) return;

        // Use same noise for both intensity and radius for synchronized flicker
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseSeed);
        float intensityFlicker = Mathf.Lerp(-intensityVariation, intensityVariation, noise);
        float radiusFlicker = Mathf.Lerp(-radiusVariation, radiusVariation, noise);

        targetIntensity = intensityBase + intensityFlicker;
        targetRadius = radiusBase + radiusFlicker;
    }

    public void ToggleTorch()
    {
        isOn = !isOn;
        UpdateTorchState();
    }

    private void UpdateTorchState()
    {
        if (animator != null)
        {
            if (isOn)
                animator.Play("TorchFlameOn");
            else
                animator.Play("TorchFlameOff");
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
}