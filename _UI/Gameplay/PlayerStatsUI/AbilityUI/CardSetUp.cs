using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class CardSetUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField, FormerlySerializedAs("abilityIcon")] protected Image cardImage;
    [SerializeField, FormerlySerializedAs("abilityName")] protected TMP_Text cardName;

    [SerializeField] protected GraphicsToRecolor primaryGraphicsToRecolor;
    [SerializeField] protected GraphicsToRecolor secondaryGraphicsToRecolor;
    
    public string LongDescription { get; private set; }
    public string Name { get; private set; }
    public ScriptableObjectBase ScriptableObjectBase { get; private set; }
    public void Init(CharacterToolkit toolkit, ScriptableObjectBase SOBase)
    {
        ScriptableObjectBase = SOBase;
        Name = SOBase.Name;
        cardName.text = Name;
        cardImage.sprite = SOBase.Icon;

        primaryGraphicsToRecolor.Recolor(toolkit.PrimaryColor * Constants.colorUIMultiplier);
        secondaryGraphicsToRecolor.Recolor(toolkit.SecondaryColor * Constants.colorUIMultiplier);

        LongDescription = SpecificInit(toolkit, SOBase);
    }

    protected abstract string SpecificInit(CharacterToolkit toolkit, ScriptableObjectBase SOBase);
}
