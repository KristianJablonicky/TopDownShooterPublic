using UnityEngine;

public class RotationController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D characterRB;
    private Vector2 cursorPosition;
    public void SetCursorPosition(Vector2 position) => cursorPosition = position;

    private void FixedUpdate()
    {
        var direction = cursorPosition - characterRB.position;
        characterRB.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    }

    public float GetRotationAngle => characterRB.rotation + 90f;
}
