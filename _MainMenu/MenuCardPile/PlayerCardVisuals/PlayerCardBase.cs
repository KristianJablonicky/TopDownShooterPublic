using UnityEngine;
using UnityEngine.UI;

public class PlayerCardBase : MonoBehaviour
{
    [SerializeField] private PlayerCardConcrete concreteInit;
    [Header("References")]
    [SerializeField] protected RectTransform heroGraphicContainer;
    [SerializeField] protected JaggedPolygonUI healthJaggedPolygon;
    [SerializeField] protected Image[] heroLetters;


    public void Init(CharacterVisualToolkit toolkit)
    {
        healthJaggedPolygon.SetColor(toolkit.PrimaryColor * Constants.colorUIMultiplier);
        InitHeroVisuals(toolkit);
        concreteInit.VirtualInit(toolkit);
    }

    private void InitHeroVisuals(CharacterVisualToolkit toolkit)
    {
        var instance = Instantiate(toolkit.CardAsset);
        instance.transform.SetParent(heroGraphicContainer);
        instance.transform.localPosition = Vector3.zero;
        instance.StretchToParent();

        foreach (var letter in heroLetters)
        {
            letter.sprite = toolkit.Letter;
        }
    }

    public void Selected()
    {
        concreteInit.Selected();
    }
}

public class PlayerCardConcrete : MonoBehaviour
{
    [SerializeField] protected PlayerCardBase cardBase;
    public virtual void VirtualInit(CharacterVisualToolkit toolkit) { }
    public virtual void Selected() { }
}