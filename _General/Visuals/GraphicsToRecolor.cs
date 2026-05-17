using UnityEngine;
using UnityEngine.UI;

public class GraphicsToRecolor : MonoBehaviour
{
    [SerializeField] private Graphic[] graphicsToRecolor;

    public void Recolor(Color color)
    {
        foreach (Graphic graphic in graphicsToRecolor)
        {
            if (graphic is JaggedShapeUI jaggedShape)
            {
                jaggedShape.SetColor(color);
                continue;
            }

            graphic.color = color;
        }
    }
}
