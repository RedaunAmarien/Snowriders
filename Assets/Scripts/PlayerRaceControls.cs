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
	public bool fire1Down, fire2Down, rightStickinUse, jumpPressed;
	public bool lockControls, raceOver, comboAble;
	PlayerUI pUI;
	RacerPhysics rPhys;
	// public string lStickH, lStickV, rStickH, rStickV, aBut, bBut, xBut, yBut, stBut, bkBut;
	public Vector2 stickPos;

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
		// lStickH = (conNum+" Axis 1");
		// lStickV = (conNum+" Axis 2");
		// rStickH = (conNum+" Axis 4");
		// rStickV = (conNum+" Axis 5");
		// aBut = (conNum+ " Button 0");
		// bBut = (conNum+ " Button 1");
		// xBut = (conNum+ " Button 2"); //Button 2 or Axis 9
		// yBut = (conNum+ " Button 3"); //Button 3 or Axis 10
		// stBut = (conNum+ " Button 7");
		// bkBut = (conNum+ " Button 6");
	}

	// void Update () {

	// 	// Make sure items will be shot one at a time.
	// 	// if (Input.GetButtonUp(bBut)) fire1Down = false;
	// 	// if (Input.GetButtonUp(xBut)) fire2Down = false;

	// 	// Control player if not finished.
	// 	if (!rPhys.finished && !lockControls) {
	// 		// Use items and attacks, making sure one press only fires one item.
	// 		// if (Input.GetButtonDown(xBut) && !fire2Down) {
	// 		// 	rPhys.Shoot();
	// 		// 	fire2Down = true;
	// 		// }
	// 		// if (Input.GetButtonDown(bBut) && !fire1Down) {
	// 		// 	rPhys.Item();
	// 		// 	fire1Down = true;
	// 		// }
	// 	}
	// }

	public void OnLookBack(InputValue val) {
		float p = val.Get<float>();
		//Look Backwards.
		if (p == 0) pUI.camIsReversed = false;
		else pUI.camIsReversed = true;
	}

	public void OnJump(InputValue val) {
		float p = val.Get<float>();
		//Tell scripts A is pressed.
		if (p == 0) jumpPressed = false;
		else jumpPressed = true;

		// Return to Menu
		if (raceOver && jumpPressed) StartCoroutine(LoadScene("HubTown"));

		// During Race
		if (!rPhys.finished && !lockControls) {
			if (rPhys.grounded) {
				if (jumpPressed) {
					// Crouch to ready tricks.
				}
				else {
					// Jump and perform tricks if touching the ground.
					rPhys.Jump();
					if (stickPos.x > 0) {
						if (stickPos.y > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (stickPos.y == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (stickPos.y < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (stickPos.x == 0) {
						if (stickPos.y > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (stickPos.y < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (stickPos.x < 0) {
						if (stickPos.y > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (stickPos.y == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (stickPos.y < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
				}
			}
			else {
				// Combo tricks in air
				if (!jumpPressed && comboAble) {
					if (stickPos.x > 0) {
						if (stickPos.y > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (stickPos.y == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (stickPos.y < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (stickPos.x == 0) {
						if (stickPos.y > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (stickPos.y < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (stickPos.x < 0) {
						if (stickPos.y > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (stickPos.y == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (stickPos.y < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
					comboAble = false;
				}
			}
		}
	}

	public void OnShoot() {
		if (!rPhys.finished && !lockControls) {
			rPhys.Shoot();
		}
		Debug.Log("Pressing Shoot.");

	}

	public void OnItem() {
		if (!rPhys.finished && !lockControls) {
			rPhys.Item();
		}
		Debug.Log("Pressing Item.");

	}

	public void OnTurn(InputValue val) {
		stickPos = val.Get<Vector2>();
		// Steer
		if (!rPhys.finished && !lockControls) {
			if (rPhys.grounded) {
				if (!jumpPressed) {
					if (stickPos.y >= 0) {
						float t = stickPos.x * rPhys.turnSpeed * Time.deltaTime;
						transform.Rotate(0,t,0);
					}
					if (stickPos.y < 0) {
						float t = stickPos.x * rPhys.turnSpeed * Time.deltaTime;
						transform.Rotate(0, t + (t*Mathf.Abs(stickPos.y)) ,0);
					}
				}
			}
			else {
				// Turn slower when not grounded, but not at all when tricking
				if (!rPhys.tricking) {
					var t = stickPos.x * rPhys.turnSpeed * Time.deltaTime;
					transform.Rotate(0,t/2,0);
				}
			}
		}
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

	public void OnStart() {
		// Pause or unpause.
		if (!pUI.paused) pUI.Pause(0);
		else pUI.Pause(1);
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
