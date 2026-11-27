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
        Handles.color = Color.yellow;
        for (int i = 0; i < manager.waypoints.Length; i++)
        {
            Vector3 newPos = Handles.FreeMoveHandle(manager.waypoints[i].Position, 1, Vector3.zero, Handles.DotHandleCap);
            if (manager.waypoints[i].Position != newPos)
            {
                Undo.RecordObject(manager, "Move Waypoint");
                manager.waypoints[i].Position = newPos;
            }
        }

        Handles.color = Color.blue;
        for (int i = 0; i < manager.waypoints.Length; i++)
        {
            float newRad = Handles.RadiusHandle(Quaternion.identity, manager.waypoints[i].Position, manager.waypoints[i].Radius);
            if (manager.waypoints[i].Radius != newRad)
            {
                Undo.RecordObject(manager, "Change Radius");
                manager.waypoints[i].Radius = newRad;
            }
        }

        Handles.color = Color.green;
        for (int i = 0; i < manager.waypoints.Length - 1; i++)
        {
            Handles.DrawLine(manager.waypoints[i].Position, manager.waypoints[i + 1].Position);
        }
    }
}
