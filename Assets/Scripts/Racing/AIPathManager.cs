//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

public class AIPathManager : MonoBehaviour
{
    SplineContainer container;
    [FormerlySerializedAs("routeDatas")]
    public List<SplinePathData> routes;
    public AIWaypoint[] waypoints;

    private void Start()
    {
        container = GetComponent<SplineContainer>();

        for (int i = 0; i < routes.Count; i++)
        {
            var enabledSlices = routes[i].slices.Where(slice => slice.isEnabled).ToList();
            var slices = new List<SplineSlice<Spline>>();
            routes[i].totalLength = 0f;

            foreach (SliceData sliceData in enabledSlices)
            {
                Spline spline = container.Splines[sliceData.splineIndex];
                var slice = new SplineSlice<Spline>(spline, sliceData.range);
                slices.Add(slice);

                sliceData.distanceFromStart = routes[i].totalLength;
                sliceData.sliceLength = slice.GetLength();
                routes[i].totalLength += sliceData.sliceLength;
            }

            routes[i].path = new SplinePath(slices);
            Debug.LogFormat("Path {0} is length {1}", i, routes[i].path.GetLength());
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Debug.DrawLine(waypoints[i].position, waypoints[i + 1].position, Color.green);
        }
    }
}
