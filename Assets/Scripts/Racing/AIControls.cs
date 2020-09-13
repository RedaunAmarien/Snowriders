using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;

public class AIControls : MonoBehaviour {

	public GameObject firstWaypoint;
	public int current, altCurrent;
	public float turnAng, jumpDelay, chainTrickDelay;
	public Canvas canvas;
	public bool finished, locked, foundTarget, usingAlt;
	AIWaypoint cwp;
	RacerPhysics rPhys;
	PlayerRaceControls pCon;
	public List<GameObject> waypoints = new List<GameObject>();
	public List<GameObject> altWaypoints = new List<GameObject>();

	void Start() {
		locked = true;
		finished = false;
		rPhys = GetComponent<RacerPhysics>();
		pCon = GetComponent<PlayerRaceControls>();

		waypoints.Add(firstWaypoint);
		int stopper1 = 255;
		int stopper2 = 0;
		int altBegindex = 0;
		Debug.Log("Starting waypoint loop.");
		for (int i = 0; i < stopper1; i++) {
			waypoints.Add(waypoints[i].GetComponent<AIWaypoint>().nextInChain);
			if (waypoints[i+1].GetComponent<AIWaypoint>().splitting) {
				Debug.Log("Splitter found.");
				altBegindex = i+1;
				stopper2 = waypoints[i+1].GetComponent<AIWaypoint>().totalInAltChain-1;
			}
			else if (waypoints[i+1] == waypoints[0]) {
				Debug.Log("End found.");
				stopper1 = i;
				waypoints.RemoveAt(i+1);
			}
		}
		altWaypoints.Add(waypoints[altBegindex].GetComponent<AIWaypoint>().nextInAltChain);
		for (int i = 0; i < stopper2; i++) {
			altWaypoints.Add(altWaypoints[i].GetComponent<AIWaypoint>().nextInChain);
		}
		Debug.LogFormat("Listed {0} waypoints and {1} alt waypoints.", waypoints.Count, altWaypoints.Count);
		cwp = waypoints[current].GetComponent<AIWaypoint>();
		canvas.gameObject.SetActive(false);
	}

	void Update() {
		if (!finished && !locked) {
			//Jump when slowed down
			if (rPhys.relVel.z <= rPhys.speed*.4f) {
				StartCoroutine(Jump());
			}

			//Update Waypoints
			float a;
			float b;
			float c;
			if (!usingAlt) {
				a = (transform.position - waypoints[current].transform.position).sqrMagnitude;
				b = (transform.position - waypoints[current+1].transform.position).sqrMagnitude;
				c = (waypoints[current].transform.position - waypoints[current+1].transform.position).sqrMagnitude;
			}
			else {
				a = (transform.position - altWaypoints[altCurrent].transform.position).sqrMagnitude;
				b = (transform.position - altWaypoints[altCurrent+1].transform.position).sqrMagnitude;
				c = (altWaypoints[altCurrent].transform.position - altWaypoints[altCurrent+1].transform.position).sqrMagnitude;
			}

			if (a - b >= c) {
				SwitchCheckpoint();
			}

			//Search for Waypoints
			// RaycastHit viewPoint;
			// Physics.Raycast(transform.position, transform.forward, out viewPoint, 50f, LayerMask.GetMask("AIWaypoint"));
			// if (viewPoint.collider != null) {
			// 	Debug.DrawLine(transform.position, viewPoint.point, Color.yellow, .1f);

			// 	if (viewPoint.transform.gameObject == waypoints[current+1] || viewPoint.transform.gameObject == altWaypoints[altCurrent+1]) {
			// 		foundTarget = true;
			// 	}
			// 	else foundTarget = false;
			// }

			//Follow Waypoints
			// if (!foundTarget) {
				if (!usingAlt) {
					turnAng = Vector3.SignedAngle(transform.forward, transform.position - waypoints[current+1].transform.position, transform.up);
					Debug.DrawLine(transform.position, waypoints[current].transform.position, Color.cyan, .1f);
					Debug.DrawLine(transform.position, waypoints[current+1].transform.position, Color.magenta, .1f);
				}
				else {
					turnAng = Vector3.SignedAngle(transform.forward, transform.position - altWaypoints[altCurrent+1].transform.position, transform.up);
					Debug.DrawLine(transform.position, altWaypoints[altCurrent].transform.position, Color.cyan, .1f);
					Debug.DrawLine(transform.position, altWaypoints[altCurrent+1].transform.position, Color.magenta, .1f);
				}
				if (turnAng > 45) pCon.lStickPos = new Vector2(-.75f, -1);
				else if (turnAng < -45) pCon.lStickPos = new Vector2(.75f, -1);
				else if (turnAng > 5) pCon.lStickPos = Vector2.left;
				else if (turnAng < -5) pCon.lStickPos = Vector2.right;
				else pCon.lStickPos = Vector2.zero;
			// }
			// else {
			// 	pCon.lStickPos = Vector2.zero;
			// }

			// Use items
			if (rPhys.itemType != rPhys.blankItem) pCon.OnItemAI();
			if (rPhys.weapType != rPhys.blankWeap) pCon.OnShootAI();
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.GetComponent<AIWaypoint>() != null) {
			// Reset Waypoint if discovering one outside of range.
			if (other.gameObject != waypoints[current+1] && other.gameObject != waypoints[current] && other.gameObject != altWaypoints[altCurrent+1] && other.gameObject != altWaypoints[altCurrent]) {
				int newIndex = waypoints.IndexOf(other.gameObject);
				if (newIndex == -1){
					newIndex = altWaypoints.IndexOf(other.gameObject);
					if (newIndex == -1) {
						Debug.LogWarningFormat("{0} cannot find the index of the waypoint they just contacted at {1}.", rPhys.charName, transform.position.ToString());
					}
					else {
						Debug.LogWarningFormat("{0} got distracted by a different waypoint in alt route at {1}, index {2}, from index {3} or {4}.", rPhys.charName, transform.position.ToString(), newIndex, current, altCurrent);
						altCurrent = newIndex;
					}
				}
				else {
					Debug.LogWarningFormat("{0} got distracted by a different waypoint at {1}, index {2}, from index {3} or {4}.", rPhys.charName, transform.position.ToString(), newIndex, current, altCurrent);
					current = newIndex;
				}

				SwitchCheckpoint();
			}
			// Act on Waypoint flags
			if (cwp.tryJump && !locked && rPhys.grounded) {
				StartCoroutine(Jump());
			}
		}
	}

	void SwitchCheckpoint() {
		foundTarget = false;
		// Get new waypoint information.
		if (usingAlt) {
			altCurrent ++;
			cwp = altWaypoints[altCurrent].GetComponent<AIWaypoint>();
		}
		else {
			current ++;
			cwp = waypoints[current].GetComponent<AIWaypoint>();
		}
		// Get new goalwaypoint.
		if (cwp.splitting) {
			int choice;
			if (rPhys.playerNum == GameRam.playerCount + 1) {
				choice = WeightedRandom.Range(new IntRange(0, 0, 20), new IntRange(1, 1, 1));
			}
			else if (rPhys.playerNum == GameRam.playerCount + 2) {
				choice = WeightedRandom.Range(new IntRange(0, 0, 10), new IntRange(1, 1, 1));
			}
			else if (rPhys.playerNum == GameRam.playerCount + 3) {
				choice = WeightedRandom.Range(new IntRange(0, 0, 1), new IntRange(1, 1, 1));
			}
			else {
				choice = WeightedRandom.Range(new IntRange(0, 0, 1), new IntRange(1, 1, 10));
			}
			if (choice == 0) {
				usingAlt = true;
				altCurrent = 1;
				current = waypoints.IndexOf(altWaypoints[altWaypoints.Count-1].GetComponent<AIWaypoint>().nextInChain);
				Debug.LogFormat("{2} is splitting from main path to alt index {1} and will rejoin at index {0}", current, altCurrent,rPhys.charName);
			}
		}
		if (cwp.joining) {
			usingAlt = false;
			Debug.LogFormat("{3} is rejoining main path at index {0} (predicted) from alt index {1}, waypoint named \"{2}\"", current, altCurrent, waypoints[current].gameObject.name, rPhys.charName);
		}
	}

	public void NewLap() {
		current = 0;
		if (usingAlt) Debug.LogErrorFormat("{2} reached new lap while in an alternate route at index {0}, waypoint named {1}", altCurrent, altWaypoints[altCurrent].gameObject.name, rPhys.charName);
	}

	public IEnumerator Jump() {
		int jumps = cwp.tricksPossible;
		if (rPhys.highJumpReady) {
			jumps ++;
		}
		if (jumps == 0) {
			pCon.OnJumpAI(true);
			yield return new WaitForSeconds(jumpDelay);
			pCon.lStickPos = Vector2.zero;
			pCon.OnJumpAI(false);
		}
		else {
			for (int i = 0; i < jumps; i++) {
				pCon.OnJumpAI(true);
				int z = Random.Range(1,9);
				switch (z) {
					case 1:
						pCon.lStickPos = Vector2.up;
					break;
					case 2:
						pCon.lStickPos = new Vector2(1, 1);
					break;
					case 3:
						pCon.lStickPos = Vector2.right;
					break;
					case 4:
						pCon.lStickPos = new Vector2(1, -1);
					break;
					case 5:
						pCon.lStickPos = Vector2.down;
					break;
					case 6:
						pCon.lStickPos = new Vector2(-1, -1);
					break;
					case 7:
						pCon.lStickPos = Vector2.left;
					break;
					case 8:
						pCon.lStickPos = new Vector2(-1, 1);
					break;
					default:
						pCon.lStickPos = Vector2.zero;
					break;
				}
				yield return new WaitForSeconds(jumpDelay);
				pCon.OnJumpAI(false);
				// Debug.Log(i + " tricks attempted. Current direction " + z + ".");
				yield return new WaitForSeconds(chainTrickDelay);
			}
		}
	}
}