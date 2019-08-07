using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameVar
{
	public static int playerCount, lapCount, saveSlot;
	public static float sfxVol, musicVol;
	public static bool itemsOn, coinsOn;
	public static int gameMode;
	public static int[] controlp, charForP, boardForP;
	public static CharacterData[] charDataCustom, charDataPermanent, allCharData;
	public static BoardData[] boardData;
	public static SaveFileData currentSaveFile;
	public static string currentSaveDirectory, nextSceneToLoad;
}
