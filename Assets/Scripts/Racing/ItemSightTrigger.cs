using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSightTrigger : MonoBehaviour {
	void OnTriggerEnter (Collider other) {
		transform.parent.GetComponent<ItemProjectile>().OnTriggerEnterExternal(other);
	}
	void OnTriggerExit (Collider other) {
		transform.parent.GetComponent<ItemProjectile>().OnTriggerExitExternal(other);
	}
}
