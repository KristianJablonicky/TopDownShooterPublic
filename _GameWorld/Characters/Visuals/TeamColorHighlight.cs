using UnityEngine;

public class TeamColorHighlight : MonoBehaviour
{
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private void Start()
    {
        ChangeColor(Color.clear);
        spriteRenderer.enabled = false;
        GameStateManager.Instance.GameStarted += OnGameStart;
    }

    private void OnGameStart()
    {
        spriteRenderer.enabled = true;
        var color = CommonColors.GetTeamColor(mediator.playerData.Team.Name);
        ChangeColor(color);
    }

    private void ChangeColor(Color newColor) => spriteRenderer.color = newColor;
}
