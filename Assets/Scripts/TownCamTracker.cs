using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownCamTracker : MonoBehaviour {
    public GameObject player;
    void LateUpdate() {
        transform.LookAt(player.transform.position, Vector3.up);
    }
}
