using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class HubRacePrep : MonoBehaviour
{
    [SerializeField] bool isActive;
    bool activatedThisFrame;
    bool navHasReset;
    HubUIBridge uiBridge;
    public int[] playerOptionA = new int[4];
    public int[] playerOptionB = new int[4];
    public int courseOption;
    public enum PlayerState { Inactive, ChoosingChar, ChoosingBoard, Ready, ChoosingCourse };
    public PlayerState[] playerState = new PlayerState[4];
    List<Course> allCourses;
    List<Course> openCourses = new();
    List<Challenge> challengeList;

    public void Activate(GameMode mode)
    {
        isActive = true;
        navHasReset = true;
        activatedThisFrame = true;
        uiBridge = GetComponent<HubUIBridge>();
        for (int i = 1; i < 4; i++)
        {
            uiBridge.DeactivatePlayer(i);
            uiBridge.DeactivateBoard(i);
            playerState[i] = PlayerState.Inactive;
        }
        uiBridge.RevealWindow(HubUIBridge.WindowSet.RacePrep);
        GameRam.gameMode = mode;
        GameRam.allCharacters = Resources.LoadAll<Character>("Objects/Characters").ToList();

        GameRam.allBoards.Clear();
        GameRam.allBoards = Resources.LoadAll<Board>("Objects/Boards/Basic").ToList();
        GameRam.allBoards.AddRange(Resources.LoadAll<Board>("Objects/Boards/Special"));
        GameRam.allBoards = GameRam.allBoards.OrderBy(x => x.boardID).ToList();

        GameRam.ownedBoards.Clear();
        foreach (int id in GameRam.currentSaveFile.ownedBoardID)
        {
            GameRam.ownedBoards.Add(GameRam.allBoards[id]);
        }
        GameRam.ownedBoards = GameRam.ownedBoards.OrderBy(x => x.shopIndex).ToList();

        allCourses = Resources.LoadAll<Course>("Objects/Courses").ToList();
        allCourses = allCourses.OrderBy(x => x.courseIndex).ToList();
        challengeList = Resources.LoadAll<Challenge>("Objects/Challenges").ToList();

        openCourses.Clear();
        switch (GameRam.gameMode)
        {
            case GameMode.Online:
            case GameMode.Battle:
                foreach (Course course in allCourses)
                {
                    if (!course.hiddenInBattleMode)
                        openCourses.Add(course);
                }
                break;
            case GameMode.Story:
                foreach (Course course in allCourses)
                {
                    if (!course.hiddenInStoryMode)
                        openCourses.Add(course);
                }
                break;
            case GameMode.Challenge:
                foreach (Challenge challenge in challengeList)
                {
                    openCourses.Add(challenge.challengeCourse);
                }
                break;
        }

        GameRam.charForP = new int[4];
        GameRam.boardForP = new int[4];
        //GameRam.inputUser = new InputUser[4];
        //GameRam.inputDevice = new InputDevice[4];
        GameRam.playerCount = 0;
    }

    void OnSubmitCustom(int playerIndex)
    {
        if (!isActive || (GameRam.gameMode != GameMode.Battle && playerIndex != 0))
            return;
        if (activatedThisFrame)
        {
            activatedThisFrame = false;
            return;
        }

        switch (playerState[playerIndex])
        {
            case PlayerState.Inactive:
                uiBridge.UpdatePlayer(playerIndex);
                GameRam.playerCount++;
                playerState[playerIndex] = PlayerState.ChoosingChar;
                break;
            case PlayerState.ChoosingChar:
                uiBridge.UpdateBoard(playerIndex);
                playerState[playerIndex] = PlayerState.ChoosingBoard;
                break;
            case PlayerState.ChoosingBoard:
                playerState[playerIndex] = PlayerState.Ready;
                break;
            case PlayerState.Ready:
                ReadyCheck();
                break;
            case PlayerState.ChoosingCourse:
                if (playerIndex == 0)
                {
                    LoadCourse();
                }
                break;
        }
    }

    void OnCancelCustom(int playerIndex)
    {
        if (!isActive)
            return;

        switch (playerState[playerIndex])
        {
            case PlayerState.ChoosingChar:
                if (playerIndex == 0)
                {
                    GetComponent<HubTownControls>().Reactivate();
                    uiBridge.HideWindow(HubUIBridge.WindowSet.RacePrep);
                    isActive = false;
                }
                else
                {
                    playerState[playerIndex] = PlayerState.Inactive;
                    GameRam.playerCount--;
                    uiBridge.DeactivatePlayer(playerIndex);
                }
                break;
            case PlayerState.ChoosingBoard:
                playerState[playerIndex] = PlayerState.ChoosingChar;
                uiBridge.DeactivateBoard(playerIndex);
                uiBridge.UpdatePlayer(playerIndex);
                break;
            case PlayerState.Ready:
                playerState[playerIndex] = PlayerState.ChoosingBoard;
                break;
            case PlayerState.ChoosingCourse:
                playerState[playerIndex] = PlayerState.ChoosingBoard;
                uiBridge.HideWindow(HubUIBridge.WindowSet.CourseSelect);
                uiBridge.RevealWindow(HubUIBridge.WindowSet.RacePrep);
                break;
        }
    }

    void OnNavigateCustom(HubMultiplayerInput.Parameters input)
    {
        if (!isActive)
            return;

        float v = input.val.Get<Vector2>().x;
        if (v < 0.5f && v > -0.5f)
        {
            navHasReset = true;
            return;
        }

        if (!navHasReset)
            return;

        switch (playerState[input.index])
        {
            case PlayerState.ChoosingChar:
                if (v > 0.5f)
                {
                    playerOptionA[input.index] += 1;
                    if (playerOptionA[input.index] >= GameRam.allCharacters.Count)
                    {
                        playerOptionA[input.index] = 0;
                    }
                    navHasReset = false;
                }
                else if (v < -0.5f)
                {
                    playerOptionA[input.index] -= 1;
                    if (playerOptionA[input.index] <= -1)
                    {
                        playerOptionA[input.index] = GameRam.allCharacters.Count - 1;
                    }
                    navHasReset = false;
                }
                GameRam.charForP[input.index] = playerOptionA[input.index];
                uiBridge.UpdatePlayer(input.index);
                break;
            case PlayerState.ChoosingBoard:
                if (v > 0.5f)
                {
                    playerOptionB[input.index] += 1;
                    if (playerOptionB[input.index] >= GameRam.ownedBoards.Count)
                    {
                        playerOptionB[input.index] = 0;
                    }
                    navHasReset = false;
                }
                else if (v < -0.5f)
                {
                    playerOptionB[input.index] -= 1;
                    if (playerOptionB[input.index] <= -1)
                    {
                        playerOptionB[input.index] = GameRam.ownedBoards.Count - 1;
                    }
                    navHasReset = false;
                }
                GameRam.boardForP[input.index] = playerOptionB[input.index];
                uiBridge.UpdateBoard(input.index);
                break;
            case PlayerState.ChoosingCourse:
                if (GameRam.gameMode == GameMode.Challenge)
                {
                    if (v > 0.5f)
                    {
                        courseOption += 1;
                        if (courseOption >= challengeList.Count)
                        {
                            courseOption = 0;
                        }
                        navHasReset = false;
                    }
                    else if (v < -0.5f)
                    {
                        courseOption -= 1;
                        if (courseOption <= -1)
                        {
                            courseOption = challengeList.Count - 1;
                        }
                        navHasReset = false;
                    }
                    //GameRam.currentChallenge = challengeList[courseOption];
                    //GameRam.courseToLoad = challengeList[courseOption].challengeCourse.courseSceneName;
                    uiBridge.UpdateChallengeSelect(challengeList[courseOption]);
                }
                else
                {
                    if (v > 0.5f)
                    {
                        courseOption += 1;
                        if (courseOption >= openCourses.Count)
                        {
                            courseOption = 0;
                        }
                        navHasReset = false;
                    }
                    else if (v < -0.5f)
                    {
                        courseOption -= 1;
                        if (courseOption <= -1)
                        {
                            courseOption = openCourses.Count - 1;
                        }
                        navHasReset = false;
                    }
                    //GameRam.courseToLoad = openCourses[courseOption].courseSceneName;
                    uiBridge.UpdateCourseSelect(openCourses[courseOption]);
                }
                break;
        }
    }

    void ReadyCheck()
    {
        int notReady = 0;
        for (int i = 0; i < 4; i++)
        {
            if (playerState[i] == PlayerState.ChoosingBoard || playerState[i] == PlayerState.ChoosingChar)
                notReady++;
        }

        if (notReady > 0)
        {
            Debug.Log(notReady + " players not ready.");
        }
        else
        {
            if (GameRam.gameMode == GameMode.Challenge)
                uiBridge.UpdateChallengeSelect(challengeList[courseOption]);
            else
                uiBridge.UpdateCourseSelect(openCourses[courseOption]);

            playerState[0] = PlayerState.ChoosingCourse;
            uiBridge.HideWindow(HubUIBridge.WindowSet.RacePrep);
            uiBridge.RevealWindow(HubUIBridge.WindowSet.CourseSelect);
        }
    }

    void LoadCourse()
    {
        if (GameRam.gameMode == GameMode.Challenge)
        {
            GameRam.currentChallenge = challengeList[courseOption];
            GameRam.courseToLoad = challengeList[courseOption].challengeCourse.courseSceneName;
            StartCoroutine(GetComponent<HubTownControls>().FadeAndLoad("TrackContainer"));
        }
        else if (GameRam.gameMode == GameMode.Story)
        {
            StartCoroutine(GetComponent<HubTownControls>().FadeAndLoad(openCourses[courseOption].courseCutsceneName));
        }
        else
        {
            GameRam.courseToLoad = openCourses[courseOption].courseSceneName;
            StartCoroutine(GetComponent<HubTownControls>().FadeAndLoad("TrackContainer"));
        }
    }
}
