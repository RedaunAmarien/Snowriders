using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;

public class Shop : MonoBehaviour {

    [Tooltip("0 = Name, 1 = Speed, 2 = Turn, 3 = Jump, 4 = Flavor")]
    public Text[] boardInfoText;
    [Tooltip("0 = Coin, 1 = Bronze, 2 = Silver, 3 = Gold")]
    public Text[] costs, funds;
    public int currentChoice;
    public GameObject loadSet, warnSet, soldOutSign, lazySusan;
    public GameObject[] boards;
    public Slider progressBar;
    bool stickMove;
    SaveData reloadData;
    public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay, startTime;

    void Start() {
        currentChoice = 4;
		fadePanel.gameObject.SetActive(true);
		StartCoroutine(Fade(true));
    }

    void Update() {
        funds[0].text = GameRam.currentSaveFile.coins.ToString("N0");
        funds[1].text = GameRam.currentSaveFile.ticketBronze.ToString();
        funds[2].text = GameRam.currentSaveFile.ticketSilver.ToString();
        funds[3].text = GameRam.currentSaveFile.ticketGold.ToString();

        lazySusan.transform.SetPositionAndRotation(lazySusan.transform.position, Quaternion.Euler(0, currentChoice*36f, 0));

        boardInfoText[0].text = GameRam.allBoards[currentChoice].name;
        boardInfoText[1].text = "Speed: " + GameRam.allBoards[currentChoice].speed;
        boardInfoText[2].text = "Turn: " + GameRam.allBoards[currentChoice].turn;
        boardInfoText[3].text = "Jump: " + GameRam.allBoards[currentChoice].jump;
        boardInfoText[4].text = GameRam.allBoards[currentChoice].description;

        if (GameRam.ownedBoards.Contains(GameRam.allBoards[currentChoice])) {
            for (int i = 0; i < 4; i++) {
                boardInfoText[i].color = Color.gray;
            }
            soldOutSign.SetActive(true);
        }
        else {
            costs[0].text = GameRam.allBoards[currentChoice].boardCost.coins.ToString("N0");
            costs[1].text = GameRam.allBoards[currentChoice].boardCost.bronzeTickets.ToString();
            costs[2].text = GameRam.allBoards[currentChoice].boardCost.silverTickets.ToString();
            costs[3].text = GameRam.allBoards[currentChoice].boardCost.goldTickets.ToString();
            for (int i = 0; i < 4; i++) {
                boardInfoText[i].color = Color.white;
            }
            soldOutSign.SetActive(false);
        }
    }

    public void OnSubmit() {
        ItemCost board = GameRam.allBoards[currentChoice].boardCost;
        SaveData save = GameRam.currentSaveFile;
        if (board.coins > save.coins
            || board.bronzeTickets > save.ticketBronze
            || board.silverTickets > save.ticketSilver
            || board.goldTickets > save.ticketGold) {
            // Not Enough.
        }
        else if (GameRam.ownedBoards.Contains(GameRam.allBoards[currentChoice])) {
            // Already Owned.
        }
        else {
            GameRam.currentSaveFile.ownedBoardID.Add(GameRam.allBoards[currentChoice].boardID);
            GameRam.currentSaveFile.coins -= board.coins;
            GameRam.currentSaveFile.ticketBronze -= board.bronzeTickets;
            GameRam.currentSaveFile.ticketSilver -= board.silverTickets;
            GameRam.currentSaveFile.ticketGold -= board.goldTickets;
            GameRam.currentSaveFile.lastSaved = System.DateTime.Now;
			FileManager.SaveFile(GameRam.currentSaveFile.fileName, GameRam.currentSaveFile, Application.persistentDataPath + "/Saves");
            GameRam.ownedBoards.Clear();
			foreach (int id in GameRam.currentSaveFile.ownedBoardID) {
				foreach (Board boardData in GameRam.allBoards) {
					if (boardData.boardID == id) {
						GameRam.ownedBoards.Add(boardData);
					}
				}
			}
            // reloadData = LoadFile(GameRam.currentSaveDirectory);
        }
    }

    public void OnCancel() {
        StartCoroutine(Fade(false));
    }

    public void OnNavigate(InputValue val) {
        var v = val.Get<Vector2>();

        if (v.x > .5f && !stickMove) {
            currentChoice ++;
            if (currentChoice > GameRam.allBoards.Count-1) currentChoice = 4;
            stickMove = true;
        }
        else if (v.x < -.5f && !stickMove) {
            currentChoice --;
            if (currentChoice < 4) currentChoice = GameRam.allBoards.Count-1;
            stickMove = true;
        }
        else if (v.x > -.5f && v.x < .5f) stickMove = false;
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