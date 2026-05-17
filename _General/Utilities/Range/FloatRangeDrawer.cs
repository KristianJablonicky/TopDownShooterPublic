using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FloatRange))]
public class FloatRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, label);

        float half = position.width / 2f;

        var minProp = property.FindPropertyRelative("start");
        var maxProp = property.FindPropertyRelative("end");

        Rect minRect = new(position.x, position.y, half - 2, position.height);
        Rect maxRect = new(position.x + half + 2, position.y, half - 2, position.height);

        EditorGUI.PropertyField(minRect, minProp, GUIContent.none);
        EditorGUI.PropertyField(maxRect, maxProp, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
#endif

[System.Serializable]
public struct FloatRange
{
    public float start;
    public float end;
    
    public readonly float GetValue(
        float pointInRange,
        Ordering order = Ordering.none,
        TweenStyle style = TweenStyle.linear)
    {
        var s = start;
        var e = end;
        if (order == Ordering.lowToHigh)
        {
            s = Mathf.Min(start, end);
            e = Mathf.Max(start, end);
        }
        else if (order == Ordering.highToLow)
        {
            s = Mathf.Max(end, start);
            e = Mathf.Min(end, start);
        }
        else if (order == Ordering.reversed)
        {
            s = end;
            e = start;
        }
        return Tweener.GetValue(s, e, pointInRange, style);
    }

    public float GetValueAtIndex(int index)
    {
        if (index == 0) return start;
        else if (index == 1) return end;

        Debug.LogWarning($"Index {index} is out of range!");
        return 0f;
    }

    private FloatRange(float value)
    {
        start = value;
        end = value;
    }

    private FloatRange(float start, float end)
    {
        this.start = start;
        this.end = end;
    }

    public static implicit operator FloatRange(float value)
        => new(value);

    public static implicit operator FloatRange((float, float) range)
        => new(range.Item1, range.Item2);
}

public enum Ordering
{
    none,
    reversed,
    lowToHigh,
    highToLow,
}