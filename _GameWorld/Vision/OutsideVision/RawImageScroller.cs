using UnityEngine;
using UnityEngine.UI;

public class RawImageScroller : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private float speedX;

    private void Update()
    {
        rawImage.uvRect = new(
            rawImage.uvRect.position
            + new Vector2(speedX * Time.deltaTime, 0),
            rawImage.uvRect.size);
    }

}
