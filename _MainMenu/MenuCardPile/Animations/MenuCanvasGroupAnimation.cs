using UnityEngine;

public class MenuCanvasGroupAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        var manager = HeroSelectionManager.Instance;
        manager.AnimationProgressed += OnProgress;
        manager.HeroCardPickedUp += () => canvasGroup.gameObject.SetActive(true);
        manager.HeroCardPutDown += () => canvasGroup.gameObject.SetActive(false);
        canvasGroup.gameObject.SetActive(false);
    }

    private void OnProgress(float progress)
    {
        canvasGroup.alpha = progress;
    }
}
