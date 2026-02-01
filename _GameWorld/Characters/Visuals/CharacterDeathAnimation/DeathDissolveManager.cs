using System.Buffers.Text;
using UnityEngine;

public class DeathDissolveManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TweenStyle tweenStyle = TweenStyle.quadraticEaseOut;
    public void Dissolve(CharacterMediator mediator, float duration,
        Vector2 linearVelocity, float bonusScale, Color color)
    {
        spriteRenderer.sprite = mediator.SpriteRenderer.sprite;
        var baseX = transform.position.x;
        var baseY = transform.position.y;

        if (color.a > 0)
        {
            spriteRenderer.material.SetColor("_OutlineColor", color);
        }

        Tweener.Tween(this, 0f, 1f, duration, tweenStyle,
            value =>
            {
                spriteRenderer.material.SetFloat("_AlphaCutoff", value);
                transform.position = new Vector2(
                    baseX + linearVelocity.x * value,
                    baseY + linearVelocity.y * value
                );
                transform.localScale = Vector2.one * (1f + bonusScale * value);
            },
            onExit: () => Destroy(gameObject)
        );
    }
}
