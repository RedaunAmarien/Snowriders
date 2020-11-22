using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;

public class AIControls : MonoBehaviour {

	public GameObject startWaypoint, prevWaypoint, nextWaypoint;
	[Tooltip("The point at which the AI will switch to the next waypoint.\n0 = Halfway between, 1 = At the next checkpoint."), Range(0,1)]
	public float wayPointChangePoint;
	public float turnAng, jumpDelay, chainTrickDelay;
	public Canvas canvas;
	public bool finished, foundTarget, usingAlt;
	AIWaypoint pwpScript;
	RacerPhysics rPhys;
	PlayerRaceControls pCon;
	// public List<GameObject> waypoints = new List<GameObject>();
	// public List<GameObject> altWaypoints = new List<GameObject>();

	void Start() {
		finished = false;
		rPhys = GetComponent<RacerPhysics>();
		pCon = GetComponent<PlayerRaceControls>();
		prevWaypoint = startWaypoint;
		pwpScript = prevWaypoint.GetComponent<AIWaypoint>();
		nextWaypoint = pwpScript.nextInChain;

		// waypoints.Add(startWaypoint);
		// int stopper1 = 255;
		// int stopper2 = 0;
		// int altBegindex = 0;
		// Debug.Log("Starting waypoint loop.");
		// for (int i = 0; i < stopper1; i++) {
		// 	waypoints.Add(waypoints[i].GetComponent<AIWaypoint>().nextInChain);
		// 	if (waypoints[i+1].GetComponent<AIWaypoint>().splitting) {
		// 		Debug.Log("Splitter found.");
		// 		altBegindex = i+1;
		// 		stopper2 = waypoints[i+1].GetComponent<AIWaypoint>().totalInAltChain-1;
		// 	}
		// 	else if (waypoints[i+1] == waypoints[0]) {
		// 		Debug.Log("End found.");
		// 		stopper1 = i;
		// 		waypoints.RemoveAt(i+1);
		// 	}
		// }
		// altWaypoints.Add(waypoints[altBegindex].GetComponent<AIWaypoint>().nextInAltChain);
		// for (int i = 0; i < stopper2; i++) {
		// 	altWaypoints.Add(altWaypoints[i].GetComponent<AIWaypoint>().nextInChain);
		// }
		// Debug.LogFormat("Listed {0} waypoints and {1} alt waypoints.", waypoints.Count, altWaypoints.Count);
		// pwpScript = prevWaypoint.GetComponent<AIWaypoint>();
		if (rPhys.playerNum != 0) canvas.gameObject.SetActive(false);
	}

	void Update() {
		if (!finished && !pCon.lockControls) {
			//Jump when slowed down
			if (rPhys.relVel.z <= rPhys.speed*.4f) {
				StartCoroutine(Jump());
			}

			//Update Waypoints
			float a = (transform.position - prevWaypoint.transform.position).sqrMagnitude;
			float b = (transform.position - nextWaypoint.transform.position).sqrMagnitude;
			float c = (prevWaypoint.transform.position - nextWaypoint.transform.position).sqrMagnitude;
			if (a - b >= c * wayPointChangePoint) {
				SwitchWaypoint();
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
				turnAng = Vector3.SignedAngle(transform.forward, transform.position - nextWaypoint.transform.position, transform.up);
				Debug.DrawLine(transform.position, prevWaypoint.transform.position, Color.cyan, .1f);
				Debug.DrawLine(transform.position, nextWaypoint.transform.position, Color.magenta, .1f);
				if (turnAng > 45) pCon.lStickPos = new Vector2(-0.7f, -0.7f);
				else if (turnAng < -45) pCon.lStickPos = new Vector2(0.7f, -0.7f);
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
			if (other.gameObject == nextWaypoint) {
				SwitchWaypoint();
			}
			// Reset Waypoint if discovering one outside of range.
			if (other.gameObject != nextWaypoint && other.gameObject != prevWaypoint) {
				GameObject lostWaypoint = nextWaypoint;
				nextWaypoint = other.gameObject;
				Debug.LogWarningFormat("{0} got distracted by waypoint {1} while looking for waypoint {2} at location {3}.", rPhys.charName, prevWaypoint, lostWaypoint, transform.position);
				SwitchWaypoint();
			}
			// Act on Waypoint flags
			if (pwpScript.tryJump && !pCon.lockControls && rPhys.grounded) {
				StartCoroutine(Jump());
			}
		}
	}

	void SwitchWaypoint() {
		foundTarget = false;
		// Get new waypoint information.
		prevWaypoint = nextWaypoint;
		pwpScript = prevWaypoint.GetComponent<AIWaypoint>();
		// Get new goalwaypoint.
		if (pwpScript.splitting) {
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
				nextWaypoint = pwpScript.nextInAltChain;
			}
			else nextWaypoint = pwpScript.nextInChain;
		}
		else nextWaypoint = pwpScript.nextInChain;
		if (pwpScript.joining) {
			Debug.LogFormat("{0} is rejoining main path at waypoint {1} from waypoint {2}", rPhys.charName, prevWaypoint.gameObject.name, nextWaypoint.gameObject.name);
		}
	}

	public void NewLap() {
		nextWaypoint = startWaypoint;
	}

	public IEnumerator Jump() {
		int jumps = pwpScript.tricksPossible;
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
						pCon.lStickPos = new Vector2(0.7f, 0.7f);
					break;
					case 3:
						pCon.lStickPos = Vector2.right;
					break;
					case 4:
						pCon.lStickPos = new Vector2(0.7f, -0.7f);
					break;
					case 5:
						pCon.lStickPos = Vector2.down;
					break;
					case 6:
						pCon.lStickPos = new Vector2(-0.7f, -0.7f);
					break;
					case 7:
						pCon.lStickPos = Vector2.left;
					break;
					case 8:
						pCon.lStickPos = new Vector2(-0.7f, 0.7f);
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