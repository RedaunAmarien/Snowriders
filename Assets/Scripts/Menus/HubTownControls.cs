using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;
using TMPro;

public class HubTownControls : MonoBehaviour
{

    [SerializeField] private float moveTime;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject loadSet;
    [SerializeField] private List<GameObject> place;
    [SerializeField] private int currentPlace;
    [SerializeField] private Slider progressBar;

    [SerializeField] private TextMeshProUGUI bronzeMedalDisplay;
    [SerializeField] private TextMeshProUGUI silverMedalDisplay;
    [SerializeField] private TextMeshProUGUI goldMedalDisplay;
    [SerializeField] private TextMeshProUGUI bronzeTicketDisplay;
    [SerializeField] private TextMeshProUGUI silverTicketDisplay;
    [SerializeField] private TextMeshProUGUI goldTicketDisplay;
    [SerializeField] private TextMeshProUGUI coinDisplay;
    [SerializeField] private TextMeshProUGUI selectionDescription;

    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDelay, startTime;
    private bool fadingIn, fadingOut;
    private bool navHasReset = true;

    void Start()
    {
        fadePanel.gameObject.SetActive(true);
        StartCoroutine(Fade(true, null));
        //Fill in UI elements from save data.
        int bronzeMedals = 0;
        int silverMedals = 0;
        int goldMedals = 0;
        GameObject.Find("StartCam").GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 0;

        if (GameRam.currentSaveFile == null)
            return;

        for (int i = 0; i < GameRam.currentSaveFile.courseGrade.Length; i++)
        {
            if (GameRam.currentSaveFile.courseGrade[i] == SaveData.CourseGrade.Bronze) bronzeMedals++;
            if (GameRam.currentSaveFile.courseGrade[i] == SaveData.CourseGrade.Silver) silverMedals++;
            if (GameRam.currentSaveFile.courseGrade[i] == SaveData.CourseGrade.Gold) goldMedals++;
        }
        bronzeMedalDisplay.text = bronzeMedals.ToString();
        silverMedalDisplay.text = silverMedals.ToString();
        goldMedalDisplay.text = goldMedals.ToString();
        bronzeTicketDisplay.text = GameRam.currentSaveFile.ticketBronze.ToString();
        silverTicketDisplay.text = GameRam.currentSaveFile.ticketSilver.ToString();
        goldTicketDisplay.text = GameRam.currentSaveFile.ticketGold.ToString();
        coinDisplay.text = GameRam.currentSaveFile.coins.ToString("N0");
    }

    void OnNavigate(InputValue val)
    {
        place[currentPlace].GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().Priority = 0;
        float v = val.Get<Vector2>().x;
        if (v < .5f && v > -.5f)
        {
            navHasReset = true;
        }
        if (navHasReset && v > 0.5f)
        {
            currentPlace += 1;
            if (currentPlace >= place.Count)
            {
                currentPlace = 0;
            }
            navHasReset = false;
        }
        if (navHasReset && v < -0.5f)
        {
            currentPlace -= 1;
            if (currentPlace <= -1)
            {
                currentPlace = place.Count - 1;
            }
            navHasReset = false;
        }
        place[currentPlace].GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().Priority = 1;
    }

    void OnSubmit()
    {
        switch (currentPlace)
        {
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

    //void FixedUpdate()
    //{
    //    Vector3 a = Vector3.zero;
    //    transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, place[currentPlace].transform.position, ref a, moveTime), transform.rotation);
    //    transform.LookAt(2 * transform.position - cam.transform.position);
    //}

    void LateUpdate()
    {
        //cam.transform.LookAt(transform.position);
        if (fadingIn)
        {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(1f, 0f, t));
        }
        else if (fadingOut)
        {
            float t = (Time.time - startTime) / fadeDelay;
            fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(0f, 1f, t));
        }
    }

    void Update()
    {
        switch (currentPlace)
        {
            case 3:
                selectionDescription.text = "Change game settings and options.\n<color=red>(Unavailable)</color>";
                break;
            case 5:
                selectionDescription.text = "Buy boards with <color=yellow>tickets</color> from <color=green>Challenges</color>.\nBuy outfits and accessories with <color=yellow>coins</color>.\n<color=red>(Under Construction)</color>";
                break;
            case 6:
                selectionDescription.text = "Create and customize personal characters.\nUse <color=yellow>coins</color> to buy new outfits and accessories at the <color=green>Shop</color>.\n<color=red>(Under Construction)</color>";
                break;
            case 0:
                selectionDescription.text = "Earn <color=yellow>medals</color> and <color=yellow>coins</color> as you progress through the story.";
                break;
            case 1:
                selectionDescription.text = "Earn <color=yellow>coins</color> with friends connected to the same computer.";
                break;
            case 2:
                selectionDescription.text = "Earn <color=yellow>coins</color> with friends and other players online.\n<color=red>(Unavailable)</color>";
                break;
            case 7:
                selectionDescription.text = "Earn <color=yellow>tickets</color> by completing various challenges, like time trials, trick combos, and expert boss battles.\n<color=red>(Under Construction)</color>";
                break;
            case 4:
                selectionDescription.text = "Return to the <color=green>Title Menu</color>.";
                break;
        }
    }

    IEnumerator LoadScene(string sceneToLoad)
    {
        //selected = true;
        GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public IEnumerator Fade(bool i, string destination)
    {
        if (i) fadingIn = true;
        else fadingOut = true;
        startTime = Time.time;
        yield return new WaitForSeconds(fadeDelay);
        if (i) fadingIn = false;
        else
        {
            fadingOut = false;
            StartCoroutine(LoadScene(destination));
        }
    }
}
