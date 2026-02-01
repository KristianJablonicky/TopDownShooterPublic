using System;
using UnityEngine;

[Serializable]
public class AnimationData
{
    [SerializeField] private Sprite[] frames;
    public (Sprite, Sprite) GetFrames(int index) => (frames[index * 2], frames[index * 2 + 1]);
    public int FrameCount => frames.Length / 2;
}

[Serializable]
public class AnimationDataWithDuration : AnimationData
{
    [field: SerializeField] public float Duration { get; private set; }
}
