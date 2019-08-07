using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerUI : MonoBehaviour {

	// UI Section
	public int playerNum;
	public float camRotSpeed;
	public Vector3 camAngle, camOffset;
	public float camSmoothTime;
	public Canvas canvas;
	public Text lapText, coinText, shotsText, trickText, timeText, lapTimeDisplay;
	public string[] lapTimeValue;
	public int[] lapTime;
	public int runningLapTime;
	public bool timerOn, paused;
	public Image blueItem, redItem, placement;
	public GameObject pauseMenu, pauseResume, finishGraphic;
	public EventSystem events;
	public StandaloneInputModule inputMod;
	public GameObject itemDisplay, assignedCam;
	public Sprite[] itemSprite, weaponSprite, placeSprite;
	public bool finished, camIsReversed;
	RacerPhysics rPhys;
	CourseControl corCon;
	public Camera subCam;

	void Start () {
		// Find objects.
		corCon = GameObject.Find("CourseSettings").GetComponent<CourseControl>();
		rPhys = GetComponent<RacerPhysics>();
		subCam = assignedCam.GetComponentInChildren<Camera>();
		events = GameObject.Find("EventSystem").GetComponent<EventSystem>();
		inputMod = GameObject.Find("EventSystem").GetComponent<StandaloneInputModule>();
		lapTime = new int[rPhys.totalLaps];
		lapTimeValue = new string[rPhys.totalLaps];

		// Initialize camera
		assignedCam.transform.SetPositionAndRotation(transform.position, transform.rotation);
		// subCam.transform.position = transform.position + camOffset;
		subCam.transform.LookAt(transform.position + camAngle);
		finishGraphic.SetActive(false);
		pauseMenu.SetActive(false);
		if (GameVar.gameMode == 2) placement.gameObject.SetActive(false);
		if (!GameVar.itemsOn) itemDisplay.SetActive(false);
		if (GameVar.playerCount == 1) canvas.worldCamera = GameObject.Find("Main Camera 0").GetComponent<Camera>();
		if (GameVar.playerCount == 2) canvas.worldCamera = GameObject.Find("Main Camera "+(playerNum+1)).GetComponent<Camera>();
		if (GameVar.playerCount >= 3) canvas.worldCamera = GameObject.Find("Main Camera "+(playerNum+3)).GetComponent<Camera>();
		canvas.planeDistance = .5f;

		// Fix Audio Listeners at some point.
		subCam.GetComponent<AudioListener>().enabled = true;
	}

	void Update () {
		if (!finished) {
			// Update UI texts.
			lapText.text = "Lap " + rPhys.currentLap + "/" + rPhys.totalLaps;
			coinText.text = rPhys.coins.ToString();
			if (rPhys.place == 1) {
				placement.sprite = placeSprite[0];
				placement.rectTransform.localScale = new Vector3 (.4f, .4f, 1);
			}
			if (rPhys.place == 2) {
				placement.sprite = placeSprite[1];
				placement.rectTransform.localScale = new Vector3 (.35f, .35f, 1);
			}
			if (rPhys.place == 3) {
				placement.sprite = placeSprite[2];
				placement.rectTransform.localScale = new Vector3 (.3f, .3f, 1);
			}
			if (rPhys.place == 4) {
				placement.sprite = placeSprite[3];
				placement.rectTransform.localScale = new Vector3 (.25f, .25f, 1);
			}
			if (rPhys.shots <= 0) shotsText.text = "";
			else shotsText.text = rPhys.shots.ToString();
			timeText.text = corCon.printTimer;
		}
		
		else finishGraphic.SetActive(true);

		// Update item and weapon displays.
		blueItem.sprite = itemSprite[rPhys.itemType];
		redItem.sprite = weaponSprite[rPhys.weapType];
	}

	void FixedUpdate() {
		// Camera Follow
		Vector3 vel = Vector3.zero;
		assignedCam.transform.position = Vector3.SmoothDamp(assignedCam.transform.position, transform.TransformPoint(camOffset), ref vel, camSmoothTime);
		if (camIsReversed) {
			subCam.transform.LookAt(transform.position + camAngle, Vector3.up);
		}
		else {
			subCam.transform.LookAt(transform.position + camAngle, Vector3.up);
		}
		if (finished) {
			subCam.transform.RotateAround(transform.position, Vector3.up, camRotSpeed);
		}
	}

	public void Pause (int option) {
		if (option == 0) {
			Time.timeScale = 0;
			corCon.timerOn = false;
			paused = true;;
			inputMod.horizontalAxis = rPhys.playerNum + " Axis 1";
			inputMod.verticalAxis = rPhys.playerNum + " Axis 2";
			inputMod.submitButton = rPhys.playerNum + " Button 0";
			inputMod.cancelButton = rPhys.playerNum + " Button 1";
			events.SetSelectedGameObject(pauseResume);
			pauseMenu.SetActive(true);
		}
		else if (option == 1) {
			corCon.timerOn = true;
			Time.timeScale = 1;
			paused = false;
			pauseMenu.SetActive(false);
		}
		else if (option == 2) {
			Time.timeScale = 1;
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		else if (option == 3) {
			Time.timeScale = 1;
			SceneManager.LoadScene("HubTown");
		}
	}

	public IEnumerator TrickComplete (int total) {
		Text textBox = Instantiate(trickText, new Vector2(0,0), Quaternion.Euler(0,0,0), canvas.gameObject.transform);
		textBox.rectTransform.localPosition = new Vector3(0,0,0);
		textBox.rectTransform.localEulerAngles = new Vector3(0,0,0);
		textBox.text = "TRICK!  " + total + "P";
		yield return new WaitForSeconds(1.5f);
		Destroy(textBox.gameObject);
	}

	public void LapTime (int lap) {
		Text textBox = Instantiate(lapTimeDisplay, new Vector3(0,0,0), Quaternion.Euler(0,0,0), timeText.gameObject.transform);
		textBox.rectTransform.anchoredPosition3D = new Vector3(0, -50*lap, 0);
		textBox.rectTransform.localEulerAngles = new Vector3(0,0,0);
		int newLapTime = corCon.totalTimer;
		if (lap == 1) {
			lapTime[lap-1] = newLapTime;
		}
		else {
			lapTime[lap-1] = newLapTime - runningLapTime;
		}
		int t1 = 0, t2 = 0, t3 = 0, t4 = 0, t5 = 0;
		t1 = lapTime[lap-1];
		for (int i = t1; i >= 10; i -= 10) {
			t2 ++;
			t1 -= 10;
			if (t2 >= 10) {
				t2 -= 10;
				t3 ++;
			}
			if (t3 >= 10) {
				t3 -= 10;
				t4 ++;
			}
			if (t4 >= 6) {
				t4 -= 6;
				t5 ++;
			}
		}
		lapTimeValue[lap-1] = t5 + ":" + t4 + t3 + "." + t2 + t1;
		textBox.text = lapTimeValue[lap-1] + " Lap " + lap;
		runningLapTime = corCon.totalTimer;
	}
}
