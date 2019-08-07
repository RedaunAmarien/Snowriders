using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {
    public Image progressBar;
    public Text progressText;

    void Start() {
        if (GameVar.nextSceneToLoad == null) StartCoroutine(LoadScene("MainMenu"));
        else StartCoroutine(LoadScene(GameVar.nextSceneToLoad));
    }

    IEnumerator LoadScene(string sceneName) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) {
			progressBar.fillAmount = asyncLoad.progress;
            progressText.text = "Loading...\n" + (asyncLoad.progress*100) + "%";
            yield return null;
        }
    }
}
