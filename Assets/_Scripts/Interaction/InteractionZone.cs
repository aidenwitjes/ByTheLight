// Flexible interaction system component
using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject targetObject; // Optional: manually specify target

    private IInteractable currentInteractable;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the interactable component
            currentInteractable = FindInteractable();
            currentInteractable?.SetPlayerInRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentInteractable?.SetPlayerInRange(false);
            currentInteractable = null;
        }
    }

    private void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private IInteractable FindInteractable()
    {
        IInteractable interactable = null;

        // 1. Check if manually assigned target
        if (targetObject != null)
        {
            interactable = targetObject.GetComponent<IInteractable>();
            if (interactable != null) return interactable;
        }

        // 2. Check this GameObject
        interactable = GetComponent<IInteractable>();
        if (interactable != null) return interactable;

        // 3. Check parent hierarchy
        interactable = GetComponentInParent<IInteractable>();
        if (interactable != null) return interactable;

        // 4. Check children (useful for complex hierarchies)
        interactable = GetComponentInChildren<IInteractable>();

        return interactable;
    }
}
