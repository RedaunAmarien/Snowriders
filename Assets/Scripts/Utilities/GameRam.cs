using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public enum GameMode { Battle, Story, Challenge, Online };

public static class GameRam
{
    public static GameMode gameMode;
    public static int playerCount, maxPlayerCount, lapCount;
    public static float masterVol, sfxVol, musicVol;
    public static bool itemsOn, coinsOn;
    public static int[] controlp, charForP, boardForP;
    public static List<CharacterData> charDataCustom = new List<CharacterData>(), charDataPermanent = new List<CharacterData>(), allCharData = new List<CharacterData>();
    public static List<BoardData> allBoardData = new List<BoardData>(), ownedBoardData = new List<BoardData>();
    public static SaveData currentSaveFile;
    public static string currentSaveDirectory;
    public static string nextSceneToLoad, courseToLoad;
    public static SceneType nextSceneType;
    public static InputDevice[] inpDev;
    public static InputUser[] inpUse;
    public static int currentChallengeIndex;
    public static ChallengeConditions currentChallenge;
    public static int lastHubSelection;
}
