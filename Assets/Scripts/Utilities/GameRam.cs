﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public static class GameRam {
	public static int gameMode;
	public static int playerCount, lapCount;
	public static float sfxVol, musicVol;
	public static bool itemsOn, coinsOn;
	public static int[] controlp, charForP, boardForP;
	public static List<CharacterData> charDataCustom = new List<CharacterData>(), charDataPermanent = new List<CharacterData>(), allCharData = new List<CharacterData>();
	public static List<BoardData> boardData = new List<BoardData>(), ownedBoardData = new List<BoardData>();
	public static SaveData currentSaveFile;
	public static string currentSaveDirectory;
	public static string nextSceneToLoad, courseToLoad;
	public enum SceneType {Menu, Race, NewCutscene};
	public static SceneType nextSceneType;
	public static InputDevice[] inpDev;
	public static InputUser[] inpUse;
}
