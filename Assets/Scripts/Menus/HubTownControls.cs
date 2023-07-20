using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class HubTownControls : MonoBehaviour
{
    [SerializeField] private string currentMenuSet;
    [SerializeField] private string optionCode;
    [SerializeField] private float moveTime;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject loadSet;
    [SerializeField] private List<HubTownOption> rootOptions;
    [SerializeField] private List<HubTownOption> currentOptions;
    [SerializeField] private int currentOptionIndex;
    //[SerializeField] private int currentOptionTier;
    [SerializeField] private Slider progressBar;

    //[SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDelay, startTime;
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] HubUIBridge uiBridge;
    private bool fadingIn, fadingOut;
    private bool navHasReset = true;
    bool overridden;

    void Start()
    {
        //Start audios in sync
        foreach (AudioSource audio in audioSources)
        {
            audio.Play();
            //Debug.LogFormat("Playing audio clip {0}", audio.clip.name);
        }

        currentOptions = rootOptions;
        currentOptionIndex = 0;

        if (GameRam.currentSaveFile == null)
        {
            StartCoroutine(WaitOneFrame());
            return;
        }

        //currentOptionIndex = GameRam.lastHubSelection;
        //rootOptions[currentOptionIndex].camera.Priority = 1;
        //if (rootOptions[currentOptionIndex].door != null) rootOptions[currentOptionIndex].door.ActivateDoor();
    }

    IEnumerator WaitOneFrame()
    {
        yield return new WaitForEndOfFrame();
        optionCode = "0";
        currentOptions = rootOptions[0].subOptions;
        OnSubmitCustom(0);
        OnSubmitCustom(0);
    }

    void Update()
    {
        if (overridden)
            return;

        uiBridge.infoDescription.text = currentOptions[currentOptionIndex].optionDescription;
        uiBridge.infoName.text = currentOptions[currentOptionIndex].optionName;
    }

    void LateUpdate()
    {
        //cam.transform.LookAt(transform.position);
        if (fadingIn)
        {
            float t = (Time.time - startTime) / fadeDelay;
            //fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(1f, 0f, t));
        }
        else if (fadingOut)
        {
            float t = (Time.time - startTime) / fadeDelay;
            //fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(0f, 1f, t));
        }
    }

    void OnNavigateCustom(HubMultiplayerInput.Paras input)
    {
        if (overridden || input.index != 0)
            return;

        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, false);
        float v = input.val.Get<Vector2>().x;
        if (v < .5f && v > -.5f)
        {
            navHasReset = true;
        }
        if (navHasReset && v > 0.5f)
        {
            currentOptionIndex += 1;
            if (currentOptionIndex >= currentOptions.Count)
            {
                currentOptionIndex = 0;
            }
            navHasReset = false;
        }
        if (navHasReset && v < -0.5f)
        {
            currentOptionIndex -= 1;
            if (currentOptionIndex <= -1)
            {
                currentOptionIndex = currentOptions.Count - 1;
            }
            navHasReset = false;
        }
        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, true);
        GameRam.lastHubSelection = currentOptionIndex;
    }

    void OnCancelCustom(int index)
    {
        if (overridden || index != 0 || optionCode == "")
            return;

        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, false);
        optionCode = optionCode.Substring(0, optionCode.Length - 1);
        currentOptions = rootOptions;
        for (int i = 0; i < optionCode.Length; i++)
        {
            int j = int.Parse(optionCode.Substring(i, 1));
            currentOptionIndex = j;
            if (i < optionCode.Length - 1)
                currentOptions = currentOptions[j].subOptions;
        }
        currentOptionIndex = 0;
        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, true);
    }

    void OnSubmitCustom(int index)
    {
        if (overridden || index != 0)
            return;

        currentOptions[currentOptionIndex].events.Invoke();
        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, false);
        optionCode += currentOptionIndex.ToString();
        //currentOptions[currentOptionIndex].currentChoice = currentOptionIndex;
        if (currentOptions[currentOptionIndex].subOptions.Count > 0)
        {
            currentOptions = currentOptions[currentOptionIndex].subOptions;
            currentOptionIndex = 0;
            //currentOptionTier++;
        }
        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, true);

        switch (optionCode)
        {
            case "0": //Kiosk
                break;
            case "00": //File Select
                gameObject.GetComponent<HubFileSelect>().Activate();
                //currentOptions[currentOptionIndex].events.Invoke();
                //Debug.Log("Opening File Select");
                overridden = true;
                break;
            case "01": //Options
                OnUnavailable();
                break;
            case "02":
                Application.Quit();
                break;

            case "1": //Race Hub
                //switch (currentOptions[currentOptionIndex].optionName)
                //{
                //    case "Story Mode":
                //        GameRam.gameMode = GameMode.Story;
                //        GameRam.maxPlayerCount = 1;
                //        StartCoroutine(Fade(false, "RacePrepMenu"));
                //        break;

                //    case "Local Multiplayer":
                //        GameRam.gameMode = GameMode.Battle;
                //        GameRam.maxPlayerCount = 4;
                //        StartCoroutine(Fade(false, "RacePrepMenu"));
                //        break;

                //    case "Online Multiplayer":
                //        GameRam.gameMode = GameMode.Online;
                //        GameRam.maxPlayerCount = 2;
                //        OnUnavailable();
                //        break;

                //    case "Challenges":
                //        GameRam.gameMode = GameMode.Challenge;
                //        GameRam.maxPlayerCount = 1;
                //        StartCoroutine(Fade(false, "RacePrepMenu"));
                //        break;
                //}
                break;
            case "10": //Story Mode
                GameRam.maxPlayerCount = 1;
                GetComponent<HubRacePrep>().Activate(GameMode.Story);
                overridden = true;
                break;
            case "11": //Battle Mode
                GameRam.maxPlayerCount = 4;
                GetComponent<HubRacePrep>().Activate(GameMode.Battle);
                overridden = true;
                break;
            case "12": //Online Battle
                OnUnavailable();
                break;
            case "13": //Challenge Mode
                GameRam.maxPlayerCount = 1;
                GetComponent<HubRacePrep>().Activate(GameMode.Challenge);
                overridden = true;
                break;

            case "2": //Mall
                break;
            case "20": //Board Shop
                break;
            case "200": //Upgrade Board
                OnUnavailable();
                break;
            case "201": //Buy Special
                gameObject.GetComponent<HubShop>().Activate();
                overridden = true;
                break;

            case "21": //Salon
                OnUnavailable();
                break;
            case "22": //Gift Shop
                OnUnavailable();
                break;
            default: //Anything Not Specified
                Debug.LogWarning("Not a valid option.");
                //currentOptionTier++;
                OnCancelCustom(0);
                break;
        }
    }

    void ToggleCam(CinemachineVirtualCamera camera = null, DoorOpener door = null, bool activate = true)
    {
        if (activate)
        {
            if (camera != null) camera.Priority = 1;
            if (door != null) door.ActivateDoor(true);
        }
        else
        {
            if (camera != null) camera.Priority = 0;
            if (door != null) door.ActivateDoor(false);
        }

    }

    public void Reactivate()
    {
        overridden = false;
        OnCancelCustom(0);
    }

    void OnUnavailable()
    {
        Debug.LogWarning("Unavailable");
        OnCancelCustom(0);

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
