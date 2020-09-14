using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayCam : MonoBehaviour {

    public Transform target, currentViewpoint;
    public Vector3 viewOffset, followOffset;
    public float camSmoothTime;
    public int currentTarget;
    GameObject[] players;
    public enum Mode {Random, Static};
    public Mode currentMode;
    [Min(1)]
    public float switchTimeMin, switchTimeMax;
    bool sitting;

    void Start() {
        players = GameObject.FindGameObjectsWithTag("Player");
        target = players[0].transform;
        currentViewpoint = players[0].transform;
        if (currentMode == Mode.Random) {
            StartCoroutine(RandomSwitch());
        }
    }

    void FixedUpdate() {
        transform.LookAt(target.transform.position + viewOffset);
        if (sitting) {
            transform.position = currentViewpoint.position;
        }
        else {
            Vector3 vel = Vector3.zero;
            transform.position = Vector3.SmoothDamp(transform.position, currentViewpoint.position + followOffset, ref vel, camSmoothTime);
        }
    }

    public void EnterCamZone(int player, Transform other) {
        if (player == currentTarget) {
            currentViewpoint = other;
            sitting = true;
            Debug.Log("Replay cam moving to watch from" + other.gameObject.name);
        }
    }

    IEnumerator RandomSwitch() {
        currentTarget = Random.Range(0,GameRam.playerCount);
        target = players[currentTarget].transform;
        currentViewpoint = target;
        sitting = false;
        Debug.Log("New replay target is player " + currentTarget + ". Following directly.");
        float switchTime = Random.Range(switchTimeMin, switchTimeMax);
        yield return new WaitForSeconds(switchTime);
        if (currentMode == Mode.Random) StartCoroutine(RandomSwitch());
    }
}
