using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Course : ScriptableObject
{
    public string courseName;
    public string courseSceneName;
    public string courseCutsceneName;
    public int courseIndex;
    public int defaultLapCount = 3;
    public int courseLength;
    [Tooltip("Default enemy racer characters.\nElement 3 will only be used if the player chooses an already-taken slot.")]
    public Character[] defaultCpu = new Character[4];
    [Tooltip("Default enemy racer boards.")]
    public Board[] defaultCpuBoard = new Board[4];
    [Tooltip("Prizes for placing. All human characters' prize money will be added to save file.")]
    public int[] prizeMoney = new int[4];
    public Sprite minimap;
    public Texture2D preview;
}
