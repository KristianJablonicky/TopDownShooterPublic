using UnityEngine;

public class RotationController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D characterRB;
    private Vector2 cursorPosition;
    public void SetCursorPosition(Vector2 position) => cursorPosition = position;

    private void FixedUpdate()
    {
        SetRotation(cursorPosition - characterRB.position);
    }

    public void LookAt(Vector2 target)
    {
        SetRotation(target - characterRB.position);
    }

    private void SetRotation(Vector2 direction)
        => characterRB.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    public void SetRotation(float rotation) => characterRB.rotation = rotation - 90f;
    public float GetRotationAngle => characterRB.rotation + 90f;
}
