using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class CharacterEditor : MonoBehaviour {

    public Text nameBox, warningText;
    public InputField newName;
    public Slider speedSl, turnSl, jumpSl, skinSL, progressBar;
    public Dropdown loadChars;
    public CharacterData currentCharData;
    // public SaveFileData saveData;
    public GameObject warningPanel, loadSet;
    public Texture[] skins;
    public Material characterMat;
    public bool exiting;
    string charDir, charDataPath, saveDir, saveDataPath;
    string[] charFile;
    
    void Start() {
        skinSL.maxValue = skins.Length-1;
        warningPanel.SetActive(false);
        loadSet.SetActive(false);
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Characters"))) {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Characters"));
        }
        charDir = Path.Combine(Application.persistentDataPath, "Characters");
        saveDir = Path.Combine(Application.persistentDataPath, "Saves");
        UpdateLoadList();
        speedSl.value = 3;
        turnSl.value = 3;
        jumpSl.value = 3;
    }

    void Update() {
        currentCharData.speed = Mathf.RoundToInt(speedSl.value);
        currentCharData.turn = Mathf.RoundToInt(turnSl.value);
        currentCharData.jump = Mathf.RoundToInt(jumpSl.value);
        currentCharData.skinCol = Mathf.RoundToInt(skinSL.value);
    }

    void UpdateLoadList() {
        // Update Custom list for customization.
        charFile = Directory.GetFiles(charDir, "*.sbcc");
        GameRam.charDataCustom = new CharacterData[charFile.Length];
        string[] names = new string[charFile.Length];
        List<string> savedChars = new List<string> {};
        for (int i = 0; i < charFile.Length; i++) {
            GameRam.charDataCustom[i] = LoadChar(charFile[i]);
            names[i] = LoadChar(charFile[i]).name;
            savedChars.Add(names[i]);
        }

        // Update Total list for gameplay.
        GameRam.allCharData.Clear();
		for (int i = 0; i < GameRam.charDataPermanent.Length; i++) {
			GameRam.allCharData.Add(GameRam.charDataPermanent[i]);
		}
		for (int i = 0; i < GameRam.charDataCustom.Length; i++) {
			GameRam.allCharData.Add(GameRam.charDataCustom[i]);
		}
        loadChars.ClearOptions();
        loadChars.AddOptions(savedChars);
    }

    public void SetStat(string stj) {
        string[] stj2 = stj.Split(',');
        speedSl.value = float.Parse(stj2[0]);
        turnSl.value = float.Parse(stj2[1]);
        jumpSl.value = float.Parse(stj2[2]);
    }

    public void SetSkin() {
        int a = Mathf.RoundToInt(skinSL.value);
        characterMat.SetTexture("Albedo",skins[a]);
    }

    public void SetName() {
        currentCharData.name = nameBox.text;
    }

    public void Save(bool forSure) {
        if (!exiting) {
            if (currentCharData.name == "") {
                warningText.text = "Character name is required.";
            }
            else {
                if (!forSure) {
                    charDataPath = Path.Combine(charDir, currentCharData.name+".sbcc");
                    if (File.Exists (charDataPath)) {
                        warningText.text = "The character \"" + currentCharData.name + "\" already exists.\nDo you want to overwrite this character?";
                    }
                    if (!File.Exists (charDataPath)) {
                        warningText.text = "Are you sure you are ready to save the character \"" + currentCharData.name + "\"?";
                    }
                }
                else {
                    currentCharData.creator = GameRam.currentSaveFile.fileName;
                    currentCharData.updateTimeStamp = System.DateTime.Now.ToString();
                    SaveChar(currentCharData, charDataPath);
                    print ("Successfully saved " + currentCharData.name);
                    UpdateLoadList();
                }
            }
        }
        else {
            UpdateLoadList();
            StartCoroutine(LoadScene("HubTown"));
        }
	}

    public void Back() {
        exiting = false;
    }

    public void Exit() {
        exiting = true;
        warningText.text = "Unsaved changes will be lost.\nContinue?";
	}

    public void Load() {
        currentCharData = LoadChar(charFile[loadChars.value]);
        newName.text = currentCharData.name;
        speedSl.value = currentCharData.speed;
        turnSl.value = currentCharData.turn;
        jumpSl.value = currentCharData.jump;
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

	IEnumerator LoadScene(string sceneToLoad) {
		GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
