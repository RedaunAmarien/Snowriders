using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class HubRacePrep : MonoBehaviour
{
    [SerializeField] bool isActive;
    bool activatedThisFrame;
    bool navHasReset;
    [SerializeField] GameMode gameMode;
    HubUIBridge uiBridge;
    public int[] playerOptionA = new int[4];
    public int[] playerOptionB = new int[4];
    public int courseOption;
    public enum PlayerState { Inactive, ChoosingChar, ChoosingBoard, Ready };
    public PlayerState[] playerState = new PlayerState[4];
    Course[] courseList;
    bool nowChoosingCourse;

    public void Activate(GameMode mode)
    {
        nowChoosingCourse = false;
        gameMode = mode;
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

        courseList = Resources.LoadAll<Course>("Objects/Courses").ToArray();
        GameRam.charForP = new int[4];
        GameRam.boardForP = new int[4];
    }

    void OnSubmitCustom(int playerIndex)
    {
        if (!isActive)
            return;
        if (activatedThisFrame)
        {
            activatedThisFrame = false;
            return;
        }

        if (playerState[playerIndex] == PlayerState.Inactive)
        {
            uiBridge.UpdatePlayer(playerIndex);
            GameRam.playerCount++;
            playerState[playerIndex] = PlayerState.ChoosingChar;
        }

        else if (playerState[playerIndex] == PlayerState.ChoosingChar)
        {
            uiBridge.UpdateBoard(playerIndex);
            playerState[playerIndex] = PlayerState.ChoosingBoard;
        }

        else if (playerState[playerIndex] == PlayerState.ChoosingBoard)
        {
            playerState[playerIndex] = PlayerState.Ready;
        }

        else if (playerState[playerIndex] == PlayerState.Ready)
        {
            TestReady();
        }
    }

    void OnCancelCustom(int playerIndex)
    {
        if (!isActive)
            return;

        if (playerIndex == 0 && playerState[0] == PlayerState.ChoosingChar)
        {
            GetComponent<HubTownControls>().Reactivate();
            uiBridge.HideWindow(HubUIBridge.WindowSet.RacePrep);
            isActive = false;
        }

        if (playerState[playerIndex] == PlayerState.ChoosingChar && playerIndex != 0)
        {
            playerState[playerIndex] = PlayerState.Inactive;
            GameRam.playerCount--;
            uiBridge.DeactivatePlayer(playerIndex);
        }

        else if (playerState[playerIndex] == PlayerState.ChoosingBoard)
        {
            playerState[playerIndex] = PlayerState.ChoosingChar;
            uiBridge.DeactivateBoard(playerIndex);
            uiBridge.UpdatePlayer(playerIndex);
        }

        else if (playerState[playerIndex] == PlayerState.Ready)
        {
            playerState[playerIndex] = PlayerState.ChoosingBoard;
        }

    }

    void OnNavigateCustom(HubMultiplayerInput.Paras input)
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

        if (!nowChoosingCourse)
        {
            if (playerState[input.index] == PlayerState.ChoosingChar)
            {
                if (v > 0.5f)
                {
                    playerOptionA[input.index] += 1;
                    if (playerOptionA[input.index] >= GameRam.allCharacters.Count)
                    {
                        playerOptionA[input.index] = 0;
                    }
                    navHasReset = false;
                }
                if (v < -0.5f)
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
            }
            else if (playerState[input.index] == PlayerState.ChoosingBoard)
            {
                if (v > 0.5f)
                {
                    playerOptionB[input.index] += 1;
                    if (playerOptionB[input.index] >= GameRam.ownedBoards.Count)
                    {
                        playerOptionB[input.index] = 0;
                    }
                    navHasReset = false;
                }
                if (v < -0.5f)
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
            }
        }
        else
        {
            if (v > 0.5f)
            {
                courseOption += 1;
                if (courseOption >= courseList.Length)
                {
                    courseOption = 0;
                }
                navHasReset = false;
            }
            if (v < -0.5f)
            {
                courseOption -= 1;
                if (courseOption <= -1)
                {
                    courseOption = courseList.Length - 1;
                }
                navHasReset = false;
            }
            //GameRam.courseToLoad = courseList[courseOption].courseSceneName;

            uiBridge.UpdateCourseSelect(courseList[courseOption]);
        }
    }

    void TestReady()
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
            nowChoosingCourse = true;
            uiBridge.HideWindow(HubUIBridge.WindowSet.RacePrep);
            uiBridge.RevealWindow(HubUIBridge.WindowSet.CourseSelect);
            uiBridge.UpdateCourseSelect(courseList[playerOptionA[0]]);
        }
    }
}
