using UnityEngine;

public class MoveToPosition : MonoBehaviour
{
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float duration = 1f;
    [SerializeField] private TweenStyle tweenStyle = TweenStyle.quadratic;
    [SerializeField] private bool relative = true;
    [SerializeField] private bool randomize = false;
    [SerializeField] private bool preserveRandomRadius = false;
    [SerializeField] private bool setRotation = false;
    private bool alreadyStarted = false;
    public void SetTargetPosition(Vector2 targetPosition, bool random)
    {
        this.targetPosition = targetPosition;
        randomize = random;

        if (alreadyStarted)
        {
            Start();
        }
    }

    private void Start()
    {
        alreadyStarted = true;
        if (randomize)
        {
            if (!preserveRandomRadius)
            {
                targetPosition = new Vector2(
                    Random.Range(-targetPosition.x, targetPosition.x),
                    Random.Range(-targetPosition.y, targetPosition.y)
                );
            }
            else
            {
                // circle of average radius
                var angle = Random.value * Mathf.PI * 2f;
                var point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                targetPosition = point * ((targetPosition.x + targetPosition.y) / 2f);
            }
        }
        
        if (relative)
        {
            targetPosition += (Vector2)transform.position;
        }
        
        if (setRotation)
        {
            var angle = Mathf.Atan2(targetPosition.y - transform.position.y, targetPosition.x - transform.position.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        Tweener.TweenPosition(gameObject, transform.position, targetPosition,
            duration, tweenStyle);
    }
}
