//using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class AIPathManager : MonoBehaviour
{
    [SerializeField] SplineContainer container;
    public AIWaypoint[] waypoints;

    private void OnValidate()
    {
        if (waypoints.Length > 0 || container == null)
            return;

        if (container.Spline != null)
        {
            waypoints = new AIWaypoint[container.Spline.Count()];
            for (int i = 0; i < container.Spline.Count(); i++)
            {
                BezierKnot current = container.Spline[i];
                waypoints[i].Position = new(current.Position.x, current.Position.y, current.Position.z);
                waypoints[i].Radius = 5;
            }
        }
    }
}
