using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIWaypoint
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 size;
    public bool tryJump;
    public bool tryGrab;
    public int tricksPossible;
    public float targetableRadius = 4;
    public float targetRadius = 0.5f;
}
