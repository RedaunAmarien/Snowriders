using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIControls : MonoBehaviour
{
    public bool isNotAI;
    public GameObject startWaypoint, prevWaypoint, nextWaypoint;
    [Tooltip("The point at which the AI will switch to the next waypoint.\n0 = Halfway between, 1 = At the next waypoint."), Range(0, 1)]
    public float wayPointChangePoint;
    public float turnAngle, jumpDelay, chainTrickDelay;
    public Canvas canvas;
    public bool finished, foundTarget, usingAlt;
    Vector3 offsetWaypoint;
    AIWaypoint aiWaypoint;
    RacerCore racerCore;
    PlayerRaceControls playerRaceControls;
    NavMeshPath navPath;
    public float distFromDest;
    public float altDistFromDest;
    public Vector3 nextCorner;
    public int cornerCount;
    public Vector3 goal;
    float navTick = 0;
    public float stickMoveSpeed;
    public Vector2 targetTurn;
    public Vector2 actualTurn;
    public float navUpdateTime = 1;
    bool alreadyCrouching;

    void Start()
    {
        if (isNotAI)
            return;

        finished = false;
        racerCore = GetComponent<RacerCore>();
        playerRaceControls = GetComponent<PlayerRaceControls>();
        prevWaypoint = startWaypoint;
        aiWaypoint = prevWaypoint.GetComponent<AIWaypoint>();
        nextWaypoint = aiWaypoint.nextInChain;
        goal = GameObject.Find("Lift").transform.position;
        navPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, goal, NavMesh.AllAreas, navPath);

        if (racerCore.playerNum != 0) canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isNotAI)
            return;

        if (!finished && !playerRaceControls.lockControls)
        {
            //Jump when slowed down
            if (racerCore.relativeVelocity.z <= racerCore.speed * .4f)
            {
                StartCoroutine(Jump(0));
            }

            //Update Waypoints
            if (NavMesh.CalculatePath(transform.position, goal, NavMesh.AllAreas, navPath))
            {
                distFromDest = Vector3.Distance(navPath.corners[0], navPath.corners[1]);
                cornerCount = navPath.corners.Length;
                for (int i = 1; i < navPath.corners.Length - 1; i++)
                {
                    Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.magenta);
                    distFromDest += Vector3.Distance(navPath.corners[i], navPath.corners[i + 1]);
                }
                nextCorner = navPath.corners[1];
                float dropDist = Vector3.Distance(transform.position, navPath.corners[1]);
                if (cornerCount > 2 && navPath.corners[1].y - navPath.corners[2].y > 2 && dropDist < 2.5f)
                {
                    StartCoroutine(Jump(dropDist));
                }
            }
            else
            {
                if (distFromDest < 5)
                {
                    nextCorner = goal;
                }
                else
                {
                    nextCorner = transform.position + transform.forward * 10;
                }
            }

            //Follow Waypoints
            turnAngle = Vector3.SignedAngle(nextCorner - transform.position, transform.forward, transform.up);
            Debug.DrawLine(transform.position, nextCorner, Color.green);

            bool leftTurn = turnAngle > 0 ? true : false;
            if (Mathf.Abs(turnAngle) > 45)
                targetTurn = leftTurn ? new Vector2(-1, -1).normalized : new Vector2(1, -1).normalized;
            else if (Mathf.Abs(turnAngle) > 5)
                targetTurn = leftTurn ? Vector2.left : Vector2.right;
            else
                targetTurn = Vector2.zero;

            actualTurn = Vector2.MoveTowards(actualTurn, targetTurn, stickMoveSpeed * Time.deltaTime);
            playerRaceControls.lStickPos = actualTurn;

            // Use items
            if (racerCore.itemType != racerCore.blankItem) playerRaceControls.OnItemAI();
            if (racerCore.weaponType != racerCore.blankWeap) playerRaceControls.OnShootAI();
        }
    }

    void OnTriggerEnter(Collider other)
    {
    }

    void SwitchWaypoint()
    {
        if (isNotAI)
            return;

        foundTarget = false;
        // Get new waypoint information.
        prevWaypoint = nextWaypoint;
        aiWaypoint = prevWaypoint.GetComponent<AIWaypoint>();

        // Get new goalwaypoint.
        if (aiWaypoint.splitting)
        {
            int choice;
            if (racerCore.playerNum == GameRam.playerCount + 1)
            {
                choice = WeightedRandom.Range(new IntRange(0, 0, 20), new IntRange(1, 1, 1));
            }
            else if (racerCore.playerNum == GameRam.playerCount + 2)
            {
                choice = WeightedRandom.Range(new IntRange(0, 0, 10), new IntRange(1, 1, 1));
            }
            else if (racerCore.playerNum == GameRam.playerCount + 3)
            {
                choice = WeightedRandom.Range(new IntRange(0, 0, 1), new IntRange(1, 1, 1));
            }
            else
            {
                choice = WeightedRandom.Range(new IntRange(0, 0, 1), new IntRange(1, 1, 10));
            }
            if (choice == 0)
            {
                nextWaypoint = aiWaypoint.nextInAltChain;
            }
            else nextWaypoint = aiWaypoint.nextInChain;
        }
        else nextWaypoint = aiWaypoint.nextInChain;
        if (aiWaypoint.joining)
        {
            Debug.LogFormat("{0} is rejoining main path at waypoint {1} from waypoint {2}", racerCore.character.characterName, prevWaypoint.name, nextWaypoint.name);
        }

        float dist = Random.Range(0, nextWaypoint.GetComponent<AIWaypoint>().targetableRadius);
        float angle = Random.Range(0, 360);
        var x = dist * Mathf.Cos(angle * Mathf.Deg2Rad);
        var y = dist * Mathf.Sin(angle * Mathf.Deg2Rad);
        offsetWaypoint = new Vector3(x, 0, y) + nextWaypoint.transform.position;
    }

    public void NewLap()
    {
        if (isNotAI)
            return;

        nextWaypoint = startWaypoint;
    }

    public IEnumerator Jump(float dropDist)
    {
        if (alreadyCrouching)
            yield break; ;
        alreadyCrouching = true;
        int jumps = Mathf.FloorToInt(dropDist/2f);
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
                alreadyCrouching = false;
                // Debug.Log(i + " tricks attempted. Current direction " + z + ".");
                yield return new WaitForSeconds(chainTrickDelay);
            }
        }
    }
}