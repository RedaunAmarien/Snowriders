using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Lift : Checkpoint
{
    public float cooldownTime;
    public bool onCooldown;
    Collider thisCollider;
    CinemachineSmoothPath path;

    private void Start()
    {
        path = GetComponent<CinemachineSmoothPath>();
        thisCollider = GetComponent<Collider>();
        type = CheckpointType.Lift;
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
        player.GetComponent<RacerCore>().isOnLift = true;
        while (scale < path.MaxPos)
        {
            yield return null;
            player.transform.position = path.EvaluatePosition(scale);
            scale += Time.deltaTime;
        }
        player.isKinematic = false;
        player.transform.SetPositionAndRotation(player.GetComponent<RacerCore>().playerStartPoint.position, player.GetComponent<RacerCore>().playerStartPoint.rotation);
        player.AddRelativeForce(new Vector3(0, 0, player.GetComponent<RacerCore>().jumpForce.z * 2));
        player.GetComponent<RacerCore>().currentLap++;
        player.GetComponent<RacerCore>().isOnLift = false;
    }
}
