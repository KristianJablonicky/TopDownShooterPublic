using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAnimationPlayer : SingletonMonoBehaviour<MapAnimationPlayer>
{
    [SerializeField] private float delayBetweenAnimations = 1f;
    [SerializeField] private int animationsPerCycle = 3;
    private List<VisualAnimation> animations = new();

    public static void RegisterAnimation(VisualAnimation animation)
    {
        Instance.animations.Add(animation);
    }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBetweenAnimations);
            for (var i = 0; i < animationsPerCycle; i++)
            {
                var index = Random.Range(0, animations.Count);
                var animation = animations[index];
                animation.PlayAnimation();
            }
        }
    }

}
