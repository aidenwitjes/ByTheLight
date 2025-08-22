public interface IRequirementInteractable : IInteractable
{
    bool CanInteract();
    string GetRequirementMessage();
}