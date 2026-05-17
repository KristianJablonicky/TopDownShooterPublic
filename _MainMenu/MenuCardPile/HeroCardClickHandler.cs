using UnityEngine;

public class HeroCardClickHandler : MonoBehaviour
{
    private HeroSelectionManager manager;
    private bool idle = true;
    private void Start()
    {
        manager = HeroSelectionManager.Instance;
    }

    public void Clicked()
    {
        if (!manager.IsAnimationFinished) return;
        if (idle)
        {
            manager.HighlightHeroCard();
        }
        else
        {
            manager.UnHighlightHeroCard();
        }
        idle = !idle;
    }
}
