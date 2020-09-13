using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class BattleSubMenu : MonoBehaviour {

	public int myNumber;
	public Text controlText, charText, boardText;
    public Image charPort, playPort, speedGauge, turnGauge, jumpGauge;
    public RectTransform rArrow, lArrow, thissun;
    int assignmentStep;
    bool charStickMove, pressingSubmit;
    BattleMenu battleMenu;
    public PlayerInput playInput;
    public MultiplayerEventSystem mpEvents;

	void Start () {
        // Initialize
        assignmentStep = 1;
        thissun = GetComponent<RectTransform>();
        playInput = GetComponent<PlayerInput>();
        if (playInput == null) Debug.LogError("PlayerInput component not found.");
        playInput.uiInputModule = GetComponent<InputSystemUIInputModule>();
        myNumber = playInput.user.index;
        battleMenu = GameObject.Find("EventSystem").GetComponent<BattleMenu>();
        battleMenu.battleSub[myNumber] = gameObject;
        battleMenu.battleSubScript[myNumber] = GetComponent<BattleSubMenu>();
        mpEvents = GetComponent<MultiplayerEventSystem>();
        
        battleMenu.pressStart.SetActive(false);
        if (myNumber < 2) {
            transform.SetParent(GameObject.Find("Grid1").transform);
        }
        else {
            GameObject.Find("Grid2").SetActive(true);
            transform.SetParent(GameObject.Find("Grid2").transform);
        }
        
        thissun.localScale = new Vector3(1,1,1);
        thissun.localPosition = new Vector3 (transform.position.x, transform.position.y, 0);

        playPort.sprite = battleMenu.playPortSrc[myNumber];

        charText.gameObject.SetActive(false);
        boardText.gameObject.SetActive(false);
        charPort.gameObject.SetActive(false);
        lArrow.gameObject.SetActive(false);
        rArrow.gameObject.SetActive(false);

        // playMan.JoinPlayer().ActivateInput();
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
            if (GameRam.currentSaveFile.boardOwned[GameRam.boardForP[myNumber]]) {
                boardText.color = Color.green;
                assignmentStep = 3;
                battleMenu.playersReady ++;
                pressingSubmit = true;
                StartCoroutine(ResetPress(.25f));
            }
        }
        if (assignmentStep == 3 && !pressingSubmit) {
            battleMenu.CheckGo();
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
            battleMenu.playersReady --;
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
                if (GameRam.boardForP[myNumber] > GameRam.boardData.Length-1) GameRam.boardForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameRam.boardForP[myNumber] --;
                if (GameRam.boardForP[myNumber] < 0) GameRam.boardForP[myNumber] = GameRam.boardData.Length-1;
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
            // if (GameRam.charForP[myNumber] >= GameRam.charDataCustom.Length) {
            //     charPort.sprite = battleMenu.charPortSrc[GameRam.charForP[myNumber]-GameRam.charDataCustom.Length];
            // }
            // else charPort.sprite = battleMenu.charPortSrc[battleMenu.charPortSrc.Length-1];

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
            if (GameRam.currentSaveFile.boardOwned[GameRam.boardForP[myNumber]]) {
                boardText.text = GameRam.boardData[GameRam.boardForP[myNumber]].name;
            }
            else {
                boardText.text = GameRam.boardData[GameRam.boardForP[myNumber]].name + " (Locked)";
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
