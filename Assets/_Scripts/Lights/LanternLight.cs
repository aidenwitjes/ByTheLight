// LanternLight - static light source with smooth transitions
public class LanternLight : BaseLight
{
    protected override void UpdateLightEffect()
    {
        // No special effects - just maintains steady light
        // The base class handles smooth fading to target values
    }
}
