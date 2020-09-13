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

	void Start () {

        TMP_Dropdown.OptionDataList nameData = new TMP_Dropdown.OptionDataList();
        for (int i = 0; i < GameRam.allCharData.Count; i++) {
            nameData.options.Add(new TMP_Dropdown.OptionData(GameRam.allCharData[i].name));
        }
        for (int i = 0; i < 4; i++) {
            charSelectors[i].ClearOptions();
            charSelectors[i].options = nameData.options;
        }

        TMP_Dropdown.OptionDataList bNameData = new TMP_Dropdown.OptionDataList();
        for (int i = 0; i < GameRam.boardData.Length; i++) {
            bNameData.options.Add(new TMP_Dropdown.OptionData(GameRam.boardData[i].name));
        }
        for (int i = 0; i < 4; i++) {
            charSelectors[i].ClearOptions();
            charSelectors[i].options = bNameData.options;
        }

        GameRam.controlp = new int[4];
        GameRam.charForP = new int[4];
        GameRam.boardForP = new int[4];
        GameRam.inpUse = new InputUser[4];
        GameRam.inpDev = new InputDevice[4];
		
		// Set defaults
        GameRam.lapCount = 0;
        playersReady = 0;
        
        StartCoroutine(StartSong(1));
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
        StartCoroutine(LoadScene("TrackContainer"));
    }

    public void Exit() {
        StartCoroutine(LoadScene("HubTown"));
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
}
