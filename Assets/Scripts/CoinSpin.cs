using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CoinSpin : MonoBehaviour {

	public int respawnTime;
	public float spinSpeed;
	int startRot;
	public Vector2 spinRange;
	bool nega, noSpawn;
	void Start() {
		startRot = Random.Range(0,360);
		int choice = Random.Range(0,10);
		if (choice > 5) nega = false;
		else nega = true;
		transform.Rotate(0,startRot,0);
		spinSpeed = Random.Range(spinRange.x,spinRange.y);
	}
	void Update () {
		if (nega) transform.Rotate(new Vector3(0,-spinSpeed,0) * Time.deltaTime);
		else transform.Rotate(new Vector3(0,spinSpeed,0) * Time.deltaTime);
	}
	public IEnumerator Respawn() {
		gameObject.SetActive(false);
		yield return new WaitForSeconds(respawnTime);
		gameObject.SetActive(true);
	}
}
