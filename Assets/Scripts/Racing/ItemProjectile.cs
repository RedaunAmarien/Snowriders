using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProjectile : MonoBehaviour {

	float lifetime = 7.8f, trigDist = .2f, speed = 40;
	public float gravity, targetSpeed;
	public bool reflected, bouncy, bomb, tracking;
	Rigidbody rigid;
	public GameObject weaponSprite, parentPlayer;
	public Collider target;
	public int weapType;
	public Material[] materials;

	void Start () {
		if (weapType == 2) bomb = true;
		if (weapType == 3 || weapType == 4) bouncy = true;
		reflected = false;
		weaponSprite.GetComponent<ParticleSystem>().GetComponent<Renderer>().material = materials[weapType];;
		rigid = GetComponent<Rigidbody>();
		rigid.AddRelativeForce(0,0,speed, ForceMode.VelocityChange);
		StartCoroutine(Lifetime());
	}

	void FixedUpdate() {
		Vector3 vel = new Vector3(Vector3.Dot(transform.right, rigid.velocity), Vector3.Dot(-transform.up, rigid.velocity), Vector3.Dot(transform.forward, rigid.velocity));
		rigid.AddRelativeForce(-vel.x,0,speed - vel.z, ForceMode.VelocityChange);
		rigid.AddForce(0,-gravity,0);
	}

	void Update() {
		if (tracking) {
			float turnAng = (Vector3.SignedAngle(transform.forward, transform.position - target.transform.position, transform.up));
			float t = targetSpeed * Time.deltaTime;
			if (turnAng > 0) {
				transform.Rotate(0,-t,0);
			}
			if (turnAng < 0) {
				transform.Rotate(0,t,0);
			}
		}
	}

	void OnCollisionEnter (Collision other) {
		if (other.gameObject.tag == "Wall") {
			if (bouncy) {
				transform.forward = Vector3.Reflect(transform.forward, other.contacts[0].normal);
			}
			else if (bomb) Explode();
			else Destroy(gameObject);
		}
	}
	
	public void OnTriggerEnterExternal(Collider other) {
		if (other.gameObject.tag == "Player" && !tracking) {
			if ((other.gameObject != parentPlayer) && !other.GetComponent<RacerPhysics>().invisible) {
				target = other;
				tracking = true;
			}
		}
	}

	public void OnTriggerExitExternal(Collider other) {
		if (other.gameObject.tag == "Player") {
			target = null;
			tracking = false;
		}
	}

	public void Explode() {
		print ("Boom");
		//TODO: Explosion effects.
		if (gameObject != null) Destroy(gameObject);
	}

	public IEnumerator Lifetime() {
		yield return new WaitForSeconds(trigDist);
		parentPlayer = null;
		yield return new WaitForSeconds(lifetime);
		if (bomb) Explode();
		if (gameObject != null) Destroy(gameObject);
	}
}
