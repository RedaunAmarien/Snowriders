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
	public Text[] fileName, fileCoins;
	public Image[] file0Medal, file1Medal, file2Medal;
	public Sprite[] medalSource;
	bool firstTime;
	string[] names, charFileCust, charFilePerm, boardFile;
	public SaveFileData[] saveData;
	public SaveFileData newSaveData;
	public string saveFileVersion;
	string dir;
	int newFileNum;

	void Start() {
		mainSet.SetActive(true);

		// Find Save Files.
        dir = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
			firstTime = true;
        }
		saveData = new SaveFileData[3];
		UpdateFileList();

		// Initialize Permanent Characters
		charFilePerm = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Characters"), "*.sbcc");
		GameVar.charDataPermanent = new CharacterData[charFilePerm.Length];
		for (int i = 0; i < charFilePerm.Length; i++) {
			GameVar.charDataPermanent[i] = LoadChar(charFilePerm[i]);
		}

		// Initialize Custom Characters
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Characters"))) {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Characters"));
        }
		
        charFileCust = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Characters"));
        GameVar.charDataCustom = new CharacterData[charFileCust.Length];
        for (int i = 0; i < charFileCust.Length; i++) {
            GameVar.charDataCustom[i] = LoadChar(charFileCust[i]);
        }

		// Combine all characters to same list for gameplay.
		GameVar.allCharData = new CharacterData[GameVar.charDataPermanent.Length + GameVar.charDataCustom.Length];
		for (int i = 0; i < GameVar.charDataCustom.Length; i++) {
			GameVar.allCharData[i] = GameVar.charDataCustom[i];
		}
		for (int i = 0; i < GameVar.charDataPermanent.Length; i++) {
			GameVar.allCharData[i+GameVar.charDataCustom.Length] = GameVar.charDataPermanent[i];
		}

		// Initialize Boards.
		boardFile = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Boards"), "*.txt");
		GameVar.boardData = new BoardData[boardFile.Length];
		for (int i = 0; i < boardFile.Length; i++) {
			if (boardFile[i].EndsWith(".txt")) GameVar.boardData[i] = LoadBoard(boardFile[i]);
		}

		// Setup Menu
		firstLoadText.text = ("Press A or Return\nto Start");
	}
	
	void Assign(int con) {
		GameVar.controlp[0] = con;
		GameVar.playerCount = 1;
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
		string dataPath = Path.Combine(dir, "Save_"+fileIndex+".sbsv");
		if (File.Exists (dataPath)) {
			saveData[fileIndex] = LoadFile(dataPath);
			GameVar.currentSaveFile = saveData[fileIndex];
			GameVar.currentSaveDirectory = dataPath;
			GameVar.saveSlot = fileIndex;
			print ("Slot " + fileIndex + " loaded.");
			StartCoroutine(LoadScene("HubTown"));
		}
		if (!File.Exists (dataPath)) {
			newFileSet.SetActive(true);
			chooseSet.SetActive(false);
			newFileNum = fileIndex;
		}
	}

	public void NameNewSave() {
		string dataPath = Path.Combine(dir, "Save_"+newFileNum+".sbsv");
		if (nameBox.text == "") {
			// Do nothing.
		}
		else {
			newSaveData.fileName = nameBox.text;
			newSaveData.version = saveFileVersion;
			newSaveData.boardOwned = new bool[26];
			newSaveData.boardOwned[0]=true;
			newSaveData.boardOwned[1]=true;
			newSaveData.boardOwned[2]=true;
			newSaveData.boardOwned[3]=true;
			newSaveData.courseGrade = new int[12];
			for (int i = 0; i < 12; i++) {
				newSaveData.courseGrade[i] = 0;
			}
			SaveFile (newSaveData, dataPath);
			print ("Created save file \"" + nameBox.text + "\" in slot " + newFileNum);
			UpdateFileList();
			newFileSet.SetActive(false);
			chooseSet.SetActive(true);
			EventSystem.current.SetSelectedGameObject(fileSelectDefault);
		}
	}
	
	void UpdateFileList() {
		for (int i = 0; i < 3; i++) {
			string thisPath = Path.Combine(dir, "Save_"+i+".sbsv");
			if (File.Exists(thisPath)) {
				saveData[i] = LoadFile(Path.Combine(dir, "Save_"+i+".sbsv"));
				fileName[i].text = saveData[i].fileName;
				fileCoins[i].text = saveData[i].coins.ToString() + "P";
				for (int c = 0; c < 12; c++) {
					SortMedal(i, c, saveData[i].courseGrade[c]);
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
		GameVar.nextSceneToLoad = sceneToLoad;
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

	static SaveFileData LoadFile (string path) {
		using (StreamReader streamReader = File.OpenText (path)) {
			string jsonString = streamReader.ReadToEnd();
			return JsonUtility.FromJson<SaveFileData> (jsonString);
		}
	}

	static void SaveFile (SaveFileData data, string path) {
		string jsonString = JsonUtility.ToJson (data);
		using (StreamWriter streamWriter = File.CreateText(path)) {
			streamWriter.Write (jsonString);
		}
	}
}
