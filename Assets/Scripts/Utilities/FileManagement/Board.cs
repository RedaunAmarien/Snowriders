using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu]
public class Board : ScriptableObject
{
	public string boardName;
    public string description;
	public int boardID, shopIndex, level, speed, turn, jump;
	public ItemCost boardCost;
	public string specialAbility;
}
