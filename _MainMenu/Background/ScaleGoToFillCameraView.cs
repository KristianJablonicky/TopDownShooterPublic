using UnityEngine;

public class ScaleGoToFillCameraView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    void Start()
    {
        var worldHeight = Camera.main.orthographicSize * 2;
        var worldWidth = worldHeight * Screen.width / Screen.height;

        transform.localScale = new Vector3(
            worldWidth / spriteRenderer.bounds.size.x,
            worldHeight / spriteRenderer.bounds.size.y,
            1
        );
    }

}
