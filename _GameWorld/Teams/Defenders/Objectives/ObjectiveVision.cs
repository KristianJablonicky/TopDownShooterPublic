using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectiveVision : VisionMesh
{
    [SerializeField] private Light2D light2D;
    [SerializeField] private Transform objectiveTransform;

    protected override void VirtualStart()
    {
        UpdateMesh(objectiveTransform.position, null);
        light2D.pointLightInnerRadius = visionRange - 1;
        light2D.pointLightOuterRadius = visionRange;
        light2D.transform.SetParent(objectiveTransform);
    }

}
