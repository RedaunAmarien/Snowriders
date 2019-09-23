using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSubMenu : MonoBehaviour {

	public int myNumber;
    public bool myTurn;
	public Text controlText, charText, boardText;
    public Image charPort, speedGauge, turnGauge, jumpGauge;
    public RectTransform rArrow, lArrow;
    int assignmentStep;
    bool charStickMove;
    BattleMenu battleMenu;

	void Start () {
        // Initialize
        battleMenu = GameObject.Find("EventSystem").GetComponent<BattleMenu>();
		if (myNumber > 0) assignmentStep = -1;
        else {
            assignmentStep = 0;
            Debug.Log("My turn (Player " + myNumber+ ")");
        }

        charText.gameObject.SetActive(false);
        boardText.gameObject.SetActive(false);
        charPort.gameObject.SetActive(false);
        lArrow.gameObject.SetActive(false);
        rArrow.gameObject.SetActive(false);
	}

    public void Restart() {
		if (myNumber > 0) assignmentStep = -1;
        else {
            assignmentStep = 0;
            Debug.Log("My turn (Player " + myNumber+ ")");
        }

        charText.gameObject.SetActive(false);
        boardText.gameObject.SetActive(false);
        charPort.gameObject.SetActive(false);
        lArrow.gameObject.SetActive(false);
        rArrow.gameObject.SetActive(false);
        
    }

	void Update() {
        // Controller Step.
        if (assignmentStep == 0) {

            charText.gameObject.SetActive(false);
            boardText.gameObject.SetActive(false);
            charPort.gameObject.SetActive(false);
            lArrow.gameObject.SetActive(false);
            rArrow.gameObject.SetActive(false);

            // Assign Controller.
            if (myTurn && Input.GetButtonDown("0 Button 0")) {
                Debug.Log("Input detected. Determining controller...");
                for (int i = -1; i < battleMenu.joyNames.Length+1; i++) {
                    Debug.Log("Testing Slot " + i);
                    if (i!=0 && Input.GetButtonDown(i+" Button 0") && !battleMenu.joySlotsTaken[i+1]) {
                        GameVar.controlp[myNumber] = i;
                        battleMenu.joySlotsTaken[i+1] = true;
                        if (i == -1) {
                            Debug.Log("Keyboard assigned to Player " + myNumber);
                        }
                        else {
                            Debug.Log(battleMenu.joyNames[i-1] + " assigned to Player " + myNumber);
                        }
                        if (myNumber < 3) {
                            battleMenu.battleSubScript[myNumber+1].myTurn = true;
                            battleMenu.battleSubScript[myNumber+1].assignmentStep = 0;
                            Debug.Log("Player " +(myNumber+1) + "'s turn");
                        }
                        controlText.gameObject.SetActive(false);
                        assignmentStep = 1;
                    }
                }
            }
        }
        
        // Character Step.
        else if (assignmentStep == 1) {

            charText.gameObject.SetActive(true);
            boardText.gameObject.SetActive(false);
            charPort.gameObject.SetActive(true);
            lArrow.gameObject.SetActive(true);
            rArrow.gameObject.SetActive(true);
            rArrow.anchorMin = new Vector2 (1, .85f);
            rArrow.anchorMax = new Vector2 (1.1f, .95f);
            lArrow.anchorMin = new Vector2 (-.1f, .85f);
            lArrow.anchorMax = new Vector2 (0, .95f);

            // Undo Player Count  and Controller Selection (Jumps back to Hub).
            if (Input.GetButtonDown(GameVar.controlp[myNumber]+" Button 1")) {
                battleMenu.Backup();
            }

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
            if (Input.GetButtonDown(GameVar.controlp[myNumber]+" Button 0")) {
                charText.color = Color.green;
                assignmentStep = 2;
            }
            if (Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") > .5f && !charStickMove) {
                GameVar.charForP[myNumber] ++;
                if (GameVar.charForP[myNumber] > GameVar.allCharData.Length-1) GameVar.charForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") < -.5f && !charStickMove) {
                GameVar.charForP[myNumber] --;
                if (GameVar.charForP[myNumber] < 0) GameVar.charForP[myNumber] = GameVar.allCharData.Length-1;
                charStickMove = true;
            }
            else if (Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") > -.5f && Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") < .5f) charStickMove = false;
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

            // Deselect character.
            if (Input.GetButtonDown(GameVar.controlp[myNumber]+" Button 1")) {
                charText.color = Color.white;
                assignmentStep = 1;
            }

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

            // Select Board.
            if (Input.GetButtonDown(GameVar.controlp[myNumber]+" Button 0")){
                if (GameVar.currentSaveFile.boardOwned[GameVar.boardForP[myNumber]]) {
                    boardText.color = Color.green;
                    assignmentStep = 3;
                    battleMenu.playersReady ++;
                }
                else {
                    // Locked.
                }
            }
            if (Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") > .5f && !charStickMove) {
                GameVar.boardForP[myNumber] ++;
                if (GameVar.boardForP[myNumber] > GameVar.boardData.Length-1) GameVar.boardForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") < -.5f && !charStickMove) {
                GameVar.boardForP[myNumber] --;
                if (GameVar.boardForP[myNumber] < 0) GameVar.boardForP[myNumber] = GameVar.boardData.Length-1;
                charStickMove = true;
            }
            else if (Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") > -.5f && Input.GetAxisRaw(GameVar.controlp[myNumber]+" Axis 1") < .5f) charStickMove = false;
        }

        // Ready Step.
        if (assignmentStep == 3) {

            // Undo chosen boards.
            if (Input.GetButtonDown(GameVar.controlp[myNumber]+" Button 1")) {
                battleMenu.playersReady --;
                boardText.color = Color.white;
                assignmentStep = 2;
            }

            // Move Forward
            if (Input.GetButtonDown(GameVar.controlp[myNumber]+" Button 0")) {
                battleMenu.CheckGo();
            }
        }
	}
}
