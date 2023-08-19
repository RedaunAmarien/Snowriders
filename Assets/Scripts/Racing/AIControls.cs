using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public class AIControls : MonoBehaviour
{
    public bool isNotAI;
    public enum Priority { Speed, Steering, Combos, AttackRed, AttackBlue, RedShop, BlueShop, Coins };
    public Priority currentPriority;
    public float turnAngle;
    public float jumpDelay;
    public float chainTrickDelay;
    public Canvas canvas;
    public bool finished;
    public bool foundTarget;
    //public bool usingAlt;
    //Vector3 offsetWaypoint;
    //AIWaypoint aiWaypoint;
    RacerCore racerCore;
    PlayerRaceControls playerRaceControls;
    public float distFromDest;
    public float altDistFromDest;
    public Vector3 nextCorner;
    public int cornerCount;
    public float stickMoveSpeed;
    public Vector2 targetTurn;
    public Vector2 actualTurn;
    //public float navUpdateTime = 1;
    bool alreadyCrouching;
    private bool shootDelayOn;
    public Vector3 turnAngleTarget;
    [Range(0f, 1f)]
    public float splinePoint;
    public float distanceFromTarget;
    public float minNavRange;
    public float maxNavRange;

    void Start()
    {
        if (isNotAI)
            return;

        finished = false;
        racerCore = GetComponent<RacerCore>();
        playerRaceControls = GetComponent<PlayerRaceControls>();
        //prevWaypoint = startWaypoint;
        //aiWaypoint = prevWaypoint.GetComponent<AIWaypoint>();
        //nextWaypoint = aiWaypoint.nextInChain;
        racerCore.navPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, racerCore.liftPosition, NavMesh.AllAreas, racerCore.navPath);

        if (racerCore.playerNum != 0)
            canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isNotAI)
            return;

        if (finished || playerRaceControls.lockControls)
            return;

        //NavMesh Navigation
        if (racerCore.navPath != null && racerCore.navPath.corners.Length > 1)
        {
            distFromDest = Vector3.Distance(racerCore.navPath.corners[0], racerCore.navPath.corners[1]);
            cornerCount = racerCore.navPath.corners.Length;
            if (racerCore.navPath.corners.Length > 2)
            {
                for (int i = 1; i < racerCore.navPath.corners.Length - 1; i++)
                {
                    distFromDest += Vector3.Distance(racerCore.navPath.corners[i], racerCore.navPath.corners[i + 1]);
                }
            }
            nextCorner = racerCore.navPath.corners[1];
        }
        else
        {
            if (racerCore.distanceToLift < 15)
            {
                nextCorner = racerCore.liftPosition;
            }
            else
            {
                nextCorner = transform.position + transform.forward * 10;
            }
        }
        turnAngle = Vector3.SignedAngle(nextCorner - transform.position, transform.forward, transform.up);
        Debug.DrawLine(transform.position, nextCorner, Color.green);

        ////Spline Navigation
        //splinePoint = racerCore.nearestSplineTime;
        //if (splinePoint >= 1)
        //    splinePoint = 1;

            //racerCore.aiSplinePath.Evaluate(splinePoint, out Unity.Mathematics.float3 pos, out Unity.Mathematics.float3 tan, out Unity.Mathematics.float3 up);

            //Vector3 tangentTarget = Vector3.Normalize(tan) + transform.position;
            //Vector3 positionTarget = (Vector3)pos + Vector3.Normalize(tan)*5;
            //float factor = Mathf.InverseLerp(minNavRange, maxNavRange, racerCore.distanceFromSpline);
            ////float factor = Mathf.Lerp(0, racerCore.speed, racerCore.relativeVelocity.z);

            //turnAngleTarget = Vector3.Lerp(tangentTarget, positionTarget, factor);
            ////distanceFromTarget = Vector3.Distance(transform.position, turnAngleTarget);
            //Debug.DrawLine(transform.position, turnAngleTarget, Color.yellow);
            //turnAngle = Vector3.SignedAngle(turnAngleTarget - transform.position, transform.forward, transform.up);

            //Decide current Priority
        if (Mathf.Abs(turnAngle) > 60)
            currentPriority = Priority.Steering;
        else if (racerCore.relativeVelocity.z <= racerCore.speed * 0.01f)
            currentPriority = Priority.Speed;
        else if (Mathf.Abs(turnAngle) > 5)
            currentPriority = Priority.Steering;
        else if (racerCore.relativeVelocity.z <= racerCore.speed * 0.5f)
            currentPriority = Priority.Speed;
        else if (cornerCount > 2 && racerCore.navPath.corners[1].y - racerCore.navPath.corners[2].y > 5)
            currentPriority = Priority.Combos;

        else if (racerCore.currentWeapon != ItemProjectile.WeaponType.None)
            currentPriority = Priority.AttackRed;
        else if (racerCore.currentItem != ItemProjectile.ItemType.None)
            currentPriority = Priority.AttackBlue;
        else if (racerCore.coins < 100)
            currentPriority = Priority.Coins;
        else if (racerCore.currentWeapon == ItemProjectile.WeaponType.None)
            currentPriority = Priority.RedShop;
        else if (racerCore.currentItem == ItemProjectile.ItemType.None)
            currentPriority = Priority.BlueShop;

        switch (currentPriority)
        {
            case Priority.Speed:
                if (racerCore.grounded)
                    StartCoroutine(Jump());
                break;
            case Priority.Steering:
                bool leftTurn = turnAngle > 0 ? true : false;
                if (Mathf.Abs(turnAngle) > 45)
                    targetTurn = leftTurn ? new Vector2(-1, -1).normalized : new Vector2(1, -1).normalized;
                else if (Mathf.Abs(turnAngle) > 5)
                    targetTurn = leftTurn ? Vector2.left : Vector2.right;
                else
                    targetTurn = Vector2.zero;
                break;
            case Priority.Combos:
                float dropDist = racerCore.navPath.corners[1].y - racerCore.navPath.corners[2].y;
                if (Vector3.Distance(transform.position, nextCorner) < 3.5f)
                {
                    StartCoroutine(Jump(dropDist));
                }
                break;
            case Priority.AttackRed:
                if (shootDelayOn)
                    break;
                playerRaceControls.OnShootAI();
                StartCoroutine(ShootDelay());
                break;
            case Priority.AttackBlue:
                playerRaceControls.OnItemAI();
                break;
            case Priority.RedShop:
                break;
            case Priority.BlueShop:
                break;
            case Priority.Coins:
                break;
        }
        if (currentPriority != Priority.Steering)
            targetTurn = Vector2.zero;
        actualTurn = Vector2.MoveTowards(actualTurn, targetTurn, stickMoveSpeed * Time.deltaTime);
        playerRaceControls.lStickPos = actualTurn;
    }

    //void SwitchWaypoint()
    //{
    //    if (isNotAI)
    //        return;

    //    foundTarget = false;
    //    // Get new waypoint information.
    //    prevWaypoint = nextWaypoint;
    //    aiWaypoint = prevWaypoint.GetComponent<AIWaypoint>();

    //    // Get new goalwaypoint.
    //    if (aiWaypoint.splitting)
    //    {
    //        int choice;
    //        if (racerCore.playerNum == GameRam.playerCount + 1)
    //        {
    //            choice = WeightedRandom.Range(new IntRange(0, 0, 20), new IntRange(1, 1, 1));
    //        }
    //        else if (racerCore.playerNum == GameRam.playerCount + 2)
    //        {
    //            choice = WeightedRandom.Range(new IntRange(0, 0, 10), new IntRange(1, 1, 1));
    //        }
    //        else if (racerCore.playerNum == GameRam.playerCount + 3)
    //        {
    //            choice = WeightedRandom.Range(new IntRange(0, 0, 1), new IntRange(1, 1, 1));
    //        }
    //        else
    //        {
    //            choice = WeightedRandom.Range(new IntRange(0, 0, 1), new IntRange(1, 1, 10));
    //        }
    //        if (choice == 0)
    //        {
    //            nextWaypoint = aiWaypoint.nextInAltChain;
    //        }
    //        else nextWaypoint = aiWaypoint.nextInChain;
    //    }
    //    else nextWaypoint = aiWaypoint.nextInChain;
    //    if (aiWaypoint.joining)
    //    {
    //        Debug.LogFormat("{0} is rejoining main path at waypoint {1} from waypoint {2}", racerCore.character.characterName, prevWaypoint.name, nextWaypoint.name);
    //    }

    //    float dist = Random.Range(0, nextWaypoint.GetComponent<AIWaypoint>().targetableRadius);
    //    float angle = Random.Range(0, 360);
    //    var x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
    //    var y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
    //    offsetWaypoint = new Vector3(x, 0, y) + nextWaypoint.transform.position;
    //}

    public void NewLap()
    {
        if (isNotAI)
            return;

        //nextWaypoint = startWaypoint;
        splinePoint = 0;
    }

    public IEnumerator ShootDelay()
    {
        shootDelayOn = true;
        yield return new WaitForSeconds(1);
        shootDelayOn = false;
    }

    public IEnumerator Jump(float dropDist = 0)
    {
        if (alreadyCrouching)
            yield break; ;
        alreadyCrouching = true;
        int jumps = Mathf.FloorToInt(dropDist / 3f);
        if (racerCore.highJumpReady)
        {
            jumps++;
        }
        if (jumps == 0)
        {
            playerRaceControls.OnJumpAI(true);
            yield return new WaitForSeconds(jumpDelay);
            playerRaceControls.lStickPos = Vector2.zero;
            playerRaceControls.OnJumpAI(false);
            alreadyCrouching = false;
        }
        else
        {
            for (int i = 0; i < jumps; i++)
            {
                Debug.LogFormat("Player {0} attempting chain of {1} more tricks off {2:N2} meter ledge.", racerCore.playerNum, jumps - i, dropDist);
                playerRaceControls.OnJumpAI(true);
                int z = Random.Range(1, 9);
                playerRaceControls.lStickPos = z switch
                {
                    1 => Vector2.up,
                    2 => new Vector2(0.7f, 0.7f),
                    3 => Vector2.right,
                    4 => new Vector2(0.7f, -0.7f),
                    5 => Vector2.down,
                    6 => new Vector2(-0.7f, -0.7f),
                    7 => Vector2.left,
                    8 => new Vector2(-0.7f, 0.7f),
                    _ => Vector2.zero,
                };
                yield return new WaitForSeconds(jumpDelay);
                playerRaceControls.OnJumpAI(false);
                // Debug.Log(i + " tricks attempted. Current direction " + z + ".");
                yield return new WaitForSeconds(chainTrickDelay);
            }
            alreadyCrouching = false;
        }
    }
}