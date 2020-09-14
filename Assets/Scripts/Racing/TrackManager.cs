using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using System.Linq;
using UnityEngine.Animations;

public class TrackManager : MonoBehaviour {

	CourseSettings cSettings;
	public int totalTimer;
	public string printTimer;
	public bool timerOn;
	int playersFinished, totalFinished;
    public GameObject playerPrefab;
	public GameObject[] player, cameras;
	public List<PlayerPosition> pp = new List<PlayerPosition>();
	GameObject[] items, weapons, coins;
	public Image miniMap;
	public RectTransform mapPanel;
	public GameObject resultsPanel;
	Dictionary <string, TextMeshProUGUI> rankBar = new Dictionary<string, TextMeshProUGUI>();
	Dictionary <string, GameObject> rankBarRoots = new Dictionary<string, GameObject>();
	public Image[] spriteHead;
	public Vector3 courseMax, courseMin;
	GameObject track;
	PlayerRaceControls[] pCon;
	PlayerUI[] pUI;
	AIControls[] aI;
	RacerPhysics[] rPhys;
	// public InputActionAsset playerInputs;
	public GameObject playerControlPrefab;
	List<Checkpoint> checkpoints;
	System.TimeSpan totalTime = System.TimeSpan.Zero;
	bool doneStarting;
	
	[Header("Fade Panel")]
	public GameObject readySetSet;
	public Image fadePanel;
	bool fadingIn, fadingOut;
	public float fadeDelay;
	float startTime;

	[Header("Demo Mode Overriders")]
	public bool demoMode;
	public string demoCourse;
	public int demoPlayerCount;
	public CharacterData[] demoChars;
	public BoardData[] demoBoards;
	public string demoLayout;

	void Awake () {
		fadePanel.gameObject.SetActive(true);
		if (GameRam.currentSaveFile == null) {
			demoMode = true;
			GameRam.courseToLoad = demoCourse;
			GameRam.playerCount = demoPlayerCount;
			GameRam.boardData.AddRange(demoBoards);
			GameRam.charForP = new int[4];
			GameRam.boardForP = new int[4];
			GameRam.inpDev = new InputDevice[4];
			GameRam.controlp = new int[4];
			GameRam.itemsOn = true;
			GameRam.coinsOn = true;
			for (int i = 0; i < 4; i++) {
				GameRam.allCharData.Add(demoChars[i]);
				GameRam.charForP[i] = i;
				GameRam.boardForP[i] = i;
				GameRam.controlp[i] = i;
			}
			for (int i = 0; i < demoPlayerCount; i++) {
				GameRam.inpDev[i] = InputSystem.GetDevice(demoLayout);
			}
		}
		if (GameRam.courseToLoad != null) SceneManager.LoadScene(GameRam.courseToLoad, LoadSceneMode.Additive);
        // Debug.Log("Loading scene " + GameRam.courseToLoad);
	}

	public void Initialize() {

		//Begin referencing Course Settings
		cSettings = GameObject.Find("CourseSettings").GetComponent<CourseSettings>();

		//Prepare for creation of players
		player = new GameObject[4];
		rPhys = new RacerPhysics[4];
		aI = new AIControls[4];
		pUI = new PlayerUI[4];
		pCon = new PlayerRaceControls[4];

		//Create and initialize players
		for (int i = 0; i < 4; i++) {
			player[i] = GameObject.Instantiate(playerPrefab, cSettings.playerSpawn[i].position, cSettings.playerSpawn[i].rotation);
			rPhys[i] = player[i].GetComponent<RacerPhysics>();
			rPhys[i].playerNum = i;
			rPhys[i].firstCheckpoint = cSettings.startCheckpoint;
			rPhys[i].playerStartPoint = cSettings.startPoint;
			rPhys[i].spotLock = true;
			aI[i] = player[i].GetComponent<AIControls>();
			aI[i].firstWaypoint = cSettings.startWaypoint;
			pUI[i] = player[i].GetComponent<PlayerUI>();
			pCon[i] = player[i].GetComponent<PlayerRaceControls>();
		}

		//Remove all other racers in Challenge Mode.
		if (GameRam.gameMode == 2) {
			for (int i = 1; i < 4; i++) {
				Destroy(player[i]);
				spriteHead[i].enabled = false;
			}
		}

		//Assign cameras to players.
		for (int i = 0; i < GameRam.playerCount; i++) {
			pUI[i].assignedCam = cameras[i];
			if (i == 3) Destroy(cameras[i].GetComponent<ReplayCam>());
			Debug.Log("Camera " + i + " assigned.");
		}

		//Deactive unused cameras.
		for (int i = GameRam.playerCount; i < 4; i++) {
			cameras[i].SetActive(false);
		}
		if (GameRam.playerCount == 3) cameras[3].SetActive(true);

		//Setup Minimap
		miniMap.sprite = cSettings.miniMapSprite;
		miniMap.SetNativeSize();
		if (miniMap.sprite.rect.height < miniMap.sprite.rect.width) {
			miniMap.rectTransform.Rotate(new Vector3(0, 0, -90));
			for (int i = 0; i < 4; i++) {
				spriteHead[i].rectTransform.Rotate(new Vector3(0, 0, 90));
			}
		}
		if (GameRam.playerCount == 3) {
			miniMap.rectTransform.Rotate(new Vector3(0, 0, 90));
			for (int i = 0; i < 4; i++) {
				spriteHead[i].rectTransform.Rotate(new Vector3(0, 0, -90));
			}
		}
		track = cSettings.track;
		courseMin = track.GetComponent<Collider>().bounds.min;
		courseMax = track.GetComponent<Collider>().bounds.max;
		if (GameRam.playerCount == 1) {
			mapPanel.anchorMax = cSettings.miniMapAnchorMax1p;
			mapPanel.anchorMin = cSettings.miniMapAnchorMin1p;
		}
		else if (GameRam.playerCount == 3) {
			mapPanel.anchorMax = cSettings.miniMapAnchorMax3p;
			mapPanel.anchorMin = cSettings.miniMapAnchorMin3p;
		}
		else {
			mapPanel.anchorMax = cSettings.miniMapAnchorMaxDefault;
			mapPanel.anchorMin = cSettings.miniMapAnchorMinDefault;
		}
		// Debug.Log("Map size: " + (courseMax - courseMin).ToString());
		// Debug.Log("Minimap size: "+ miniMap.rectTransform.rect.height + ", " + miniMap.rectTransform.rect.width);

		//Setup Results Screen
		resultsPanel = GameObject.Find("ResultsPanel");
		for (int i = 0; i < 4; i++) {
			rankBar.Add("Name"+i, GameObject.Find("Rank"+i+"Name").GetComponent<TextMeshProUGUI>());
			rankBar.Add("Time"+i, GameObject.Find("Rank"+i+"Time").GetComponent<TextMeshProUGUI>());
			rankBar.Add("Reward"+i, GameObject.Find("Rank"+i+"Reward").GetComponent<TextMeshProUGUI>());
			rankBarRoots.Add("Root"+i, GameObject.Find("Rank " + i));
			rankBarRoots["Root"+i].SetActive(false);
			// Debug.Log(rankBar["Name"+i]);
		}
		resultsPanel.SetActive(false);

		//Check custom options.
		if (GameRam.playerCount == 0) {
			GameRam.lapCount = cSettings.defaultLapCount;
		}
		else {
			if (GameRam.lapCount == 0) {
				GameRam.lapCount = cSettings.defaultLapCount;
			}
			if (GameRam.itemsOn == false) {
				Debug.Log("Finding red item boxes");
				items = GameObject.FindGameObjectsWithTag("RedBox");
				Debug.Log("Listing red item boxes");
				for (int i = 0; i < items.Length; i++){
					items[i].SetActive(false);
				}
				weapons = GameObject.FindGameObjectsWithTag("BlueBox");
				for (int i = 0; i < weapons.Length; i++){
					weapons[i].SetActive(false);
				}
			}
			if (GameRam.coinsOn == false) {
				coins = GameObject.FindGameObjectsWithTag("Coin");
				for (int i = 0; i < coins.Length; i++) {
					coins[i].SetActive(false);
				}
			}
		}
		
		//Initialize Controllers, UI, and AI scripts, and destroy unused scripts.

		List<int> pCharacterList = new List<int>();
		for (int i = 0; i < GameRam.playerCount; i++) {
			pCharacterList.Add(GameRam.charForP[i]);
			// Debug.Log("Adding input for player " + i);
			var pInp = PlayerInput.Instantiate(playerControlPrefab, -1, null, -1, GameRam.inpDev[i]);
			player[i].transform.SetParent(pInp.transform);
			pInp.ActivateInput();
			pInp.camera = cameras[i].GetComponent<Camera>();
			// Debug.Log("Player " + i + "'s inputs complete.\n" + pInp.user.index + ", " + GameRam.inpDev[i]);

			pUI[i].playerNum = i;
			pCon[i].conNum = GameRam.controlp[i];
			Destroy(aI[i]);
		}
		for (int i = GameRam.playerCount; i < player.Length; i++) {
			//Attempt to assign character.
			int offset = 0;
			if (!demoMode) {
				GameRam.charForP[i] = cSettings.defaultCpu[i-GameRam.playerCount+offset];
				GameRam.boardForP[i] = cSettings.defaultCpuBoard[i-GameRam.playerCount+offset];
				//Check for repeat character and reassign if found.
				for (int j = 0; j < pCharacterList.Count; j++) {
					if (GameRam.charForP[i] == pCharacterList[j]) {
						offset ++;
						GameRam.charForP[i] = cSettings.defaultCpu[i-GameRam.playerCount+offset];
						GameRam.boardForP[i] = cSettings.defaultCpuBoard[i-GameRam.playerCount+offset];
					}
				}
			}
			pCharacterList.Add(GameRam.charForP[i]);
			// Destroy(pCon[i]);
			Destroy(pUI[i]);
		}
		Debug.Log(pCharacterList.ToString());

		for (int i = 0; i < 4; i++) {

			//Initialize character stats from data.

			//Speed: Max 18, Min 15.
			rPhys[i].speed = 14f + (2f/3f) + (GameRam.allCharData[GameRam.charForP[i]].speed + GameRam.boardData[GameRam.boardForP[i]].speed)/3f;

			//Traction: Max .04, Min .015.
			rPhys[i].traction = (11f/900f) + (GameRam.allCharData[GameRam.charForP[i]].turn + GameRam.boardData[GameRam.boardForP[i]].turn)/360f;

			//Jump: Max 250, Min 175.
			rPhys[i].jumpForce.y = 166f + (2f/3f) + (GameRam.allCharData[GameRam.charForP[i]].jump + GameRam.boardData[GameRam.boardForP[i]].jump) * (8f+(1f/3f));

			//Display stats
			rPhys[i].charName = GameRam.allCharData[GameRam.charForP[i]].name;
			rPhys[i].boardName = GameRam.boardData[GameRam.boardForP[i]].name;
			rPhys[i].totalLaps = GameRam.lapCount;
		}

		//Start Race
		StartCoroutine(Countdown());
		doneStarting = true;
	}
	
	void FixedUpdate () {
		// Run timer.
		if (timerOn) {
			totalTime += System.TimeSpan.FromMilliseconds(20);
		}
		printTimer = string.Format("{0:d2}:{1:d2}.{2:d2}", totalTime.Minutes, totalTime.Seconds, totalTime.Milliseconds/10);
	}

	void Update() {

		if (GameRam.gameMode < 2 && doneStarting) {
		
			// Check who is in first place.
			pp.Clear();
			for (int i = 0; i < player.Length; i++) {
				pp.Add(new PlayerPosition());
				pp[i].index = rPhys[i].playerNum;
				pp[i].lap = rPhys[i].currentLap;
				pp[i].checkpoint = rPhys[i].nextCheckVal;
				pp[i].distance = rPhys[i].checkDist;
				pp[i].finished = rPhys[i].finished;
			}
			pp = pp.OrderByDescending(x => x.lap).ThenByDescending(x => x.checkpoint).ThenByDescending(x => x.distance).ToList();
			for (int i = 0; i < player.Length; i++) {
				pp[i].place = i+1;
				rPhys[pp[i].index].place = i+1;
			}
		}
		
		//Update Minimap
		for (int i = 0; i < player.Length; i++) {
			Vector2 sHL = new Vector2(
				Mathf.InverseLerp(courseMin.x, courseMax.x, player[i].transform.position.x),
				Mathf.InverseLerp(courseMin.z, courseMax.z, player[i].transform.position.z)
			);
			spriteHead[i].rectTransform.anchoredPosition = new Vector2(sHL.x * miniMap.rectTransform.rect.width, sHL.y * miniMap.rectTransform.rect.height) + miniMap.rectTransform.rect.min;
		}
	}
    
    public IEnumerator Fade(bool i) {
		if (i) fadingIn = true;
		else fadingOut = true;
		startTime = Time.time;
		yield return new WaitForSeconds(fadeDelay);
		if (i) fadingIn = false;
		else {
			fadingOut = false;
			StartCoroutine(LoadScene("HubTown"));
		}
	}

	IEnumerator LoadScene(string sceneToLoad) {
		GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

	void LateUpdate() {
        if (fadingIn) {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(1f, 0f, t));
        }
        else if (fadingOut) {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(0f, 1f, t));
        }
        
    }

	public IEnumerator Countdown (bool boostable = true) {
		//Fade In
		readySetSet.GetComponent<Animator>().SetBool("Going", false);
		StartCoroutine(Fade(true));
		yield return new WaitForSeconds(fadeDelay);

		//Start Countdown
		readySetSet.GetComponent<Animator>().SetBool("Going", true);
		yield return new WaitForSeconds(2);

		//Kickoff
		mapPanel.gameObject.SetActive(true);
		rPhys[0].SetGo();
		if (GameRam.gameMode < 2) {
			rPhys[1].SetGo();
			rPhys[2].SetGo();
			rPhys[3].SetGo();
		}
		timerOn = true;
		yield return new WaitForSeconds(1);
		readySetSet.SetActive(false);
	}

	public void UseSteal(int userIndex) {
		int coinTotal = 0;
		for (int i = 0; i < 4; i++) {
			if (i != userIndex) {
				if (totalFinished == 0) {
					if (rPhys[i].place == 1) {
						Debug.Log("Stealing from 1st place.");
						coinTotal = rPhys[i].coins;
						rPhys[i].coins = 0;
					}
					else if (rPhys[userIndex].place == 1 && rPhys[i].place == 2) {
						Debug.Log("Stealing from 2nd place.");
						coinTotal = rPhys[i].coins;
						rPhys[i].coins = 0;
					}
				}
				else if (totalFinished == 1) {
					if (rPhys[i].place == 2) {
						Debug.Log("Stealing from 2nd place.");
						coinTotal = rPhys[i].coins;
						rPhys[i].coins = 0;
					}
					else if (rPhys[userIndex].place == 2 && rPhys[i].place == 3) {
						Debug.Log("Stealing from 3rd place.");
						coinTotal = rPhys[i].coins;
						rPhys[i].coins = 0;
					}
				}
				else if (totalFinished == 2) {
					if (rPhys[i].place == 3) {
						Debug.Log("Stealing from 3rd place.");
						coinTotal = rPhys[i].coins;
						rPhys[i].coins = 0;
					}
					else if (rPhys[i].place == 4) {
						Debug.Log("Stealing from 4th place.");
						coinTotal = rPhys[i].coins;
						rPhys[i].coins = 0;
					}
				}
			}
		}
		rPhys[userIndex].coins += coinTotal;
	}

	public void UseTripleSteal(int userIndex) {
		int coinTotal = 0;
		for (int i = 0; i < 4; i++) {
			if (i != userIndex && !rPhys[i].finished) {
				coinTotal += rPhys[i].coins/2;
				rPhys[i].coins = rPhys[i].coins/2;
			}
		}
		rPhys[userIndex].coins += coinTotal;
	}

	public void UseTripleStop(int userIndex) {
		for (int i = 0; i < 4; i++) {
			if (i != userIndex) StartCoroutine(rPhys[i].Trip());
		}
	}

	public void UseSlow(int userIndex) {
		for (int i = 0; i < 4; i++) {
			if (i != userIndex) {
				if (totalFinished == 0) {
					if (rPhys[i].place == 1) {
						StartCoroutine(rPhys[i].GetSlowed());
					}
					else if (rPhys[userIndex].place == 1 && rPhys[i].place == 2) {
						StartCoroutine(rPhys[i].GetSlowed());
					}
				}
				else if (totalFinished == 1) {
					if (rPhys[i].place == 2) {
						StartCoroutine(rPhys[i].GetSlowed());
					}
					else if (rPhys[userIndex].place == 2 && rPhys[i].place == 3) {
						StartCoroutine(rPhys[i].GetSlowed());
					}
				}
				else if (totalFinished == 2) {
					if (rPhys[i].place == 3) {
						StartCoroutine(rPhys[i].GetSlowed());
					}
					else if (rPhys[i].place == 4) {
						StartCoroutine(rPhys[i].GetSlowed());
					}
				}
			}
		}
	}

	public void UseTripleSlow(int userIndex) {
		for (int i = 0; i < 4; i++) {
			if (i != userIndex) {
				StartCoroutine(rPhys[i].GetSlowed());
			}
		}
	}

	public void Finish(int userIndex) {
		int savePlace = rPhys[userIndex].finalPlace;
		if (GameRam.gameMode < 2) {
			rankBarRoots["Root"+(savePlace-1)].SetActive(true);
			rankBar["Name"+(savePlace-1)].text = rPhys[userIndex].charName;
			rankBar["Time"+(savePlace-1)].text = printTimer;
			// rPhys[userIndex].coins += cSettings.prize[savePlace-1];
			rankBar["Reward"+(savePlace-1)].text = rPhys[userIndex].coins.ToString("N0");
			// Debug.Log(rankBar["Name"+(savePlace-1)]);
			Debug.Log(rPhys[userIndex].charName + " finished in " + savePlace + " at " + printTimer + ".");
		}
		else {
			rankBarRoots["Root0"].SetActive(true);
			rankBar["Name0"].text = rPhys[0].charName;
			rankBar["Time0"].text = printTimer;
			rankBar["Reward0"].text = rPhys[0].coins.ToString();
			for (int i = 1; i < 4; i++) {
				rankBar["Name"+i].text = "";
				rankBar["Time"+i].text = "";
				rankBar["Reward"+i].text = "";
			}
			// Debug.LogFormat("{0} finshed at {1} with {2:N0}.", rPhys[userIndex].charName, printTimer, rPhys[userIndex].coins);
		}

		if (GameRam.gameMode == 1 && userIndex == 0) {
			if (GameRam.currentSaveFile.courseGrade[cSettings.courseIndex] == 0) {
				GameRam.currentSaveFile.courseGrade[cSettings.courseIndex] = savePlace;
			}
			else if (savePlace < GameRam.currentSaveFile.courseGrade[cSettings.courseIndex]) {
				GameRam.currentSaveFile.courseGrade[cSettings.courseIndex] = savePlace;
			}
		}

		if (userIndex < GameRam.playerCount) {
			playersFinished ++;
		}
		totalFinished ++;
		if (playersFinished >= GameRam.playerCount) {
			FinishRace();
		}
	}

	public void FinishRace() {

		//Results Panel
		resultsPanel.SetActive(true);
		mapPanel.gameObject.SetActive(false);
		for (int i = 0; i < GameRam.playerCount; i++) {
			pCon[i].raceOver = true;;
			pUI[i].finishGraphic.SetActive(false);
		}

		// Battle Mode
		if (GameRam.gameMode == 0) {
			int coinTotal = 0;
			for (int i = 0; i < GameRam.playerCount; i++) {
				coinTotal += (rPhys[i].coins + cSettings.prize[i]);	
			}
			GameRam.currentSaveFile.coins += coinTotal;
		}

		// Adventure Mode
		else if (GameRam.gameMode == 1) {
			GameRam.currentSaveFile.coins += rPhys[0].coins;
		}

		// Challenge Mode
		else if (GameRam.gameMode == 2) {
			//Save Challenge Mode files.
		}

		//Save File
		Debug.LogWarning("Autosaving to file " + GameRam.currentSaveFile.fileName);
        GameRam.currentSaveFile.lastSaved = System.DateTime.Now;
		FileManager.SaveFile(GameRam.currentSaveFile.fileName, GameRam.currentSaveFile, Path.Combine(Application.persistentDataPath, "Saves"));
		GameRam.currentSaveFile = (SaveData)FileManager.LoadFile(GameRam.currentSaveDirectory);
	}
}
