using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

	public GameObject mainSet, loadSet, newFileSet, chooseSet, firstTimeSet, firstLoad, fileSelectDefault;
	public Text nameBox, firstLoadText;
	public Text[] fileName, fileCoins, fileBTick, fileSTick, fileGTick;
	public Image[] file0Medal, file1Medal, file2Medal, file0Tick, file1Tick, file2Tick;
	public Sprite[] medalSource;
	bool firstTime;
	string[] names, charFileCust, charFilePerm, boardFile;
	public SaveData[] saveData;
	public SaveData newSaveData;
	public string saveFileVersion;
	string dir;
	int newFileNum;

	void Start() {
		mainSet.SetActive(true);

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

		GameRam.charDataPermanent = GameObject.Find("Board and Char Data").GetComponent<DefaultCharacterData>().defaultCharacters;

		// Initialize Custom Characters
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Characters"))) {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Characters"));
        }
		
        charFileCust = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Characters"), "*.sbcc");
        GameRam.charDataCustom = new CharacterData[charFileCust.Length];
        for (int i = 0; i < charFileCust.Length; i++) {
            GameRam.charDataCustom[i] = LoadChar(charFileCust[i]);
        }

		// Combine all characters to same list for gameplay.
		for (int i = 0; i < GameRam.charDataPermanent.Length; i++) {
			GameRam.allCharData.Add(GameRam.charDataPermanent[i]);
		}
		for (int i = 0; i < GameRam.charDataCustom.Length; i++) {
			GameRam.allCharData.Add(GameRam.charDataCustom[i]);
		}

		FileManager.SaveFile("data", GameRam.allCharData, "/Characters");

		// Initialize Boards.
		boardFile = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Boards"), "*.txt");
		GameRam.boardData = new BoardData[boardFile.Length];
		for (int i = 0; i < boardFile.Length; i++) {
			if (boardFile[i].EndsWith(".txt")) GameRam.boardData[i] = LoadBoard(boardFile[i]);
		}

		GameRam.boardData = GameObject.Find("Board and Char Data").GetComponent<AllBoardData>().boards;

		// Setup Menu
		firstLoadText.text = ("Press A or Return\nto Start");
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

	public void Load (int fileIndex) {
		if (saveData[fileIndex] != null) {
			GameRam.currentSaveFile = saveData[fileIndex];
			GameRam.currentSaveDirectory = Path.Combine(dir, saveData[fileIndex].saveSlot + "_" + saveData[fileIndex].fileName + ".srd");
			GameRam.currentSaveFile.saveSlot = fileIndex;
			print ("Slot " + fileIndex + " loaded.");
			StartCoroutine(LoadScene("HubTown"));
		}
		else {
			newFileSet.SetActive(true);
			chooseSet.SetActive(false);
			newFileNum = fileIndex;
		}
	}

	public void NameNewSave() {
		if (nameBox.text == "") {
			// Do nothing.
		}
		else {
			newSaveData.fileName = nameBox.text;
			newSaveData.version = saveFileVersion;
			newSaveData.saveSlot = newFileNum;
			newSaveData.boardOwned = new bool[26];
			newSaveData.boardOwned[0]=true;
			newSaveData.boardOwned[1]=true;
			newSaveData.boardOwned[2]=true;
			newSaveData.boardOwned[3]=true;
			newSaveData.ticketBronze = 1;
			newSaveData.courseGrade = new int[12];
			for (int i = 0; i < 12; i++) {
				newSaveData.courseGrade[i] = 0;
			}
			FileManager.SaveFile(newSaveData.saveSlot + "_" + newSaveData.fileName, newSaveData, "/Saves");
			Debug.Log("Created save file \"" + newSaveData.saveSlot + "_" + newSaveData.fileName + "\" in slot " + newSaveData.saveSlot);
			UpdateFileList();
			newFileSet.SetActive(false);
			chooseSet.SetActive(true);
			EventSystem.current.SetSelectedGameObject(fileSelectDefault);
		}
	}
	
	void UpdateFileList() {
		saveData = new SaveData[3];
		string[] saveFiles = Directory.GetFiles(dir, "*.srd");
		// Debug.Log("Found " + saveFiles.Length + " save files.");
		for (int i = 0; i < saveFiles.Length; i++)
		{
			saveData[i] = (SaveData)FileManager.LoadFile(saveFiles[i]);
			if (saveData[i] == null) {
				Debug.LogError("File not read.");
			}
		}
		for (int i = 0; i < 3; i++) {
			if (saveData[i] != null) {
				fileName[i].text = saveData[i].fileName;
				fileCoins[i].text = saveData[i].coins.ToString("N0");
				fileBTick[i].text = saveData[i].ticketBronze.ToString();
				fileSTick[i].text = saveData[i].ticketSilver.ToString();
				fileGTick[i].text = saveData[i].ticketGold.ToString();
				for (int j = 0; j < 4; j++) {
					if (i == 0) file0Tick[j].color = Color.white;
					if (i == 1) file1Tick[j].color = Color.white;
					if (i == 2) file2Tick[j].color = Color.white;
				}
				for (int c = 0; c < 12; c++) {
					SortMedal(i, c, saveData[i].courseGrade[c]);
				}
			}
			else {
				fileCoins[i].text = "No Data";
				fileBTick[i].text = "";
				fileSTick[i].text = "";
				fileGTick[i].text = "";
				for (int j = 0; j < 4; j++) {
					if (i == 0) file0Tick[j].color = Color.clear;
					if (i == 1) file1Tick[j].color = Color.clear;
					if (i == 2) file2Tick[j].color = Color.clear;
				}
				for (int c = 0; c < 12; c++) {
					SortMedal(i, c, 0);
				}	
			}
		}
	}

	void SortMedal(int file, int course, int grade) {
		if (file == 0) {
			if (grade == 0) {
				file0Medal[course].sprite = medalSource[3];
				file0Medal[course].color = Color.clear;
			}
			else {
				file0Medal[course].sprite = medalSource[grade - 1];
			}
		}
		else if (file == 1) {
			if (grade == 0) {
				file1Medal[course].sprite = medalSource[3];
				file1Medal[course].color = Color.clear;
			}
			else {
				file1Medal[course].sprite = medalSource[grade - 1];
			}
		}
		else if (file == 2) {
			if (grade == 0) {
				file2Medal[course].sprite = medalSource[3];
				file2Medal[course].color = Color.clear;
			}
			else {
				file2Medal[course].sprite = medalSource[grade - 1];
			}
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
