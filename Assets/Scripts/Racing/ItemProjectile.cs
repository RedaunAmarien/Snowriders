using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProjectile : MonoBehaviour
{
    private readonly float lifetime = 7.8f;
    private readonly float trigDist = .2f;
    private readonly float speed = 40;
    private Rigidbody rigid;

    [SerializeField] private float gravity;
    [SerializeField] private float targetSpeed;
    [SerializeField] private bool bouncy;
    [SerializeField] private bool bomb;
    [SerializeField] private bool tracking;
    [SerializeField] private GameObject weaponSprite;
    [SerializeField] private Collider target;
    [SerializeField] private Material[] material;

    public enum WeaponType { None, Ice, Parachute, Bomb, Snowman, Tornado, Slapstick };
    public WeaponType weaponType;
    public enum ItemType { None, Invisibility, HighJump, Slow, TripleSlow, Rock, TripleRock, Steal, TripleSteal, Rocket, SuperRocket };
    public bool reflected;
    public GameObject parentPlayer;

    void Start()
    {
        if (weaponType == WeaponType.Bomb) bomb = true;
        if (weaponType == WeaponType.Snowman || weaponType == WeaponType.Tornado) bouncy = true;
        reflected = false;
        weaponSprite.GetComponent<ParticleSystem>().GetComponent<Renderer>().material = material[(int)weaponType-1];
        rigid = GetComponent<Rigidbody>();
        rigid.AddRelativeForce(0, 0, speed, ForceMode.VelocityChange);
        StartCoroutine(Lifetime());
    }

    void FixedUpdate()
    {
        Vector3 vel = new(Vector3.Dot(transform.right, rigid.velocity), Vector3.Dot(-transform.up, rigid.velocity), Vector3.Dot(transform.forward, rigid.velocity));
        rigid.AddRelativeForce(-vel.x, 0, speed - vel.z, ForceMode.VelocityChange);
        rigid.AddForce(0, -gravity, 0);
    }

    void Update()
    {
        if (tracking)
        {
            float turnAng = (Vector3.SignedAngle(transform.forward, transform.position - target.transform.position, transform.up));
            float t = targetSpeed * Time.deltaTime;
            if (turnAng > 0)
            {
                transform.Rotate(0, -t, 0);
            }
            if (turnAng < 0)
            {
                transform.Rotate(0, t, 0);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            if (bouncy)
            {
                transform.forward = Vector3.Reflect(transform.forward, other.contacts[0].normal);
            }
            else if (bomb) Explode();
            else Destroy(gameObject);
        }
    }

    public void OnTriggerEnterExternal(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !tracking)
        {
            if ((other.gameObject != parentPlayer) && !other.GetComponent<RacerCore>().invisible)
            {
                target = other;
                tracking = true;
            }
        }
    }

    public void OnTriggerExitExternal(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target = null;
            tracking = false;
        }
    }

    public void Explode()
    {
        print("Boom");
        //TODO: Explosion effects.
        if (gameObject != null) Destroy(gameObject);
    }

    public IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(trigDist);
        parentPlayer = null;
        yield return new WaitForSeconds(lifetime);
        if (bomb) Explode();
        if (gameObject != null) Destroy(gameObject);
    }
}
