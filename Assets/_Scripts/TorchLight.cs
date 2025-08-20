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
    [SerializeField] private float intensitySpeed = 2f;

    // Seed for randomizing the flicker pattern
    private float noiseSeed;

    private void Awake()
    {
        if (torchLight == null)
            torchLight = GetComponentInChildren<Light2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        // Assign a random seed for the Perlin noise to make each torch flicker differently
        noiseSeed = Random.Range(0f, 100f);

        UpdateTorchState(); // Set the correct state on start
    }

    private void Update()
    {
        // Example toggle input
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleTorch();
        }

        if (isOn)
        {
            AnimateFlame();
        }
    }

    private void AnimateFlame()
    {
        if (torchLight == null) return;

        // Use Perlin noise for a more natural, radiating glow effect
        float noise = Mathf.PerlinNoise(Time.time * intensitySpeed, noiseSeed);

        // Remap the noise value from 0-1 to a range around the base intensity
        // This ensures the light never turns completely off
        torchLight.intensity = Mathf.Lerp(intensityBase - intensityVariation, intensityBase + intensityVariation, noise);
    }

    public void ToggleTorch()
    {
        isOn = !isOn;
        UpdateTorchState();
    }

    private void UpdateTorchState()
    {
        if (torchLight != null)
            torchLight.enabled = isOn;

        if (animator != null)
        {
            // Use a boolean parameter in the Animator for cleaner state management
            // instead of calling Play directly. But this works too.
            if (isOn)
                animator.Play("TorchFlameOn");
            else
                animator.Play("TorchFlameOff");
        }
    }
}