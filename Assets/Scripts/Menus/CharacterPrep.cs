using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class CharacterPrep : MonoBehaviour {

	public int myNumber;
	public Text controlText, charText, boardText;
    public Image charPort, playPort, tSpeed, tTurn, tJump, pSpeed, pTurn, pJump;
    public RectTransform rArrow, lArrow, thissun;
    int assignmentStep;
    bool charStickMove, pressingSubmit;
    RacePrep prepMenu;
    public PlayerInput playInput;
    public EventSystem spEvents;
    public MultiplayerEventSystem mpEvents;

	void Start () {
        // Initialize
        assignmentStep = 1;
        myNumber = playInput.user.index;
        prepMenu = GameObject.Find("EventSystem").GetComponent<RacePrep>();
        prepMenu.charPrep[myNumber] = gameObject;
        prepMenu.charPrepScript[myNumber] = GetComponent<CharacterPrep>();
        
        //Update Visuals
        if (GameRam.gameMode != GameMode.Battle)
        {
            prepMenu.pressStart.SetActive(false);
            prepMenu.GetComponent<PlayerInputManager>().enabled = false;
        }
        transform.SetParent(prepMenu.joinParent[myNumber].transform);
        prepMenu.joinText[myNumber].SetActive(false);
        playPort.sprite = prepMenu.playPortSrc;
        thissun.localScale = new Vector3(1,1,1);
        thissun.localPosition = new Vector3(transform.position.x, transform.position.y, 0);
        thissun.offsetMin = new Vector2(0,0);
        thissun.offsetMax = new Vector2(0,0);

        charText.gameObject.SetActive(false);
        boardText.gameObject.SetActive(false);
        charPort.gameObject.SetActive(false);
        lArrow.gameObject.SetActive(false);
        rArrow.gameObject.SetActive(false);
	}

    public void OnSubmit() {
        Debug.LogFormat("Player {0} is pressing submit.", myNumber + 1);
        if (assignmentStep == 1 && !pressingSubmit) {
            charText.color = Color.green;
            assignmentStep = 2;
            pressingSubmit = true;
            StartCoroutine(ResetPress(.25f));
        }
        if (assignmentStep == 2 && !pressingSubmit) {
            boardText.color = Color.green;
            assignmentStep = 3;
            prepMenu.playersReady ++;
            pressingSubmit = true;
            StartCoroutine(ResetPress(.25f));
        }
        if (assignmentStep == 3 && !pressingSubmit) {
            prepMenu.CheckGo();
            pressingSubmit = true;
            StartCoroutine(ResetPress(.25f));
        }
    }

    public void OnCancel() {
        if (assignmentStep == 1)
        {
            prepMenu.Backup();
        }
        if (assignmentStep == 2)
        {
            charText.color = Color.white;
            assignmentStep = 1;
        }
        if (assignmentStep == 3)
        {
            prepMenu.playersReady --;
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
                if (GameRam.charForP[myNumber] > GameRam.allCharacters.Count-1) GameRam.charForP[myNumber] = 0;
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameRam.charForP[myNumber] --;
                if (GameRam.charForP[myNumber] < 0) GameRam.charForP[myNumber] = GameRam.allCharacters.Count-1;
                charStickMove = true;
            }
            else if (v.x > -.5f && v.x < .5f) charStickMove = false;
        }
        if (assignmentStep == 2) {

            // Select Board.
            if (v.x > .5f && !charStickMove) {
                GameRam.boardForP[myNumber] ++;
                if (GameRam.boardForP[myNumber] > GameRam.ownedBoards.Count-1) {
                    GameRam.boardForP[myNumber] = 0;
                }
                charStickMove = true;
            }
            else if (v.x < -.5f && !charStickMove) {
                GameRam.boardForP[myNumber] --;
                if (GameRam.boardForP[myNumber] < 0) {
                    GameRam.boardForP[myNumber] = GameRam.ownedBoards.Count-1;
                }
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
            charText.text = GameRam.allCharacters[GameRam.charForP[myNumber]].name;
            pSpeed.fillAmount = GameRam.allCharacters[GameRam.charForP[myNumber]].speed/10f;
            tSpeed.fillAmount = 0;
            pTurn.fillAmount = GameRam.allCharacters[GameRam.charForP[myNumber]].turn/10f;
            tTurn.fillAmount = 0;
            pJump.fillAmount = GameRam.allCharacters[GameRam.charForP[myNumber]].jump/10f;
            tJump.fillAmount = 0;
            if (GameRam.charForP[myNumber] < GameRam.defaultCharacters.Count) {
                charPort.sprite = prepMenu.charPortSrc[GameRam.charForP[myNumber]];
            }
            else charPort.sprite = prepMenu.charPortSrc[prepMenu.charPortSrc.Length-1];

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
            // if (GameRam.ownedBoardData.Contains(GameRam.boardData[GameRam.boardForP[myNumber]])) {
                boardText.text = GameRam.ownedBoards[GameRam.boardForP[myNumber]].name;
                boardText.color = Color.white;
            // }
            // else {
            //     boardText.text = GameRam.ownedBoardData[GameRam.boardForP[myNumber]].name + " (Locked)";
            //     boardText.color = Color.gray;
            // }
            tSpeed.fillAmount = (GameRam.allCharacters[GameRam.charForP[myNumber]].speed + GameRam.ownedBoards[GameRam.boardForP[myNumber]].speed)/10f;
            tTurn.fillAmount = (GameRam.allCharacters[GameRam.charForP[myNumber]].turn + GameRam.ownedBoards[GameRam.boardForP[myNumber]].turn)/10f;
            tJump.fillAmount = (GameRam.allCharacters[GameRam.charForP[myNumber]].jump + GameRam.ownedBoards[GameRam.boardForP[myNumber]].jump)/10f;
        }
        else if (assignmentStep == 3) {
            lArrow.gameObject.SetActive(false);
            rArrow.gameObject.SetActive(false);
        }
	}

    IEnumerator ResetPress(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        pressingSubmit = false;
    }
}
