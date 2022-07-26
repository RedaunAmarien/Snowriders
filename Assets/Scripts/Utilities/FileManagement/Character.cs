using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Character : ScriptableObject
{
    public new string name;
    public string creator, updateTimeStamp;
    public int charSpriteIndex, speed, turn, jump, special, skinCol, head, hairStyle, hairCol, top, bottom, shoe;
    public bool custom;
}
