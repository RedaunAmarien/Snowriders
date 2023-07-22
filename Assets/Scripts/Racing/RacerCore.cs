using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class RacerCore : MonoBehaviour
{

    public int playerNum;

    [Header("Character Settings")]
    public Character character;
    public Board board;
    //public string charName;
    //public string boardName;
    public float speed, traction, turnSpeed;
    public Vector3 jumpForce;

    [Header("Race Stats")]
    public bool finished;
    public int place, currentLap;
    public float nextCheckVal;
    public float checkDist;
    public int finalPlace, totalLaps;
    public int coins;
    public GameObject firstCheckpoint, lastCheckpoint, nextCheckpoint;

    [Header("Items and Weapons")]
    public int weaponType;
    public int shots, blankWeap, itemType, blankItem, statusTime, rollTime, standTime;
    [Tooltip("X = Lifetime, Y = Acceleration, Z = Additional Max Speed")]
    public Vector3 rocket1, rocket2;
    public Vector3 highJumpForce;
    public GameObject rock, rockSpawn, projectile, shootSpawn, dropCoin, characterModel, iceCube, snowman, balloon, highJumpParticles, lockParticles, slowed, rocketParticles1, rocketParticles2;
    public SpriteRenderer headSprite;
    public Sprite[] headSpriteSrc;
    public float moneyBoardTime;

    [Header("Physics")]
    public bool grounded;
    public bool boostOn;
    public float boostForce, boostAddSpeed;
    public bool invisible, spotLock, grabbing, tricking, jumping, highJumpReady, respawning;
    public int ltdr, lgdr, grabsChained, grabsCombo, tricksChained, slows;

    // Objects
    PlayerRaceControls playerRaceControls;
    public TrackManager trackManager;
    public CourseSettings courseSettings;
    public PlayerUI playerUI;
    public AudioMaker audioMaker;
    public AudioSource audioSource;
    public AIControls aiControls;
    public Animator animator;
    public Transform playerStartPoint;
    public Rigidbody rigid;
    public GameObject playerCamera;
    public Vector3 relativeVelocity;
    bool initialized, replayCamOn;

    void Start()
    {
        // Initialize Objects and Stats
        trackManager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        courseSettings = GameObject.Find("CourseSettings").GetComponent<CourseSettings>();
        rigid = gameObject.GetComponent<Rigidbody>();

        playerRaceControls = gameObject.GetComponent<PlayerRaceControls>();
        playerUI = GetComponent<PlayerUI>();
        aiControls = GetComponent<AIControls>();
        audioMaker = GetComponentInChildren<AudioMaker>();
        audioSource = GetComponentInChildren<AudioSource>();
        if (playerNum < GameRam.playerCount) audioSource.spatialBlend = 0;
        // course = GameObject.FindGameObjectWithTag("CourseController");
        // corCon = course.GetComponent<CourseControl>();
        jumpForce.z = speed * 10;
        // firstCheckpoint = GameObject.Find("Start");
        lastCheckpoint = firstCheckpoint;
        nextCheckpoint = firstCheckpoint.GetComponent<Checkpoint>().nextCheck;
        nextCheckVal = nextCheckpoint.GetComponent<Checkpoint>().value;

        currentLap = 1;

        rigid = gameObject.GetComponent<Rigidbody>();
        finished = false;
        rigid.maxAngularVelocity = 0.05f;

        // Become character.
        if (trackManager.demoMode)
        {
            headSprite.sprite = character.charSprite;
        }
        else if (GameRam.charForP[playerNum] < GameRam.defaultCharacters.Count)
        {
            headSprite.sprite = character.charSprite;
        }
        else
        {
            headSprite.sprite = headSpriteSrc[^1];
        }
        if (playerNum < GameRam.playerCount) headSprite.gameObject.SetActive(false);

        // Initialize items.
        blankWeap = playerUI.weaponSprite.Length - 1;
        weaponType = blankWeap;
        blankItem = playerUI.itemSprite.Length - 1;
        itemType = blankItem;
        shots = 0;
        coins = 0;

        initialized = true;
        replayCamOn = true;
    }

    public void Initialize(bool demoMode = false)
    {
        //Speed: Max 18, Min 15.
        speed = demoMode ? 15f : Mathf.LerpUnclamped(15, 18, (character.speed + board.speed) / 10f);

        //Traction: Max .04, Min .015.
        traction = demoMode ? .015f : Mathf.LerpUnclamped(0.015f, 0.04f, (character.turn + board.turn) / 10f);

        //Jump: Max 250, Min 175.
        jumpForce.y = demoMode ? 175f : Mathf.LerpUnclamped(175, 250, (character.jump + board.jump) / 10f);

        //Display stats
        //charName = character.characterName;
        //boardName = demoMode ? "Demo Board" : board.name;
        totalLaps = GameRam.lapCount;

        AssignSpecialBoards();
    }

    public void AssignSpecialBoards()
    {
        if (board.boardName == "Pound of Feather")
        {
            jumpForce.y = 300;
            jumpForce.z = 20;
        }
        if (board.boardName == "Block of Soap")
        {
            traction = 0;
        }
    }

    void Update()
    {
        if (initialized)
        {
            // Update Checkpoints
            checkDist = (transform.position - lastCheckpoint.transform.position).sqrMagnitude - (transform.position - nextCheckpoint.transform.position).sqrMagnitude;
            // if (finished) checkDist += 1000000;
            Debug.DrawLine(transform.position, lastCheckpoint.transform.position, Color.gray, .25f);
            Debug.DrawLine(transform.position, nextCheckpoint.transform.position, Color.green, .25f);
            //Debug.DrawLine(lastCheckpoint.transform.position, nextCheckpoint.transform.position, Color.red, .1f);

            // Update variable limits.
            if (coins < 0) coins = 0;
            if (shots <= 0)
            {
                weaponType = 6;
            }
        }
    }

    void FixedUpdate()
    {
        if (initialized)
        {
            // Get relative velocity.
            relativeVelocity = new Vector3(Vector3.Dot(transform.right, rigid.velocity), Vector3.Dot(-transform.up, rigid.velocity), Vector3.Dot(transform.forward, rigid.velocity));

            // Slow down player when finished.
            if (finished)
            {
                rigid.AddRelativeForce(-relativeVelocity * 2, ForceMode.Acceleration);
            }

            // Control player when not finished.
            else
            {
                // Slowdown from other players.
                if (!boostOn && relativeVelocity.z > speed / (slows + 1)) rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
                if (boostOn && relativeVelocity.z > (speed + boostAddSpeed) / (slows + 1)) rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
                if (slows > 0) slowed.SetActive(true);
                else slowed.SetActive(false);

                // Use rockets and fans.
                if (boostOn)
                {
                    rigid.AddRelativeForce(0, 0, boostForce, ForceMode.Acceleration);
                    if (relativeVelocity.z > speed + boostAddSpeed) rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
                }

                // Keep player below max speeds.
                if (relativeVelocity.x > Mathf.Abs(speed)) rigid.AddRelativeForce(-relativeVelocity.x, 0, 0, ForceMode.Acceleration);
                if (relativeVelocity.z > speed && !boostOn) rigid.AddRelativeForce(0, 0, -relativeVelocity.z, ForceMode.Acceleration);
                if (spotLock) rigid.AddRelativeForce(-relativeVelocity.x, 0, -relativeVelocity.z, ForceMode.VelocityChange);

                if (grounded)
                {
                    // Slow horizontal movement.
                    rigid.AddRelativeForce(traction * -relativeVelocity.x, 0, 0, ForceMode.VelocityChange);

                    // Turn board around when going backwards.
                    if (relativeVelocity.z < -1) transform.Rotate(0, 180, 0);
                }
                else
                {
                    // Apply feather board
                    if (board.boardName == "Pound of Feather")
                    {
                        rigid.AddRelativeForce(-Physics.gravity / 2f);
                    }
                }
            }
        }
    }

    void LateUpdate()
    {
        if (initialized)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
            if (Mathf.Abs(transform.localEulerAngles.x) > 60)
            {
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
        }
    }

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

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Track") || other.gameObject.CompareTag("OutofBounds"))
        {
            grounded = false;
            animator.SetBool("Grounded", false);
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("OutofBounds"))
        {
            // Speed down player.
            rigid.AddRelativeForce(-relativeVelocity, ForceMode.Acceleration);
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
            Checkpoint chPnt = other.GetComponent<Checkpoint>();
            if (!finished && chPnt.gameObject != lastCheckpoint)
            {
                lastCheckpoint = nextCheckpoint;
                nextCheckpoint = chPnt.nextCheck;
                nextCheckVal = chPnt.nextCheck.GetComponent<Checkpoint>().value;
                if (chPnt.isLift)
                {
                    lastCheckpoint = firstCheckpoint;
                    nextCheckpoint = firstCheckpoint.GetComponent<Checkpoint>().nextCheck;
                    if (playerUI != null && GameRam.gameMode == GameMode.Challenge)
                    {
                        playerUI.LapTime(currentLap);
                    }
                    currentLap++;
                    boostOn = false;
                    rigid.velocity = Vector3.zero;
                    transform.SetPositionAndRotation(playerStartPoint.position, playerStartPoint.rotation);
                    rigid.AddRelativeForce(new Vector3(0, 0, jumpForce.z * 2));
                    if (aiControls != null) aiControls.NewLap();
                }
                if (chPnt.isFinish && currentLap >= totalLaps)
                {
                    finalPlace = place;
                    // corCon.Finish(playerNum);
                    trackManager.Finish(playerNum);
                    if (playerUI != null)
                    {
                        if (GameRam.gameMode == GameMode.Challenge) playerUI.LapTime(currentLap);
                        playerUI.finished = true;
                    }
                    aiControls.finished = true;
                    finished = true;
                    playerUI.timerOn = false;
                }
            }
            else if (!finished && chPnt.gameObject == lastCheckpoint)
            {
                Debug.LogWarning("Wrong direction.");
            }
        }
        // Coin
        if (other.gameObject.CompareTag("Coin"))
        {
            coins += 100;
            audioMaker.Play("coinPickup");
            if (other.GetComponent<CoinSpin>().respawnTime == -1) Destroy(other.gameObject);
            else StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
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
                switch (place)
                {
                    case 1:
                        itemType = WeightedRandom.Range(
                            new IntRange(0, 0, 5)/*Invis*/,
                            new IntRange(1, 1, 5)/*HighJump*/,
                            new IntRange(2, 2, 4)/*SlowOne*/,
                            new IntRange(3, 3, 2)/*SlowThree*/,
                            new IntRange(4, 4, 5)/*Rock*/,
                            new IntRange(5, 5, 1)/*Pan*/,
                            new IntRange(6, 6, 4)/*Steal*/,
                            new IntRange(7, 7, 1)/*StealThree*/,
                            new IntRange(8, 8, 1)/*Rocket*/,
                            new IntRange(9, 9, 0)/*SuperRocket*/);
                        break;
                    case 2:
                        itemType = WeightedRandom.Range(
                            new IntRange(0, 0, 4)/*Invis*/,
                            new IntRange(1, 1, 4)/*HighJump*/,
                            new IntRange(2, 2, 5)/*SlowOne*/,
                            new IntRange(3, 3, 4)/*SlowThree*/,
                            new IntRange(4, 4, 4)/*Rock*/,
                            new IntRange(5, 5, 3)/*Pan*/,
                            new IntRange(6, 6, 4)/*Steal*/,
                            new IntRange(7, 7, 1)/*StealThree*/,
                            new IntRange(8, 8, 1)/*Rocket*/,
                            new IntRange(9, 9, .5f)/*SuperRocket*/);
                        break;
                    case 3:
                        itemType = WeightedRandom.Range(
                            new IntRange(0, 0, 1)/*Invis*/,
                            new IntRange(1, 1, 1)/*HighJump*/,
                            new IntRange(2, 2, 1)/*SlowOne*/,
                            new IntRange(3, 3, 1)/*SlowThree*/,
                            new IntRange(4, 4, 1)/*Rock*/,
                            new IntRange(5, 5, 1)/*Pan*/,
                            new IntRange(6, 6, 3)/*Steal*/,
                            new IntRange(7, 7, 5)/*StealThree*/,
                            new IntRange(8, 8, 5)/*Rocket*/,
                            new IntRange(9, 9, 3)/*SuperRocket*/);
                        break;
                    case 4:
                        itemType = WeightedRandom.Range(
                            new IntRange(0, 0, 1)/*Invis*/,
                            new IntRange(1, 1, 1)/*HighJump*/,
                            new IntRange(2, 2, 2)/*SlowOne*/,
                            new IntRange(3, 3, 3)/*SlowThree*/,
                            new IntRange(4, 4, 1)/*Rock*/,
                            new IntRange(5, 5, 5)/*Pan*/,
                            new IntRange(6, 6, 2)/*Steal*/,
                            new IntRange(7, 7, 4)/*StealThree*/,
                            new IntRange(8, 8, 4)/*Rocket*/,
                            new IntRange(9, 9, 5)/*SuperRocket*/);
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
                switch (place)
                {
                    case 1:
                        weaponType = WeightedRandom.Range(
                            new IntRange(0, 0, 3)/*Ice*/,
                            new IntRange(1, 1, 1)/*Balloon*/,
                            new IntRange(2, 2, 5)/*Bomb*/,
                            new IntRange(3, 3, 5)/*Snow*/,
                            new IntRange(4, 4, 1)/*Tornado*/,
                            new IntRange(5, 5, 3)/*Slap*/);
                        break;
                    case 2:
                        weaponType = WeightedRandom.Range(
                            new IntRange(0, 0, 2)/*Ice*/,
                            new IntRange(1, 1, 1)/*Balloon*/,
                            new IntRange(2, 2, 3)/*Bomb*/,
                            new IntRange(3, 3, 3)/*Snow*/,
                            new IntRange(4, 4, 1)/*Tornado*/,
                            new IntRange(5, 5, 2)/*Slap*/);
                        break;
                    case 3:
                        weaponType = WeightedRandom.Range(
                            new IntRange(0, 0, 2)/*Ice*/,
                            new IntRange(1, 1, 3)/*Balloon*/,
                            new IntRange(2, 2, 1)/*Bomb*/,
                            new IntRange(3, 3, 1)/*Snow*/,
                            new IntRange(4, 4, 3)/*Tornado*/,
                            new IntRange(5, 5, 2)/*Slap*/);
                        break;
                    case 4:
                        weaponType = WeightedRandom.Range(
                            new IntRange(0, 0, 3)/*Ice*/,
                            new IntRange(1, 1, 5)/*Balloon*/,
                            new IntRange(2, 2, 1)/*Bomb*/,
                            new IntRange(3, 3, 1)/*Snow*/,
                            new IntRange(4, 4, 5)/*Tornado*/,
                            new IntRange(5, 5, 3)/*Slap*/);
                        break;
                }
                shots = 3;
            }
        }
        // Get hit by weapons.
        if (!invisible && other.gameObject.CompareTag("Projectile"))
        {
            var itemProj = other.transform.parent.GetComponent<ItemProjectile>();
            if (itemProj.parentPlayer != gameObject)
            {
                if ((grabbing || tricking) && !itemProj.reflected)
                {
                    GameObject clone = Instantiate(projectile, shootSpawn.transform.position, shootSpawn.transform.rotation);
                    clone.transform.Rotate(0, 180, 0);
                    clone.GetComponent<ItemProjectile>().weaponType = itemProj.weaponType;
                    clone.GetComponent<ItemProjectile>().parentPlayer = gameObject;
                    clone.GetComponent<ItemProjectile>().reflected = true;
                    audioMaker.Play("reflectAttack");
                }
                else
                {
                    switch (itemProj.weaponType)
                    {
                        case ItemProjectile.WeaponType.Ice:
                            StartCoroutine(Ice());
                            break;
                        case ItemProjectile.WeaponType.Parachute:
                            StartCoroutine(Parachute());
                            break;
                        case ItemProjectile.WeaponType.Bomb:
                            StartCoroutine(Bomb());
                            break;
                        case ItemProjectile.WeaponType.Snowman:
                            StartCoroutine(Snowman());
                            break;
                        case ItemProjectile.WeaponType.Tornado:
                            StartCoroutine(Tornado());
                            break;
                        case ItemProjectile.WeaponType.Slapstick:
                            Slapstick();
                            break;
                    }
                }
                Destroy(itemProj.gameObject);
            }
        }
    }

    // Start race.
    public void SetGo()
    {
        if (playerUI != null) playerUI.timerOn = true;
        if (playerRaceControls != null) playerRaceControls.lockControls = false;
        spotLock = false;
        if (board.boardName == "Poverty Board") StartCoroutine(Poverty());
        if (board.boardName == "Wealth Board") StartCoroutine(Wealth());
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
            Transform spawnSpot = nextCheckpoint.GetComponent<Checkpoint>().respawn.transform;
            transform.SetPositionAndRotation(spawnSpot.position + new Vector3(0, 2, 0), spawnSpot.rotation);
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
        if (shots > 0)
        {
            GameObject clone = Instantiate(projectile, shootSpawn.transform.position, shootSpawn.transform.rotation);
            clone.GetComponent<ItemProjectile>().weaponType = (ItemProjectile.WeaponType)weaponType;
            clone.GetComponent<ItemProjectile>().parentPlayer = gameObject;
            shots--;
        }
    }

    public void Item()
    {
        switch (itemType)
        {
            case 0:
                // Invisibility
                StartCoroutine(UseInvisibility());
                itemType = blankItem;
                break;
            case 1:
                // High Jump
                UseHighJump();
                itemType = blankItem;
                break;
            case 2:
                // Slow
                // corCon.UseSlow(playerNum);
                trackManager.UseSlow(playerNum);
                itemType = blankItem;
                break;
            case 3:
                // Triple Slow
                // corCon.UseTripleSlow(playerNum);
                trackManager.UseTripleSlow(playerNum);
                itemType = blankItem;
                break;
            case 4:
                // Rock
                Instantiate(rock, rockSpawn.transform.position, rockSpawn.transform.rotation);
                itemType = blankItem;
                break;
            case 5:
                // Triple Rock
                // corCon.UseTripleStop(playerNum);
                trackManager.UseTripleStop(playerNum);
                itemType = blankItem;
                break;
            case 6:
                // Steal
                // corCon.UseSteal(playerNum);
                trackManager.UseSteal(playerNum);
                itemType = blankItem;
                break;
            case 7:
                // Triple Steal
                // corCon.UseTripleSteal(playerNum);
                trackManager.UseTripleSteal(playerNum);
                itemType = blankItem;
                break;
            case 8:
                // Rocket
                StartCoroutine(UseBoost(rocket1));
                itemType = blankItem;
                break;
            case 9:
                // Super Rocket
                StartCoroutine(UseBoost(rocket2));
                itemType = blankItem;
                break;
            case 10:
                // None
                break;
        }
    }

    // Get affected by Red Weapons.
    IEnumerator Ice()
    {
        iceCube.SetActive(true);
        boostOn = false;
        playerRaceControls.lockControls = true;
        rigid.velocity = Vector3.zero;
        spotLock = true;
        yield return new WaitForSeconds(statusTime);
        playerRaceControls.lockControls = false;
        spotLock = false;
        iceCube.SetActive(false);
        StartCoroutine(Trip());
    }

    IEnumerator Snowman()
    {
        snowman.SetActive(true);
        playerRaceControls.lockControls = true;
        yield return new WaitForSeconds(statusTime);
        playerRaceControls.lockControls = false;
        snowman.SetActive(false);
    }

    IEnumerator Tornado()
    {
        if (!finished)
        {
            boostOn = false;
            playerRaceControls.lockControls = true;
            rigid.velocity = Vector3.zero;
            rigid.AddRelativeForce(0, jumpForce.y * 2, 0);
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
            playerRaceControls.lockControls = true;
            balloon.SetActive(true);
            rigid.velocity = Vector3.zero;
            rigid.AddRelativeForce(0, jumpForce.y * 2, 0);
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
        boostOn = false;
        playerRaceControls.lockControls = true;
        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(jumpForce.y, transform.forward, 3);
        while (!grounded)
        {
            yield return null;
        }
        playerRaceControls.lockControls = false;
        StartCoroutine(Roll());
    }

    void Slapstick()
    {
        StartCoroutine(Roll());
        Vector3 midcoin = new(0, 0.5f, -1.5f);
        Vector3 leftcoin = new(-.75f, 0.5f, -0.5f);
        Vector3 rightcoin = new(.75f, 0.5f, -0.5f);
        if (coins >= 100) Instantiate(dropCoin, transform.TransformPoint(midcoin), transform.rotation);
        if (coins >= 200) Instantiate(dropCoin, transform.TransformPoint(leftcoin), transform.rotation);
        if (coins >= 300) Instantiate(dropCoin, transform.TransformPoint(rightcoin), transform.rotation);
        coins -= 300;
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
            yield return new WaitForSeconds(stat.x);
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
        if (slows == 0) slowed.SetActive(true);
        slows++;
        yield return new WaitForSeconds(statusTime);
        slows--;
        if (slows == 0) slowed.SetActive(false);
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
}
