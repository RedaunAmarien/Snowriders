using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System;
using UnityEngine.Animations;

public class TrackManager : MonoBehaviour
{

    CourseSettings courseSettings;
    public TimeSpan totalTime = System.TimeSpan.Zero;
    public TimeSpan remainingTime;
    public string printTimer;
    public bool timerOn;
    int playersFinished, totalFinished;
    public GameObject playerPrefab;
    public GameObject[] player;
    public List<PlayerPosition> playerPosition = new();
    GameObject[] items, weapons, coins;
    public Image miniMap;
    public RectTransform mapPanel;
    public GameObject resultsPanel;
    readonly Dictionary<string, TextMeshProUGUI> rankBar = new();
    readonly Dictionary<string, GameObject> rankBarRoots = new();
    public Image[] spriteHead;
    public Vector3 courseMax, courseMin;
    GameObject track;
    PlayerRaceControls[] playerRaceControls;
    PlayerUI[] playerUI;
    AIControls[] aiControls;
    RacerCore[] racerCore;
    // public InputActionAsset playerInputs;
    public GameObject playerControlPrefab;
    //List<Checkpoint> checkpoints;
    bool doneStarting;

    [Header("FadeAndLoad Panel")]
    public GameObject readySetSet;
    public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay;
    float startTime;

    [Header("Demo Mode Overriders")]
    public bool demoMode = false;
    public Course demoCourse;
    public int demoPlayerCount;
    public Character[] demoChars;
    public Board[] demoBoards;
    public string demoLayout;

    void Awake()
    {
        fadePanel.gameObject.SetActive(true);
        if (GameRam.currentSaveFile == null)
        {
            Debug.LogWarning("No save file found. Activating Demo Mode. Game progress will not be saved.");
            demoMode = true;
            GameRam.courseToLoad = demoCourse.courseSceneName;
            GameRam.playerCount = demoPlayerCount;
            GameRam.ownedBoards.AddRange(Resources.LoadAll<Board>("Objects/Boards/Basic"));
            GameRam.ownedBoards.AddRange(Resources.LoadAll<Board>("Objects/Boards/Special"));
            GameRam.allBoards = GameRam.ownedBoards;
            GameRam.allCharacters.AddRange(Resources.LoadAll<Character>("Objects/Characters"));
            GameRam.charForP = new int[4];
            GameRam.boardForP = new int[4];
            //GameRam.inputDevice = new InputDevice[4];
            //GameRam.controlp = new int[4];
            GameRam.itemsOn = true;
            GameRam.coinsOn = true;
            GameRam.playerCount = demoPlayerCount;
            for (int i = 0; i < 4; i++)
            {
                GameRam.charForP[i] = demoChars[i].characterIndex;
                GameRam.boardForP[i] = demoBoards[i].boardID;
                //GameRam.controlp[i] = i;
            }
            for (int i = 0; i < demoPlayerCount; i++)
            {
                GameRam.inputDevice[i] = InputSystem.GetDevice(demoLayout);
            }
        }
        if (GameRam.courseToLoad != null) SceneManager.LoadScene(GameRam.courseToLoad, LoadSceneMode.Additive);
    }

    public void Initialize()
    {

        //Begin referencing Course Settings
        courseSettings = GameObject.Find("CourseSettings").GetComponent<CourseSettings>();

        //Prepare for creation of players
        player = new GameObject[4];
        racerCore = new RacerCore[4];
        aiControls = new AIControls[4];
        playerUI = new PlayerUI[4];
        playerRaceControls = new PlayerRaceControls[4];
        //cameras = new GameObject[4,2];

        //Create and initialize players
        for (int i = 0; i < 4; i++)
        {
            player[i] = Instantiate(playerPrefab, courseSettings.playerSpawn[i].position, courseSettings.playerSpawn[i].rotation);
            racerCore[i] = player[i].GetComponent<RacerCore>();
            racerCore[i].playerNum = i;
            racerCore[i].firstCheckpoint = courseSettings.startCheckpoint;
            racerCore[i].playerStartPoint = courseSettings.startPoint;
            racerCore[i].spotLock = true;
            aiControls[i] = player[i].GetComponent<AIControls>();
            //aiControls[i].startWaypoint = courseSettings.startWaypoint;
            playerUI[i] = player[i].GetComponent<PlayerUI>();
            playerRaceControls[i] = player[i].GetComponent<PlayerRaceControls>();
            playerUI[i].reverseVCam.gameObject.layer = 13 + i;
            playerUI[i].forwardVCam.gameObject.layer = 13 + i;
            //cameras[i] = player[i];
        }

        //Remove all other racers in Challenge Mode.
        if (GameRam.gameMode == GameMode.Challenge)
        {
            for (int i = 1; i < 4; i++)
            {
                Destroy(player[i]);
                spriteHead[i].enabled = false;
            }
        }

        if (GameRam.playerCount == 0)
        {
            foreach (PlayerUI ui in playerUI)
            {
                ui.playerCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            //Deactivate unused cameras.
            for (int i = GameRam.playerCount; i < 4; i++)
            {
                playerUI[i].playerCamera.gameObject.SetActive(false);
            }
        }

        //Setup Minimap
        miniMap.sprite = courseSettings.miniMapSprite;
        miniMap.SetNativeSize();
        if (miniMap.sprite.rect.height < miniMap.sprite.rect.width)
        {
            miniMap.rectTransform.Rotate(new Vector3(0, 0, -90));
            for (int i = 0; i < 4; i++)
            {
                spriteHead[i].rectTransform.Rotate(new Vector3(0, 0, 90));
            }
        }
        if (GameRam.playerCount == 3)
        {
            miniMap.rectTransform.Rotate(new Vector3(0, 0, 90));
            for (int i = 0; i < 4; i++)
            {
                spriteHead[i].rectTransform.Rotate(new Vector3(0, 0, -90));
            }
        }
        track = courseSettings.track;
        courseMin = track.GetComponent<Collider>().bounds.min;
        courseMax = track.GetComponent<Collider>().bounds.max;
        if (GameRam.playerCount == 1)
        {
            mapPanel.anchorMax = courseSettings.miniMapAnchorMax1p;
            mapPanel.anchorMin = courseSettings.miniMapAnchorMin1p;
        }
        else if (GameRam.playerCount == 3)
        {
            mapPanel.anchorMax = courseSettings.miniMapAnchorMax3p;
            mapPanel.anchorMin = courseSettings.miniMapAnchorMin3p;
        }
        else
        {
            mapPanel.anchorMax = courseSettings.miniMapAnchorMaxDefault;
            mapPanel.anchorMin = courseSettings.miniMapAnchorMinDefault;
        }
        // Debug.Log("Map size: " + (courseMax - courseMin).ToString());
        // Debug.Log("Minimap size: "+ miniMap.rectTransform.rect.height + ", " + miniMap.rectTransform.rect.width);

        //Setup Results Screen
        resultsPanel = GameObject.Find("ResultsPanel");
        for (int i = 0; i < 4; i++)
        {
            rankBar.Add("Name" + i, GameObject.Find("Rank" + i + "Name").GetComponent<TextMeshProUGUI>());
            rankBar.Add("Time" + i, GameObject.Find("Rank" + i + "Time").GetComponent<TextMeshProUGUI>());
            rankBar.Add("Reward" + i, GameObject.Find("Rank" + i + "Reward").GetComponent<TextMeshProUGUI>());
            rankBarRoots.Add("Root" + i, GameObject.Find("Rank " + i));
            rankBarRoots["Root" + i].SetActive(false);
            // Debug.Log(rankBar["Name"+i]);
        }
        resultsPanel.SetActive(false);

        //Check custom rootOptions.
        if (GameRam.playerCount == 0)
        {
            GameRam.lapCount = courseSettings.defaultLapCount;
        }
        else
        {
            if (GameRam.lapCount == 0)
            {
                GameRam.lapCount = courseSettings.defaultLapCount;
            }
            if (GameRam.itemsOn == false)
            {
                //Debug.Log("Finding red collectItem boxes");
                items = GameObject.FindGameObjectsWithTag("RedBox");
                //Debug.Log("Listing red collectItem boxes");
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].SetActive(false);
                }
                weapons = GameObject.FindGameObjectsWithTag("BlueBox");
                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].SetActive(false);
                }
            }
            if (GameRam.coinsOn == false)
            {
                coins = GameObject.FindGameObjectsWithTag("Coin");
                for (int i = 0; i < coins.Length; i++)
                {
                    coins[i].SetActive(false);
                }
            }
        }

        //Initialize Controllers, UI, and AI scripts, and destroy unused scripts.

        List<int> pCharacterList = new();
        for (int i = 0; i < GameRam.playerCount; i++)
        {
            pCharacterList.Add(GameRam.charForP[i]);
            // Debug.Log("Adding input for player " + i);
            var playerInput = PlayerInput.Instantiate(playerControlPrefab, pairWithDevice: GameRam.inputDevice[i]);
            player[i].transform.SetParent(playerInput.transform);
            playerInput.ActivateInput();
            //playerInput.camera = cameras[i].GetComponent<Camera>();
            // Debug.Log("Player " + i + "'s inputs complete.\n" + playerInput.user.index + ", " + GameRam.inputDevice[i]);

            //playerUI[i].playerNum = i;
            playerRaceControls[i].controllerIndex = i;
            aiControls[i].isNotAI = true;
        }
        for (int i = GameRam.playerCount; i < player.Length; i++)
        {
            //Attempt to assign character.
            int offset = 0;
            //if (!demoMode)
            //{
            GameRam.charForP[i] = courseSettings.defaultCpu[i - GameRam.playerCount + offset];
            GameRam.boardForP[i] = courseSettings.defaultCpuBoard[i - GameRam.playerCount + offset];
            //Check for repeat character and reassign if found.
            for (int j = 0; j < pCharacterList.Count; j++)
            {
                if (GameRam.charForP[i] == pCharacterList[j])
                {
                    offset++;
                    GameRam.charForP[i] = courseSettings.defaultCpu[i - GameRam.playerCount + offset];
                    GameRam.boardForP[i] = courseSettings.defaultCpuBoard[i - GameRam.playerCount + offset];
                }
            }
            //}
            pCharacterList.Add(GameRam.charForP[i]);
            // Destroy(playerRaceControls[i]);
            if (GameRam.playerCount != 0 && i != 0) Destroy(playerUI[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            //Initialize character stats from data.
            racerCore[i].character = GameRam.allCharacters[GameRam.charForP[i]];
            racerCore[i].board = GameRam.ownedBoards[GameRam.boardForP[i]];
            racerCore[i].Initialize(demoMode);
        }

        //Setup Challenges
        if (GameRam.gameMode == GameMode.Challenge)
        {
            Debug.LogFormat("Attempting challenge \"{0}\"", GameRam.currentChallenge.challengeName);
            if (GameRam.currentChallenge.timeRule == true)
            {
                remainingTime = GameRam.currentChallenge.timeLimit;
            }
        }

        //Start Race
        StartCoroutine(Countdown());
        doneStarting = true;
    }

    void FixedUpdate()
    {
        // Run timer.
        if (timerOn)
        {
            totalTime += TimeSpan.FromMilliseconds(20);
            remainingTime -= TimeSpan.FromMilliseconds(20);
        }
        printTimer = string.Format("{0:d2}:{1:d2}.{2:d2}", totalTime.Minutes, totalTime.Seconds, totalTime.Milliseconds / 10);
    }

    void Update()
    {
        if (doneStarting)
        {
            if (GameRam.gameMode < GameMode.Challenge)
            {

                // Check who is in first place.
                playerPosition.Clear();
                for (int i = 0; i < player.Length; i++)
                {
                    playerPosition.Add(new PlayerPosition());
                    playerPosition[i].index = racerCore[i].playerNum;
                    playerPosition[i].lap = racerCore[i].currentLap;
                    playerPosition[i].checkpoint = racerCore[i].nextCheckVal;
                    playerPosition[i].distance = racerCore[i].distanceToLift;
                    playerPosition[i].finished = racerCore[i].finished;
                }
                playerPosition = playerPosition.OrderByDescending(x => x.lap).ThenBy(x => x.distance).ToList();
                for (int i = 0; i < player.Length; i++)
                {
                    playerPosition[i].place = i + 1;
                    racerCore[playerPosition[i].index].place = i + 1;
                }

                //Update Minimap
                for (int i = 0; i < player.Length; i++)
                {
                    Vector2 sHL = new(
                        Mathf.InverseLerp(courseMin.x, courseMax.x, player[i].transform.position.x),
                        Mathf.InverseLerp(courseMin.z, courseMax.z, player[i].transform.position.z)
                    );
                    spriteHead[i].rectTransform.anchoredPosition = new Vector2(sHL.x * miniMap.rectTransform.rect.width, sHL.y * miniMap.rectTransform.rect.height) + miniMap.rectTransform.rect.min;
                }
            }
            else
            {
                Vector2 sHL = new(
                    Mathf.InverseLerp(courseMin.x, courseMax.x, player[0].transform.position.x),
                    Mathf.InverseLerp(courseMin.z, courseMax.z, player[0].transform.position.z)
                );
                spriteHead[0].rectTransform.anchoredPosition = new Vector2(sHL.x * miniMap.rectTransform.rect.width, sHL.y * miniMap.rectTransform.rect.height) + miniMap.rectTransform.rect.min;
            }
        }
    }

    public IEnumerator Fade(bool i)
    {
        if (i) fadingIn = true;
        else fadingOut = true;
        startTime = Time.time;
        yield return new WaitForSeconds(fadeDelay);
        if (i) fadingIn = false;
        else
        {
            fadingOut = false;
            StartCoroutine(LoadScene("HubTown"));
        }
    }

    IEnumerator LoadScene(string sceneToLoad)
    {
        GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    void LateUpdate()
    {
        if (fadingIn)
        {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(1f, 0f, t));
        }
        else if (fadingOut)
        {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(0f, 1f, t));
        }

    }

    public IEnumerator Countdown(/*bool _canBoost = true*/)
    {
        //FadeAndLoad In
        readySetSet.GetComponent<Animator>().SetBool("Going", false);
        StartCoroutine(Fade(true));
        yield return new WaitForSeconds(fadeDelay);

        //Start Countdown
        readySetSet.GetComponent<Animator>().SetBool("Going", true);
        yield return new WaitForSeconds(2);

        //Kickoff
        mapPanel.gameObject.SetActive(true);
        racerCore[0].SetGo();
        if (GameRam.gameMode < GameMode.Challenge)
        {
            racerCore[1].SetGo();
            racerCore[2].SetGo();
            racerCore[3].SetGo();
        }
        timerOn = true;
        yield return new WaitForSeconds(1);
        readySetSet.SetActive(false);
    }

    public void UseSteal(int userIndex)
    {
        int coinTotal = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != userIndex && racerCore[i].board.boardName != "Wealth Board")
            {
                if (totalFinished == 0)
                {
                    if (racerCore[i].place == 1)
                    {
                        Debug.Log("Stealing from 1st place.");
                        coinTotal = racerCore[i].coins;
                        racerCore[i].coins = 0;
                    }
                    else if (racerCore[userIndex].place == 1 && racerCore[i].place == 2)
                    {
                        Debug.Log("Stealing from 2nd place.");
                        coinTotal = racerCore[i].coins;
                        racerCore[i].coins = 0;
                    }
                }
                else if (totalFinished == 1)
                {
                    if (racerCore[i].place == 2)
                    {
                        Debug.Log("Stealing from 2nd place.");
                        coinTotal = racerCore[i].coins;
                        racerCore[i].coins = 0;
                    }
                    else if (racerCore[userIndex].place == 2 && racerCore[i].place == 3)
                    {
                        Debug.Log("Stealing from 3rd place.");
                        coinTotal = racerCore[i].coins;
                        racerCore[i].coins = 0;
                    }
                }
                else if (totalFinished == 2)
                {
                    if (racerCore[i].place == 3)
                    {
                        Debug.Log("Stealing from 3rd place.");
                        coinTotal = racerCore[i].coins;
                        racerCore[i].coins = 0;
                    }
                    else if (racerCore[i].place == 4)
                    {
                        Debug.Log("Stealing from 4th place.");
                        coinTotal = racerCore[i].coins;
                        racerCore[i].coins = 0;
                    }
                }
            }
        }
        racerCore[userIndex].coins += coinTotal;
    }

    public void UseTripleSteal(int userIndex)
    {
        int coinTotal = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != userIndex && !racerCore[i].finished && racerCore[i].board.boardName != "Wealth Board")
            {
                coinTotal += racerCore[i].coins / 2;
                racerCore[i].coins = racerCore[i].coins / 2;
            }
        }
        racerCore[userIndex].coins += coinTotal;
    }

    public void UseTripleStop(int userIndex)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i != userIndex) StartCoroutine(racerCore[i].Trip());
        }
    }

    public void UseSlow(int userIndex)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i != userIndex)
            {
                if (totalFinished == 0)
                {
                    if (racerCore[i].place == 1)
                    {
                        StartCoroutine(racerCore[i].GetSlowed());
                    }
                    else if (racerCore[userIndex].place == 1 && racerCore[i].place == 2)
                    {
                        StartCoroutine(racerCore[i].GetSlowed());
                    }
                }
                else if (totalFinished == 1)
                {
                    if (racerCore[i].place == 2)
                    {
                        StartCoroutine(racerCore[i].GetSlowed());
                    }
                    else if (racerCore[userIndex].place == 2 && racerCore[i].place == 3)
                    {
                        StartCoroutine(racerCore[i].GetSlowed());
                    }
                }
                else if (totalFinished == 2)
                {
                    if (racerCore[i].place == 3)
                    {
                        StartCoroutine(racerCore[i].GetSlowed());
                    }
                    else if (racerCore[i].place == 4)
                    {
                        StartCoroutine(racerCore[i].GetSlowed());
                    }
                }
            }
        }
    }

    public void UseTripleSlow(int userIndex)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i != userIndex)
            {
                StartCoroutine(racerCore[i].GetSlowed());
            }
        }
    }

    public void Finish(int userIndex)
    {
        int placement = racerCore[userIndex].finalPlace;
        if (GameRam.gameMode == GameMode.Challenge) placement = 1;
        SaveData.CourseGrade savePlace = SaveData.CourseGrade.None;
        switch (placement)
        {
            case 1:
                savePlace = SaveData.CourseGrade.Gold;
                break;
            case 2:
                savePlace = SaveData.CourseGrade.Silver;
                break;
            case 3:
                savePlace = SaveData.CourseGrade.Bronze;
                break;
            case 4:
                savePlace = SaveData.CourseGrade.Glass;
                break;
            default:
                Debug.LogErrorFormat("Player's rank of {0}(st/nd/rd/th) is outside of range 1st - 4th.", placement);
                break;
        }
        if (GameRam.gameMode < GameMode.Challenge)
        {
            rankBarRoots["Root" + (placement - 1)].SetActive(true);
            rankBar["Name" + (placement - 1)].text = racerCore[userIndex].character.characterName;
            rankBar["Time" + (placement - 1)].text = printTimer;
            racerCore[userIndex].coins += courseSettings.prize[placement - 1];
            rankBar["Reward" + (placement - 1)].text = racerCore[userIndex].coins.ToString("N0");
            // Debug.Log(racerCore[userIndex].charName + " finished in " + savePlace + " at " + printTimer + ".");
        }
        else
        {
            rankBarRoots["Root0"].SetActive(true);
            rankBar["Name0"].text = racerCore[0].character.characterName;
            rankBar["Time0"].text = printTimer;
            rankBar["Reward0"].text = racerCore[0].coins.ToString("N0");
            for (int i = 1; i < 4; i++)
            {
                rankBar["Name" + i].text = "";
                rankBar["Time" + i].text = "";
                rankBar["Reward" + i].text = "";
            }
            // Debug.LogFormat("{0} finshed at {1} with {2:N0}.", racerCore[userIndex].charName, printTimer, racerCore[userIndex].coins);
        }

        if (GameRam.gameMode == GameMode.Story && userIndex == 0)
        {
            if (GameRam.currentSaveFile.courseGrade[courseSettings.courseIndex] == 0)
            {
                GameRam.currentSaveFile.courseGrade[courseSettings.courseIndex] = savePlace;
            }
            else if (savePlace > GameRam.currentSaveFile.courseGrade[courseSettings.courseIndex])
            {
                GameRam.currentSaveFile.courseGrade[courseSettings.courseIndex] = savePlace;
            }
        }

        totalFinished++;

        if (userIndex < GameRam.playerCount)
        {
            playersFinished++;
            if (playersFinished >= GameRam.playerCount)
            {
                FinishRace();
            }
        }
    }

    public void FinishRace()
    {

        //Results Panel
        resultsPanel.SetActive(true);
        mapPanel.gameObject.SetActive(false);
        for (int i = 0; i < GameRam.playerCount; i++)
        {
            playerRaceControls[i].raceOver = true; ;
            playerUI[i].finishGraphic.SetActive(false);
        }

        // Battle Mode
        if (GameRam.gameMode == GameMode.Battle && !demoMode)
        {
            int coinTotal = 0;
            for (int i = 0; i < GameRam.playerCount; i++)
            {
                coinTotal += (racerCore[i].coins + courseSettings.prize[i]);
                racerCore[i].coins = 0;
            }
            GameRam.currentSaveFile.coins += coinTotal;
        }

        // Adventure Mode
        else if (GameRam.gameMode == GameMode.Story)
        {
            GameRam.currentSaveFile.coins += racerCore[0].coins;
            racerCore[0].coins = 0;
        }

        // Challenge Mode
        else if (GameRam.gameMode == GameMode.Challenge)
        {
            GameRam.currentSaveFile.coins += racerCore[0].coins;
            Challenge challenge = GameRam.currentChallenge;
            bool succeeded = true;
            string failReason = string.Empty;
            challenge.timeLimit = new System.TimeSpan(0, 0, challenge.timeLimitInSeconds);

            //Test Challenge Conditions
            if (racerCore[0].coins < challenge.requiredCoinCount)
            {
                succeeded = false;
                failReason = string.Format("you did not finish with more than {0} coins", challenge.requiredCoinCount);
            }
            if (GameRam.ownedBoards[GameRam.boardForP[0]] != challenge.requiredBoard)
            {
                succeeded = false;
                string boardName = string.Empty;
                foreach (Board board in GameRam.allBoards)
                {
                    if (board == challenge.requiredBoard)
                        boardName = board.name;
                }
                failReason = string.Format("you did not use the board {0}", boardName);
            }
            if (totalTime > challenge.timeLimit)
            {
                succeeded = false;
                failReason = string.Format("you did not finish within {0:d2}:{1:d2}.{2:d2}", challenge.timeLimit.Minutes, challenge.timeLimit.Seconds, challenge.timeLimit.Milliseconds / 10);
            }

            //Save Results
            if (!succeeded)
            {
                Debug.LogFormat("Challenge {0} failed because {1}. Please try again.", challenge.challengeName, failReason);
            }
            else
            {
                GameRam.currentSaveFile.completedChallenge ??= new List<int>();
                GameRam.currentSaveFile.completedChallenge.Add(challenge.challengeIndex);
                switch (challenge.challengeLevel)
                {
                    case Challenge.TicketLevel.Gold:
                        GameRam.currentSaveFile.ticketGold++;
                        break;
                    case Challenge.TicketLevel.Silver:
                        GameRam.currentSaveFile.ticketSilver++;
                        break;
                    case Challenge.TicketLevel.Bronze:
                        GameRam.currentSaveFile.ticketBronze++;
                        break;
                }
            }
        }

        //Save File
        //Debug.LogWarningFormat("Demo Mode?: {0}", demoMode.ToString());
        if (!demoMode)
        {
            Debug.LogFormat("Autosaving to file {0} at {1} in directory {2}.", GameRam.currentSaveFile.fileName, System.DateTime.Now, GameRam.currentSaveDirectory);
            GameRam.currentSaveFile.lastSaved = System.DateTime.Now;
            FileManager.SaveFile(GameRam.currentSaveFile.fileName, GameRam.currentSaveFile, Application.persistentDataPath + "/Saves");
            GameRam.currentSaveFile = (SaveData)FileManager.LoadFile(GameRam.currentSaveDirectory);
        }
        else Debug.LogWarningFormat("Autosaving is disabled.");
    }
}
