using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class BattleSubMenu : MonoBehaviour {

	public int myNumber;
	public Text controlText, charText, boardText;
    public Image charPort, speedGauge, turnGauge, jumpGauge;
    public RectTransform rArrow, lArrow, thissun;
    int assignmentStep;
    bool charStickMove, pressingSubmit;
    BattleMenu battleMenu;
    public PlayerInput playInput;
    public MultiplayerEventSystem events;

	void Start () {
        // Initialize
        assignmentStep = 1;
        thissun = GetComponent<RectTransform>();
        playInput = GetComponent<PlayerInput>();
        playInput.uiInputModule = GetComponent<InputSystemUIInputModule>();
        myNumber = playInput.user.index;
        battleMenu = GameObject.Find("EventSystem").GetComponent<BattleMenu>();
        battleMenu.battleSub[myNumber] = gameObject;
        battleMenu.battleSubScript[myNumber] = GetComponent<BattleSubMenu>();
        events = GetComponent<MultiplayerEventSystem>();

        if (myNumber < 2) {
            transform.SetParent(GameObject.Find("Grid1").transform);
        }
        else {
            GameObject.Find("Grid2").SetActive(true);
            transform.SetParent(GameObject.Find("Grid2").transform);
        }
        
        thissun.localScale = new Vector3(1,1,1);
        thissun.localPosition = new Vector3 (transform.position.x, transform.position.y, 0);

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
            if (GameVar.currentSaveFile.boardOwned[GameVar.boardForP[myNumber]]) {
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

    public void OnMove(InputValue val) {
        var v = val.Get<Vector2>();
        if (assignmentStep == 1) {

            //Select Character.
            if (v.x > .5f && !charStickMove) {
                GameVar.charForP[myNumber] ++;
                if (GameVar.charForP[myNumber] > GameVar.allCharData.Length-1) GameVar.charForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameVar.charForP[myNumber] --;
                if (GameVar.charForP[myNumber] < 0) GameVar.charForP[myNumber] = GameVar.allCharData.Length-1;
                charStickMove = true;
            }
            else if (v.x > -.5f && v.x < .5f) charStickMove = false;
        }
        if (assignmentStep == 2) {

            // Select Board.
            if (v.x > .5f && !charStickMove) {
                GameVar.boardForP[myNumber] ++;
                if (GameVar.boardForP[myNumber] > GameVar.boardData.Length-1) GameVar.boardForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameVar.boardForP[myNumber] --;
                if (GameVar.boardForP[myNumber] < 0) GameVar.boardForP[myNumber] = GameVar.boardData.Length-1;
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
            charText.text = GameVar.allCharData[GameVar.charForP[myNumber]].name;
            speedGauge.fillAmount = GameVar.allCharData[GameVar.charForP[myNumber]].speed/10f;
            turnGauge.fillAmount = GameVar.allCharData[GameVar.charForP[myNumber]].turn/10f;
            jumpGauge.fillAmount = GameVar.allCharData[GameVar.charForP[myNumber]].jump/10f;
            if (GameVar.charForP[myNumber] >= GameVar.charDataCustom.Length) {
                charPort.sprite = battleMenu.charPortSrc[GameVar.charForP[myNumber]-GameVar.charDataCustom.Length];
            }
            else charPort.sprite = battleMenu.charPortSrc[battleMenu.charPortSrc.Length-1];

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
            if (GameVar.currentSaveFile.boardOwned[GameVar.boardForP[myNumber]]) {
                boardText.text = GameVar.boardData[GameVar.boardForP[myNumber]].name;
            }
            else {
                boardText.text = GameVar.boardData[GameVar.boardForP[myNumber]].name + " (Locked)";
            }
            speedGauge.fillAmount = (GameVar.allCharData[GameVar.charForP[myNumber]].speed + GameVar.boardData[GameVar.boardForP[myNumber]].speed)/10f;
            turnGauge.fillAmount = (GameVar.allCharData[GameVar.charForP[myNumber]].turn + GameVar.boardData[GameVar.boardForP[myNumber]].turn)/10f;
            jumpGauge.fillAmount = (GameVar.allCharData[GameVar.charForP[myNumber]].jump + GameVar.boardData[GameVar.boardForP[myNumber]].jump)/10f;
        }
	}

    IEnumerator ResetPress(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        pressingSubmit = false;
    }
}
