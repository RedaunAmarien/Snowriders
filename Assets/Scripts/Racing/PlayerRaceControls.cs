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
	public Vector2 lStickPos, rStickPos;

	void Start () {
		//Find objects.
		pUI = GetComponent<PlayerUI>();
		rPhys = GetComponent<RacerPhysics>();

		//Initialize objects.
		rigid = gameObject.GetComponent<Rigidbody>();
		rPhys.spotLock = true;
		lockControls = true;
		rigid.maxAngularVelocity = 0.05f;
	}

	void Update () {

		// Control player if not finished.
		if (!rPhys.finished && !lockControls) {
		// Steer
			if (rPhys.grounded) {
				if (!jumpPressed) {
					if (lStickPos.y >= 0) {
						float t = lStickPos.x * rPhys.turnSpeed * Time.deltaTime;
						transform.Rotate(0,t,0);
					}
					if (lStickPos.y < 0) {
						float t = lStickPos.x * rPhys.turnSpeed * Time.deltaTime;
						transform.Rotate(0, t + (t*Mathf.Abs(lStickPos.y)) ,0);
					}
				}
			}
			else {
				// Turn slower when not grounded, but not at all when tricking
				if (!rPhys.tricking) {
					var t = lStickPos.x * rPhys.turnSpeed * Time.deltaTime;
					transform.Rotate(0,t/2,0);
				}
			}
		
		// Do board grabs.
		if (!rPhys.grabbing && !rPhys.grounded && !rightStickinUse) {
			if (rStickPos.x < 0) {
				StartCoroutine(rPhys.BoardGrab(7, rPhys.lgdr));
				rightStickinUse = true;
			}
			else if (rStickPos.x > 0) {
				StartCoroutine(rPhys.BoardGrab(3, rPhys.lgdr));
				rightStickinUse = true;
			}
			if (rStickPos.y < 0) {
				StartCoroutine(rPhys.BoardGrab(5, rPhys.lgdr));
				rightStickinUse = true;
			}
			else if (rStickPos.y > 0) {
				StartCoroutine(rPhys.BoardGrab(1, rPhys.lgdr));
				rightStickinUse = true;
			}
		}
		if (rStickPos.x == 0 && rStickPos.y == 0) rightStickinUse = false;
			// Use items and attacks, making sure one press only fires one item.
			// if (Input.GetButtonDown(xBut) && !fire2Down) {
			// 	rPhys.Shoot();
			// 	fire2Down = true;
			// }
			// if (Input.GetButtonDown(bBut) && !fire1Down) {
			// 	rPhys.Item();
			// 	fire1Down = true;
			// }
		}
	}

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
		if (raceOver && jumpPressed) StartCoroutine(rPhys.tManage.Fade(false));

		// During Race
		if (!rPhys.finished && !lockControls) {
			if (rPhys.grounded) {
				if (jumpPressed) {
					// Crouch to ready tricks.
				}
				else {
					// Jump and perform tricks if touching the ground.
					rPhys.Jump();
					if (lStickPos.x > 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (lStickPos.x == 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (lStickPos.x < 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
				}
			}
			else {
				// Combo tricks in air
				if (!jumpPressed && comboAble) {
					if (lStickPos.x > 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (lStickPos.x == 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (lStickPos.x < 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
					comboAble = false;
				}
			}
		}
	}

	public void OnJumpAI(bool down) {

		// During Race
		if (!rPhys.finished && !lockControls) {
			if (rPhys.grounded) {
				if (down) {
					// Crouch to ready tricks.
				}
				else {
					// Jump and perform tricks if touching the ground.
					rPhys.Jump();
					if (lStickPos.x > 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (lStickPos.x == 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (lStickPos.x < 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
				}
			}
			else {
				// Combo tricks in air
				if (!down && comboAble) {
					if (lStickPos.x > 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(2, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(3, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(4, rPhys.ltdr));
					}
					else if (lStickPos.x == 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(1, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(5, rPhys.ltdr));
					}
					else if (lStickPos.x < 0) {
						if (lStickPos.y > 0) StartCoroutine(rPhys.Trick(8, rPhys.ltdr));
						else if (lStickPos.y == 0) StartCoroutine(rPhys.Trick(7, rPhys.ltdr));
						else if (lStickPos.y < 0) StartCoroutine(rPhys.Trick(6, rPhys.ltdr));
					}
					comboAble = false;
				}
			}
		}
	}

	public void OnShoot(InputValue val) {
		var v = val.Get<float>();
		if (v > 0) fire1Down = true;
		else fire1Down = false;
		if (!rPhys.finished && !lockControls && !fire1Down) {
			rPhys.Shoot();
		}
	}

	public void OnShootAI() {
		if (!rPhys.finished && !lockControls) {
			rPhys.Shoot();
		}
	}

	public void OnItem(InputValue val) {
		var v = val.Get<float>();
		if (v > 0) fire2Down = true;
		else fire2Down = false;
		if (!rPhys.finished && !lockControls && !fire2Down) {
			rPhys.Item();
		}
	}

	public void OnItemAI() {
		if (!rPhys.finished && !lockControls) {
			rPhys.Item();
		}
	}

	public void OnTurn(InputValue val) {
		lStickPos = val.Get<Vector2>();
	}

	public void OnGrab(InputValue val) {
		rStickPos = val.Get<Vector2>();
	}

	public void OnPause(InputValue val) {
		if (val.Get<float>() < .5f) {
			// Pause or unpause.
			if (!pUI.paused) pUI.Pause(0);
			else pUI.Pause(1);
		}
	}
}
