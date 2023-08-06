using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public float value;
    public GameObject nextCheck;
    public enum CheckpointType { Default, StartLine, FinishLine, Lift };
    public CheckpointType type;
    public float cooldownTime;
    public bool onCooldown;
    Collider thisCollider;
    CinemachineSmoothPath path;

    private void Start()
    {
        thisCollider = GetComponent<Collider>();
        path = GetComponent<CinemachineSmoothPath>();
        if (gameObject.name == "Lift")
            type = CheckpointType.Lift;
        else if (gameObject.name == "Start")
            type = CheckpointType.StartLine;
        else if (gameObject.name == "Finish")
            type = CheckpointType.FinishLine;
    }

    public void Cooldown(Rigidbody player)
    {
        if (onCooldown)
            return;

        StartCoroutine(CooldownTimer());
        player.isKinematic = true;
        //player.transform.position = path.m_Waypoints[0].position;
        StartCoroutine(Animate(player));
    }

    public IEnumerator CooldownTimer()
    {
        thisCollider.enabled = false;
        onCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        onCooldown = false;
        thisCollider.enabled = true;
    }

    public IEnumerator Animate(Rigidbody player)
    {
        float scale = 0;
        while (scale < path.MaxPos)
        {
            yield return null;
            player.transform.position = path.EvaluatePosition(scale);
            scale += Time.deltaTime;
        }
        player.isKinematic = false;
        player.transform.SetPositionAndRotation(player.GetComponent<RacerCore>().playerStartPoint.position, player.GetComponent<RacerCore>().playerStartPoint.rotation);
        player.AddRelativeForce(new Vector3(0, 0, player.GetComponent<RacerCore>().jumpForce.z * 2));
    }
}
