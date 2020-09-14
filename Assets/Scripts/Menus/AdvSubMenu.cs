using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class AdvSubMenu : MonoBehaviour {

	public int myNumber;
	public Text controlText, charText, boardText;
    public Image charPort, playPort, speedGauge, turnGauge, jumpGauge;
    public RectTransform rArrow, lArrow, thissun;
    int assignmentStep;
    bool charStickMove, pressingSubmit;
    AdvMenu advMenu;
    public PlayerInput playInput;
    public EventSystem spEvents;

	void Start () {
        // Initialize
        assignmentStep = 1;
        thissun = GetComponent<RectTransform>();
        playInput = GetComponent<PlayerInput>();
        myNumber = playInput.user.index;
        advMenu = GameObject.Find("EventSystem").GetComponent<AdvMenu>();
        advMenu.advSub = gameObject;
        advMenu.advSubScript = GetComponent<AdvSubMenu>();
        spEvents = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        playInput.uiInputModule = spEvents.GetComponent<InputSystemUIInputModule>();
        
        //Update Visuals
        advMenu.pressStart.SetActive(false);
        transform.SetParent(GameObject.Find("Grid1").transform);
        playPort.sprite = advMenu.playPortSrc;
        thissun.localScale = new Vector3(1,1,1);
        thissun.localPosition = new Vector3 (transform.position.x, transform.position.y, 0);

        charText.gameObject.SetActive(false);
        boardText.gameObject.SetActive(false);
        charPort.gameObject.SetActive(false);
        lArrow.gameObject.SetActive(false);
        rArrow.gameObject.SetActive(false);
	}

    public void OnSubmit() {
        Debug.Log("Player " + myNumber + " is pressing submit.");
        if (assignmentStep == 1 && !pressingSubmit) {
            charText.color = Color.green;
            assignmentStep = 2;
            pressingSubmit = true;
            StartCoroutine(ResetPress(.25f));
        }
        if (assignmentStep == 2 && !pressingSubmit) {
            if (GameRam.currentSaveFile.boardsOwned.Contains(GameRam.boardData[GameRam.boardForP[myNumber]].name)) {
                boardText.color = Color.green;
                assignmentStep = 3;
                advMenu.playersReady ++;
                pressingSubmit = true;
                StartCoroutine(ResetPress(.25f));
            }
        }
        if (assignmentStep == 3 && !pressingSubmit) {
            advMenu.CheckGo();
            pressingSubmit = true;
            StartCoroutine(ResetPress(.25f));
        }
    }

    public void OnCancel() {
        if (assignmentStep == 2) {
            charText.color = Color.white;
            assignmentStep = 1;
        }
        if (assignmentStep == 3) {
            advMenu.playersReady --;
            boardText.color = Color.white;
            assignmentStep = 2;
        }
    }

    public void OnNavigate(InputValue val) {
        var v = val.Get<Vector2>();
        if (assignmentStep == 1) {

            //Select Character.
            if (v.x > .5f && !charStickMove) {
                GameRam.charForP[myNumber] ++;
                if (GameRam.charForP[myNumber] > GameRam.allCharData.Count-1) GameRam.charForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameRam.charForP[myNumber] --;
                if (GameRam.charForP[myNumber] < 0) GameRam.charForP[myNumber] = GameRam.allCharData.Count-1;
                charStickMove = true;
            }
            else if (v.x > -.5f && v.x < .5f) charStickMove = false;
        }
        if (assignmentStep == 2) {

            // Select Board.
            if (v.x > .5f && !charStickMove) {
                GameRam.boardForP[myNumber] ++;
                if (GameRam.boardForP[myNumber] > GameRam.boardData.Count-1) GameRam.boardForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameRam.boardForP[myNumber] --;
                if (GameRam.boardForP[myNumber] < 0) GameRam.boardForP[myNumber] = GameRam.boardData.Count-1;
                charStickMove = true;
            }
            else if (v.x > -.5f && v.x < .5f) charStickMove = false;
        }
    }

	void Update() {
        
        // Character Step.
        if (assignmentStep == 1) {
            controlText.gameObject.SetActive(false);
            charText.gameObject.SetActive(true);
            boardText.gameObject.SetActive(false);
            charPort.gameObject.SetActive(true);
            lArrow.gameObject.SetActive(true);
            rArrow.gameObject.SetActive(true);
            rArrow.anchorMin = new Vector2 (1, .85f);
            rArrow.anchorMax = new Vector2 (1.1f, .95f);
            lArrow.anchorMin = new Vector2 (-.1f, .85f);
            lArrow.anchorMax = new Vector2 (0, .95f);

            // Keep text and image fields updated.
            charText.text = GameRam.allCharData[GameRam.charForP[myNumber]].name;
            speedGauge.fillAmount = GameRam.allCharData[GameRam.charForP[myNumber]].speed/10f;
            turnGauge.fillAmount = GameRam.allCharData[GameRam.charForP[myNumber]].turn/10f;
            jumpGauge.fillAmount = GameRam.allCharData[GameRam.charForP[myNumber]].jump/10f;
            if (GameRam.charForP[myNumber] < GameRam.charDataPermanent.Count) {
                charPort.sprite = advMenu.charPortSrc[GameRam.charForP[myNumber]];
            }
            else charPort.sprite = advMenu.charPortSrc[advMenu.charPortSrc.Length-1];

            // Select Character.
        }

        // Board Step.
        else if (assignmentStep == 2) {

            charText.gameObject.SetActive(true);
            boardText.gameObject.SetActive(true);
            charPort.gameObject.SetActive(true);
            lArrow.gameObject.SetActive(true);
            rArrow.gameObject.SetActive(true);
            rArrow.anchorMin = new Vector2 (1, .05f);
            rArrow.anchorMax = new Vector2 (1.1f, .15f);
            lArrow.anchorMin = new Vector2 (-.1f, .05f);
            lArrow.anchorMax = new Vector2 (0, .15f);

            // Keep text fields updated.
            if (GameRam.currentSaveFile.boardsOwned.Contains(GameRam.boardData[GameRam.boardForP[myNumber]].name)) {
                boardText.text = GameRam.boardData[GameRam.boardForP[myNumber]].name;
                boardText.color = Color.white;
            }
            else {
                boardText.text = GameRam.boardData[GameRam.boardForP[myNumber]].name + " (Locked)";
                boardText.color = Color.gray;
            }
            speedGauge.fillAmount = (GameRam.allCharData[GameRam.charForP[myNumber]].speed + GameRam.boardData[GameRam.boardForP[myNumber]].speed)/10f;
            turnGauge.fillAmount = (GameRam.allCharData[GameRam.charForP[myNumber]].turn + GameRam.boardData[GameRam.boardForP[myNumber]].turn)/10f;
            jumpGauge.fillAmount = (GameRam.allCharData[GameRam.charForP[myNumber]].jump + GameRam.boardData[GameRam.boardForP[myNumber]].jump)/10f;
        }
	}

    IEnumerator ResetPress(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        pressingSubmit = false;
    }
}
