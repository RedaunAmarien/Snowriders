using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public static class GameVar
{
	public static int playerCount, lapCount, saveSlot;
	public static float sfxVol, musicVol;
	public static bool itemsOn, coinsOn;
	public static int gameMode;
	public static int[] controlp, charForP, boardForP;
	public static CharacterData[] charDataCustom, charDataPermanent, allCharData;
	public static BoardData[] boardData;
	public static SaveData currentSaveFile;
	public static string currentSaveDirectory, nextSceneToLoad;
	public static InputDevice[] inpDev;
	public static InputUser[] inpUse;
}
