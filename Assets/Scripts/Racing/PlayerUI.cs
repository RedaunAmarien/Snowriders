using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Cinemachine;

public class PlayerUI : MonoBehaviour
{

    // UI Section
    //public int playerNum;
    public float camRotSpeed;
    public Vector3 camAngle, camOffset, revCamOffset;
    public float camSmoothTime;
    public Canvas canvas;
    public TextMeshProUGUI lapText, coinText, shotsText, trickText, timeText, lapTimeDisplay, placementText;
    public GameObject reverseView;
    public RenderTexture[] reverseViews;
    public Color[] placementColor;
    public System.TimeSpan[] lapTime;
    public System.TimeSpan lapStartTime;
    public bool timerOn, paused;
    public Image blueItem, redItem;
    public GameObject pauseMenu, pauseResume, finishGraphic;
    public EventSystem events;
    public StandaloneInputModule inputMod;
    public GameObject itemDisplay;
    public Camera playerCamera;
    public CinemachineVirtualCamera forwardVCam, reverseVCam;
    public Sprite[] itemSprite, weaponSprite, placeSprite;
    public bool finished;
    RacerCore racerCore;
    // CourseControl corCon;
    //public Camera reverseCam;

    void Start()
    {
        // Find objects.
        // corCon = GameObject.Find("CourseSettings").GetComponent<CourseControl>();
        racerCore = GetComponent<RacerCore>();
        //reverseCam = GameObject.Find("Reverse Camera " + playerNum).GetComponent<Camera>();
        //reverseView.GetComponent<RawImage>().texture = reverseViews[playerNum];
        //reverseView.SetActive(false);
        events = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        inputMod = GameObject.Find("EventSystem").GetComponent<StandaloneInputModule>();
        lapTime = new System.TimeSpan[racerCore.totalLaps];
        // Initialize camera
        //playerCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
        //reverseCam.transform.SetPositionAndRotation(transform.position, transform.rotation);
        // subCam.transform.position = transform.position + camOffset;
        //playerCamera.transform.LookAt(transform.position + camAngle);
        //reverseCam.transform.LookAt(transform.position + camAngle);
        if (GameRam.gameMode == GameMode.Challenge) placementText.gameObject.SetActive(false);
        if (!GameRam.itemsOn) itemDisplay.SetActive(false);
        //Are these canvas assignments correct??
        canvas.worldCamera = playerCamera.GetComponent<Camera>();
        // if (GameRam.playerCount == 1) canvas.worldCamera = GameObject.Find("Main Camera 0").GetComponent<Camera>();
        // if (GameRam.playerCount >= 2) canvas.worldCamera = GameObject.Find("Main Camera "+playerNum).GetComponent<Camera>();
        canvas.planeDistance = .5f;

        switch (racerCore.playerNum)
        {
            case 0:
                playerCamera.cullingMask = ~(1 << LayerMask.NameToLayer("P2Cam") | 1 << LayerMask.NameToLayer("P3Cam") | 1 << LayerMask.NameToLayer("P4Cam"));
                if (GameRam.playerCount == 1)
                    playerCamera.rect = new Rect(Vector2.zero, Vector2.one);
                else if (GameRam.playerCount == 2)
                    playerCamera.rect = new Rect(Vector2.zero, new Vector2(.5f, 1));
                else
                    playerCamera.rect = new Rect(new Vector2(0, .5f), new Vector2(.5f, .5f));
                break;
            case 1:
                playerCamera.cullingMask = ~(1 << LayerMask.NameToLayer("P1Cam") | 1 << LayerMask.NameToLayer("P3Cam") | 1 << LayerMask.NameToLayer("P4Cam"));
                if (GameRam.playerCount == 2)
                    playerCamera.rect = new Rect(new Vector2(.5f, 0), new Vector2(.5f, 1));
                else
                    playerCamera.rect = new Rect(new Vector2(.5f, .5f), new Vector2(.5f, .5f));
                break;
            case 2:
                playerCamera.cullingMask = ~(1 << LayerMask.NameToLayer("P1Cam") | 1 << LayerMask.NameToLayer("P2Cam") | 1 << LayerMask.NameToLayer("P4Cam"));
                playerCamera.rect = new Rect(Vector2.zero, new Vector2(.5f, .5f));
                break;
            case 3:
                playerCamera.cullingMask = ~(1 << LayerMask.NameToLayer("P1Cam") | 1 << LayerMask.NameToLayer("P2Cam") | 1 << LayerMask.NameToLayer("P3Cam"));
                playerCamera.rect = new Rect(new Vector2(.5f, 0), new Vector2(.5f, .5f));
                break;
        }

        // Fix Audio Listeners at some point.
        TrackManager track = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        if (!track.demoMode || racerCore.playerNum == 0)
            playerCamera.GetComponent<AudioListener>().enabled = true;
        finishGraphic.SetActive(false);
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (!finished)
        {
            // Update UI texts.
            lapText.text = "Lap " + racerCore.currentLap + "/" + racerCore.totalLaps;
            coinText.text = racerCore.coins.ToString("N0");
            if (racerCore.place == 1)
            {
                placementText.text = "1<sup>st</sup>";
                placementText.fontSize = 200;
                placementText.color = placementColor[0];
            }
            if (racerCore.place == 2)
            {
                placementText.text = "2<sup>nd</sup>";
                placementText.fontSize = 175;
                placementText.color = placementColor[1];
            }
            if (racerCore.place == 3)
            {
                placementText.text = "3<sup>rd</sup>";
                placementText.fontSize = 150;
                placementText.color = placementColor[2];
            }
            if (racerCore.place == 4)
            {
                placementText.text = "4<sup>th</sup>";
                placementText.fontSize = 130;
                placementText.color = placementColor[3];
            }
            if (racerCore.weaponAmmo <= 0) shotsText.text = "";
            else shotsText.text = racerCore.weaponAmmo.ToString();
            timeText.text = racerCore.trackManager.printTimer;
        }

        else finishGraphic.SetActive(true);

        // Update collectItem and weapon displays.
        blueItem.sprite = itemSprite[(int)racerCore.currentItem];
        redItem.sprite = weaponSprite[(int)racerCore.currentWeapon];
    }

    public void Pause(int option)
    {
        if (option == 0)
        {
            Time.timeScale = 0;
            // corCon.timerOn = false;
            racerCore.trackManager.timerOn = false;
            paused = true;
            // inputMod.horizontalAxis = (playerNum-1) + " Axis 1";
            // inputMod.verticalAxis = (playerNum-1) + " Axis 2";
            // inputMod.submitButton = (playerNum-1) + " Button 0";
            // inputMod.cancelButton = (playerNum-1) + " Button 1";
            // events.SetSelectedGameObject(pauseResume);
            pauseMenu.SetActive(true);
        }
        else if (option == 1)
        {
            // corCon.timerOn = true;
            racerCore.trackManager.timerOn = true;
            Time.timeScale = 1;
            paused = false;
            pauseMenu.SetActive(false);
        }
        else if (option == 2)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (option == 3)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("HubTown");
        }
    }

    public IEnumerator TrickComplete(int total)
    {
        TextMeshProUGUI textBox = Instantiate(trickText, new Vector2(0, 0), Quaternion.Euler(0, 0, 0), canvas.gameObject.transform);
        textBox.rectTransform.localPosition = new Vector3(0, 0, 0);
        textBox.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
        textBox.text = string.Format("Trick! +{0}", total);
        yield return new WaitForSeconds(1.5f);
        Destroy(textBox.gameObject);
    }

    public void LapTime(int lap)
    {
        TextMeshProUGUI textBox = Instantiate(lapTimeDisplay, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), timeText.gameObject.transform);
        textBox.rectTransform.anchoredPosition3D = new Vector3(0, -50 * lap, 0);
        textBox.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
        System.TimeSpan newLapTime = racerCore.trackManager.totalTime;
        if (lap == 1)
        {
            lapTime[lap - 1] = newLapTime;
        }
        else
        {
            lapTime[lap - 1] = newLapTime - lapStartTime;
        }
        textBox.text = string.Format("{0:d2}:{1:d2}.{2:d2} Lap {3}", lapTime[lap - 1].Minutes, lapTime[lap - 1].Seconds, lapTime[lap - 1].Milliseconds / 10, lap);
        lapStartTime = newLapTime;
    }
}
