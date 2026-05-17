using UnityEngine;
public class PlayerCardMenu : PlayerCardConcrete
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private SoundPlayer soundPlayer;
    private CharacterVisualToolkit toolkit;
    private static bool ignoredFirstInstance = false;
    public override void VirtualInit(CharacterVisualToolkit toolkit)
    {
        this.toolkit = toolkit;
        base.VirtualInit(toolkit);
        rectTransform.StretchToParent();
    }

    public override void Selected()
    {
        if (!ignoredFirstInstance)
        {
            ignoredFirstInstance = true;
            return;
        }
        foreach (var clip in toolkit.HeroClips)
        {
            soundPlayer.RequestPlaySound(transform, clip, false);
        }
        //soundPlayer.RequestPlaySound(transform, toolkit.HeroClip, false);
    }

    private void OnDestroy()
    {
        ignoredFirstInstance = false;
    }
}