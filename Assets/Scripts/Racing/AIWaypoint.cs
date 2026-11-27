using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AIWaypoint
{
    [field:SerializeField]
    public Vector3 Position { get; set; }

    [field:SerializeField]
    public float Radius { get; set; }
}

[System.Serializable]
public struct CheckSection
{
    [field:SerializeField]
    public Vector3 Position { get; set; }

    [field: SerializeField]
    public Vector2 Size { get; set; }

    [field:SerializeField]
    public Vector3 Rotation { get; set; }

    public CheckSection[] connections;
}
