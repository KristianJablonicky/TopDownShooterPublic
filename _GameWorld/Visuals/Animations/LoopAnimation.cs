public class LoopAnimation : AnimationBase
{
    private void OnEnable()
    {
        PlayAnimation();
        AnimationEnded += () => PlayAnimation();
    }
}
