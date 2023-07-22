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
    public int characterIndex;
    public string creator, updateTimeStamp;
    public int speed, turn, jump, special, skinCol, head, hairStyle, hairCol, top, bottom, shoe;
    public bool custom;
    public Sprite charSprite;
    public Color charColor;
}
