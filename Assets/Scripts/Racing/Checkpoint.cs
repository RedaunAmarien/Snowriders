using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public float value;
    public GameObject nextCheck;
    [Tooltip("The location to respawn at, relative to the object's transform.")]
    public Vector3 respawnPositionOffset = Vector3.zero;
    [Tooltip("The rotation to respawn at, in world rotation.")]
    public float respawnRotation = 0;
    public enum CheckpointType { Default, StartLine, FinishLine, Lift };
    public CheckpointType type;

    private void Start()
    {
        if (gameObject.name == "Start")
            type = CheckpointType.StartLine;
        else if (gameObject.name == "Finish")
            type = CheckpointType.FinishLine;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + respawnPositionOffset, 0.25f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(respawnPositionOffset + transform.position,  Quaternion.Euler(0, respawnRotation, 0) * transform.forward);
    }
}
