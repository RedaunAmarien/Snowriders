using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIPathManager))]
public class AIPathEditor : Editor
{
    AIPathManager manager;
    void OnEnable()
    {
        manager = target as AIPathManager;
    }

    void OnSceneGUI()
    {
        Draw();
    }

    void Draw()
    {
        for (int i = 0; i < manager.waypoints.Length; i++)
        {
            Vector3 newPos = Handles.FreeMoveHandle(manager.waypoints[i].position, 1, Vector3.zero, Handles.DotHandleCap);
            if (manager.waypoints[i].position != newPos)
            {
                Undo.RecordObject(manager, "Move Waypoint");
                manager.waypoints[i].position = newPos;
            }
        }
        for (int i = 0; i < manager.waypoints.Length -1; i++)
        {
            Handles.DrawLine(manager.waypoints[i].position, manager.waypoints[i + 1].position);
        }
    }
}
