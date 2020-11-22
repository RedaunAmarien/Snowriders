using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
public enum SceneType {Menu, Race, NewCutscene};

public class CutsceneManager : MonoBehaviour {
    [Header("Scene Settings")]
    public int thisSceneIndex;
    public float sceneLength, fadeDelay, fadeSpeed;
    public bool timeStatic;
    public int staticHour, staticMinute;
    [Header("Next Scene Settings")]
    public string nextSceneName;
    public SceneType nextSceneType;
    [Header("Object References")]
    public Sun sunlight;
    public Image fadePanel;

    bool fadingIn, fadingOut;
    float startTime;

    void Awake() {
        sunlight.staticHour = staticHour;
        sunlight.staticMinute = staticMinute;
        sunlight.timeStatic = timeStatic;
        fadePanel.gameObject.SetActive(true);
    }

    void Start() {
        StartCoroutine(Timer());
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

    public void EndScene() {
        if (nextSceneType == SceneType.Race) {
            GameRam.nextSceneToLoad = "TrackContainer";
            GameRam.courseToLoad = nextSceneName;
            SceneManager.LoadScene("LoadingScreen");
        }
        else if (nextSceneType == SceneType.Menu) {
            GameRam.nextSceneToLoad = nextSceneName;
            SceneManager.LoadScene("LoadingScreen");
        }
        else if (nextSceneType == SceneType.NewCutscene) {
            SceneManager.LoadSceneAsync(nextSceneName);
        }
    }

    IEnumerator Timer() {
        fadingIn = true;
        startTime = Time.time;
        yield return new WaitForSeconds(fadeDelay);
        //Start Scene
        yield return new WaitForSeconds(sceneLength);
        fadingIn = false;
        fadingOut = true;
        startTime = Time.time;
        yield return new WaitForSeconds(fadeDelay);
        EndScene();
    }
}
