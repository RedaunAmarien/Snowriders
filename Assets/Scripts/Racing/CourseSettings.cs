using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CourseSettings : MonoBehaviour {

	public string courseName;
	public int courseIndex, defaultLapCount;
	[Tooltip("Default enemy racer character indices.\nElement 3 will only be used if the player chooses an already-taken slot.")]
	public int[] defaultCpu;
	[Tooltip("Default enemy racer boards.")]
	public int[] defaultCpuBoard;
	[Tooltip("Reward added to total coins at the end of the race, per placement.")]
	public int[] prize;
	public int challengeTicketLevel;
	public Vector2 miniMapAnchorMinDefault, miniMapAnchorMaxDefault, miniMapAnchorMin1p, miniMapAnchorMax1p, miniMapAnchorMin3p, miniMapAnchorMax3p;
    public Transform[] playerSpawn;
	public Transform startPoint;
	public Sprite miniMapSprite;
	public GameObject track;
	public GameObject startCheckpoint, startWaypoint;

	void Awake() {
		GameObject.Find("TrackManager").GetComponent<TrackManager>().Initialize();
	}
}
