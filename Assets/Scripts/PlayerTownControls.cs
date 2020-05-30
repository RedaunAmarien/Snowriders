﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;

public class PlayerTownControls : MonoBehaviour {

	[Header("ControlSetup")]
    public InputAction turn;
    public InputAction select;

	public float moveTime;
	int conNum;
	public GameObject cam;
	public GameObject loadSet;
	public List <GameObject> place;
	public int currentPlace;
	public Slider progressBar;
	// public SaveFileData saveData;
	// public CustomCharacterData charData;
	string lStickH, aBut;
	// string bBut, lStickV, rStickH, rStickV, xBut, yBut;
	bool selected = false;
	bool bump = false;

	void Awake() {
        select.performed += ctx => Select();
        select.Enable();
        turn.performed += ctx => Turn();
        turn.Enable();
	}

	void Start() {
		// conNum = GameVar.controlp[0];
		// Load files.
		// saveData = LoadFile(Path.Combine(Path.Combine(Application.persistentDataPath, "Saves"), "Save_" + GameVar.saveSlot + ".sbsv"));
		// charData = LoadChar(Path.Combine(Path.Combine(Application.persistentDataPath, "Characters"), "_" + GameVar.saveData.charName + ".sbcc"));

		// lStickH = (conNum + " Axis 1");
		// lStickV = (conNum+" Axis 2");
		// rStickH = (conNum+" Axis 4");
		// rStickV = (conNum+" Axis 5");
		// aBut = (conNum + " Button 0");
		// bBut = (conNum +" Button 1");
		// xBut = (conNum +" Button 2"); //Button 2 or Axis 9
		// yBut = (conNum +" Button 3"); //Button 3 or Axis 10
	}

	void Update () {
		Vector3 a = Vector3.zero;
		transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, place[currentPlace].transform.position, ref a, moveTime), transform.rotation);
		if (turn.ReadValue<float>() == 0) {
			bump = false;
		}
		if (!bump && turn.ReadValue<float>() > 0.5f) {
			currentPlace += 1;
			if (currentPlace >= place.Count) {
				currentPlace = 0;
			}
			bump = true;
		}
		if (!bump && turn.ReadValue<float>() < -0.5f) {
			currentPlace -= 1;
			if (currentPlace <= -1) {
				currentPlace = place.Count-1;
			}
			bump = true;
		}
	}

	void Turn() {
		
	}

	void Select() {

	}

	void LateUpdate() {
		cam.transform.LookAt(transform.position);
	}

	void OnTriggerStay (Collider other) {
		if (select.ReadValue<float>() > 0 && !selected) {
			switch (other.name) {
				case "Option":
				Debug.LogError("Unavailable");
				break;
				case "Shop":
				StartCoroutine(LoadScene("Shop"));
				break;
				case "Custom":
				StartCoroutine(LoadScene("CharacterEditor"));
				break;
				case "Story":
				GameVar.gameMode = 1;
				StartCoroutine(LoadScene("BattleMenu"));
				break;
				case "Battle":
				GameVar.gameMode = 0;
				StartCoroutine(LoadScene("BattleMenu"));
				break;
				case "Online":
				Debug.LogError("Unavailable");
				break;
				case "Challenge":
				GameVar.gameMode = 2;
				StartCoroutine(LoadScene("BattleMenu"));
				break;
				case "Exit":
				StartCoroutine(LoadScene("MainMenu"));
				break;
			}
		}
	}

	// static CustomCharacterData LoadChar (string path) {
	// 	using (StreamReader streamReader = File.OpenText (path)) {
	// 		string jsonString = streamReader.ReadToEnd();
	// 		return JsonUtility.FromJson<CustomCharacterData> (jsonString);
	// 	}
	// }

	// static void SaveChar (CustomCharacterData charData, string path) {
	// 	string jsonString = JsonUtility.ToJson (charData);
	// 	using (StreamWriter streamWriter = File.CreateText(path)) {
	// 		streamWriter.Write (jsonString);
	// 	}
	// }
    
    // static SaveFileData LoadFile (string path) {
	// 	using (StreamReader streamReader = File.OpenText (path)) {
	// 		string jsonString = streamReader.ReadToEnd();
	// 		return JsonUtility.FromJson<SaveFileData> (jsonString);
	// 	}
	// }

    // static void SaveFile (SaveFileData saveData, string path) {
	// 	string jsonString = JsonUtility.ToJson (saveData);
	// 	using (StreamWriter streamWriter = File.CreateText(path)) {
	// 		streamWriter.Write (jsonString);
	// 	}
    // }

	IEnumerator LoadScene(string sceneToLoad) {
		selected = true;
		GameVar.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
