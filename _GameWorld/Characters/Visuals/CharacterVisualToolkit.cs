using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVisualToolkit", menuName = "Scriptable Objects/CharacterVisualToolkit")]
public class CharacterVisualToolkit : ScriptableObject
{
    [field: SerializeField] public Color PrimaryColor { get; private set; } = Color.white;
    [field: SerializeField] public Color SecondaryColor { get; private set; } = Color.white;
    [field: SerializeField] public RectTransform CardAsset { get; private set; }
    [field: SerializeField] public Sprite Letter { get; private set; }
    [field: SerializeField] public bool The { get; private set; }
    [field: SerializeField] public AudioClip[] HeroClips { get; private set; }
    [SerializeField] private Sprite[] emotes;
    [SerializeField] private string[] emoteTexts;

    public Sprite GetEmote(EmoteType type) => emotes[(int)type];
    public string GetEmoteText(EmoteType type) => emoteTexts[(int)type];
}
