using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public enum GameMode { Battle, Story, Challenge, Online };

public static class GameRam
{
    public static GameMode gameMode;
    public static int playerCount;
    public static int maxPlayerCount;
    public static int lapCount;
    public static float masterVolume;
    public static float sfxVol;
    public static float musicVol;
    public static bool itemsOn = true;
    public static bool coinsOn = true;
    public static int[] controlp;
    public static int[] charForP;
    public static int[] boardForP;
    public static List<Character> customCharacters = new();
    public static List<Character> defaultCharacters = new();
    public static List<Character> allCharacters = new();
    public static List<Board> allBoards = new();
    public static List<Board> ownedBoards = new();
    public static SaveData currentSaveFile;
    public static int currentSaveSlot;
    public static string currentSaveDirectory;
    public static string nextSceneToLoad;
    public static string courseToLoad;
    public static SceneType nextSceneType;
    public static InputDevice[] inputDevice;
    public static InputUser[] inputUser;
    public static int currentChallengeIndex;
    public static Challenge currentChallenge;
    public static int lastHubSelection;
}
