using UnityEngine;
using UnityEngine.Serialization;

public class JaggedPolygonUI : JaggedShapeUI
{
    [Header("Randomness")]
    [SerializeField, FormerlySerializedAs("randomness")] protected ShapeRandomness randomness = ShapeRandomness.Random;
    [SerializeField, Range(0f, 1f), FormerlySerializedAs("PointRandomness")] protected float pointRandomness = 0.25f;
    [SerializeField, FormerlySerializedAs("sizeRandomness")] protected SizeRandomness sizeRandomness = SizeRandomness.Shrink;
    [SerializeField, Range(0f, 0.5f), FormerlySerializedAs("roundedCorners")] protected float roundedCorners = 0.05f;

    [SerializeField, Range(1f, 2f)] private float imageSafetyScale = 1.2f;
    private System.Random random;
    private int? seed;

    protected override void Awake()
    {
        base.Awake();
        if (imageTexture != null)
        {
            if (sizeRandomness == SizeRandomness.Shrink)
            {
                // set the masked image to match polygon boundaries
                // when size randomness shrinks the polygon
                imageTexture.rectTransform.anchorMin = Vector2.zero;
                imageTexture.rectTransform.anchorMax = Vector2.one;
            }
            else
            {
                imageTexture.rectTransform.anchorMin = Vector2.one * (1f - imageSafetyScale);
                imageTexture.rectTransform.anchorMax = Vector2.one * imageSafetyScale;
            }
        }
    }
    protected override void BeforeMeshPopulation()
    {
        random = new(GetSeed());

        rect = rectTransform.rect;
        ResetPoints();

        if (pointRandomness > 0f)
        {
            MakePointsJagged();
        }

        if (roundedCorners > 0f)
        {
            MakeCornersRounded();
        }
    }

    private void MakePointsJagged()
    {
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            var randomOffset = new Vector2(
                RandomValueSize * pointRandomness,
                RandomValueSize * pointRandomness
            );
            randomOffset += Vector2.one;
            points[i] = p * randomOffset;
        }
    }

    private void MakeCornersRounded()
    {
        var count = points.Length;
        var result = new Vector2[count * 2];

        for (int i = 0; i < count; i++)
        {
            var prev = points[(i - 1 + count) % count];
            var curr = points[i];
            var next = points[(i + 1) % count];

            var dirToPrev = prev - curr;
            var dirToNext = next - curr;

            var lenPrev = dirToPrev.magnitude;
            var lenNext = dirToNext.magnitude;

            var offsetToPrev = lenPrev > 0f
                ? dirToPrev.normalized * (lenPrev * roundedCorners)
                : Vector2.zero;

            var offsetToNext = lenNext > 0f
                ? dirToNext.normalized * (lenNext * roundedCorners)
                : Vector2.zero;

            result[i * 2] = curr + offsetToPrev;
            result[i * 2 + 1] = curr + offsetToNext;
        }

        points = result;
    }

    private const int RandomScale = 1000;
    /// <summary>
    /// Return a random value between 0 and 1, or between -1 and 1 if canBeSquished is true.
    /// </summary>
    private float RandomValueSize
    {
        get
        {
            int floor = -RandomScale, ceiling = RandomScale;
            if (sizeRandomness == SizeRandomness.Shrink) ceiling = 0;
            if (sizeRandomness == SizeRandomness.Expand) floor = 0;
            return (float)random.Next(floor, ceiling) / RandomScale;
        }
    }

    /// <summary>
    /// Return a random value between 0 and 1.
    /// </summary>
    private float RandomValue
    {
        get
        {
            return (float)random.Next(0, RandomScale) / RandomScale;
        }
    }
    private int GetSeed()
    {
        seed ??= randomness switch
        {
            ShapeRandomness.Random => (int)Random.Range(1f, 100f),
            ShapeRandomness.PerPosition => (int)(transform.position.x + transform.position.y),
            ShapeRandomness.PerPrefab => (gameObject.name.GetHashCode()),
            ShapeRandomness.None => 0,
            _ => new()
        };
        return seed.Value;
    }

    public void RenderAgain(int seed)
    {
        this.seed = seed;
        SetVerticesDirty();
    }


    public (ShapeRandomness, float, SizeRandomness, float) GetSettings()
    {
        return (randomness, pointRandomness, sizeRandomness, roundedCorners);
    }
}

public enum ShapeRandomness
{
    Random,
    PerPosition,
    PerPrefab,
    None
}

public enum SizeRandomness
{
    Expand,
    Shrink,
    Deform
}