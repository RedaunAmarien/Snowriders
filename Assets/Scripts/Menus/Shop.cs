using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;

public class Shop : MonoBehaviour {

    public Text boardInfo, flavorText, totalPoints;
    public int currentChoice;
    public GameObject loadSet, warnSet;
    public Slider progressBar;
    bool stickMove;
    SaveData reloadData;

    void Start() {
        
    }

    void Update() {
        totalPoints.text = ("Coins Available: " + GameVar.currentSaveFile.coins);
        flavorText.text = GameVar.boardData[currentChoice].description;

        if (GameVar.currentSaveFile.boardOwned[currentChoice]) {
            boardInfo.text = GameVar.boardData[currentChoice].name + "\nSpeed: " + GameVar.boardData[currentChoice].speed + "\nTurn: " + GameVar.boardData[currentChoice].turn + "\nJump: " + GameVar.boardData[currentChoice].jump + "\nSOLD OUT";
            boardInfo.color = Color.green;
        }
        else {
            boardInfo.text = GameVar.boardData[currentChoice].name + "\nSpeed: " + GameVar.boardData[currentChoice].speed + "\nTurn: " + GameVar.boardData[currentChoice].turn + "\nJump: " + GameVar.boardData[currentChoice].jump + "\nPrice: " + GameVar.boardData[currentChoice].price + " coins";
            boardInfo.color = Color.white;
        }
    }

    public void OnSubmit() {
        if (GameVar.boardData[currentChoice].price > GameVar.currentSaveFile.coins) {
            // Not Enough.
        }
        else if (GameVar.currentSaveFile.boardOwned[currentChoice]) {
            // Already Owned.
        }
        else {
            GameVar.currentSaveFile.boardOwned[currentChoice] = true;
            GameVar.currentSaveFile.coins -= GameVar.boardData[currentChoice].price;
            FileManager.SaveFile(GameVar.currentSaveFile.fileName, GameVar.currentSaveFile);
            // reloadData = LoadFile(GameVar.currentSaveDirectory);
        }
    }

    public void OnCancel() {
        StartCoroutine(LoadScene("HubTown"));
    }

    public void OnNavigate(InputValue val) {
        var v = val.Get<Vector2>();

        if (v.x > .5f && !stickMove) {
            currentChoice ++;
            if (currentChoice > GameVar.boardData.Length-1) currentChoice = 0;
            stickMove = true;
        }
        else if (v.x < -.5f && !stickMove) {
            currentChoice --;
            if (currentChoice < 0) currentChoice =GameVar.boardData.Length-1;
            stickMove = true;
        }
        else if (v.x > -.5f && v.x < .5f) stickMove = false;

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
