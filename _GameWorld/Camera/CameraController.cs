using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    protected GameObject followedGO;
    [field: SerializeField] public Camera Camera { get; protected set; }
    [SerializeField] protected float maxDistanceDelta = 10f;
    [SerializeField] protected GameObject renderUIImage;
    [SerializeField] private bool snappyMovement;
    protected Func<(float, float)> updateAction;
    protected const float zOffset = -1f;

    protected virtual void Awake()
    {
        enabled = false;
    }
    public void InitialSetTarget(CharacterMediator mediator, bool teamMate)
    {
        enabled = true;
        renderUIImage.SetActive(true);

        if (teamMate)
        {
            var localPlayer = CharacterManager.Instance.LocalPlayer.Mediator;
            localPlayer.Ascendance.TeamMateAscended += () => { renderUIImage.SetActive(false); };
            localPlayer.Respawned += (_) => { renderUIImage.SetActive(true); };
        }

        if (snappyMovement) updateAction = SnapPosition;
        else updateAction = UpdatePosition;

        followedGO = mediator.MovementController.gameObject;
        mediator.MovementController.PositionChanged += OnPositionChanged;

        //mediator.Disconnected += () => { enabled = false; };
    }

    private void OnPositionChanged(Vector2 newPos)
    {
        transform.position = new(newPos.x, newPos.y, zOffset);
    }
    private (float, float) SnapPosition()
    {
        if (followedGO == null) return (0f, 0f);
        return (followedGO.transform.position.x, followedGO.transform.position.y);
    }

    protected (float, float) UpdatePosition()
    {
        if (followedGO == null) return(0f, 0f);
        var t = transform.position.WithXY(
            Vector2.MoveTowards(transform.position,
                (Vector2)followedGO.transform.position,
                maxDistanceDelta * Time.deltaTime
            )
        );
        return (t.x, t.y);
    }
    private void LateUpdate()
    {
        var (x, y) = updateAction();
        transform.position = new(x, y, zOffset);
    }
}
