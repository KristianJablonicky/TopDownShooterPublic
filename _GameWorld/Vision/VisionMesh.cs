using UnityEngine;

/// <summary>
/// Mesh part is mostly taken from
/// https://www.youtube.com/watch?v=CSeUMTaNFYk
/// </summary>
public class VisionMesh : MonoBehaviour
{
    [SerializeField] protected int rayCount = 200;
    [Header("Vision settings")]
    [SerializeField] protected float visionRange = 10f;
    [SerializeField] protected float guaranteedVisionRangeMultiplier = 0.75f;
    [SerializeField] protected float frontalFov = 45f;

    [Header("Misc")]
    [SerializeField] private LayerMask onlyBlockingLayer;

    private Mesh mesh;
    [SerializeField] private MeshFilter meshFilter;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    private float angleIncrease;

    private float angle;

    private void Start()
    {
        mesh = new();
        meshFilter.mesh = mesh;

        vertices = new Vector3[rayCount + 2];
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];

        angleIncrease = 360f / rayCount;
        VirtualStart();
    }

    protected virtual void VirtualStart() { }

    protected void UpdateMesh(Vector3 origin, float? facingAngle)
    {
        vertices[0] = origin;

        float angleDiff = 0f;
        if (facingAngle.HasValue)
        {
            angle = facingAngle.Value + frontalFov * 0.5f;
        }
        // Start angle at left edge of FOV cone

        var vertexIndex = 1;
        var trianglesIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            // How far from the center facing angle this ray is
            if (facingAngle.HasValue)
            {
                angleDiff = Mathf.DeltaAngle(facingAngle.Value, angle);
            }

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
        mesh.bounds = new(origin, Vector3.one * 1000f);
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        var angleRad = angle * Mathf.Deg2Rad;
        return new(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}
