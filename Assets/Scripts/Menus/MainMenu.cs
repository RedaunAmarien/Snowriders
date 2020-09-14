﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenu : MonoBehaviour {

	public GameObject mainSet, loadSet, newFileSet, chooseSet, firstTimeSet, firstLoad, fileSelectDefault;
	public TMP_InputField saveBox, deleteBox;
	// public Text[] fileName, fileCoins, bTickCount, sTickCount, gTickCount, nonCount, bronzeCount, silverCount, goldCount;
	// public Image[] file0Medal, file1Medal, file2Medal, file0Icon, file1Icon, file2Icon;
	public Button[] fileButton;
	public FileButton[] fileScript;
	public TextMeshProUGUI[] fileName;
	public GameObject[] fileContainer;
	// public Sprite[] medalSource;
	public RectTransform scrollContent;
	public Scrollbar scrollbar;
	public GameObject filePrefab;
	bool firstTime;
	public Image fadePanel;
	public bool fadingIn, fadingOut;
	public float fadeDelay, startTime;
	string[] names, charFileCust, charFilePerm, boardFile;
	public SaveData[] saveData;
	public SaveData newSaveData;
	public string saveFileVersion;
	string dir;
	int newFileNum;

	void Start() {
		fadePanel.gameObject.SetActive(true);
		chooseSet.SetActive(false);
		mainSet.SetActive(true);
		StartCoroutine(Fade(true));

		// Find Save Files.
        dir = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
			firstTime = true;
        }
		UpdateFileList();

		// Initialize Permanent Characters
		// charFilePerm = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Characters"), "*.sbcc");
		// GameRam.charDataPermanent = new CharacterData[charFilePerm.Length];
		// for (int i = 0; i < charFilePerm.Length; i++) {
		// 	GameRam.charDataPermanent[i] = LoadChar(charFilePerm[i]);
		// }

		GameRam.charDataPermanent.AddRange(GameObject.Find("Board and Char Data").GetComponent<DefaultCharacterData>().defaultCharacters);
		FileManager.SaveFile("data_perm", GameRam.charDataPermanent, Path.Combine(Application.streamingAssetsPath, "Characters"));

		// Initialize Custom Characters
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Characters"))) {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Characters"));
        }
		
        charFileCust = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Characters"), "*.sbcc");
        // GameRam.charDataCustom = new CharacterData[charFileCust.Length];
        for (int i = 0; i < charFileCust.Length; i++) {
            GameRam.charDataCustom.Add(LoadChar(charFileCust[i]));
        }
		FileManager.SaveFile("data_custom", GameRam.charDataCustom, Path.Combine(Application.persistentDataPath, "Characters"));

		// Combine all characters to same list for gameplay.
		GameRam.allCharData.AddRange(GameRam.charDataPermanent);
		GameRam.allCharData.AddRange(GameRam.charDataCustom);
		FileManager.SaveFile("data_all", GameRam.allCharData, Path.Combine(Application.streamingAssetsPath, "Characters"));

		// Initialize Boards.
		// boardFile = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Boards"), "*.txt");
		// GameRam.boardData = new List<BoardData>(boardFile.Length);
		// for (int i = 0; i < boardFile.Length; i++) {
		// 	if (boardFile[i].EndsWith(".txt")) GameRam.boardData[i] = LoadBoard(boardFile[i]);
		// }

		GameRam.boardData.AddRange(GameObject.Find("Board and Char Data").GetComponent<AllBoardData>().boards);
		GameRam.boardData.Sort((b1,b2)=>b1.shopIndex.CompareTo(b2.shopIndex));
		FileManager.SaveFile("data", GameRam.boardData, Path.Combine(Application.streamingAssetsPath, "Boards"));
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
	
	void Assign(int con) {
		GameRam.controlp[0] = con;
		GameRam.playerCount = 1;
		firstLoad.SetActive(false);
		if (firstTime) {
			firstTimeSet.SetActive(true);
		}
		else {
			mainSet.SetActive(true);
		}
	}

	public void ChooseQuit() {
		Application.Quit();
	}

	public void NewFile() {
		newFileSet.SetActive(true);
		chooseSet.SetActive(false);
	}

	public void Load (int fileIndex) {
		if (saveData[fileIndex] != null) {
			GameRam.currentSaveFile = saveData[fileIndex];
			GameRam.currentSaveDirectory = Path.Combine(dir, saveData[fileIndex].fileName + ".srd");
			// GameRam.currentSaveFile.saveSlot = fileIndex;
			Debug.LogFormat("File \"{0}\" loaded. Last saved on {1}.", GameRam.currentSaveFile.fileName, GameRam.currentSaveFile.lastSaved);
			foreach (string name in GameRam.currentSaveFile.boardsOwned) {
				foreach (BoardData board in GameRam.boardData) {
					if (board.name == name) {
						GameRam.ownedBoardData.Add(board);
					}
				}
			}
			StartCoroutine(Fade(false));
		}
		else {
			newFileSet.SetActive(true);
			chooseSet.SetActive(false);
			newFileNum = fileIndex;
		}
	}

	public void OnNavigate(InputValue val) {
		Debug.Log(val);
	}

	public void NameNewSave() {
		if (saveBox.text == "") {
			// Do nothing.
		}
		else {
			newSaveData.fileName = saveBox.text;
			newSaveData.version = saveFileVersion;
			// newSaveData.saveSlot = newFileNum;
			newSaveData.boardsOwned.Add("Balance Lv. 1");
			newSaveData.boardsOwned.Add("Speed Lv. 1");
			newSaveData.boardsOwned.Add("Trick Lv. 1");
			newSaveData.boardsOwned.Add("Control Lv. 1");
			newSaveData.ticketBronze = 1;
			newSaveData.courseGrade = new int[12];
            newSaveData.lastSaved = System.DateTime.Now;
			// for (int i = 0; i < 12; i++) {
			// 	newSaveData.courseGrade[i] = 0;
			// }
			FileManager.SaveFile(newSaveData.fileName, newSaveData, Path.Combine(Application.persistentDataPath, "Saves"));
			Debug.LogFormat("Created new save file \"{0}\".", newSaveData.fileName);
			UpdateFileList();
			newFileSet.SetActive(false);
			chooseSet.SetActive(true);
			EventSystem.current.SetSelectedGameObject(fileSelectDefault);
		}
	}

	public void DeleteFile() {
		string path = Application.persistentDataPath + "/Saves/" + deleteBox.text + ".srd";
		bool test = FileManager.DeleteFile(path);
		if (test) Debug.LogWarningFormat("Successfully deleted file {0}", path);
		else Debug.LogErrorFormat("Failed to delete file at {0}", path);
		UpdateFileList();
	}
	
	void UpdateFileList() {
		for (int i = 0; i < fileContainer.Length; i++) {
			Destroy(fileContainer[i]);
		}
		string[] saveFiles = Directory.GetFiles(dir, "*.srd");
		saveData = new SaveData[saveFiles.Length];
		// Debug.Log("Found " + saveFiles.Length + " save files.");
		for (int i = 0; i < saveFiles.Length; i++)
		{
			saveData[i] = (SaveData)FileManager.LoadFile(saveFiles[i]);
			if (saveData[i] == null) {
				Debug.LogErrorFormat("File {0} not read.", i);
			}
		}

		fileButton = new Button[saveData.Length];
		fileName = new TextMeshProUGUI[saveData.Length];
		fileContainer = new GameObject[saveData.Length];
		fileScript = new FileButton[saveData.Length];

		for (int i = 0; i < saveData.Length; i++) {
			if (saveData[i] != null) {
				fileContainer[i] = Instantiate(filePrefab, scrollContent.position, Quaternion.identity, scrollContent);
				fileScript[i] = fileContainer[i].GetComponent<FileButton>();
				fileScript[i].fileSlot = i;
				fileScript[i].mainMenu = this;
				fileScript[i].fileName = string.Format("{0}\n{1:g}", saveData[i].fileName, saveData[i].lastSaved);
				fileButton[i] = fileScript[i].fileButton;
				int golds = 0;
				int silvers = 0;
				int bronzes = 0;
				int non = 0;
				for (int j = 0; j < saveData[i].courseGrade.Length; j++) {
					if (saveData[i].courseGrade[j] == 4) non ++;
					if (saveData[i].courseGrade[j] == 3) bronzes ++;
					if (saveData[i].courseGrade[j] == 2) silvers ++;
					if (saveData[i].courseGrade[j] == 1) golds ++;
				}
				fileScript[i].fileStats = string.Format(
					"<mspace=1em>{0:N0}<sprite=\"Tickets\" index=0> {1}<sprite=\"Tickets\" index=3> {2}<sprite=\"Tickets\" index=2> {3}<sprite=\"Tickets\" index=1>\n{4}<sprite=\"Medals\" index=3> {5}<sprite=\"Medals\" index=2> {6}<sprite=\"Medals\" index=1> {7}<sprite=\"Medals\" index=0>",
					saveData[i].coins, saveData[i].ticketBronze, saveData[i].ticketSilver, saveData[i].ticketGold,
					non, bronzes, silvers, golds);
				fileContainer[i].transform.localPosition.Set(0, 0, 0);

				//Old List
				// fileName[i].text = saveData[i].fileName.ToString();
				// fileCoins[i].text = saveData[i].coins.ToString("N0");
				// bTickCount[i].text = saveData[i].ticketBronze.ToString();
				// sTickCount[i].text = saveData[i].ticketSilver.ToString();
				// gTickCount[i].text = saveData[i].ticketGold.ToString();
				// goldCount[i].text = golds.ToString();
				// silverCount[i].text = silvers.ToString();
				// bronzeCount[i].text = bronzes.ToString();
				// nonCount[i].text = non.ToString();
				// for (int j = 0; j < file0Icon.Length; j++) {
				// 	if (i == 0) file0Icon[j].color = Color.white;
				// 	if (i == 1) file1Icon[j].color = Color.white;
				// 	if (i == 2) file2Icon[j].color = Color.white;
				// }
				// for (int c = 0; c < 12; c++) {
				// 	SortMedal(i, c, saveData[i].courseGrade[c]);
				// }
			}
			else {
				fileContainer[i] = Instantiate(filePrefab, Vector3.zero, Quaternion.identity, scrollContent);
				fileContainer[i].transform.position.Set(0, 0, 0);
				fileScript[i] = fileContainer[i].GetComponent<FileButton>();
				fileScript[i].fileSlot = i;
				fileScript[i].mainMenu = this;
				fileScript[i].fileName = "File " + i;
				fileButton[i] = fileScript[i].fileButton;				
				fileScript[i].fileStats = "File Error";

				//Old List
				// fileButton[i].interactable = false;
				// fileCoins[i].text = "No Data";
				// bTickCount[i].text = "";
				// sTickCount[i].text = "";
				// gTickCount[i].text = "";
				// for (int j = 0; j < file0Icon.Length; j++) {
				// 	if (i == 0) file0Icon[j].color = Color.clear;
				// 	if (i == 1) file1Icon[j].color = Color.clear;
				// 	if (i == 2) file2Icon[j].color = Color.clear;
				// }
				// // for (int c = 0; c < 12; c++) {
				// // 	SortMedal(i, c, 0);
				// // }	
			}
		}
		float h = fileContainer.Length * 200 - 10;
		scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
		scrollbar.numberOfSteps = fileContainer.Length;
	}

	// void SortMedal(int file, int course, int grade) {
	// 	if (file == 0) {
	// 		if (grade == 0) {
	// 			file0Medal[course].sprite = medalSource[3];
	// 			file0Medal[course].color = Color.clear;
	// 		}
	// 		else {
	// 			file0Medal[course].sprite = medalSource[grade - 1];
	// 		}
	// 	}
	// 	else if (file == 1) {
	// 		if (grade == 0) {
	// 			file1Medal[course].sprite = medalSource[3];
	// 			file1Medal[course].color = Color.clear;
	// 		}
	// 		else {
	// 			file1Medal[course].sprite = medalSource[grade - 1];
	// 		}
	// 	}
	// 	else if (file == 2) {
	// 		if (grade == 0) {
	// 			file2Medal[course].sprite = medalSource[3];
	// 			file2Medal[course].color = Color.clear;
	// 		}
	// 		else {
	// 			file2Medal[course].sprite = medalSource[grade - 1];
	// 		}
	// 	}
	// }

	IEnumerator LoadScene(string sceneToLoad) {
		GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

	static CharacterData LoadChar (string path) {
		using (StreamReader streamReader = File.OpenText (path)) {
			string jsonString = streamReader.ReadToEnd();
			return JsonUtility.FromJson<CharacterData> (jsonString);
		}
	}

	static void SaveChar (CharacterData charData, string path) {
		string jsonString = JsonUtility.ToJson (charData);
		using (StreamWriter streamWriter = File.CreateText(path)) {
			streamWriter.Write (jsonString);
		}
	}

	static BoardData LoadBoard (string path) {
		using (StreamReader streamReader = File.OpenText (path)) {
			string jsonString = streamReader.ReadToEnd();
			return JsonUtility.FromJson<BoardData> (jsonString);
		}
	}
}
