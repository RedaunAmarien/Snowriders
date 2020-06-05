using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class CourseControl : MonoBehaviour {

	public string courseName;
	public int courseIndex, defaultLapCount;
	public int[] defaultCpu, defaultCpuBoard, prize;
	int t1, t2, t3, t4, t5;
	public int totalTimer;
	public string printTimer;
	public bool timerOn;
	int playersFinished, totalFinished;
	public Vector2 miniMapAnchorMinDefault, miniMapAnchorMaxDefault, miniMapAnchorMin1p, miniMapAnchorMax1p, miniMapAnchorMin3p, miniMapAnchorMax3p;
	public GameObject[] player, cameras;
	GameObject[] items, weapons, coins;
	CharacterData[] charData;
	public GameObject[] spriteHead;
	public Image miniMap;
	public RectTransform mapPanel;
	public GameObject resultsPanel;
	Dictionary <string, Text> rankBar = new Dictionary <string, Text>();
	public Image[] spriteHeadImage;
	public float mapScale, miniMapRef;
	public Vector3 mapRef;
	public bool mapRotated;
	GameObject track;
	Dictionary <string, ScriptableObject> pScripts = new Dictionary <string, ScriptableObject>();
	PlayerRaceControls[] pCon;
	PlayerUI[] pUI;
	AIControls[] aI;
	RacerPhysics[] rPhys;
	// InputUser[] inpUser;
	// PlayerInputManager pIM;
	public InputActionAsset playerInputs;
	public GameObject playerPrefab;

	void Awake () {
		// Initialize Minimap settings.
		track = GameObject.FindGameObjectWithTag("Track");
		mapRef = track.GetComponent<Collider>().bounds.size;

		spriteHead = new GameObject[4];
		spriteHead[0] = GameObject.Find("SpriteHead 0");
		spriteHead[1] = GameObject.Find("SpriteHead 1");
		spriteHead[2] = GameObject.Find("SpriteHead 2");
		spriteHead[3] = GameObject.Find("SpriteHead 3");

		spriteHeadImage = new Image[4];
		spriteHeadImage[0] = spriteHead[0].GetComponent<Image>();
		spriteHeadImage[1] = spriteHead[1].GetComponent<Image>();
		spriteHeadImage[2] = spriteHead[2].GetComponent<Image>();
		spriteHeadImage[3] = spriteHead[3].GetComponent<Image>();

		miniMap = GameObject.Find("MiniMap").GetComponent<Image>();
		mapPanel = GameObject.Find("MapPanel").GetComponent<RectTransform>();
		if (GameVar.playerCount == 1) {
			mapPanel.anchorMax = miniMapAnchorMax1p;
			mapPanel.anchorMin = miniMapAnchorMin1p;
		}
		else if (GameVar.playerCount == 3) {
			mapPanel.anchorMax = miniMapAnchorMax3p;
			mapPanel.anchorMin = miniMapAnchorMin3p;
		}
		else {
			mapPanel.anchorMax = miniMapAnchorMaxDefault;
			mapPanel.anchorMin = miniMapAnchorMinDefault;
		}
		Debug.Log("Map size: " + mapRef);
		Debug.Log("Minimap size: "+ miniMap.rectTransform.rect.height + ", " + miniMap.rectTransform.rect.width);

		// Ready the Results Screen
		resultsPanel = GameObject.Find("ResultsPanel");
		for (int i = 0; i < 4; i++) {
			rankBar.Add("Name"+i, GameObject.Find("Rank"+i+"Name").GetComponent<Text>());
			rankBar.Add("Time"+i, GameObject.Find("Rank"+i+"Time").GetComponent<Text>());
			rankBar.Add("Reward"+i, GameObject.Find("Rank"+i+"Reward").GetComponent<Text>());
			Debug.Log(rankBar["Name"+i]);
		}
		resultsPanel.SetActive(false);

		// Check custom settings: Coins, Items, and Lap Count.
		if (GameVar.playerCount == 0) {
			GameVar.lapCount = defaultLapCount;
		}
		else {
			if (GameVar.lapCount == 0) {
				GameVar.lapCount = defaultLapCount;
			}
			if (GameVar.itemsOn == false) {
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
			if (GameVar.coinsOn == false) {
				coins = GameObject.FindGameObjectsWithTag("Coin");
				for (int i = 0; i < coins.Length; i++) {
					coins[i].SetActive(false);
				}
			}
		}

		// Initialize cameras and players.
		// player = new GameObject[4];
		pCon = new PlayerRaceControls[4];
		pUI = new PlayerUI[4];
		aI = new AIControls[4];
		rPhys = new RacerPhysics[4];
		Debug.Log("Syncing scripts.");
		// inpUser = new InputUser[4];

		for (int i = 0; i < 4; i++) {
			// player[i] = GameObject.Find("Player" + (i+1));
			cameras[i] = GameObject.Find("Main Camera " + i);
			pCon[i] = player[i].GetComponent<PlayerRaceControls>();
			pUI[i] = player[i].GetComponent<PlayerUI>();
			aI[i] = player[i].GetComponent<AIControls>();
			rPhys[i] = player[i].GetComponent<RacerPhysics>();
			Debug.Log("Player " + i + " scripts synced.");
		}

		// Remove other racers in Challenge Mode.
		if (GameVar.gameMode == 2) {
			for (int i = 1; i < 4; i++) {
				Destroy(player[i]);
				spriteHeadImage[i].enabled = false;
			}
		}

		// Assign cameras to players and deactivate unused ones.
		for (int i = 0; i < GameVar.playerCount; i++)
		{
			pUI[i].assignedCam = cameras[i];
			Debug.Log("Camera " + i + " assigned.");
		}
		for (int i = GameVar.playerCount; i < 4; i++)
		{
			cameras[i].SetActive(false);
		}
		
		// Initialize Controller, UI, and AI scripts, and destroy unused scripts.
		if (GameVar.playerCount == 0) {
			GameVar.charForP[0] = Random.Range(0,GameVar.charDataCustom.Length+GameVar.charDataPermanent.Length);
			GameVar.boardForP[0] = Random.Range(0,GameVar.boardData.Length);
			Destroy(pCon[0]);
			for (int i = 1; i < 4; i++) {
				GameVar.charForP[i] = defaultCpu[i-1]+GameVar.charDataCustom.Length;
				GameVar.boardForP[i] = defaultCpuBoard[i-1];
				Destroy(pCon[i]);
			}
		}
		else {
			for (int i = 0; i < GameVar.playerCount; i++) {
				Debug.Log("Adding input for player " + i);
				var pInp = PlayerInput.Instantiate(playerPrefab, -1, null, -1, GameVar.inpDev[i]);
				player[i].transform.SetParent(pInp.transform);
				// pInp.actions = playerInputs;
				pInp.ActivateInput();
				pInp.camera = cameras[i].GetComponent<Camera>();
				Debug.Log("Player " + i + "'s inputs complete.\n" + pInp.user.index + ", " + GameVar.inpDev[i]);

				pUI[i].playerNum = i;
				pCon[i].conNum = GameVar.controlp[i];
				Destroy(aI[i]);
			}
			for (int i = GameVar.playerCount; i < player.Length; i++) {
				GameVar.charForP[i] = defaultCpu[i-1]+GameVar.charDataCustom.Length;
				GameVar.boardForP[i] = defaultCpuBoard[i-1];
				Destroy(pCon[i]);
				Destroy(pUI[i]);
			}
		}

		for (int i = 0; i < 4; i++) {

			// Initialize character stats from data.
			// Speed: Max 18, Min 15.
			rPhys[i].speed = 14f + (2f/3f) + (GameVar.allCharData[GameVar.charForP[i]].speed + GameVar.boardData[GameVar.boardForP[i]].speed)/3f;
			// Traction: Max .04, Min .015.
			rPhys[i].traction = (11f/900f) + (GameVar.allCharData[GameVar.charForP[i]].turn + GameVar.boardData[GameVar.boardForP[i]].turn)/360f;
			// Jump: Max 250, Min 175.
			rPhys[i].jumpForce.y = 166f + (2f/3f) + (GameVar.allCharData[GameVar.charForP[i]].jump + GameVar.boardData[GameVar.boardForP[i]].jump) * (8f+(1f/3f));
			rPhys[i].charName = GameVar.allCharData[GameVar.charForP[i]].name;
			rPhys[i].boardName = GameVar.boardData[GameVar.boardForP[i]].name;
			// rPhys[i].GetComponentInChildren<MeshRenderer>().material = GameVar.allCharData[GameVar.charForP[i]].skinCol;
		}

		// Set Laps
		for (int i = 0; i < 4; i++) {
			rPhys[i].totalLaps = GameVar.lapCount;
			rPhys[i].playerNum = i;
		}
	}

	void Start() {
		StartCoroutine(Countdown());
	}
	
	void Update () {
		// Run timer.
		if (timerOn) {
			totalTimer += 2;
			t1 += 2;
			if (t1 >= 10) {
				t1 -= 10;
				t2 ++;
			}
			if (t2 >= 10) {
				t2 -= 10;
				t3 ++;
			}
			if (t3 >= 10) {
				t3 -= 10;
				t4 ++;
			}
			if (t4 >= 6) {
				t4 -= 6;
				t5 ++;
			}
		}
		printTimer = t5 + ":" + t4+t3 + "." + t2+t1;

		// Check who is in first place.
		if (GameVar.gameMode < 2) {
			for (int i = 0; i < player.Length; i++) {
				rPhys[i].place = 4;
			}
			for (int i = 0; i < player.Length; i++) {
				for (int j = 0; j < player.Length; j++) {
					if (rPhys[i].currentLap > rPhys[j].currentLap) {
						rPhys[i].place --;
					}
					else if (rPhys[i].currentLap == rPhys[j].currentLap) {
						if (rPhys[i].nextCheckVal > rPhys[j].nextCheckVal) {
							rPhys[i].place --;
						}
						else if (rPhys[i].nextCheckVal == rPhys[j].nextCheckVal) {
							if (rPhys[i].checkDist > rPhys[j].checkDist) {
								rPhys[i].place --;
							}
						}
					}
				}	
			}
		}
		
		// Update Minimap
		if (mapRotated) miniMapRef = miniMap.rectTransform.rect.width;
		else miniMapRef = miniMap.rectTransform.rect.height;
		mapScale = miniMapRef/mapRef.z;
		if (GameVar.gameMode == 2) {
			Vector2 sHL = new Vector2(player[0].transform.position.x, player[0].transform.position.z);
			spriteHeadImage[0].rectTransform.anchoredPosition = sHL*mapScale;
		}
		else {
			for (int i = 0; i < player.Length; i++) {
				Vector2 sHL = new Vector2(player[i].transform.position.x, player[i].transform.position.z);
				spriteHeadImage[i].rectTransform.anchoredPosition = sHL*mapScale;
			}
		}
	}

	public IEnumerator Countdown (bool boostable = true) {
		rPhys[0].spotLock = true;
		if (GameVar.gameMode < 2) {
			rPhys[1].spotLock = true;
			rPhys[2].spotLock = true;
			rPhys[3].spotLock = true;
		}
		yield return new WaitForSeconds(1);
		print("Ready");
		// if (Input.GetButton(bBut)) {
		// 	boostable = false;
		// }
		yield return new WaitForSeconds(1);
		print("Set");
		// if (Input.GetButton(bBut)) {
		// 	boostable = false;
		// }

		yield return new WaitForSeconds(1);
		print("Go!");
		// if (Input.GetButton(bBut) && boostable) {
		// 	pUI.itemType = 9;
		// 	pUI.Item();
		// }
		rPhys[0].SetGo();
		if (GameVar.gameMode < 2) {
			rPhys[1].SetGo();
			rPhys[2].SetGo();
			rPhys[3].SetGo();
		}
		timerOn = true;
		yield return new WaitForSeconds(1);
		// Clear countdown graphics.
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
		if (GameVar.gameMode < 2) {
			rankBar["Name"+(savePlace-1)].text = rPhys[userIndex].charName;
			rankBar["Time"+(savePlace-1)].text = printTimer;
			rPhys[userIndex].coins += prize[savePlace-1];
			rankBar["Reward"+(savePlace-1)].text = rPhys[userIndex].coins.ToString();
			Debug.Log(rankBar["Name"+(savePlace-1)]);
			Debug.Log(rPhys[userIndex].charName + " finished in " + savePlace + " at " + printTimer + ".");
		}
		else {
			rankBar["Name0"].text = rPhys[0].charName;
			rankBar["Time0"].text = printTimer;
			rankBar["Reward0"].text = rPhys[0].coins.ToString();
			for (int i = 1; i < 4; i++) {
				rankBar["Name"+i].text = "";
				rankBar["Time"+i].text = "";
				rankBar["Reward"+i].text = "";
			}
			// Debug.Log(rPhys[userIndex].charName + " finished at " + printTimer + " with " rPhys[userIndex].coins + " points.");
		}

		if (GameVar.gameMode == 1 && userIndex == 0) {
			if (GameVar.currentSaveFile.courseGrade[courseIndex] == 0) {
				GameVar.currentSaveFile.courseGrade[courseIndex] = savePlace;
			}
			else if (savePlace < GameVar.currentSaveFile.courseGrade[courseIndex]) {
				GameVar.currentSaveFile.courseGrade[courseIndex] = savePlace;
			}
		}

		if (userIndex < GameVar.playerCount) {
			playersFinished ++;
		}
		totalFinished ++;
		if (playersFinished >= GameVar.playerCount) {
			FinishRace();
		}
	}

	public void FinishRace() {

		//Results Panel
		resultsPanel.SetActive(true);
		for (int i = 0; i < GameVar.playerCount; i++) {
			pCon[i].raceOver = true;;
		}

		// Battle Mode
		if (GameVar.gameMode == 0) {
			int coinTotal = 0;
			for (int i = 0; i < GameVar.playerCount; i++) {
				coinTotal += (rPhys[i].coins + prize[i]);	
			}
			GameVar.currentSaveFile.coins += coinTotal;
		}

		// Adventure Mode
		else if (GameVar.gameMode == 1) {
			GameVar.currentSaveFile.coins += rPhys[0].coins;
		}

		// Challenge Mode
		else if (GameVar.gameMode == 2) {
			//Save Challenge Mode files.
		}

		//Save File
		Debug.LogWarning("Autosaving to file " + GameVar.saveSlot + " " + GameVar.currentSaveFile.fileName);
		SaveFile(GameVar.currentSaveFile, GameVar.currentSaveDirectory);
		GameVar.currentSaveFile = LoadFile(GameVar.currentSaveDirectory);
	}
    
	static SaveFileData LoadFile (string path) {
		using (StreamReader streamReader = File.OpenText (path)) {
			string jsonString = streamReader.ReadToEnd();
			return JsonUtility.FromJson<SaveFileData> (jsonString);
		}
	}

	static void SaveFile (SaveFileData saveData, string path) {
		string jsonString = JsonUtility.ToJson (saveData);
		using (StreamWriter streamWriter = File.CreateText(path)) {
			streamWriter.Write (jsonString);
		}
	}
}
