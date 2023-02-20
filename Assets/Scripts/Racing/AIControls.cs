using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;

public class AIControls : MonoBehaviour {

	public GameObject startWaypoint, prevWaypoint, nextWaypoint;
	[Tooltip("The point at which the AI will switch to the next waypoint.\n0 = Halfway between, 1 = At the next waypoint."), Range(0,1)]
	public float wayPointChangePoint;
	public float turnAng, jumpDelay, chainTrickDelay;
	public Canvas canvas;
	public bool finished, foundTarget, usingAlt;
	Vector3 offsetWaypoint;
	AIWaypoint pwpScript;
	RacerPhysics rPhys;
	PlayerRaceControls pCon;

	void Start() {
		finished = false;
		rPhys = GetComponent<RacerPhysics>();
		pCon = GetComponent<PlayerRaceControls>();
		prevWaypoint = startWaypoint;
		pwpScript = prevWaypoint.GetComponent<AIWaypoint>();
		nextWaypoint = pwpScript.nextInChain;

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

			//Follow Waypoints
			turnAng = Vector3.SignedAngle(transform.forward, transform.position - offsetWaypoint, transform.up);
			Debug.DrawLine(transform.position, prevWaypoint.transform.position, Color.white, .1f);
			Debug.DrawLine(transform.position, offsetWaypoint, Color.magenta, .1f);

			if (turnAng > 45) pCon.lStickPos = new Vector2(-0.7f, -0.7f);
			else if (turnAng < -45) pCon.lStickPos = new Vector2(0.7f, -0.7f);
			else if (turnAng > 5) pCon.lStickPos = Vector2.left;
			else if (turnAng < -5) pCon.lStickPos = Vector2.right;
			else pCon.lStickPos = Vector2.zero;

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
		
		float dist = Random.Range(0, nextWaypoint.GetComponent<AIWaypoint>().targetableRadius);
		float angle = Random.Range(0,360);
		var x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
		var y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
		offsetWaypoint = new Vector3(x, 0, y) + nextWaypoint.transform.position;
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
                pCon.lStickPos = z switch
                {
                    1 => Vector2.up,
                    2 => new Vector2(0.7f, 0.7f),
                    3 => Vector2.right,
                    4 => new Vector2(0.7f, -0.7f),
                    5 => Vector2.down,
                    6 => new Vector2(-0.7f, -0.7f),
                    7 => Vector2.left,
                    8 => new Vector2(-0.7f, 0.7f),
                    _ => Vector2.zero,
                };
                yield return new WaitForSeconds(jumpDelay);
				pCon.OnJumpAI(false);
				// Debug.Log(i + " tricks attempted. Current direction " + z + ".");
				yield return new WaitForSeconds(chainTrickDelay);
			}
		}
	}
}