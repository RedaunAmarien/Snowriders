using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioSource), true)]
[CanEditMultipleObjects]
public class AudioOverride : Editor
{
    bool useMaster = true;
    bool isSFX = false;
    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Use Master");
        GUILayout.Toggle(useMaster, "");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Sound Effect");
        GUILayout.Toggle(isSFX, "");
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        DrawDefaultInspector();
    }
}
