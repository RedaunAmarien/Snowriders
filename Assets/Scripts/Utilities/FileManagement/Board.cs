using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Board : ScriptableObject
{
	public new string name;
    public string description;
	public int boardID, shopIndex, level, speed, turn, jump;
	public ItemCost boardCost;
	public string specialAbility;
}
