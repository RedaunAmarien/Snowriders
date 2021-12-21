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
    public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay, startTime;
    
    void Start() {
		fadePanel.gameObject.SetActive(true);
		StartCoroutine(Fade(true));
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
        currentCharData.speed = (int)speedSl.value;
        currentCharData.turn = (int)turnSl.value;
        currentCharData.jump = (int)jumpSl.value;
        currentCharData.skinCol = (int)skinSL.value;
        currentCharData.custom = true;
    }

    void UpdateLoadList() {
        GameRam.charDataCustom.Clear();
        // Update Custom list for customization.
        charFile = Directory.GetFiles(charDir, "*.sbcc");
        // GameRam.charDataCustom = new CharacterData[charFile.Length];
        List<string> savedChars = new List<string> {};
        for (int i = 0; i < charFile.Length; i++) {
            GameRam.charDataCustom.Add(LoadChar(charFile[i]));
            savedChars.Add(GameRam.charDataCustom[i].name);
        }

        // Update Total list for gameplay.
        GameRam.allCharData.Clear();
        GameRam.allCharData.AddRange(GameRam.charDataPermanent);
        GameRam.allCharData.AddRange(GameRam.charDataCustom);
		// for (int i = 0; i < GameRam.charDataPermanent.Count; i++) {
		// 	GameRam.allCharData.Add(GameRam.charDataPermanent[i]);
		// }
		// for (int i = 0; i < GameRam.charDataCustom.Count; i++) {
		// 	GameRam.allCharData.Add(GameRam.charDataCustom[i]);
		// }
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
        int a = (int)skinSL.value;
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
            StartCoroutine(Fade(false));
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
		string jsonString = JsonUtility.ToJson (charData, true);
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
}
