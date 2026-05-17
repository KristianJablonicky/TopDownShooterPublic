using UnityEngine;

public class MenuTransformAnimation : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;

    private Vector2 startPosition, startScale;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        startScale = transform.localScale;
       
        var manager = HeroSelectionManager.Instance;
        manager.AnimationProgressed += OnProgress;
    }

    private void OnProgress(float progress)
    {
        transform.Lerp(startPosition, startScale, startRotation, targetTransform, progress);
        /*
        transform.localPosition = Vector2.Lerp(startPosition, targetTransform.localPosition, progress);
        transform.localScale = Vector2.Lerp(startScale, targetTransform.localScale, progress);
        //transform.rotation = Quaternion.Lerp(currentStart.rotation, currentTarget.rotation, progress);
        var rot = transform.rotation;
        rot.z = Mathf.Lerp(startRotation.z, targetTransform.rotation.z, progress);
        transform.rotation = rot;
        */
    }
}
