using System.Collections.Generic;
using UnityEngine;

public partial class ConveyorbeltArrow : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] List<Transform> posSetupArrowEditor;
    [ContextMenu("Setup Arrow")]
    void SetupArrowEditor()
    {
        List<Vector3> pos = new List<Vector3>();

        foreach (var t in posSetupArrowEditor)
        {
            pos.Add(new Vector3(t.position.x, t.position.z, 0));
        }

        pos = CurveInterpolationEditor(pos);

        lineRenderer.positionCount = pos.Count;
        lineRenderer.SetPositions(pos.ToArray());
    }

    private List<Vector3> CurveInterpolationEditor(List<Vector3> path)
    {
        var result = new List<Vector3> { path[0] };
        for (var i = 0; i < path.Count - 2; i++)
        {
            result.AddRange(CurveInterpolationEditor(path[i], path[i + 1], path[i + 2]));
        }
        result.Add(path[^1]);
        return result;
    }

    private static List<Vector3> CurveInterpolationEditor(Vector3 pointA, Vector3 pointC, Vector3 pointB)
    {
        var nSteps = 1;
        var offset = 0.4f;

        var vCA = (pointA - pointC).normalized;
        var vCB = (pointB - pointC).normalized;
        pointA = pointC + vCA * offset;
        pointB = pointC + vCB * offset;

        var centerAB = (pointA + pointB) / 2;
        var vCO = (centerAB - pointC).normalized;
        var angleACB = Mathf.Acos(Vector3.Dot(vCA, vCB));
        var angleAOB = Mathf.PI - angleACB;
        var angleOCB = angleACB / 2;
        var lengthCO = offset / Mathf.Cos(angleOCB);
        var pointO = pointC + lengthCO * vCO;
        var r = lengthCO * Mathf.Sin(angleOCB);

        var result = new List<Vector3> { pointA };

        var vOA = (pointA - pointO).normalized;
        for (var i = 0; i < nSteps; i++)
        {
            var angle = (i + 1) * (angleAOB / (nSteps + 1));
            var distOH = Mathf.Cos(angle) * r;
            var pointH = pointO + vOA * distOH;
            var distTH = Mathf.Sin(angle) * r;
            var pointT = pointH - vCA * distTH;
            result.Add(pointT);
        }

        result.Add(pointB);

        return result;
    }
#endif
}
