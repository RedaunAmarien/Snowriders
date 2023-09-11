using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu]
public class Character : ScriptableObject
{
    [FormerlySerializedAs("name")]
    public string characterName;
    public string characterFullName;
    public int characterIndex;
    public string creator;
    //public string updateTimeStamp;
    public int speed;
    public int turn;
    public int jump;
    //public int special;
    //public int skinCol;
    //public int head;
    //public int hairStyle;
    //public int hairCol;
    //public int top;
    //public int bottom;
    //public int shoe;
    public bool isCustom;
    public bool isSecret;
    public Sprite charSprite;
    //public Color charColor;
}
