using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class SliceData
{
    public int splineIndex;
    public SplineRange range;

    public bool isEnabled = true;
    public float sliceLength;
    public float distanceFromStart;
}

[System.Serializable]
public class SplinePathData
{
    public List<SliceData> slices;
    public SplinePath path;
    public float totalLength;
}

