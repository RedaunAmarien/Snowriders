using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWaypoint : MonoBehaviour {

    public GameObject nextInChain;
    public bool splitting;
    public GameObject nextInAltChain;
    public int totalInAltChain;
    public bool joining;
    public bool tryJump, tryGrab;
    public int tricksPossible;
    public float targetableRadius = 4;
    public float targetRadius = 0.5f;

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(gameObject.transform.position, targetableRadius);
    }
}
