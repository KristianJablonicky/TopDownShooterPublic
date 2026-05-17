using System;
using UnityEngine;
public class PopUpWindowBase : CanvasFaderBase, IActivatable
{
    public event Action Shown, Hidden;
    public event Action<bool> VisibilityChanged;
    [field: SerializeField] public GameObject BackgroundFade { get; private set; }
    [SerializeField] private AnimationWhenEnabled[] animations;

    public event Action AfterStart;
    public void GetActivated()
    {
        TweenState(true);
        Shown?.Invoke();
        VisibilityChanged?.Invoke(true);
    }

    public void GetDeactivated()
    {
        TweenState(false, () => gameObject.SetActive(false));
        foreach (var animation in animations)
        {
            animation.PlayAnimationReversed();
        }
        Hidden?.Invoke();
        VisibilityChanged?.Invoke(false);
    }

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        foreach (var animation in animations)
        {
            animation.AnimationDuration = animationDuration;
        }
    }
    private void Start()
    {
        gameObject.SetActive(false);
        AfterStart?.Invoke();
    }
    private bool ignoreFirstEnable = true;
    private void OnEnable()
    {
        if (ignoreFirstEnable)
        {
            ignoreFirstEnable = false;
            return;
        }
        WindowStackManager.Instance.AddWindow(this);
    }
}
