using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.UI;
using System.IO;

public class BattleMenu : MonoBehaviour {

	public GameObject pressStart, countSet, characterSet, mainSet, optionSet, courseSet, loadSet, player3And4, readySet, bypassButton;
    public GameObject[] battleSub;
    public CharacterPrep[] battleSubScript;
    public AudioSource[] songLayer;
    public Sprite[] charPortSrc, playPortSrc;
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
        battleSubScript = new CharacterPrep[4];
        battleSub = new GameObject[4];

        GameRam.controlp = new int[4];
        GameRam.charForP = new int[4];
        GameRam.boardForP = new int[4];
        GameRam.inpUse = new InputUser[4];
        GameRam.inpDev = new InputDevice[4];

		// Main Menu
        loadSet.SetActive(false);
		mainSet.SetActive(false);
		optionSet.SetActive(false);
		courseSet.SetActive(false);
		
		// Set defaults
        GameRam.lapCount = 0;
        playersReady = 0;

        //Game Mode 0 = Battle(Multiplayer), 1 = Adventure, 2 = Challenge, 3 = Online(Unused).
        if (GameRam.gameMode == GameMode.Battle) {
            titleText.text = "Battle Mode";
            countSet.SetActive(false);
            characterSet.SetActive(true);
            StartCoroutine(StartSong(1));
            optionButton.interactable = true;
            GameRam.itemsOn = true;
            GameRam.coinsOn = true;
            items.isOn = true;
            coins.isOn = true;
        }
	}

    void Update() {
        if (playersReady == GameRam.playerCount && GameRam.playerCount != 0) {
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
            for (int i = 0; i < GameRam.playerCount; i++)
            {
                battleSubScript[i].mpEvents = gameObject.GetComponent<MultiplayerEventSystem>();
                battleSubScript[i].mpEvents.firstSelectedGameObject = bypassButton;
                battleSubScript[i].mpEvents.playerRoot = null;
            }
            readySet.SetActive(false);
            mainSet.SetActive(true);
            for (int i = 0; i < GameRam.playerCount; i++)
            {
                battleSub[i].transform.localPosition = new Vector3 (transform.position.x, transform.position.y, 350);
            }
            playersReady = 0;
            // for (int i = GameRam.playerCount; i < 4; i++) {
            //     // Fix later to be specific levels.
            //     GameRam.charForP[i] = Random.Range(0, GameRam.allCharData.Count);
            //     GameRam.boardForP[i] = Random.Range(0, GameRam.boardData.Length);
            // }
            StartCoroutine(StartSong(2));
            StartCoroutine(StopSong(1));
        }
    }

    public void Backup() {
        StartCoroutine(LoadScene("HubTown"));
    }

    public void OnPlayerJoined(PlayerInput player) {
        Debug.Log("Player " + (player.user.index) + " joined.");
        GameRam.playerCount = player.user.index+1;
        if (GameRam.playerCount > 2) {
            player3And4.SetActive(true);
        }
        GameRam.inpUse[player.user.index] = player.user;
        GameRam.inpDev[player.user.index] = player.user.pairedDevices[0];
        Debug.Log(player.user.id + "\n" + player.user.pairedDevices[0]);
    }

    // public void OnPlayerLeft(PlayerInput player) {
    //     Debug.Log("Player " + player.user.index + " disconnected.");
    //     GameRam.playerCount --;
    //     battleSub[player.user.index].SetActive(false);
    //     if (GameRam.playerCount < 3) player3And4.SetActive(false);
    // }

	public void SetLaps() {
        if (laps.value == 0) {
            lapCountText.text = "Lap Count: Default";
        }
        else {
            lapCountText.text = "Lap Count: " + laps.value;
        }
        GameRam.lapCount = Mathf.RoundToInt(laps.value);
	}

	public void ItemToggle() {
		GameRam.itemsOn = items.isOn;
	}

	public void CoinToggle() {
		GameRam.coinsOn = coins.isOn;
	}

	public void ChooseCourse(string sceneName) {
        GameRam.courseToLoad = sceneName;
        StartCoroutine(LoadScene("TrackContainer"));
	}

	IEnumerator LoadScene(string sceneToLoad) {
		GameRam.nextSceneToLoad = sceneToLoad;
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
