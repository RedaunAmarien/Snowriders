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
using TMPro;

public class BattleMenuTemp : MonoBehaviour {

    public TMP_Dropdown[] charSelectors, boardSelectors;
    public TMP_Dropdown courseDrop;
    public TextMeshProUGUI playerCount;
	public Toggle items, coins;
    public TMP_InputField laps;
    public AudioSource[] songLayer;
    public float maxLayerVolume, volumePercentSpeed;
    public Slider progressBar;
    public int playersReady;
    public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay, startTime;

	void Start () {
		fadePanel.gameObject.SetActive(true);
		StartCoroutine(Fade(true, null));

        TMP_Dropdown.OptionDataList nameData = new TMP_Dropdown.OptionDataList();
        for (int i = 0; i < GameRam.allCharData.Count; i++) {
            nameData.options.Add(new TMP_Dropdown.OptionData(string.Format("{0}: {1}/{2}/{3}", GameRam.allCharData[i].name, GameRam.allCharData[i].speed, GameRam.allCharData[i].turn, GameRam.allCharData[i].jump)));
        }
        for (int i = 0; i < 4; i++) {
            charSelectors[i].ClearOptions();
            charSelectors[i].options = nameData.options;
        }

        TMP_Dropdown.OptionDataList bNameData = new TMP_Dropdown.OptionDataList();
        for (int i = 0; i < GameRam.currentSaveFile.boardsOwned.Count; i++) {
            bNameData.options.Add(new TMP_Dropdown.OptionData(string.Format("{0}: +{1}/{2}/{3}", GameRam.ownedBoardData[i].name, GameRam.ownedBoardData[i].speed, GameRam.ownedBoardData[i].turn, GameRam.ownedBoardData[i].jump)));
        }
        for (int i = 0; i < 4; i++) {
            boardSelectors[i].ClearOptions();
            boardSelectors[i].options = bNameData.options;
        }

        GameRam.controlp = new int[4];
        GameRam.charForP = new int[4];
        GameRam.boardForP = new int[4];
        GameRam.inpUse = new InputUser[4];
        GameRam.inpDev = new InputDevice[4];
		
		// Set defaults
        GameRam.lapCount = 0;
        GameRam.itemsOn = true;
        GameRam.coinsOn = true;
        playersReady = 0;
        
        // StartCoroutine(StartSong(1));
	}

    public void OnPlayerJoined(PlayerInput player) {
        Debug.Log("Player " + (player.user.index) + " joined.");
        GameRam.playerCount = player.user.index+1;
        GameRam.inpUse[player.user.index] = player.user;
        GameRam.inpDev[player.user.index] = player.user.pairedDevices[0];
        playerCount.text = GameRam.playerCount + "players entered.";
        Debug.Log(player.user.id + "\n" + player.user.pairedDevices[0]);
    }

    public void CharSelect(int p) {
        GameRam.charForP[p] = charSelectors[p].value;
    }

    public void BoardSelect(int p) {
        GameRam.boardForP[p] = boardSelectors[p].value;
    }

	public void ChooseCourse() {
        GameRam.courseToLoad = "Track"+(courseDrop.value+1);
	}

	public void SetLaps() {
        if (laps.text != null) GameRam.lapCount = int.Parse(laps.text);
	}

	public void ItemToggle() {
		GameRam.itemsOn = items.isOn;
	}

	public void CoinToggle() {
		GameRam.coinsOn = coins.isOn;
	}

    public void Finish() {
        if (GameRam.courseToLoad == "Track0" || GameRam.courseToLoad == null) GameRam.courseToLoad = "Track1";
        StartCoroutine(Fade(false, "TrackContainer"));
    }

    public void Exit() {
        StartCoroutine(Fade(false, "HubTown"));
    }

	IEnumerator LoadScene(string sceneToLoad) {
		GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator StartSong(int layer) {
        for (float i = 0; i < maxLayerVolume; i += volumePercentSpeed) {
            songLayer[layer].volume = i;
            yield return null;
        }
        if (songLayer[layer].volume > maxLayerVolume) songLayer[layer].volume = maxLayerVolume;
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
