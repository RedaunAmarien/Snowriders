using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;
using Cinemachine;

public class HubTownControls : MonoBehaviour
{
    public enum TownState { Browsing, RacePrep, Options, FileSelect, Mall, Shop, Customize };
    public TownState townState;
    [SerializeField] private float moveTime;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject loadSet;
    [SerializeField] private List<HubTownLocation> locations;
    private HubTownLocation[] subLocations;
    [SerializeField] private int currentPlace;
    [SerializeField] private int currentSubPlace;
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
    [SerializeField] private AudioSource[] audioSources;
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
        {
            currentPlace = 0;
            gameObject.GetComponent<HubFileSelect>().Activate();
            townState = TownState.FileSelect;
            return;
        }

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
        currentPlace = GameRam.lastHubSelection;
        locations[currentPlace].virtualCamera.Priority = 1;
        if (locations[currentPlace].door != null) locations[currentPlace].door.ActivateDoor();
        foreach (AudioSource audio in audioSources)
        {
            audio.Play();
        }
    }

    void Update()
    {
        if (townState == TownState.Browsing)
            selectionDescription.text = locations[currentPlace].locationDescription;
        else
            selectionDescription.text = subLocations[currentSubPlace].locationDescription;
    }

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

    void OnNavigate(InputValue val)
    {
        if (townState == TownState.Browsing)
        {
            locations[currentPlace].virtualCamera.Priority = 0;
            if (locations[currentPlace].door != null) locations[currentPlace].door.ActivateDoor();
            float v = val.Get<Vector2>().x;
            if (v < .5f && v > -.5f)
            {
                navHasReset = true;
            }
            if (navHasReset && v > 0.5f)
            {
                currentPlace += 1;
                if (currentPlace >= locations.Count)
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
                    currentPlace = locations.Count - 1;
                }
                navHasReset = false;
            }
            locations[currentPlace].virtualCamera.Priority = 1;
            if (locations[currentPlace].door != null) locations[currentPlace].door.ActivateDoor();
            GameRam.lastHubSelection = currentPlace;
        }
        else
        {
            subLocations[currentSubPlace].virtualCamera.Priority = 0;
            if (subLocations[currentSubPlace].door != null) subLocations[currentSubPlace].door.ActivateDoor();
            float v = val.Get<Vector2>().x;
            if (v < .5f && v > -.5f)
            {
                navHasReset = true;
            }
            if (navHasReset && v > 0.5f)
            {
                currentSubPlace += 1;
                if (currentSubPlace >= subLocations.Length)
                {
                    currentSubPlace = 0;
                }
                navHasReset = false;
            }
            if (navHasReset && v < -0.5f)
            {
                currentSubPlace -= 1;
                if (currentSubPlace <= -1)
                {
                    currentSubPlace = subLocations.Length - 1;
                }
                navHasReset = false;
            }
            subLocations[currentSubPlace].virtualCamera.Priority = 2;
            if (subLocations[currentSubPlace].door != null) subLocations[currentSubPlace].door.ActivateDoor();
        }
    }

    void OnCancel()
    {
        if (townState != TownState.Browsing)
        {
            townState = TownState.Browsing;
            subLocations[currentSubPlace].virtualCamera.Priority = 0;
        }
    }

    void OnSubmit()
    {
        switch (townState)
        {
            case TownState.Browsing:
                switch (locations[currentPlace].locationName)
                {
                    case "Race":
                        townState = TownState.RacePrep;
                        subLocations = locations[currentPlace].subLocations;
                        currentSubPlace = 0;
                        subLocations[currentSubPlace].virtualCamera.Priority = 2;
                        break;

                    case "Options":
                        Debug.LogWarning("Unavailable");
                        break;

                    case "Main Menu":
                        StartCoroutine(Fade(false, "FileSelect"));
                        break;

                    case "Mall":
                        townState = TownState.Mall;
                        subLocations = locations[currentPlace].subLocations;
                        currentSubPlace = 0;
                        subLocations[currentSubPlace].virtualCamera.Priority = 2;
                        break;
                }
                break;
            case TownState.RacePrep:
                switch (subLocations[currentSubPlace].locationName)
                {
                    case "Story Mode":
                        GameRam.gameMode = GameMode.Story;
                        GameRam.maxPlayerCount = 1;
                        StartCoroutine(Fade(false, "RacePrepMenu"));
                        break;

                    case "Local Multiplayer":
                        GameRam.gameMode = GameMode.Battle;
                        GameRam.maxPlayerCount = 4;
                        StartCoroutine(Fade(false, "RacePrepMenu"));
                        break;

                    case "Online Multiplayer":
                        GameRam.gameMode = GameMode.Online;
                        GameRam.maxPlayerCount = 2;
                        Debug.LogWarning("Unavailable");
                        break;

                    case "Challenges":
                        GameRam.gameMode = GameMode.Challenge;
                        GameRam.maxPlayerCount = 1;
                        StartCoroutine(Fade(false, "RacePrepMenu"));
                        break;
                }
                break;
            case TownState.Options:
                break;
            case TownState.FileSelect:
                gameObject.GetComponent<HubFileSelect>().Activate();
                townState = TownState.FileSelect;
                break;
            case TownState.Mall:
                switch (subLocations[currentSubPlace].locationName)
                {
                    case "Shop":
                        //StartCoroutine(Fade(false, "Shop"));
                        gameObject.GetComponent<HubShop>().Activate();
                        townState = TownState.Shop;
                        break;

                    case "Character Customization":
                        // StartCoroutine(Fade(false, "CharacterEditor"));
                        Debug.LogWarning("Unavailable");
                        break;
                }
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

[System.Serializable]
public class HubTownLocation
{
    public string locationName;
    [Multiline]
    public string locationDescription;
    public CinemachineVirtualCamera virtualCamera;
    public DoorOpener door;
    public HubTownLocation[] subLocations;
}
