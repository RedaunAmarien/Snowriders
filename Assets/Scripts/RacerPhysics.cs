using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class RacerPhysics : MonoBehaviour {

    public int playerNum;

    [Header("Character Settings")]
	public string charName;
	public string boardName;
    public float speed, traction, turnSpeed;
    public Vector3 jumpForce;

    [Header("Race Stats")]
    public bool finished;
	public int place, currentLap;
    public int nextCheckVal;
	public float checkDist;
    public int finalPlace, totalLaps;
    public int coins;
	public GameObject firstCheckpoint, lastCheckpoint, nextCheckpoint;

    [Header("Items and Weapons")]
    public int weapType;
	public int shots, blankWeap, itemType, blankItem, statusTime, rollTime, standTime;
	[Tooltip("X = Lifetime, Y = Acceleration, Z = Additional Max Speed")]
    public Vector3 rocket1, rocket2;
	public Vector3 highJumpForce;
    public GameObject rock, rockSpawn, projectile, shootSpawn, dropCoin, characterModel, iceCube, snowman, balloon, highJumpParticles, lockParticles, slowed, rocketParticles1, rocketParticles2;
	public AnimationCurve blueWeights1, blueWeights2, blueWeights3, blueWeights4, redWeights1, redWeights2, redWeights3, redWeights4;
	public SpriteRenderer headSprite;
	public Sprite[] headSpriteSrc;

    [Header("Physics")]
    public bool grounded;
	public bool boostOn;
    public float boostForce, boostAddSpeed;
    public bool invisible, spotLock, grabbing, tricking, jumping, highJumpReady, respawning;
    public int ltdr, lgdr, grabsChained, grabsCombo, tricksChained, slows;

    // Objects
    PlayerRaceControls pCon;
    CourseControl corCon;
    PlayerUI pUI;
    AudioMaker aUD;
	AudioSource audioSrc;
    AIControls aI;
    Transform playerStartPoint;
    public Rigidbody rigid;
    GameObject course;
    public Vector3 relVel;

    void Start() {
        // Initialize Objects and Stats
        rigid = gameObject.GetComponent<Rigidbody>();
        pCon = gameObject.GetComponent<PlayerRaceControls>();
        playerStartPoint = GameObject.Find("Players").transform;
		pUI = GetComponent<PlayerUI>();
		aI = GetComponent<AIControls>();
		aUD = GetComponentInChildren<AudioMaker>();
		audioSrc = GetComponentInChildren<AudioSource>();
		if (playerNum < GameVar.playerCount) audioSrc.spatialBlend = 0;
		course = GameObject.FindGameObjectWithTag("CourseController");
        corCon = course.GetComponent<CourseControl>();
        jumpForce.z = speed*10;
        firstCheckpoint = GameObject.Find("Start");
		lastCheckpoint = firstCheckpoint;
        nextCheckpoint = firstCheckpoint.GetComponent<Checkpoint>().nextCheck;
		nextCheckVal = nextCheckpoint.GetComponent<Checkpoint>().value;

        currentLap = 1;

		rigid = gameObject.GetComponent<Rigidbody>();
		finished = false;
		rigid.maxAngularVelocity = 0.05f;

		// Become character.
		if (GameVar.charForP[playerNum] >= GameVar.charDataCustom.Length) {
			headSprite.sprite = headSpriteSrc[GameVar.charForP[playerNum]-GameVar.charDataCustom.Length];
		}
		else headSprite.sprite = headSpriteSrc[headSpriteSrc.Length-1];
		if (playerNum < GameVar.playerCount) headSprite.gameObject.SetActive(false);
        
        // Initialize items.
		blankWeap = pUI.weaponSprite.Length-1;
		weapType = blankWeap;
		blankItem = pUI.itemSprite.Length-1;
		itemType = blankItem;
        shots = 0;
        coins = 0;
    }

    void Update() {
		// Update Checkpoints
		checkDist = (transform.position - lastCheckpoint.transform.position).magnitude - (transform.position - nextCheckpoint.transform.position).magnitude;
		if (finished) checkDist += 1000000;
		Debug.DrawLine(transform.position, lastCheckpoint.transform.position, Color.green, .25f);
		Debug.DrawLine(transform.position, nextCheckpoint.transform.position, Color.blue, .25f);
		Debug.DrawLine(lastCheckpoint.transform.position, nextCheckpoint.transform.position, Color.red, .1f);

		// Update variable limits.
		if (coins < 0) coins = 0;
		if (shots <= 0) {
			weapType = 6;
		}
    }

    void FixedUpdate() {
		// Get relative velocity.
		relVel = new Vector3(Vector3.Dot(transform.right, rigid.velocity), Vector3.Dot(-transform.up, rigid.velocity), Vector3.Dot(transform.forward, rigid.velocity));

        // Slow down player when finished.
		if (finished) {
			rigid.AddRelativeForce(-relVel*2, ForceMode.Acceleration);
		}

        // Control player when not finished.
        else {
            // Slowdown from other players.
            if (!boostOn && relVel.z > speed/(slows+1)) rigid.AddRelativeForce(0,0,-relVel.z, ForceMode.Acceleration);
            if (boostOn && relVel.z > (speed + boostAddSpeed)/(slows+1)) rigid.AddRelativeForce(0,0,-relVel.z, ForceMode.Acceleration);
			if (slows > 0) slowed.SetActive(true);
			else slowed.SetActive(false);

			// Use rockets and fans.
			if (boostOn) {
				rigid.AddRelativeForce(0,0,boostForce, ForceMode.Acceleration);
				if (relVel.z > speed + boostAddSpeed) rigid.AddRelativeForce(0,0,-relVel.z, ForceMode.Acceleration);
			}
            
			// Keep player below max speeds.
			if (relVel.x > Mathf.Abs(speed)) rigid.AddRelativeForce(-relVel.x,0,0, ForceMode.Acceleration);
			if (relVel.z > speed && !boostOn) rigid.AddRelativeForce(0,0,-relVel.z, ForceMode.Acceleration);
			if (spotLock) rigid.AddRelativeForce(-relVel.x,0,-relVel.z, ForceMode.VelocityChange);

            if (grounded) {
				// Slow horizontal movement.
				rigid.AddRelativeForce(traction * -relVel.x,0,0, ForceMode.VelocityChange);

				// Turn board around when going backwards.
				if (relVel.z < -1) transform.Rotate(0,180,0);
			}
        }
    }

    void LateUpdate() {
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
		if (Mathf.Abs(transform.localEulerAngles.x) > 60) {
			transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
		}
    }
	
    void OnCollisionEnter (Collision other) {
		if (other.gameObject.tag == "Track" || other.gameObject.tag == "OutofBounds") {
			if (!grabbing && !tricking) {
				int totalTrickGain = (grabsChained * 30) + (tricksChained * 100) + (grabsCombo * 20);
				if (totalTrickGain > 0) {
					coins += totalTrickGain;
		            aUD.Play("trickwin");
					if (pUI != null) StartCoroutine(pUI.TrickComplete(totalTrickGain));
				}
			}
			if (grabbing || tricking) {
				StartCoroutine(Roll());
				aUD.Play("trickfail");
			}
			if (other.gameObject.tag == "OutofBounds") {
				StartCoroutine(Respawn());
			}

			jumping = false;
			grounded = true;
			ltdr = 0;
			lgdr = 0;
			tricksChained = 0;
			grabsChained = 0;
			grabsCombo = 0;
		}
		if (other.gameObject.tag == "Rock") {
			StartCoroutine(Trip());
			Destroy(other.gameObject);
		}
	}
    
	void OnCollisionExit (Collision other) {
		if (other.gameObject.tag == "Track" || other.gameObject.tag == "OutofBounds") {
			grounded = false;
		}
	}

    void OnCollisionStay (Collision other) {
		if (other.gameObject.tag == "OutofBounds") {
			// Speed down player.
			rigid.AddRelativeForce(-relVel, ForceMode.Acceleration);
		}
	}
    
    void OnTriggerEnter (Collider other) {
		// Checkpoint
		if (other.gameObject.tag == "Checkpoint") {
			Checkpoint chPnt = other.GetComponent<Checkpoint>();
			if (!finished && chPnt.gameObject != lastCheckpoint) {
				lastCheckpoint = nextCheckpoint;
				nextCheckpoint = chPnt.nextCheck;
				nextCheckVal = chPnt.nextCheck.GetComponent<Checkpoint>().value;
				if (chPnt.isLift) {
					lastCheckpoint = firstCheckpoint;
					nextCheckpoint = firstCheckpoint.GetComponent<Checkpoint>().nextCheck;
					if (pUI != null && GameVar.gameMode == 2) pUI.LapTime(currentLap);
					currentLap ++;
					boostOn = false;
					rigid.velocity = Vector3.zero;
					transform.SetPositionAndRotation(playerStartPoint.position, playerStartPoint.rotation);
					rigid.AddRelativeForce(new Vector3 (0,0,jumpForce.z*2));
					if (aI != null) aI.NewLap();
				}
				if (chPnt.isFinish && currentLap >= totalLaps) {
					finalPlace = place;
					corCon.Finish(playerNum);
					if (pUI != null) {
						if (GameVar.gameMode == 2) pUI.LapTime(currentLap);
						pUI.finished = true;
					}
					aI.finished = true;
					finished = true;
					pUI.timerOn = false;
				}
			}
			else if (!finished && chPnt.gameObject == lastCheckpoint) {
				Debug.LogWarning("Wrong direction.");
			}
		}
		// Coin
		if (other.gameObject.tag == "Coin") {
			coins += 100;
			aUD.Play("coin");
			if (other.GetComponent<CoinSpin>().respawnTime == -1) Destroy(other.gameObject);
			else StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
		}
		// Item Box
		if (other.gameObject.tag == "BlueBox") {
			if (coins < 100) {
				aUD.Play("error");
			}
			else {
				coins = coins - 100;
				StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
				aUD.Play("item");
				switch (place) {
					case 1:
						itemType = Mathf.RoundToInt(blueWeights1.Evaluate(Random.value) * pUI.itemSprite.Length-1);
					break;
					case 2:
						itemType = Mathf.RoundToInt(blueWeights2.Evaluate(Random.value) * pUI.itemSprite.Length-1);
					break;
					case 3:
						itemType = Mathf.RoundToInt(blueWeights3.Evaluate(Random.value) * pUI.itemSprite.Length-1);
					break;
					case 4:
						itemType = Mathf.RoundToInt(blueWeights4.Evaluate(Random.value) * pUI.itemSprite.Length-1);
					break;
					default:
						Debug.LogError("Placement out of range for choosing item.");
					break;
				}
			}
		}
		// Ammo Box
		if (other.gameObject.tag == "RedBox") {
			if (coins < 100) {
				aUD.Play("error");
			}
			else {
				coins = coins - 100;
				StartCoroutine(other.GetComponent<CoinSpin>().Respawn());
				aUD.Play("item");
				switch (place) {
					case 1:
						weapType = Mathf.RoundToInt(redWeights1.Evaluate(Random.value) * pUI.weaponSprite.Length-1);
					break;
					case 2:
						weapType = Mathf.RoundToInt(redWeights2.Evaluate(Random.value) * pUI.weaponSprite.Length-1);
					break;
					case 3:
						weapType = Mathf.RoundToInt(redWeights3.Evaluate(Random.value) * pUI.weaponSprite.Length-1);
					break;
					case 4:
						weapType = Mathf.RoundToInt(redWeights4.Evaluate(Random.value) * pUI.weaponSprite.Length-1);
					break;
					default:
						Debug.LogError("Placement out of range for choosing weapon.");
					break;
				}
				shots = 3;
			}
		}
		// Get hit by weapons.
		if (!invisible && other.gameObject.tag == "Projectile") {
			var itemProj = other.transform.parent.GetComponent<ItemProjectile>();
			if (itemProj.parentPlayer != gameObject) {
				if ((grabbing || tricking) && !itemProj.reflected) {
					GameObject clone = Instantiate(projectile, shootSpawn.transform.position, shootSpawn.transform.rotation);
					clone.transform.Rotate(0,180,0);
					clone.GetComponent<ItemProjectile>().weapType = itemProj.weapType;
					clone.GetComponent<ItemProjectile>().parentPlayer = gameObject;
					clone.GetComponent<ItemProjectile>().reflected = true;
					aUD.Play("reflect");
				}
				else {
					switch (itemProj.weapType) {
						case 0:
						StartCoroutine(Ice());
						break;
						case 1:
						StartCoroutine(Parachute());
						break;
						case 2:
						StartCoroutine(Bomb());
						break;
						case 3:
						StartCoroutine(Snowman());
						break;
						case 4:
						StartCoroutine(Tornado());
						break;
						case 5:
						Slapstick();
						break;
					}
				}
			Destroy(itemProj.gameObject);
			}
		}
	}

    // Start race.
	public void SetGo() {
		if (pUI != null) pUI.timerOn = true;
		if (pCon != null) pCon.lockControls = false;
        if (aI != null) aI.locked = false;
		spotLock = false;
    }

    // Physics Functions.
	public void Jump() {
        if (!jumping) {
            jumping = true;
            if (!highJumpReady) {
                rigid.AddRelativeForce(jumpForce, ForceMode.Acceleration);
            }
            else {
                rigid.AddRelativeForce(Vector3.Scale(jumpForce,highJumpForce), ForceMode.Acceleration);
                highJumpParticles.SetActive(false);
                highJumpReady = false;
            }
        } 
    }

	public IEnumerator Trip() {
		boostOn = false;
		invisible = false;
		characterModel.SetActive(true);
		rigid.velocity = Vector3.zero;
		pCon.lockControls = true;
		aI.locked = true;
		spotLock = true;
		lockParticles.SetActive(true);
		yield return new WaitForSeconds(standTime);
		lockParticles.SetActive(false);
		pCon.lockControls = false;
		aI.locked = false;
		spotLock = false;
	}

	public IEnumerator Roll() {
		boostOn = false;
		invisible = false;
		characterModel.SetActive(true);
		pCon.lockControls = true;
		aI.locked = true;
		lockParticles.SetActive(true);
		yield return new WaitForSeconds(rollTime);
		spotLock = true;
		yield return new WaitForSeconds(standTime);
		pCon.lockControls = false;
		lockParticles.SetActive(false);
		aI.locked = false;
		spotLock = false;
	}

	public IEnumerator Respawn () {
		if (!respawning) {
			grounded = true;
			respawning = true;
			if (pCon != null) pCon.lockControls = true;
            if (aI != null) aI.locked = true;
			yield return new WaitForSeconds(3);
            Transform spawnSpot = nextCheckpoint.GetComponent<Checkpoint>().respawn.transform;
			transform.SetPositionAndRotation(spawnSpot.position + new Vector3(0,2,0), spawnSpot.rotation);
			rigid.velocity = Vector3.zero;
			if (pCon != null) pCon.lockControls = false;
            if (aI != null) aI.locked = false;
			respawning = false;
		}
	}

	// Do tricks and board grabs.
	public IEnumerator BoardGrab (int dir, int lastDir) {
		grabbing = true;
		switch (dir) {
			case 1:
			// print ("Front Grab");
			break;
			case 3:
			// print ("Right Grab");
			break;
			case 5:
			// print ("Rear Grab");
			break;
			case 7:
			// print ("Left Grab");
			break;
		}
		yield return new WaitForSeconds(0.4f);
		if (!grounded) {
			grabsChained ++;
			if (lastDir != dir && lastDir != 0) grabsCombo ++;
			lgdr = dir;
		}
		grabbing = false;
	}

	public IEnumerator Trick (int dir, int lastDir) {
		tricking = true;
		aUD.Play("trickstart");
		switch (dir) {
			case 1:
			// print ("Frontflip");
			break;
			case 2:
			// print ("Right Aerial");
			break;
			case 3:
			// print ("Right 360");
			break;
			case 4:
			// print ("Right Backaerial");
			break;
			case 5:
			// print ("Backflip");
			break;
			case 6:
			// print ("Left Backaerial");
			break;
			case 7:
			// print ("Left 360");
			break;
			case 8:
			// print ("Left Aerial");
			break;
		}
		yield return new WaitForSeconds(.5f);
		if (!grounded) {
			tricksChained ++;
			if (lastDir != dir && lastDir != 0) tricksChained ++;
			ltdr = dir;
		}
		pCon.comboAble = true;
		yield return new WaitForSeconds(.25f);
		pCon.comboAble = false;
		tricking = false;
	}

    // Use weapons and items.
	public void Shoot() {
		if (shots > 0) {
			GameObject clone = Instantiate(projectile, shootSpawn.transform.position, shootSpawn.transform.rotation);
			clone.GetComponent<ItemProjectile>().weapType = weapType;
			clone.GetComponent<ItemProjectile>().parentPlayer = gameObject;
			shots --;
		}
	}

    public void Item() {
		switch(itemType) {
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
				corCon.UseSlow(playerNum);
				itemType = blankItem; 
				break;
			case 3:
			// Triple Slow
				corCon.UseTripleSlow(playerNum);
				itemType = blankItem;
				break;
			case 4:
			// Rock
				Instantiate(rock, rockSpawn.transform.position, rockSpawn.transform.rotation);
				itemType = blankItem;
				break;
			case 5:
			// Triple Rock
				corCon.UseTripleStop(playerNum);
				itemType = blankItem;
				break;
			case 6:
			// Steal
				corCon.UseSteal(playerNum);
				itemType = blankItem;
				break;
			case 7:
			// Triple Steal
				corCon.UseTripleSteal(playerNum);
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
	IEnumerator Ice() {
		iceCube.SetActive(true);
		boostOn = false;
		pCon.lockControls = true;
        aI.locked = true;
		rigid.velocity = Vector3.zero;
		spotLock = true;
		yield return new WaitForSeconds(statusTime);
        aI.locked = false;
		pCon.lockControls = false;
		spotLock = false;
		iceCube.SetActive(false);
		StartCoroutine(Trip());
	}

	IEnumerator Snowman() {
		snowman.SetActive(true);
		pCon.lockControls = true;
        aI.locked = true;
		yield return new WaitForSeconds(statusTime);
		pCon.lockControls = false;
        aI.locked = false;
		snowman.SetActive(false);
	}

	IEnumerator Tornado() {
		if (!finished) {
			boostOn = false;
			pCon.lockControls = true;
			aI.locked = true;
			rigid.velocity = Vector3.zero;
			rigid.AddRelativeForce(0,jumpForce.y*2,0);
			yield return new WaitForSeconds(.25f);
			while (!grounded) {
				yield return null;
			}
			spotLock = true;
			rigid.velocity = Vector3.zero;
			yield return new WaitForSeconds(2);
			pCon.lockControls = false;
			aI.locked = false;
			spotLock = false;
		}
	}

	IEnumerator Parachute() {
		if (!finished) {
			boostOn = false;
			pCon.lockControls = true;
			aI.locked = true;
			balloon.SetActive(true);
			rigid.velocity = Vector3.zero;
			rigid.AddRelativeForce(0,jumpForce.y*2,0);
			yield return new WaitForSeconds(.25f);
			rigid.useGravity = false;
			while (!grounded) {
				rigid.AddForce(0, -2, 0, ForceMode.Acceleration);
				yield return null;
			}
			rigid.useGravity = true;
			rigid.velocity = Vector3.zero;
			yield return new WaitForSeconds(2);
			balloon.SetActive(false);
			pCon.lockControls = false;
			aI.locked = false;
		}
	}
	IEnumerator Bomb() {
		boostOn = false;
		pCon.lockControls = true;
        aI.locked = true;
		rigid.velocity = Vector3.zero;
		rigid.AddExplosionForce(jumpForce.y, transform.forward, 3);
		while (!grounded) {
			yield return null;
		}
		pCon.lockControls = false;
        aI.locked = false;
		StartCoroutine(Roll());
	}
    
    void Slapstick() {
		StartCoroutine(Roll());
		Vector3 midcoin = new Vector3(0, 0.5f, -1.5f);
		Vector3 leftcoin = new Vector3(-.75f, 0.5f, -0.5f);
		Vector3 rightcoin = new Vector3(.75f, 0.5f, -0.5f);
		if (coins >= 100) Instantiate(dropCoin, transform.TransformPoint(midcoin), transform.rotation);
		if (coins >= 200) Instantiate(dropCoin, transform.TransformPoint(leftcoin), transform.rotation);
		if (coins >= 300) Instantiate(dropCoin, transform.TransformPoint(rightcoin), transform.rotation);
		coins -= 300;
	}

	// Get affected by Blue Items.
    IEnumerator UseBoost (Vector3 stat) {
		if (!boostOn) {
			boostOn = true;
			boostForce = stat.y;
			boostAddSpeed = stat.z;
			if (stat.x == 7) rocketParticles1.SetActive(true);
			if (stat.x == 5) rocketParticles2.SetActive(true);
			yield return new WaitForSeconds(stat.x);
			boostOn = false;
			if (stat.x == 7) rocketParticles1.SetActive(false);
			if (stat.x == 5) rocketParticles2.SetActive(false);
		}
	}

    IEnumerator UseInvisibility() {
		if (!invisible) {
			characterModel.SetActive(false);
			invisible = true;
			yield return new WaitForSeconds(statusTime);
			characterModel.SetActive(true);
			invisible = false;
		}
	}

	public IEnumerator GetSlowed() {
		if (slows == 0) slowed.SetActive(true);
		slows ++;
		yield return new WaitForSeconds(statusTime);
		slows --;
		if (slows == 0) slowed.SetActive(false);
	}
    
    void UseHighJump () {
		if (!highJumpReady) {
			highJumpReady = true;
			highJumpParticles.SetActive(true);
		}
	}
}
