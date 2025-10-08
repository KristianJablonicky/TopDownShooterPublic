using UnityEngine;

/// <summary>
/// Mesh part is mostly taken from
/// https://www.youtube.com/watch?v=CSeUMTaNFYk
/// </summary>
public class PlayerVision : MonoBehaviour, IResettable
{
    [Header("Vision settings")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float guaranteedVisionRangeMultiplier = 0.75f;
    [SerializeField] private float frontalFov = 45f;

    [Header("References")]
    [SerializeField] private Transform playerPosition;
    [SerializeField] private VisionLight visionLight;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private CharacterMediator mediator;

    [SerializeField] private GameObject guaranteedTeamMateVision;

    [Header("Misc")]
    [SerializeField] private int rayCount = 10;
    [SerializeField] private LayerMask onlyBlockingLayer;
    [SerializeField] private LayerMask teamMateMask;


    private Mesh mesh;
    private Transform playerTransform;
    private Vector3 origin;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    private float angle, angleIncrease;
    private float baseVisionRange;
    public void GetEnabled(bool teamMate)
    {
        if (teamMate)
        {
            gameObject.SetActive(true);

            gameObject.layer = Mathf.RoundToInt(Mathf.Log(teamMateMask.value, 2));

            SwitchLights(false);
            Debug.Log("TeamMate vision switched on");

            // TODO: reconsider
            // guaranteedTeamMateVision.SetActive(true);
        }
        else
        {
            SwitchLights(true);
        }
    }

    public void SwitchLights(bool enable) => visionLight.ChangeLightState(enable);


    private void Start()
    {
        playerTransform = mediator.MovementController.transform;

        visionLight.UpdateVision(visionRange, visionRange * guaranteedVisionRangeMultiplier, frontalFov);

        mesh = new();
        meshFilter.mesh = mesh;

        vertices = new Vector3[rayCount + 2];
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];

        angleIncrease = 360f / rayCount;

        baseVisionRange = visionRange;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) { return; }

        origin = playerTransform.position;

        vertices[0] = origin;

        // Character facing direction (in degrees)
        var facingAngle = mediator.RotationController.GetRotationAngle;

        // Start angle at left edge of FOV cone
        angle = facingAngle + frontalFov * 0.5f;

        var vertexIndex = 1;
        var trianglesIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            // How far from the center facing angle this ray is
            var angleDiff = Mathf.DeltaAngle(facingAngle, angle);

            // Decide vision length depending on FOV
            var range = Mathf.Abs(angleDiff) <= frontalFov * 0.5f
                ? visionRange
                : visionRange * guaranteedVisionRangeMultiplier;

            var dir = GetVectorFromAngle(angle);
            var raycastHit = Physics2D.Raycast(origin, dir, range, onlyBlockingLayer);

            Vector3 vertex;
            if (raycastHit.collider == null)
            {
                vertex = origin + dir * range;
            }
            else
            {
                vertex = raycastHit.point;
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[trianglesIndex] = 0;
                triangles[trianglesIndex + 1] = vertexIndex - 1;
                triangles[trianglesIndex + 2] = vertexIndex;
                trianglesIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);
    }
    private Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public void Reset()
    {
        // reset before the Start() method executed leads to bugs
        if (baseVisionRange == 0) return;

        SetVisionRange(baseVisionRange);
    }

    public void SetVisionRangeProportional(float newVisionPercentage)
    {
        SetVisionRange(visionRange * newVisionPercentage);
    }
    public void SetVisionRange(float newVisionRange)
    {
        if (newVisionRange == visionRange) return;
        Tweener.Tween(this, visionRange, newVisionRange, 0.25f, TweenStyle.quadratic,
            value => {
                visionRange = value;
                visionLight.UpdateVision(value, guaranteedVisionRangeMultiplier * value);
            }
        );
    }
}
