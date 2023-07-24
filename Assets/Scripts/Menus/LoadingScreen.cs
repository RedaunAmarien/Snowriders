using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {
    public Image progressBar, progressMask;
    public Gradient progressBarColor;
    public Text progressText, tipText;
    public string[] tips;
    public string defaultScene;

    void Start() {
        tipText.text = tips[Random.Range(0, tips.Length)];
        if (GameRam.nextSceneToLoad == null) {
            GameRam.nextSceneType = SceneType.Menu;
            GameRam.nextSceneToLoad = defaultScene;
            StartCoroutine(LoadScene("OpeningCinematic"));
        }
        else {
            StartCoroutine(LoadScene(GameRam.nextSceneToLoad));
        }
    }

    IEnumerator LoadScene(string sceneName) {
        // Debug.Log("Loading scene " + sceneName);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) {
			progressMask.fillAmount = asyncLoad.progress;
            progressText.text = "Loading...\n" + asyncLoad.progress.ToString("P2");
            progressBar.color = progressBarColor.Evaluate(asyncLoad.progress);
            yield return null;
        }
    }
}
