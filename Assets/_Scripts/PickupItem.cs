// Pickupable item that adds itself to inventory when interacted with
using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Matchbox";
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private bool showPickupMessage = true;
    [SerializeField] private string pickupMessage = "Picked up {0}!";

    [Header("Visual Feedback")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private bool shouldBob = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private bool playerInRange = false;
    private Vector3 startPosition;
    private AudioSource audioSource;

    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Optional bobbing animation
        if (shouldBob)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    public void Interact()
    {
        if (PlayerInventory.Instance != null)
        {
            bool wasAdded = PlayerInventory.Instance.AddItem(itemName);

            if (wasAdded)
            {
                // Play pickup sound
                if (pickupSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

                // Show pickup message
                if (showPickupMessage)
                {
                    Debug.Log(string.Format(pickupMessage, itemName));
                    // You could trigger UI notification here instead of Debug.Log
                }

                // Destroy or hide the item
                if (destroyOnPickup)
                {
                    // Small delay if playing sound
                    if (pickupSound != null)
                        Destroy(gameObject, pickupSound.length);
                    else
                        Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;

        // Optional: Change visual appearance when player is nearby
        // For example, highlight the item or show interaction prompt
    }

    // Optional: Visual feedback when player is in range
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Could show "Press E to pick up" UI here
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide interaction prompt
        }
    }
}