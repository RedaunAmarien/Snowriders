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
	bool assigning, firstTime;
	string[] names, charFileCust, charFilePerm, boardFile;
	public SaveFileData[] saveData;
	public SaveFileData newSaveData;
	public string saveFileVersion;
	string dir;
	int newFileNum;

	void Start() {
		firstLoad.SetActive(true);

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

		// Initialize controllers.
		GameVar.controlp = new int[4];
		names = Input.GetJoystickNames();
		for (int i = 0; i < names.Length; i++) {
			print (names[i] + " in " + i);
		}
		assigning = true;

		// Setup Menu
		firstLoadText.text = ("Press A or Return\nto Start");
	}

	void Update() {
		if (assigning) {
			if (Input.GetButtonDown("1 Button 0")) {
				Assign(1);
				print ("Player 1 using controller 1");
			}
			else if (Input.GetButtonDown("2 Button 0")) {
				Assign(2);
				print ("Player 1 using controller 2");
			}
			else if (Input.GetButtonDown("3 Button 0")) {
				Assign(3);
				print ("Player 1 using controller 3");
			}
			else if (Input.GetButtonDown("4 Button 0")) {
				Assign(4);
				print ("Player 1 using controller 4");
			}
			else if (Input.GetButtonDown("-1 Button 0")) {
				Assign(-1);
				print ("Player 1 using keyboard.");
			}
			else if (Input.GetButtonDown("0 Button 0")) {
				Assign(0);
				print ("Player 1 using a controller beyond 1-4");
            }
		}
	}
	void Assign(int con) {
		assigning = false;
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
				if (i == 0) {
					for (int m = 0; m < 12; m++) {
						if (saveData[0].courseGrade[m] == 0) {
							file0Medal[m].sprite = medalSource[3];
							file0Medal[m].color = Color.clear;
						}
						if (saveData[0].courseGrade[m] == 1) {
							file0Medal[m].sprite = medalSource[0];
						}
						if (saveData[0].courseGrade[m] == 2) {
							file0Medal[m].sprite = medalSource[1];
						}
						if (saveData[0].courseGrade[m] == 3) {
							file0Medal[m].sprite = medalSource[2];
						}
						if (saveData[0].courseGrade[m] == 4) {
							file0Medal[m].sprite = medalSource[3];
						}
					}
				}
				else if (i == 1) {
					for (int m = 0; m < 12; m++) {
						if (saveData[1].courseGrade[m] == 0) {
							file1Medal[m].sprite = medalSource[3];
							file1Medal[m].color = Color.clear;
						}
						if (saveData[1].courseGrade[m] == 1) {
							file1Medal[m].sprite = medalSource[0];
						}
						if (saveData[1].courseGrade[m] == 2) {
							file1Medal[m].sprite = medalSource[1];
						}
						if (saveData[1].courseGrade[m] == 3) {
							file1Medal[m].sprite = medalSource[2];
						}
						if (saveData[1].courseGrade[m] == 4) {
							file1Medal[m].sprite = medalSource[3];
						}
					}

				}
				else if (i == 2) {
					for (int m = 0; m < 12; m++) {
						if (saveData[2].courseGrade[m] == 0) {
							file2Medal[m].sprite = medalSource[3];
							file2Medal[m].color = Color.clear;
						}
						if (saveData[2].courseGrade[m] == 1) {
							file2Medal[m].sprite = medalSource[0];
						}
						if (saveData[2].courseGrade[m] == 2) {
							file2Medal[m].sprite = medalSource[1];
						}
						if (saveData[2].courseGrade[m] == 3) {
							file2Medal[m].sprite = medalSource[2];
						}
						if (saveData[2].courseGrade[m] == 4) {
							file2Medal[m].sprite = medalSource[3];
						}
					}

				}
			}
			else {
				fileName[i].text = "New File";
				fileCoins[i].text = "";
				if (i == 0) {
					for (int m = 0; m < 12; m++) {
						file0Medal[m].gameObject.SetActive(false);
					}
				}
				else if (i == 1) {
					for (int m = 0; m < 12; m++) {
						file1Medal[m].gameObject.SetActive(false);
					}
				}
				else if (i == 2) {
					for (int m = 0; m < 12; m++) {
						file2Medal[m].gameObject.SetActive(false);
					}
				}
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
