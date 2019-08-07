using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class Shop : MonoBehaviour {

    public Text boardInfo, flavorText, totalPoints;
    public int currentChoice;
    public GameObject loadSet, warnSet;
    public Slider progressBar;
    bool stickMove;
    SaveFileData reloadData;

    void Start() {
        
    }

    void Update() {
        totalPoints.text = ("Points Available: " + GameVar.currentSaveFile.coins + "P");
        flavorText.text = GameVar.boardData[currentChoice].description;

        if (GameVar.currentSaveFile.boardOwned[currentChoice]) {
            boardInfo.text = GameVar.boardData[currentChoice].name + "\nSpeed: " + GameVar.boardData[currentChoice].speed + "\nTurn: " + GameVar.boardData[currentChoice].turn + "\nJump: " + GameVar.boardData[currentChoice].jump + "\nSOLD OUT";
            boardInfo.color = Color.green;
        }
        else {
            boardInfo.text = GameVar.boardData[currentChoice].name + "\nSpeed: " + GameVar.boardData[currentChoice].speed + "\nTurn: " + GameVar.boardData[currentChoice].turn + "\nJump: " + GameVar.boardData[currentChoice].jump + "\nPrice: " + GameVar.boardData[currentChoice].price + "P";
            boardInfo.color = Color.white;
        }
        if (Input.GetButtonDown("0 Button 1")) StartCoroutine(LoadScene("HubTown"));
        
        if (Input.GetButtonDown("0 Button 0")) {
            if (GameVar.boardData[currentChoice].price > GameVar.currentSaveFile.coins) {
                // Not Enough.
            }
            else if (GameVar.currentSaveFile.boardOwned[currentChoice]) {
                // Already Owned.
            }
            else {
                GameVar.currentSaveFile.boardOwned[currentChoice] = true;
                GameVar.currentSaveFile.coins -= GameVar.boardData[currentChoice].price;
                SaveFile(GameVar.currentSaveFile, GameVar.currentSaveDirectory);
                // reloadData = LoadFile(GameVar.currentSaveDirectory);
            }
        }
        else if (Input.GetAxisRaw("0 Axis 1") > .5f && !stickMove) {
            currentChoice ++;
            if (currentChoice > GameVar.boardData.Length-1) currentChoice = 0;
            stickMove = true;
        }
        else if (Input.GetAxisRaw("0 Axis 1") < -.5f && !stickMove) {
            currentChoice --;
            if (currentChoice < 0) currentChoice =GameVar.boardData.Length-1;
            stickMove = true;
        }
        else if (Input.GetAxisRaw("0 Axis 1") > -.5f && Input.GetAxisRaw("0 Axis 1") < .5f) stickMove = false;
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

	IEnumerator LoadScene(string sceneToLoad) {
		GameVar.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
