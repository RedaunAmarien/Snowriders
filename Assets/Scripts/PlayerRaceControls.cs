using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerRaceControls : MonoBehaviour {

	Rigidbody rigid;
	public int conNum;
	public bool fire1Down, fire2Down, rightStickinUse;
	public bool lockControls, raceOver, comboAble;
	PlayerUI pUI;
	RacerPhysics rPhys;
	public string lStickH, lStickV, rStickH, rStickV, aBut, bBut, xBut, yBut, stBut, bkBut;

	void Start () {
		//Find objects.
		pUI = GetComponent<PlayerUI>();
		rPhys = GetComponent<RacerPhysics>();

		//Initialize objects.
		rigid = gameObject.GetComponent<Rigidbody>();
		rPhys.spotLock = true;
		lockControls = true;
		rigid.maxAngularVelocity = 0.05f;

		//Initialize controls.
		lStickH = (conNum+" Axis 1");
		lStickV = (conNum+" Axis 2");
		rStickH = (conNum+" Axis 4");
		rStickV = (conNum+" Axis 5");
		aBut = (conNum+ " Button 0");
		bBut = (conNum+ " Button 1");
		xBut = (conNum+ " Button 2"); //Button 2 or Axis 9
		yBut = (conNum+ " Button 3"); //Button 3 or Axis 10
		stBut = (conNum+ " Button 7");
		bkBut = (conNum+ " Button 6");
	}
	void Update () {
		if (Input.GetButtonDown(yBut)) {
			pUI.camIsReversed = true;
		}
		if (Input.GetButtonUp(yBut)) {
			pUI.camIsReversed = false;
		}
		if (!pUI.paused && Input.GetButtonDown(stBut)) {
			pUI.Pause(0);
		}

		// Make sure items will be shot one at a time.
		if (Input.GetButtonUp(bBut)) fire1Down = false;
		if (Input.GetButtonUp(xBut)) fire2Down = false;

		// Allow return to menu on race completion.
		if (raceOver && Input.GetButtonDown(aBut)) StartCoroutine(LoadScene("HubTown"));

		// Control player if not finished.
		if (!rPhys.finished && !lockControls) {
			// Use items and attacks, making sure one press only fires one item.
			if (Input.GetButtonDown(xBut) && !fire2Down) {
				rPhys.Shoot();
				fire2Down = true;
			}
			if (Input.GetButtonDown(bBut) && !fire1Down) {
				rPhys.Item();
				fire1Down = true;
			}

			// Grounded Section
			if (rPhys.grounded) {
				// Crouch to prepare tricks.
				if (Input.GetButton(aBut)) {
					
				}
				// Turn.
				else if (!Input.GetButton(aBut)) {
					if (Input.GetAxis(lStickV) >= 0) {
						float t = Input.GetAxis(lStickH) * rPhys.turnSpeed * Time.deltaTime;
						transform.Rotate(0,t,0);
					}
					if (Input.GetAxis(lStickV) < 0) {
						float t = Input.GetAxis(lStickH) * rPhys.turnSpeed * Time.deltaTime;
						transform.Rotate(0, t + (t*Mathf.Abs(Input.GetAxis(lStickV))) ,0);
					}
				}
				// Jump and perform tricks if touching the ground.
				if (Input.GetButtonUp(aBut)) {
					rPhys.Jump();
					if (Input.GetAxisRaw(lStickH) > 0) {
						if (Input.GetAxisRaw(lStickV) > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (Input.GetAxisRaw(lStickH) == 0) {
						if (Input.GetAxisRaw(lStickV) > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (Input.GetAxisRaw(lStickH) < 0) {
						if (Input.GetAxisRaw(lStickV) > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
				}
			}

			// Not Grounded Section
			else {
				// Turn slower when not grounded, but not at all when tricking
				if (!rPhys.tricking) {
					var t = Input.GetAxis(lStickH) * rPhys.turnSpeed * Time.deltaTime;
					transform.Rotate(0,t/2,0);
				}
				// Combo tricks in air
				if (Input.GetButtonUp(aBut) && comboAble) {
					comboAble = false;
					if (Input.GetAxisRaw(lStickH) > 0) {
						if (Input.GetAxisRaw(lStickV) > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (Input.GetAxisRaw(lStickH) == 0) {
						if (Input.GetAxisRaw(lStickV) > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (Input.GetAxisRaw(lStickH) < 0) {
						if (Input.GetAxisRaw(lStickV) > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (Input.GetAxisRaw(lStickV) < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
				}
			}
		}
	}

	public void OnLookBack() {
		Debug.Log("Pressing Lookback.");
	}

	public void OnJump() {
		Debug.Log("Pressing Jump.");

	}

	public void OnShoot() {
		Debug.Log("Pressing Shoot.");

	}

	public void OnItem() {
		Debug.Log("Pressing Item.");

	}

	public void OnTurn(InputValue val) {
	}

	public void OnGrab(InputValue val) {
		Vector2 d = val.Get<Vector2>();
		
		// Do board grabs.
		if (!rPhys.grabbing && !rPhys.finished && !rPhys.grounded && !lockControls && !rightStickinUse) {
			if (d.x < 0) {
				StartCoroutine(rPhys.BoardGrab(7, rPhys.lgdr));
				rightStickinUse = true;
			}
			else if (d.x > 0) {
				StartCoroutine(rPhys.BoardGrab(3, rPhys.lgdr));
				rightStickinUse = true;
			}
			if (d.y < 0) {
				StartCoroutine(rPhys.BoardGrab(5, rPhys.lgdr));
				rightStickinUse = true;
			}
			else if (d.y > 0) {
				StartCoroutine(rPhys.BoardGrab(1, rPhys.lgdr));
				rightStickinUse = true;
			}
		}
		if (d.x == 0 && d.y == 0) rightStickinUse = false;
	}

	IEnumerator LoadScene(string sceneToLoad) {
		GameVar.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
