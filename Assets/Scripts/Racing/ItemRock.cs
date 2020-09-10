using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRock : MonoBehaviour {
	public float lifetime;
	public float useForce;
	Rigidbody rigid;

	void Start () {
		rigid = GetComponent<Rigidbody>();
		rigid.AddRelativeForce(0,useForce,0, ForceMode.VelocityChange);
	}

	IEnumerator Lifetime() {
		yield return new WaitForSeconds(lifetime);
		Destroy(gameObject);
	}
}
