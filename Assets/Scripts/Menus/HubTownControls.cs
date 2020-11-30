using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;
using TMPro;

public class HubTownControls : MonoBehaviour {

	public float moveTime;
	int conNum;
	public GameObject cam;
	public GameObject loadSet;
	public List <GameObject> place;
	public int currentPlace;
	public Slider progressBar;

	public TextMeshProUGUI goldCount, silverCount, bronzeCount, coinCount, lowTicketCount, midTicketCount, highTicketCount, destDesc;
	// public SaveFileData saveData;
	// public CustomCharacterData charData;
	string lStickH, aBut;
	// string bBut, lStickV, rStickH, rStickV, xBut, yBut;
	bool selected = false;
	bool bump = false;
    public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay, startTime;

	void Start() {
		fadePanel.gameObject.SetActive(true);
		StartCoroutine(Fade(true, null));
		//Fill in UI elements from save data.
		int golds = 0;
		int silvers = 0;
		int bronzes = 0;
		for (int i = 0; i < GameRam.currentSaveFile.courseGrade.Length; i++) {
			if (GameRam.currentSaveFile.courseGrade[i] == SaveData.CourseGrade.Bronze) bronzes ++;
			if (GameRam.currentSaveFile.courseGrade[i] == SaveData.CourseGrade.Silver) silvers ++;
			if (GameRam.currentSaveFile.courseGrade[i] == SaveData.CourseGrade.Gold) golds ++;
		}
		goldCount.text = golds.ToString();
		silverCount.text = silvers.ToString();
		bronzeCount.text = bronzes.ToString();
		coinCount.text = GameRam.currentSaveFile.coins.ToString("N0");
		lowTicketCount.text = GameRam.currentSaveFile.ticketBronze.ToString();
		midTicketCount.text = GameRam.currentSaveFile.ticketSilver.ToString();
		highTicketCount.text = GameRam.currentSaveFile.ticketGold.ToString();
	}

	void OnNavigate(InputValue val)
	{
		float v = val.Get<Vector2>().x;
		if (v <.5f && v > -.5f)
		{
			bump = false;
		}
		if (!bump && v > 0.5f)
		{
			currentPlace += 1;
			if (currentPlace >= place.Count)
			{
				currentPlace = 0;
			}
			bump = true;
		}
		if (!bump && v < -0.5f)
		{
			currentPlace -= 1;
			if (currentPlace <= -1)
			{
				currentPlace = place.Count-1;
			}
			bump = true;
		}
	}

	void OnSubmit()
	{
		switch (currentPlace) {
			case 0:
				GameRam.gameMode = GameMode.Story;
				GameRam.maxPlayerCount = 1;
				StartCoroutine(Fade(false, "RacePrepMenu"));
			break;

			case 1:
				GameRam.gameMode = GameMode.Battle;
				GameRam.maxPlayerCount = 4;
				StartCoroutine(Fade(false, "RacePrepMenu"));
			break;

			case 2:
				GameRam.gameMode = GameMode.Online;
				GameRam.maxPlayerCount = 2;
				Debug.LogWarning("Unavailable");
			break;

			case 3:
				Debug.LogWarning("Unavailable");
			break;

			case 4:
				StartCoroutine(Fade(false, "MainMenu"));
			break;

			case 5:
				StartCoroutine(Fade(false, "Shop"));
			break;

			case 6:
				// StartCoroutine(Fade(false, "CharacterEditor"));
				Debug.LogWarning("Unavailable");
			break;

			case 7:
				GameRam.gameMode = GameMode.Challenge;
				GameRam.maxPlayerCount = 1;
				StartCoroutine(Fade(false, "RacePrepMenu"));
			break;
		}
	}

	void FixedUpdate ()
	{
		Vector3 a = Vector3.zero;
		transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, place[currentPlace].transform.position, ref a, moveTime), transform.rotation);
		transform.LookAt(2 * transform.position - cam.transform.position);
	}

	void LateUpdate() {
		cam.transform.LookAt(transform.position);
        if (fadingIn) {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(1f, 0f, t));
        }
        else if (fadingOut) {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(0f, 1f, t));
        }
        
    }

	void OnTriggerEnter (Collider other) {
		switch (other.name) {
				case "Option":
					destDesc.text = "Change game settings and options.\n<color=red>(Unavailable)</color>";
				break;
				case "Shop":
					destDesc.text = "Buy boards with <color=yellow>tickets</color> from <color=green>Challenges</color>.\nBuy outfits and accessories with <color=yellow>coins</color>.\n<color=red>(Under Construction)</color>";
				break;
				case "Custom":
					destDesc.text = "Create and customize personal characters.\nUse <color=yellow>coins</color> to buy new outfits and accessories at the <color=green>Shop</color>.\n<color=red>(Under Construction)</color>";
				break;
				case "Story":
					destDesc.text = "Earn <color=yellow>medals</color> and <color=yellow>coins</color> as you progress through the story.";
				break;
				case "Battle":
					destDesc.text = "Earn <color=yellow>coins</color> with friends connected to the same computer.";
				break;
				case "Online":
					destDesc.text = "Earn <color=yellow>coins</color> with friends and other players online.\n<color=red>(Unavailable)</color>";
				break;
				case "Challenge":
					destDesc.text = "Earn <color=yellow>tickets</color> by completing various challenges, like time trials, trick combos, and expert boss battles.\n<color=red>(Under Construction)</color>";
				break;
				case "Exit":
					destDesc.text = "Return to the <color=green>Title Menu</color>.";
				break;
		}
	}

	IEnumerator LoadScene(string sceneToLoad) {
		selected = true;
		GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
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
}
