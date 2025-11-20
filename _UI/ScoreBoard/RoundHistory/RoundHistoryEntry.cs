using UnityEngine;
using UnityEngine.UI;

public class RoundHistoryEntry : MonoBehaviour
{
    [SerializeField] private Image team, role;

    public void SetUp(Sprite teamSprite, Sprite roleSprite)
    {
        team.sprite = teamSprite;
        role.sprite = roleSprite;
    }
}
