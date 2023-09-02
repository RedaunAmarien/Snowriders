using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public class RacerCore : MonoBehaviour
{

    public int playerNum;

    [Header("Character Settings")]
    public Character character;
    public Board board;
    public float speed;
    public float traction;
    public float turnSpeed;
    public Vector3 jumpForce;

    [Header("Race Stats")]
    public bool finished;
    public int place;
    public int currentLap;
    public float nextCheckVal;
    public float checkDist;
    public int finalPlace;
    public int totalLaps;
    public int coins;
    public float splineLength;
    public Vector3 liftPosition;
    public float distanceToLift;
    public float distanceFromSpline;
    public Unity.Mathematics.float3 nearestSplinePoint;
    public float nearestSplineTime;
    [SerializeField] Vector3 respawnPosition;
    [SerializeField] Quaternion respawnRotation;
    SplineContainer aiSpline;
    public SplinePath aiSplinePath;
    public bool isOnLift;

    [Header("Items and Weapons")]
    public ItemProjectile.ItemType currentItem;
    public ItemProjectile.WeaponType currentWeapon;
    public int weaponAmmo;
    public enum DefenseStatus { Normal, BoostLow, BoostHigh, Parachuting, Ice, Snow, Rolling, Tripped, Flattened, OOB };
    public DefenseStatus status;
    [SerializeField] int statusTime;
    [SerializeField] int rollTime;
    [SerializeField] int standTime;
    [Tooltip("X = Lifetime, Y = Acceleration, Z = Additional Max Speed")]
    [SerializeField] Vector3 rocket1;
    [SerializeField] Vector3 rocket2;
    [SerializeField] Vector3 highJumpForce;
    [SerializeField] float moneyBoardTime;
    [SerializeField] float statusTimer;
    [SerializeField] float iceTimer;

    [Header("Physics")]
    public Vector3 relativeVelocity;
    public float turnFactor;
    public bool grounded;
    [SerializeField] bool boostOn;
    [SerializeField] float boostForce;
    [SerializeField] float boostAddSpeed;
    public bool invisible;
    public bool spotLock;
    public bool grabbing;
    public bool tricking;
    [SerializeField] bool jumping;
    public bool highJumpReady;
    [SerializeField] bool respawning;
    public int ltdr;
    public int lgdr;
    [SerializeField] int grabsChained;
    [SerializeField] int grabsCombo;
    [SerializeField] int tricksChained;
    [SerializeField] int slows;

    // Objects
    [Header("References")]
    public List<Checkpoint> checkpointList;
    public GameObject firstCheckpoint;
    public TrackManager trackManager;
    public Animator animator;
    public Transform playerStartPoint;
    public NavMeshPath navPath;
    [SerializeField] GameObject lastCheckpoint;
    [SerializeField] GameObject nextCheckpoint;
    [SerializeField] AudioMaker audioMaker;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AIControls aiControls;
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject rock;
    [SerializeField] GameObject rockSpawn;
    [SerializeField] GameObject projectile;
    [SerializeField] GameObject shootSpawn;
    [SerializeField] GameObject dropCoin;
    [SerializeField] GameObject characterModel;
    [SerializeField] GameObject iceCube;
    [SerializeField] GameObject snowman;
    [SerializeField] GameObject balloon;
    [SerializeField] GameObject highJumpParticles;
    [SerializeField] GameObject lockParticles;
    [SerializeField] GameObject slowed;
    [SerializeField] GameObject rocketParticles1;
    [SerializeField] GameObject rocketParticles2;
    [SerializeField] SpriteRenderer headSprite;
    [SerializeField] Sprite[] headSpriteSrc;

    //Internal
    public PlayerUI playerUI;
    PlayerRaceControls playerRaceControls;
    Rigidbody rigid;
    bool initialized;
    bool replayCamOn;
    float timer = 0;

    void Start()
    {
        // Initialize Objects and Stats
        trackManager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        rigid = gameObject.GetComponent<Rigidbody>();

        playerRaceControls = gameObject.GetComponent<PlayerRaceControls>();
        playerUI = GetComponent<PlayerUI>();
        aiControls = GetComponent<AIControls>();
        audioMaker = GetComponentInChildren<AudioMaker>();
        audioSource = GetComponentInChildren<AudioSource>();
        if (playerNum < GameRam.playerCount)
            audioSource.spatialBlend = 0;
        jumpForce.z = speed * 10;
        lastCheckpoint = firstCheckpoint;
        nextCheckpoint = firstCheckpoint.GetComponent<Checkpoint>().nextCheck;
        nextCheckVal = nextCheckpoint.GetComponent<Checkpoint>().value;

        currentLap = 1;
        liftPosition = GameObject.Find("Lift").transform.position;

        rigid = gameObject.GetComponent<Rigidbody>();
        finished = false;
        //rigid.maxAngularVelocity = 0.05f;

        // Initialize items.
        currentWeapon = ItemProjectile.WeaponType.None;
        currentItem = ItemProjectile.ItemType.None;
        weaponAmmo = 0;
        coins = 0;

        replayCamOn = true;

        liftPosition = GameObject.Find("Lift").transform.position;
        navPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, liftPosition, NavMesh.AllAreas, navPath);
    }

    public void Initialize(bool demoMode = false)
    {
        //if (!demoMode && playerNum == 0)
        //    playerUI.playerCamera.GetComponent<AudioListener>().enabled = true;

        // Become character.
        headSprite.sprite = character.charSprite;
        if (playerNum < GameRam.playerCount)
            headSprite.gameObject.SetActive(false);

        //Speed: Max 18, Min 15.
        speed = Mathf.LerpUnclamped(15, 18, (character.speed + board.speed) / 10f);

        //Traction: Max .04, Min .015.
        traction = Mathf.LerpUnclamped(0.015f, 0.04f, (character.turn + board.turn) / 10f);

        //Jump: Max 250, Min 175.
        jumpForce.y = Mathf.LerpUnclamped(175, 250, (character.jump + board.jump) / 10f);

        totalLaps = GameRam.lapCount;

        if (board.boardName == "Pound of Feather")
        {
            jumpForce.y = 300;
            jumpForce.z = 20;
        }
        else if (board.boardName == "Block of Soap")
        {
            traction = 0;
        }
        initialized = true;
        spotLock = true;

        //StartCoroutine(DelayStart());
    }

    //IEnumerator DelayStart()
    //{
    //    yield return new WaitForEndOfFrame();

    //    //Choose Path
    //    aiSpline = GameObject.Find("AI Path").GetComponent<SplineContainer>();
    //    var manager = aiSpline.GetComponent<AIPathManager>();
    //    int route = Random.Range(0, manager.routes.Count);
    //    aiSplinePath = manager.routes[route].path;
    //    splineLength = aiSplinePath.GetLength();
    //}

    void Update()
    {
        if (!initialized)
            return;

        // Update variable limits.
        if (coins < 0) coins = 0;
        if (weaponAmmo <= 0)
        {
            currentWeapon = ItemProjectile.WeaponType.None;
        }

        //Update Navigation Timer
        timer += Time.deltaTime;
        if (timer > 0.125f)
        {
            UpdateNav();
            timer = 0;
        }

        //Update status timers.
        if (statusTimer > 0)
        {
            statusTimer -= Time.deltaTime;
        }
        else
        {
            ClearStatuses();
        }
    }

    void UpdateNav()
    {
        //NavMesh
        if (NavMesh.CalculatePath(transform.position, liftPosition, NavMesh.AllAreas, navPath))
        {
            distanceToLift = Vector3.Distance(transform.position, navPath.corners[1]);
            for (int i = 1; i < navPath.corners.Length - 1; i++)
            {
                Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.magenta, 0.25f);
                distanceToLift += Vector3.Distance(navPath.corners[i], navPath.corners[i + 1]);
            }
        }

        ////Spline
        //await Awaitable.BackgroundThreadAsync();
        //distanceFromSpline = SplineUtility.GetNearestPoint(aiSplinePath, transform.position, out nearestSplinePoint, out nearestSplineTime, 2, 2);
        //distanceToLift = splineLength - nearestSplineTime * splineLength;
        //await Awaitable.MainThreadAsync();
    }

    void FixedUpdate()
    {
        if (!initialized)
            return;

        // Get relative velocity.
        relativeVelocity = new Vector3(Vector3.Dot(transform.right, rigid.velocity), Vector3.Dot(-transform.up, rigid.velocity), Vector3.Dot(transform.forward, rigid.velocity));
        //relVel2 = rigid.GetRelativePointVelocity(transform.TransformPoint(transform.position));

        // Slow down player when finished.
        if (finished)
        {
            rigid.AddRelativeForce(-relativeVelocity * 2, ForceMode.Acceleration);
            if (relativeVelocity.magnitude < 0.01f)
                rigid.isKinematic = true;
            return;
        }

        // Control player when not finished.

        // Slowdown from other players.
        if (!boostOn && Mathf.Abs(relativeVelocity.z) > speed / (slows + 1))
            rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
        if (boostOn && Mathf.Abs(relativeVelocity.z) > (speed + boostAddSpeed) / (slows + 1))
            rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);

        if (slows > 0)
            slowed.SetActive(true);
        else
            slowed.SetActive(false);

        // Use rockets and fans.
        if (boostOn)
        {
            rigid.AddRelativeForce(0, 0, boostForce, ForceMode.Acceleration);
            if (Mathf.Abs(relativeVelocity.z) > speed + boostAddSpeed)
                rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
        }

        // Keep player below max speeds.
        if (Mathf.Abs(relativeVelocity.x) > speed)
            rigid.AddRelativeForce(-relativeVelocity.x, 0, 0, ForceMode.Acceleration);
        if (Mathf.Abs(relativeVelocity.z) > speed && !boostOn)
            rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
        if (spotLock)
            rigid.AddRelativeForce(-relativeVelocity.x, 0, -relativeVelocity.z, ForceMode.VelocityChange);

        if (grounded)
        {
            // Slow horizontal movement.
            rigid.AddRelativeForce(traction * -relativeVelocity.x, 0, 0, ForceMode.VelocityChange);
            rigid.MoveRotation(rigid.rotation * Quaternion.Euler(0, turnFactor * turnSpeed * Time.fixedDeltaTime, 0));

            // Turn around when going backwards.
            if ((relativeVelocity.z < -1 && !playerUI.isReversed) || (relativeVelocity.z > 1 && playerUI.isReversed))
            {
                playerUI.ReverseCams();
            }

        }
        else
        {
            // Apply feather board
            if (board.boardName == "Pound of Feather")
            {
                rigid.AddRelativeForce(-Physics.gravity / 2f);
            }
        }

        rigid.MoveRotation(Quaternion.Euler(rigid.rotation.eulerAngles.x, rigid.rotation.eulerAngles.y, 0));
        if (Mathf.Abs(rigid.rotation.eulerAngles.x) > 60)
        {
            rigid.MoveRotation(Quaternion.Euler(0, rigid.rotation.eulerAngles.y, 0));
        }
    }

    //void LateUpdate()
    //{
    //    if (!initialized)
    //        return;

    //    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
    //    if (Mathf.Abs(rigid.rotation.eulerAngles.x) > 60)
    //    {
    //        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
    //    }
    //}

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Track") || other.gameObject.CompareTag("OutofBounds"))
        {
            if (!grabbing && !tricking)
            {
                animator.SetBool("Grounded", true);
                int totalTrickGain = (grabsChained * 30) + (tricksChained * 100) + (grabsCombo * 20);
                if (totalTrickGain > 0)
                {
                    coins += totalTrickGain;
                    audioMaker.Play("trickwin");
                    if (playerUI != null) StartCoroutine(playerUI.TrickComplete(totalTrickGain));
                }
            }
            if (grabbing || tricking)
            {
                StartCoroutine(Roll());
                audioMaker.Play("trickfail");
            }
            if (other.gameObject.CompareTag("OutofBounds"))
            {
                StartCoroutine(Respawn());
            }
            animator.SetInteger("TrickType", 0);

            jumping = false;
            grounded = true;
            ltdr = 0;
            lgdr = 0;
            tricksChained = 0;
            grabsChained = 0;
            grabsCombo = 0;
        }
        if (other.gameObject.CompareTag("Rock"))
        {
            StartCoroutine(Trip());
            Destroy(other.gameObject);
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("OutofBounds"))
        {
            // Speed down player.
            rigid.AddRelativeForce(-relativeVelocity, ForceMode.Acceleration);
        }

        else if (other.gameObject.CompareTag("Track"))
        {
            rigid.MoveRotation(Quaternion.RotateTowards(rigid.rotation, rigid.rotation * Quaternion.Euler(Vector3.SignedAngle(transform.up, other.contacts[0].normal, transform.right), 0, 0), 1));
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Track") || other.gameObject.CompareTag("OutofBounds"))
        {
            grounded = false;
            animator.SetBool("Grounded", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CamZone") && replayCamOn)
        {
            //if (trackManager.cameras[3].TryGetComponent<ReplayCam>(out var rep)) rep.EnterCamZone(playerNum, other.transform);
            //else replayCamOn = false;
        }
        // Checkpoint
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            Checkpoint check = other.GetComponent<Checkpoint>();
            respawnPosition = check.transform.position + check.respawnPositionOffset;
            respawnRotation = Quaternion.LookRotation(Quaternion.Euler(0, check.respawnRotation, 0) * check.transform.forward, Vector3.up);
            if (!finished && check.gameObject != lastCheckpoint)
            {
                lastCheckpoint = nextCheckpoint;
                nextCheckpoint = check.nextCheck;
                nextCheckVal = check.nextCheck.GetComponent<Checkpoint>().value;
                //if (check.type == Checkpoint.CheckpointType.FinishLine)
                //{
                //    Debug.LogFormat("Player {0} crossing finish line on lap {1}/{2}.", playerNum, currentLap, totalLaps);
                //}
                if (check.type == Checkpoint.CheckpointType.FinishLine && currentLap >= totalLaps)
                {
                    finalPlace = place;
                    trackManager.Finish(playerNum);
                    if (playerUI != null)
                    {
                        if (GameRam.gameMode == GameMode.Challenge)
                            playerUI.LapTime(currentLap);
                        playerUI.finished = true;
                    }
                    aiControls.finished = true;
                    finished = true;
                    playerUI.timerOn = false;
                }
            }
            else if (!finished && check.gameObject == lastCheckpoint)
            {
                Debug.LogWarning("Wrong direction.");
            }

        }
        // Lift
        if (other.gameObject.CompareTag("Lift"))
        {
            Lift lift = other.gameObject.GetComponent<Lift>();
            lift.Cooldown(rigid);
            lastCheckpoint = firstCheckpoint;
            nextCheckpoint = firstCheckpoint.GetComponent<Checkpoint>().nextCheck;
            if (playerUI != null && GameRam.gameMode == GameMode.Challenge)
            {
                playerUI.LapTime(currentLap);
            }
            boostOn = false;
            if (aiControls != null) aiControls.NewLap();
        }
        // Coin
        if (other.gameObject.CompareTag("Coin"))
        {
            coins += 100;
            audioMaker.Play("coinPickup");
            if (other.GetComponent<CoinSpin>().respawnTime == -1)
                Destroy(other.gameObject);
            else
                StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
        }
        // Item Box
        if (other.gameObject.CompareTag("BlueBox"))
        {
            if (coins < 100)
            {
                audioMaker.Play("error");
            }
            else
            {
                coins -= 100;
                StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
                audioMaker.Play("collectItem");
                int val = Mathf.RoundToInt(Random.value * 100);
                switch (place)
                {
                    case 1:
                        switch (val)
                        {
                            case <= 18:
                                currentItem = ItemProjectile.ItemType.Invisibility;
                                break;
                            case <= 36:
                                currentItem = ItemProjectile.ItemType.HighJump;
                                break;
                            case <= 50:
                                currentItem = ItemProjectile.ItemType.Slow;
                                break;
                            case <= 57:
                                currentItem = ItemProjectile.ItemType.TripleSlow;
                                break;
                            case <= 75:
                                currentItem = ItemProjectile.ItemType.Rock;
                                break;
                            case <= 79:
                                currentItem = ItemProjectile.ItemType.TripleRock;
                                break;
                            case <= 93:
                                currentItem = ItemProjectile.ItemType.Steal;
                                break;
                            case <= 97:
                                currentItem = ItemProjectile.ItemType.TripleSteal;
                                break;
                            case <= 100:
                                currentItem = ItemProjectile.ItemType.Rocket;
                                break;
                            default:
                                currentItem = ItemProjectile.ItemType.SuperRocket;
                                break;
                        }
                        break;
                    case 2:
                        switch (val)
                        {
                            case <= 13:
                                currentItem = ItemProjectile.ItemType.Invisibility;
                                break;
                            case <= 26:
                                currentItem = ItemProjectile.ItemType.HighJump;
                                break;
                            case <= 42:
                                currentItem = ItemProjectile.ItemType.Slow;
                                break;
                            case <= 55:
                                currentItem = ItemProjectile.ItemType.TripleSlow;
                                break;
                            case <= 68:
                                currentItem = ItemProjectile.ItemType.Rock;
                                break;
                            case <= 78:
                                currentItem = ItemProjectile.ItemType.TripleRock;
                                break;
                            case <= 91:
                                currentItem = ItemProjectile.ItemType.Steal;
                                break;
                            case <= 95:
                                currentItem = ItemProjectile.ItemType.TripleSteal;
                                break;
                            case <= 98:
                                currentItem = ItemProjectile.ItemType.Rocket;
                                break;
                            default:
                                currentItem = ItemProjectile.ItemType.SuperRocket;
                                break;
                        }
                        break;
                    case 3:
                        switch (val)
                        {
                            case <= 5:
                                currentItem = ItemProjectile.ItemType.Invisibility;
                                break;
                            case <= 10:
                                currentItem = ItemProjectile.ItemType.HighJump;
                                break;
                            case <= 15:
                                currentItem = ItemProjectile.ItemType.Slow;
                                break;
                            case <= 20:
                                currentItem = ItemProjectile.ItemType.TripleSlow;
                                break;
                            case <= 25:
                                currentItem = ItemProjectile.ItemType.Rock;
                                break;
                            case <= 30:
                                currentItem = ItemProjectile.ItemType.TripleRock;
                                break;
                            case <= 44:
                                currentItem = ItemProjectile.ItemType.Steal;
                                break;
                            case <= 67:
                                currentItem = ItemProjectile.ItemType.TripleSteal;
                                break;
                            case <= 90:
                                currentItem = ItemProjectile.ItemType.Rocket;
                                break;
                            default:
                                currentItem = ItemProjectile.ItemType.SuperRocket;
                                break;
                        }
                        break;
                    case 4:
                        switch (val)
                        {
                            case <= 4:
                                currentItem = ItemProjectile.ItemType.Invisibility;
                                break;
                            case <= 8:
                                currentItem = ItemProjectile.ItemType.HighJump;
                                break;
                            case <= 15:
                                currentItem = ItemProjectile.ItemType.Slow;
                                break;
                            case <= 26:
                                currentItem = ItemProjectile.ItemType.TripleSlow;
                                break;
                            case <= 30:
                                currentItem = ItemProjectile.ItemType.Rock;
                                break;
                            case <= 48:
                                currentItem = ItemProjectile.ItemType.TripleRock;
                                break;
                            case <= 55:
                                currentItem = ItemProjectile.ItemType.Steal;
                                break;
                            case <= 69:
                                currentItem = ItemProjectile.ItemType.TripleSteal;
                                break;
                            case <= 82:
                                currentItem = ItemProjectile.ItemType.Rocket;
                                break;
                            default:
                                currentItem = ItemProjectile.ItemType.SuperRocket;
                                break;
                        }
                        break;
                }
            }
        }
        // Ammo Box
        if (other.gameObject.CompareTag("RedBox"))
        {
            if (coins < 100)
            {
                audioMaker.Play("error");
            }
            else
            {
                coins -= 100;
                StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
                audioMaker.Play("collectItem");
                int val = Mathf.RoundToInt(Random.value * 100);
                switch (place)
                {
                    case 1:
                        switch (val)
                        {
                            case <= 17:
                                currentWeapon = ItemProjectile.WeaponType.Ice;
                                break;
                            case <= 23:
                                currentWeapon = ItemProjectile.WeaponType.Parachute;
                                break;
                            case <= 51:
                                currentWeapon = ItemProjectile.WeaponType.Bomb;
                                break;
                            case <= 79:
                                currentWeapon = ItemProjectile.WeaponType.Snowman;
                                break;
                            case <= 85:
                                currentWeapon = ItemProjectile.WeaponType.Tornado;
                                break;
                            default:
                                currentWeapon = ItemProjectile.WeaponType.Slapstick;
                                break;
                        }
                        break;
                    case 2:
                        switch (val)
                        {
                            case <= 17:
                                currentWeapon = ItemProjectile.WeaponType.Ice;
                                break;
                            case <= 25:
                                currentWeapon = ItemProjectile.WeaponType.Parachute;
                                break;
                            case <= 50:
                                currentWeapon = ItemProjectile.WeaponType.Bomb;
                                break;
                            case <= 75:
                                currentWeapon = ItemProjectile.WeaponType.Snowman;
                                break;
                            case <= 83:
                                currentWeapon = ItemProjectile.WeaponType.Tornado;
                                break;
                            default:
                                currentWeapon = ItemProjectile.WeaponType.Slapstick;
                                break;
                        }
                        break;
                    case 3:
                        switch (val)
                        {
                            case <= 17:
                                currentWeapon = ItemProjectile.WeaponType.Ice;
                                break;
                            case <= 42:
                                currentWeapon = ItemProjectile.WeaponType.Parachute;
                                break;
                            case <= 50:
                                currentWeapon = ItemProjectile.WeaponType.Bomb;
                                break;
                            case <= 58:
                                currentWeapon = ItemProjectile.WeaponType.Snowman;
                                break;
                            case <= 83:
                                currentWeapon = ItemProjectile.WeaponType.Tornado;
                                break;
                            default:
                                currentWeapon = ItemProjectile.WeaponType.Slapstick;
                                break;
                        }
                        break;
                    case 4:
                        switch (val)
                        {
                            case <= 17:
                                currentWeapon = ItemProjectile.WeaponType.Ice;
                                break;
                            case <= 45:
                                currentWeapon = ItemProjectile.WeaponType.Parachute;
                                break;
                            case <= 50:
                                currentWeapon = ItemProjectile.WeaponType.Bomb;
                                break;
                            case <= 55:
                                currentWeapon = ItemProjectile.WeaponType.Snowman;
                                break;
                            case <= 83:
                                currentWeapon = ItemProjectile.WeaponType.Tornado;
                                break;
                            default:
                                currentWeapon = ItemProjectile.WeaponType.Slapstick;
                                break;
                        }
                        break;
                }
                weaponAmmo = 3;
            }
        }
        // Get hit by weapons.
        if (!invisible && other.gameObject.CompareTag("Projectile"))
        {
            var projectile = other.transform.parent.GetComponent<ItemProjectile>();
            if (projectile.parentPlayer != gameObject)
            {
                if ((grabbing || tricking) && !projectile.reflected)
                {
                    GameObject clone = Instantiate(this.projectile, shootSpawn.transform.position, shootSpawn.transform.rotation);
                    clone.transform.Rotate(0, 180, 0);
                    clone.GetComponent<ItemProjectile>().weaponType = projectile.weaponType;
                    clone.GetComponent<ItemProjectile>().parentPlayer = gameObject;
                    clone.GetComponent<ItemProjectile>().reflected = true;
                    audioMaker.Play("reflectAttack");
                }
                else
                {
                    switch (projectile.weaponType)
                    {
                        case ItemProjectile.WeaponType.Ice:
                            Ice();
                            break;
                        case ItemProjectile.WeaponType.Parachute:
                            StartCoroutine(Parachute());
                            break;
                        case ItemProjectile.WeaponType.Bomb:
                            StartCoroutine(Bomb());
                            break;
                        case ItemProjectile.WeaponType.Snowman:
                            Snowman();
                            break;
                        case ItemProjectile.WeaponType.Tornado:
                            StartCoroutine(Tornado());
                            break;
                        case ItemProjectile.WeaponType.Slapstick:
                            Slapstick();
                            break;
                    }
                }
                Destroy(projectile.gameObject);
            }
        }
    }

    // Start race.
    public void SetGo()
    {
        if (playerUI != null)
            playerUI.timerOn = true;
        if (playerRaceControls != null)
            playerRaceControls.lockControls = false;
        spotLock = false;
        if (board == null)
        {
            Debug.LogError("Board not found. Constant boards will not work this race.");
            return;
        }
        if (board.boardName == "Poverty Board")
            StartCoroutine(Poverty());
        if (board.boardName == "Wealth Board")
            StartCoroutine(Wealth());
    }

    // Physics Functions.
    public void Jump()
    {
        if (!jumping)
        {
            animator.SetBool("Crouching", false);
            animator.SetBool("Grounded", false);
            jumping = true;
            if (!highJumpReady)
            {
                rigid.AddRelativeForce(jumpForce, ForceMode.Acceleration);
            }
            else
            {
                rigid.AddRelativeForce(Vector3.Scale(jumpForce, highJumpForce), ForceMode.Acceleration);
                highJumpParticles.SetActive(false);
                highJumpReady = false;
            }
        }
    }

    public IEnumerator Trip()
    {
        animator.SetTrigger("Trip");
        boostOn = false;
        invisible = false;
        characterModel.SetActive(true);
        rigid.velocity = Vector3.zero;
        turnFactor = 0;
        playerRaceControls.lockControls = true;
        spotLock = true;
        lockParticles.SetActive(true);
        yield return new WaitForSeconds(standTime);
        lockParticles.SetActive(false);
        playerRaceControls.lockControls = false;
        spotLock = false;
    }

    public IEnumerator Roll()
    {
        animator.SetBool("Rolling", true);
        boostOn = false;
        invisible = false;
        characterModel.SetActive(true);
        turnFactor = 0;
        playerRaceControls.lockControls = true;
        lockParticles.SetActive(true);
        yield return new WaitForSeconds(rollTime);
        animator.SetBool("Rolling", false);
        animator.SetTrigger("Trip");
        spotLock = true;
        yield return new WaitForSeconds(standTime);
        playerRaceControls.lockControls = false;
        lockParticles.SetActive(false);
        spotLock = false;
    }

    public IEnumerator Respawn()
    {
        if (!respawning)
        {
            animator.SetBool("OOB", true);
            grounded = true;
            respawning = true;
            playerRaceControls.lockControls = true;
            yield return new WaitForSeconds(3);
            transform.SetPositionAndRotation(respawnPosition, respawnRotation);
            rigid.velocity = Vector3.zero;
            playerRaceControls.lockControls = false;
            respawning = false;
            animator.SetBool("OOB", false);
            animator.SetBool("Grounded", true);
        }
    }

    // Do tricks and board grabs.
    public IEnumerator BoardGrab(int dir, int lastDir)
    {
        grabbing = true;
        switch (dir)
        {
            case 1:
                // print ("Front Grab");
                animator.SetInteger("GrabType", 1);
                break;
            case 3:
                // print ("Right Grab");
                animator.SetInteger("GrabType", 3);
                break;
            case 5:
                // print ("Back Grab");
                animator.SetInteger("GrabType", 5);
                break;
            case 7:
                // print ("Left Grab");
                animator.SetInteger("GrabType", 7);
                break;
        }
        yield return new WaitForSeconds(0.4f);
        if (!grounded)
        {
            grabsChained++;
            if (lastDir != dir && lastDir != 0) grabsCombo++;
            lgdr = dir;
        }
        grabbing = false;
        animator.SetInteger("GrabType", 0);
    }

    public IEnumerator Trick(int dir, int lastDir)
    {
        tricking = true;
        audioMaker.Play("trickstart");
        switch (dir)
        {
            case 1:
                // print ("Frontflip");
                animator.SetInteger("TrickType", 1);
                break;
            case 2:
                // print ("Right Aerial");
                animator.SetInteger("TrickType", 2);
                break;
            case 3:
                // print ("Right 360");
                animator.SetInteger("TrickType", 3);
                break;
            case 4:
                // print ("Right Backaerial");
                animator.SetInteger("TrickType", 4);
                break;
            case 5:
                // print ("Backflip");
                animator.SetInteger("TrickType", 5);
                break;
            case 6:
                // print ("Left Backaerial");
                animator.SetInteger("TrickType", 6);
                break;
            case 7:
                // print ("Left 360");
                animator.SetInteger("TrickType", 7);
                break;
            case 8:
                // print ("Left Aerial");
                animator.SetInteger("TrickType", 8);
                break;
        }
        animator.SetTrigger("Trick");

        yield return new WaitForSeconds(.5f);
        if (!grounded)
        {
            tricksChained++;
            if (lastDir != dir && lastDir != 0) tricksChained++;
            ltdr = dir;
        }
        animator.SetInteger("TrickType", 0);
        playerRaceControls.comboAble = true;
        yield return new WaitForSeconds(.25f);
        playerRaceControls.comboAble = false;
        tricking = false;
    }

    // Use weapons and items.
    public void Shoot()
    {
        if (weaponAmmo > 0)
        {
            GameObject clone = Instantiate(projectile, shootSpawn.transform.position, shootSpawn.transform.rotation);
            clone.GetComponent<ItemProjectile>().weaponType = currentWeapon;
            clone.GetComponent<ItemProjectile>().parentPlayer = gameObject;
            weaponAmmo--;
        }
    }

    public void Item()
    {
        switch (currentItem)
        {
            case ItemProjectile.ItemType.None:
                break;
            case ItemProjectile.ItemType.Invisibility:
                StartCoroutine(UseInvisibility());
                break;
            case ItemProjectile.ItemType.HighJump:
                UseHighJump();
                break;
            case ItemProjectile.ItemType.Slow:
                trackManager.UseSlow(playerNum);
                break;
            case ItemProjectile.ItemType.TripleSlow:
                trackManager.UseTripleSlow(playerNum);
                break;
            case ItemProjectile.ItemType.Rock:
                Instantiate(rock, rockSpawn.transform.position, rockSpawn.transform.rotation);
                break;
            case ItemProjectile.ItemType.TripleRock:
                trackManager.UseTripleStop(playerNum);
                break;
            case ItemProjectile.ItemType.Steal:
                trackManager.UseSteal(playerNum);
                break;
            case ItemProjectile.ItemType.TripleSteal:
                trackManager.UseTripleSteal(playerNum);
                break;
            case ItemProjectile.ItemType.Rocket:
                StartCoroutine(UseBoost(rocket1));
                break;
            case ItemProjectile.ItemType.SuperRocket:
                StartCoroutine(UseBoost(rocket2));
                break;
        }
        currentItem = ItemProjectile.ItemType.None;
    }

    // Get affected by Red Weapons.
    void Ice()
    {
        if (status == DefenseStatus.Ice)
            return;
        iceCube.SetActive(true);
        boostOn = false;
        turnFactor = 0;
        playerRaceControls.lockControls = true;
        rigid.velocity = Vector3.zero;
        spotLock = true;
        statusTimer = statusTime;
        StartCoroutine(Trip());
    }

    void Snowman()
    {
        if (status == DefenseStatus.Snow)
            return;
        snowman.SetActive(true);
        turnFactor = 0;
        playerRaceControls.lockControls = true;
        statusTimer = statusTime;
    }

    IEnumerator Tornado()
    {
        if (!finished)
        {
            boostOn = false;
            turnFactor = 0;
            playerRaceControls.lockControls = true;
            rigid.velocity = Vector3.zero;
            rigid.AddRelativeForce(0, jumpForce.y * 2, 0, ForceMode.Acceleration);
            yield return new WaitForSeconds(.25f);
            while (!grounded)
            {
                yield return null;
            }
            spotLock = true;
            rigid.velocity = Vector3.zero;
            yield return new WaitForSeconds(2);
            playerRaceControls.lockControls = false;
            spotLock = false;
        }
    }

    IEnumerator Parachute()
    {
        if (!finished)
        {
            boostOn = false;
            turnFactor = 0;
            playerRaceControls.lockControls = true;
            balloon.SetActive(true);
            rigid.velocity = Vector3.zero;
            rigid.AddRelativeForce(0, jumpForce.y * 2, 0, ForceMode.Acceleration);
            yield return new WaitForSeconds(.25f);
            rigid.useGravity = false;
            while (!grounded)
            {
                rigid.AddForce(0, -2, 0, ForceMode.Acceleration);
                yield return null;
            }
            rigid.useGravity = true;
            rigid.velocity = Vector3.zero;
            yield return new WaitForSeconds(2);
            balloon.SetActive(false);
            playerRaceControls.lockControls = false;
        }
    }
    IEnumerator Bomb()
    {
        status = DefenseStatus.Rolling;
        boostOn = false;
        turnFactor = 0;
        playerRaceControls.lockControls = true;
        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(jumpForce.y, transform.position, 3, 3, ForceMode.Acceleration);
        while (!grounded)
        {
            yield return null;
        }
        playerRaceControls.lockControls = false;
        StartCoroutine(Roll());
    }

    void Slapstick()
    {
        status = DefenseStatus.Rolling;
        StartCoroutine(Roll());
        Vector3 midcoin = new(0, 0.5f, -1.5f);
        Vector3 leftcoin = new(-.75f, 0.5f, -0.5f);
        Vector3 rightcoin = new(.75f, 0.5f, -0.5f);
        if (coins >= 100) Instantiate(dropCoin, transform.TransformPoint(midcoin), transform.rotation);
        if (coins >= 200) Instantiate(dropCoin, transform.TransformPoint(leftcoin), transform.rotation);
        if (coins >= 300) Instantiate(dropCoin, transform.TransformPoint(rightcoin), transform.rotation);
        coins -= 300;
    }
    void ClearStatuses(bool tripAfter = false)
    {
        status = DefenseStatus.Normal;
        playerRaceControls.lockControls = false;
        spotLock = false;
        snowman.SetActive(false);
        iceCube.SetActive(false);
        if (tripAfter)
        {
            StartCoroutine(Trip());
        }
    }

    // Get affected by Blue Items.
    IEnumerator UseBoost(Vector3 stat)
    {
        if (!boostOn)
        {
            boostOn = true;
            boostForce = stat.y;
            boostAddSpeed = stat.z;
            if (stat.x == 7)
            {
                rocketParticles1.SetActive(true);
            }
            if (stat.x == 5)
            {
                rocketParticles2.SetActive(true);
                rocketParticles1.SetActive(true);
            }
            float delay = 0;
            while (delay < stat.x && boostOn)
            {
                yield return null;
                delay += Time.deltaTime;
            }
            boostOn = false;
            if (stat.x == 7)
            {
                rocketParticles1.SetActive(false);
            }
            if (stat.x == 5)
            {
                rocketParticles2.SetActive(false);
                rocketParticles1.SetActive(false);
            }
        }
    }

    IEnumerator UseInvisibility()
    {
        if (!invisible)
        {
            characterModel.SetActive(false);
            invisible = true;
            yield return new WaitForSeconds(statusTime);
            characterModel.SetActive(true);
            invisible = false;
        }
    }

    public IEnumerator GetSlowed()
    {
        if (slows >= 0) slowed.SetActive(true);
        slows++;
        yield return new WaitForSeconds(statusTime);
        slows--;
        if (slows <= 0) slowed.SetActive(false);
    }

    void UseHighJump()
    {
        if (!highJumpReady)
        {
            highJumpReady = true;
            highJumpParticles.SetActive(true);
        }
    }

    IEnumerator Poverty()
    {
        yield return new WaitForSeconds(moneyBoardTime);
        if (coins > 0) coins--;
        StartCoroutine(Poverty());
    }

    IEnumerator Wealth()
    {
        yield return new WaitForSeconds(moneyBoardTime);
        coins++;
        StartCoroutine(Wealth());
    }

    //private void OnDrawGizmosSelected()
    //{
    //    rigid = GetComponent<Rigidbody>();
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(rigid.centerOfMass, 0.25f);
    //}
}
