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

public class RacePrep : MonoBehaviour {

	public GameObject pressStart, countSet, characterSet, mainSet, optionSet, courseSet, loadSet, readySet, bypassButton;
    public GameObject[] charPrep;
    public CharacterPrep[] charPrepScript;
    public AudioSource[] songLayer;
    public Sprite[] charPortSrc;
    public Sprite playPortSrc;
    public float maxLayerVolume, volumePercentSpeed;
    public Text lapCountText, titleText;
    public Button optionButton;
	public Toggle items, coins;
    public Slider progressBar, laps;
    public int playersReady;
    bool readyToGo;
    public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay, startTime;

    public ChallengeConditions[] challenges;

    void Awake() {
        songLayer[0].volume = 0;
        songLayer[1].volume = 0;
        songLayer[2].volume = 0;
        songLayer[3].volume = 0;
    }

	void Start () {
		fadePanel.gameObject.SetActive(true);

        charPrep = new GameObject[4];
        charPrepScript = new CharacterPrep[4];
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
        countSet.SetActive(false);
        characterSet.SetActive(true);
        StartCoroutine(StartSong(1));
        switch (GameRam.gameMode) {
            case GameMode.Challenge:
                titleText.text = "Challenge Mode";
                optionButton.interactable = false;
                GameRam.itemsOn = false;
                GameRam.coinsOn = false;
                // items.isOn = false;
                // coins.isOn = false;
            break;
            case GameMode.Story:
                titleText.text = "Story Mode";
                optionButton.interactable = false;
                GameRam.itemsOn = true;
                GameRam.coinsOn = true;
                items.isOn = true;
                coins.isOn = true;
            break;
            case GameMode.Battle:
                titleText.text = "Battle Mode";
                optionButton.interactable = true;
                GameRam.itemsOn = true;
                GameRam.coinsOn = true;
                items.isOn = true;
                coins.isOn = true;
            break;
            default:
                Debug.LogErrorFormat("Gamemode {0} not set up.", GameRam.gameMode);
            break;
        }

		StartCoroutine(Fade(true, null));
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
            readySet.SetActive(false);
            charPrepScript[0].mpEvents.firstSelectedGameObject = bypassButton;
            charPrep[0].transform.localPosition = new Vector3 (transform.position.x, transform.position.y, 350);
            mainSet.SetActive(true);
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
        StartCoroutine(Fade(false, "HubTown"));
    }

    public void OnPlayerJoined(PlayerInput player) {
        GameRam.playerCount = player.user.index + 1;
        // if (GameRam.playerCount > 2) {
        //     player3And4.SetActive(true);
        // }
        GameRam.inpUse[player.user.index] = player.user;
        GameRam.inpDev[player.user.index] = player.user.pairedDevices[0];
        Debug.LogFormat("Player {0} joined using {1}.", player.user.id, player.user.pairedDevices[0]);
    }

    // public void OnPlayerLeft(PlayerInput player) {
    //     Debug.Log("Player " + player.user.index + " disconnected.");
    //     GameRam.playerCount --;
    //     // advSub.SetActive(false);
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

	public void ChooseCourse(ChosenCourse course) {
        if (course.isChallenge) {
            GameRam.currentChallenge = challenges[course.challengeIndex];
        }
        if (course.useCutscene) {
            StartCoroutine(Fade(false, course.cutsceneName));
        }
        else {
            GameRam.courseToLoad = course.trackName;
            StartCoroutine(Fade(false, "TrackContainer"));
        }
	}

	IEnumerator LoadScene(string sceneToLoad) {
        //Reroute through loading screen to load scene.
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
    
    public IEnumerator Fade(bool i, string destination) {
		if (i) fadingIn = true;
		else fadingOut = true;
		startTime = Time.time;
		yield return new WaitForSeconds(fadeDelay);
		if (i) fadingIn = false;
		else {
			fadingOut = false;
			StartCoroutine(LoadScene(destination));
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
