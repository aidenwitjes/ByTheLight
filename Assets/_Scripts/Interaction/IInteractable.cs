public interface IInteractable
{
    void Interact();               // Called when the player presses E
    void SetPlayerInRange(bool inRange); // Called by InteractionZone trigger
}