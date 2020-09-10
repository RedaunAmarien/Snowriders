using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;

public class AIControls : MonoBehaviour {

	public GameObject currentWaypoint, nextWaypoint, firstWaypoint;
	public float aiTurnSpeed;
	public Vector3 currLookAt;
	public Canvas canvas;
	public bool finished, locked;
	AIWaypoint cwp;
	RacerPhysics rPhys;

	void Start() {
		firstWaypoint = GameObject.Find("FirstWaypoint");
		locked = true;
		finished = false;
		rPhys = GetComponent<RacerPhysics>();
		currentWaypoint = firstWaypoint;
		nextWaypoint = currentWaypoint.GetComponent<AIWaypoint>().nextInChain;
		cwp = currentWaypoint.GetComponent<AIWaypoint>();
		canvas.gameObject.SetActive(false);
	}
	void Update() {
		if (!finished && !locked) {
			// Jump when slowed down
			if (rPhys.relVel.z <= rPhys.speed/2) rPhys.Jump();

			// Follow Waypoints
			float turnAng = (Vector3.SignedAngle(transform.forward, transform.position - nextWaypoint.transform.position, transform.up));
			float t = rPhys.turnSpeed * Time.deltaTime;
			if (turnAng > 0) {
				transform.Rotate(0,-t,0);
			}
			if (turnAng < 0) {
				transform.Rotate(0,t,0);
			}

			// Use items
			if (rPhys.itemType != rPhys.blankItem) rPhys.Item();
			if (rPhys.weapType != rPhys.blankWeap) rPhys.Shoot();

			if (Vector3.Distance(transform.position, currentWaypoint.transform.position) > Vector3.Distance(transform.position, nextWaypoint.transform.position)) {
				SwitchCheckpoint();
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.GetComponent<AIWaypoint>() != null) {
			if (other.gameObject != nextWaypoint && other.gameObject != currentWaypoint) {
				nextWaypoint = other.gameObject.GetComponent<AIWaypoint>().nextInChain;
				SwitchCheckpoint();
			}
			// Act on Waypoint flags
			if (cwp.jump && !locked && rPhys.grounded) {
				StartCoroutine(Jump(1));
			}
		}
	}

	void SwitchCheckpoint() {
		// Get new waypoint information.
		currentWaypoint = nextWaypoint;
		cwp = currentWaypoint.GetComponent<AIWaypoint>();
		// Get new goalwaypoint.
		if (cwp.splitting) {
			int choice = Random.Range(0,2);
			if (choice == 1) nextWaypoint = currentWaypoint.GetComponent<AIWaypoint>().nextInChain2;
			else nextWaypoint = currentWaypoint.GetComponent<AIWaypoint>().nextInChain;
		}
		else {
			nextWaypoint = currentWaypoint.GetComponent<AIWaypoint>().nextInChain;
		}

	}

	public void NewLap() {
		currentWaypoint = firstWaypoint;
		nextWaypoint = currentWaypoint.GetComponent<AIWaypoint>().nextInChain;
	}

	public IEnumerator Jump(int x) {
		int y = cwp.tricksPossible;
		if (rPhys.highJumpReady) y ++;
		rPhys.Jump();
		for (int i = x; i <= y; i++){
			int z = Random.Range(1,9);
			StartCoroutine(rPhys.Trick(z, rPhys.ltdr));
			print (i + " tricks attempted. Current direction " + z + ".");
			yield return new WaitForSeconds(.55f);
		}
	}
}