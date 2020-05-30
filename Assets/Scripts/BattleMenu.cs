using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.IO;

public class BattleMenu : MonoBehaviour {

	public GameObject countSet, characterSet, mainSet, optionSet, courseSet, loadSet, player3And4, readySet, bypassButton;
    public GameObject[] battleSub;
    public BattleSubMenu[] battleSubScript;
    public AudioSource[] songLayer;
    public Sprite[] charPortSrc;
    public float maxLayerVolume, volumePercentSpeed;
    public Text lapCountText, titleText;
    public Button optionButton;
	public Toggle items, coins;
    public Slider progressBar, laps;
    public int playersReady;
    bool readyToGo;

    void Awake() {
        songLayer[0].volume = 0;
        songLayer[1].volume = 0;
        songLayer[2].volume = 0;
        songLayer[3].volume = 0;
    }

	void Start () {
        battleSubScript = new BattleSubMenu[4];
        battleSub = new GameObject[4];

        GameVar.controlp = new int[4];
        GameVar.charForP = new int[4];
        GameVar.boardForP = new int[4];

		// Main Menu
        loadSet.SetActive(false);
		mainSet.SetActive(false);
		optionSet.SetActive(false);
		courseSet.SetActive(false);
		
		// Set defaults
        GameVar.lapCount = 0;
        playersReady = 0;

        //Game Mode 0 = Battle (Multiplayer), 1 = Adventure, 2 = Challenge, 3 = Online (Not implemented).
        if (GameVar.gameMode == 0) {
            titleText.text = "Battle Mode";
            countSet.SetActive(false);
            characterSet.SetActive(true);
            StartCoroutine(StartSong(1));
            optionButton.interactable = true;
            GameVar.itemsOn = true;
            GameVar.coinsOn = true;
            items.isOn = true;
            coins.isOn = true;
        }
        else {
            countSet.SetActive(false);
            characterSet.SetActive(true);
            StartCoroutine(StartSong(1));
            if (GameVar.gameMode == 1) {
                titleText.text = "Adventure Mode";
                optionButton.interactable = false;
                GameVar.itemsOn = true;
                GameVar.coinsOn = true;
                items.isOn = true;
                coins.isOn = true;
            }
            else if (GameVar.gameMode == 2) {
                titleText.text = "Challenge Mode";
                optionButton.interactable = true;
                GameVar.itemsOn = false;
                GameVar.coinsOn = false;
                items.isOn = false;
                coins.isOn = false;
            }
        }
	}

    void Update() {
        if (playersReady == GameVar.playerCount && GameVar.playerCount != 0) {
            readyToGo = true;
            readySet.SetActive(true);
        }
        else {
            readyToGo = false;
            readySet.SetActive(false);
        }
    }

    public void CheckGo() {
        if (readyToGo) {
            readySet.SetActive(false);
            mainSet.SetActive(true);
            characterSet.SetActive(false);
            playersReady = 0;
            battleSubScript[0].events.firstSelectedGameObject = bypassButton;
            battleSubScript[0].events.playerRoot = null;
            for (int i = GameVar.playerCount; i < 4; i++) {
                // Fix later to be specific levels.
                GameVar.charForP[i] = Random.Range(0, GameVar.allCharData.Length);
                GameVar.boardForP[i] = Random.Range(0, GameVar.boardData.Length);
            }
            StartCoroutine(StartSong(2));
            StartCoroutine(StopSong(1));
        }
    }

    public void Backup() {
        StartCoroutine(LoadScene("HubTown"));
    }

    public void OnPlayerJoined(PlayerInput player) {
        Debug.Log("Player " + player.user.index + " joined.");
        GameVar.playerCount = player.user.index+1;
        if (GameVar.playerCount > 2) {
            player3And4.SetActive(true);
        }
    }

    public void OnPlayerLeft(PlayerInput player) {
        Debug.Log("Player " + player.user.index + " disconnected.");
        GameVar.playerCount --;
        battleSub[player.user.index].SetActive(false);
        if (GameVar.playerCount < 3) player3And4.SetActive(false);
    }

	public void SetLaps() {
        if (laps.value == 0) {
            lapCountText.text = "Lap Count: Default";
        }
        else {
            lapCountText.text = "Lap Count: " + laps.value;
        }
        GameVar.lapCount = Mathf.RoundToInt(laps.value);
	}

	public void ItemToggle() {
		GameVar.itemsOn = items.isOn;
	}

	public void CoinToggle() {
		GameVar.coinsOn = coins.isOn;
	}

	public void ChooseCourse(string sceneName) {
		StartCoroutine(LoadScene(sceneName));
	}

	IEnumerator LoadScene(string sceneToLoad) {
		GameVar.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void StartSongButton(int newLayer) {
        StartCoroutine(StartSong(newLayer));
    }

    public void StopSongButton(int oldLayer) {
        StartCoroutine(StopSong(oldLayer));
    }

    IEnumerator StartSong(int layer) {
        for (float i = 0; i < maxLayerVolume; i += volumePercentSpeed) {
            songLayer[layer].volume = i;
            yield return null;
        }
        if (songLayer[layer].volume > maxLayerVolume) songLayer[layer].volume = maxLayerVolume;
    }
    IEnumerator StopSong(int layer) {
        for (float i = maxLayerVolume; i > 0; i -= volumePercentSpeed) {
            songLayer[layer].volume = i;
            yield return null;
        }
    }
}
