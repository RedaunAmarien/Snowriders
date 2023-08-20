using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEditor.TextCore.Text;
using System.Linq;
using System;

public class HubTownControls : MonoBehaviour
{
    [SerializeField] private string optionCode;
    [SerializeField] private int currentOptionIndex;
    [SerializeField] private float moveTime;
    [SerializeField] private float fadeDelay;
    [SerializeField] private List<HubTownOption> rootOptions;
    [SerializeField] private List<HubTownOption> currentOptions;
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] HubUIBridge uiBridge;
    private bool navHasReset = true;
    bool overridden;
    bool cancelling;

    void Start()
    {
        uiBridge.StartFlash(uiBridge.pressStart);
        GameRam.inputUser = new UnityEngine.InputSystem.Users.InputUser[4];
        GameRam.inputDevice = new UnityEngine.InputSystem.InputDevice[4];
        //Start audios in sync
        foreach (AudioSource audio in audioSources)
        {
            audio.Play();
            //Debug.LogFormat("Playing audio clip {0}", audio.clip.characterName);
        }

        currentOptions = rootOptions;
        currentOptionIndex = 0;

        if (GameRam.currentSaveFile == null)
            StartCoroutine(FirstLoad());
        else
            uiBridge.UpdateFileDisplay();
    }

    IEnumerator FirstLoad()
    {
        yield return null;
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
        if (!currentOptions[currentOptionIndex].isAvailable)
            uiBridge.infoName.AddToClassList("unavailable");
        else
            uiBridge.infoName.RemoveFromClassList("unavailable");
    }

    void OnNavigateCustom(HubMultiplayerInput.Parameters input)
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

        cancelling = true;
        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, false);
        //Debug.LogFormat("Cancel: {0} >> {1}", optionCode, optionCode.Substring(0, optionCode.Length - 1));
        int[] codeArray = Array.ConvertAll(optionCode.ToCharArray(), (c) => (int)Char.GetNumericValue(c));
        optionCode = "";

        currentOptions = rootOptions;
        for (int i = 0; i < codeArray.Length; i++)
        {
            currentOptionIndex = codeArray[i];
            if (i < codeArray.Length - 1)
                OnSubmitCustom(0);
        }
        ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, true);
        cancelling = false;
    }

    void OnSubmitCustom(int index)
    {
        if (overridden || index != 0)
            return;

        currentOptions[currentOptionIndex].events.Invoke();
        if (!cancelling)
            ToggleCam(currentOptions[currentOptionIndex].camera, currentOptions[currentOptionIndex].door, false);
        //Debug.LogFormat("Submit: {0} >> {1}{2}", optionCode, optionCode + currentOptionIndex, cancelling ? " (Cancel)" : "");
        optionCode += currentOptionIndex;
        //currentOptions[currentOptionIndex].currentChoice = currentOptionIndex;
        if (currentOptions[currentOptionIndex].subOptions.Count > 0)
        {
            currentOptions = currentOptions[currentOptionIndex].subOptions;
            currentOptionIndex = 0;
            //currentOptionTier++;
        }
        if (!cancelling)
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
                OnUnavailable();
                break;

            case "1": //Race Hub
                break;
            case "10": //Story Mode
                GameRam.maxPlayerCount = 1;
                GameRam.itemsOn = true;
                GameRam.coinsOn = true;
                GetComponent<HubRacePrep>().Activate(GameMode.Story);
                overridden = true;
                break;
            case "11": //Battle Mode
                GameRam.maxPlayerCount = 4;
                GameRam.itemsOn = true;
                GameRam.coinsOn = true;
                GetComponent<HubRacePrep>().Activate(GameMode.Battle);
                overridden = true;
                break;
            case "12": //Online Battle
                OnUnavailable();
                break;
            case "13": //Challenge Mode
                GameRam.maxPlayerCount = 1;
                GameRam.itemsOn = false;
                GameRam.coinsOn = false;
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
                Debug.LogWarningFormat("{0} not a valid option.", optionCode);
                //currentOptionTier++;
                OnCancelCustom(0);
                break;
        }
    }

    public void InputStart(int index)
    {
        if (index != 0)
            return;

        uiBridge.StopFlash(uiBridge.pressStart);
        uiBridge.FadeIn();
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
        Debug.LogWarningFormat("{0}:{1} Unavailable", optionCode, currentOptions[currentOptionIndex].optionName);
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

    public IEnumerator FadeAndLoad(string destination)
    {
        uiBridge.FadeOut();
        yield return new WaitForSeconds(fadeDelay);
        StartCoroutine(LoadScene(destination));
    }
}
