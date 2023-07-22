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
    public static float masterVolume, sfxVol, musicVol;
    public static bool itemsOn, coinsOn;
    public static int[] controlp, charForP, boardForP;
    public static List<Character> customCharacters = new(), defaultCharacters = new(), allCharacters = new();
    public static List<Board> allBoards = new(), ownedBoards = new();
    public static SaveData currentSaveFile;
    public static int currentSaveSlot;
    public static string currentSaveDirectory;
    public static string nextSceneToLoad, courseToLoad;
    public static SceneType nextSceneType;
    public static InputDevice[] inputDevice;
    public static InputUser[] inputUser;
    public static int currentChallengeIndex;
    public static Challenge currentChallenge;
    public static int lastHubSelection;
}
