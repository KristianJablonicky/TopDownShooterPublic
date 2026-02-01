using System.Collections;
using TMPro;
using UnityEngine;

public class FadeBasedOnLocalPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TMP_Text[] tmpTexts;

    [Header("Settings")]
    [SerializeField] private bool fadeOutWhenPlayerIsNear = false;
    [SerializeField] float
        checkInterval = 0.2f,
        radiusToFade = 5f;

    private CharacterMediator localPlayer;
    private float far = 0f, near = 1f;
    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += player => localPlayer = player;
        if (fadeOutWhenPlayerIsNear)
        {
            (far, near) = (near, far);
        }
    }

    private IEnumerator Start()
    {
        var wait = new WaitForSeconds(checkInterval);
        float distance, alpha;
        while (true)
        {
            yield return wait;

            if (localPlayer == null) continue;

            distance = localPlayer.GetDistance(transform.position);
            if (distance > radiusToFade)
            {
                SetActivity(false);
                if (distance > 2f * radiusToFade) // wait longer if player is far away
                {
                    yield return wait;
                }
            }
            else
            {
                SetActivity(true);
                alpha = Tweener.QuadraticEaseOut(far, near, 1f - distance / radiusToFade);
                SetAlpha(alpha);
            }
        }
    }


    private void SetActivity(bool state)
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.enabled != state)
                spriteRenderer.enabled = state;
        }
        foreach (var tmpText in tmpTexts)
        {
            if (tmpText.enabled != state)
                tmpText.enabled = state;
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.SetAlpha(alpha);
        }
        foreach (var tmpText in tmpTexts)
        {
            var color = tmpText.color;
            color.a = alpha;
            tmpText.color = color;
        }
    }
}
