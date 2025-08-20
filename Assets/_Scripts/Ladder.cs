using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ladder : MonoBehaviour
{
    private void Reset()
    {
        // Make collider a trigger automatically
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var climber = other.GetComponent<LadderClimber>();
        if (climber != null)
        {
            climber.SetOnLadder(true, this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var climber = other.GetComponent<LadderClimber>();
        if (climber != null)
        {
            climber.SetOnLadder(false, this);
        }
    }
}
