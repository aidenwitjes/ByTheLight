using UnityEngine;
using UnityEngine.Events;

public class CampfireCheckpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool requiresLitFire = true; // Must campfire be lit to save?

    [Header("Visual Feedback")]
    [SerializeField] private GameObject checkpointIndicator; // Optional UI element
    [SerializeField] private ParticleSystem saveEffect; // Optional particle effect

    [Header("Audio")]
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private float interactionRange = 2f;

    [Header("Events")]
    public UnityEvent OnCheckpointActivated;
    public UnityEvent OnCheckpointUsed;

    // References
    private FireLight fireLight; // Reference to the fire lighting component
    private bool playerInRange = false;
    // private GameManager gameManager; // Uncomment if you have a GameManager

    private void Awake()
    {
        // Get the FireLight component on the same GameObject
        fireLight = GetComponent<FireLight>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Find GameManager - uncomment and adjust this based on your setup
        // gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        // Hide checkpoint indicator initially
        if (checkpointIndicator != null)
            checkpointIndicator.SetActive(false);

        UpdateCheckpointVisuals();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryActivateCheckpoint();
        }
    }

    private void TryActivateCheckpoint()
    {
        // Check if fire needs to be lit
        if (requiresLitFire && fireLight != null && !fireLight.IsOn)
        {
            Debug.Log("The campfire must be lit to activate this checkpoint!");
            return;
        }

        if (!isActivated)
        {
            ActivateCheckpoint();
        }
        else
        {
            UseCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        // Save game state - uncomment if you have a GameManager
        // if (gameManager != null)
        // {
        //     gameManager.SetCheckpoint(transform.position);
        //     gameManager.SaveGame(); // Optional: auto-save when checkpoint is activated
        // }

        // For now, just save the position to PlayerPrefs as an example
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.Save();

        // Visual and audio feedback
        PlayCheckpointEffect();
        UpdateCheckpointVisuals();

        // Trigger event
        OnCheckpointActivated?.Invoke();

        Debug.Log("Checkpoint activated!");
    }

    private void UseCheckpoint()
    {
        // This could be used for manual saving, healing, or other checkpoint benefits
        // if (gameManager != null)
        // {
        //     gameManager.SaveGame();
        // }

        // For now, just update the saved position
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.Save();

        PlayCheckpointEffect();
        OnCheckpointUsed?.Invoke();

        Debug.Log("Game saved at checkpoint!");
    }

    private void PlayCheckpointEffect()
    {
        // Play sound effect
        if (audioSource != null && checkpointSound != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }

        // Play particle effect
        if (saveEffect != null)
        {
            saveEffect.Play();
        }
    }

    private void UpdateCheckpointVisuals()
    {
        if (checkpointIndicator != null)
        {
            checkpointIndicator.SetActive(isActivated);
        }
    }

    // Public method to check if this checkpoint is activated (useful for other systems)
    public bool IsActivated => isActivated;

    // Public method to manually activate (useful for scripted events)
    public void ForceActivateCheckpoint()
    {
        ActivateCheckpoint();
    }

    // Trigger detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Show interaction prompt
            if (checkpointIndicator != null && isActivated)
            {
                checkpointIndicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Hide interaction prompt
            if (checkpointIndicator != null)
            {
                checkpointIndicator.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}