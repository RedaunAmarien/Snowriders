using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCannon : MonoBehaviour
{

    public bool active;
    bool turningRight;
    [Tooltip("Item Prefab")]
    public GameObject weapon;
    [Tooltip("Item Choice")]
    public ItemProjectile.WeaponType weaponType;
    public float shootSpeed, turnSpeed, turnRange;
    float turnOffset;

    void Start()
    {
        turnOffset = transform.localEulerAngles.y;
        StartCoroutine(Shoot());
    }

    void Update()
    {
        if (turnRange == 0)
        {
            // Don't turn.
        }
        else
        {
            if (turningRight)
            {
                transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
                if (transform.localEulerAngles.y > turnOffset + turnRange)
                {
                    turningRight = false;
                }
            }
            else
            {
                transform.Rotate(0, -turnSpeed * Time.deltaTime, 0);
                if (transform.localEulerAngles.y < turnOffset - turnRange)
                {
                    turningRight = true;
                }
            }
        }
    }

    IEnumerator Shoot()
    {
        if (active)
        {
            GameObject clone = Instantiate(weapon, transform.position, transform.rotation);
            clone.GetComponentInChildren<ItemProjectile>().weaponType = weaponType;
        }
        yield return new WaitForSeconds(shootSpeed);
        StartCoroutine(Shoot());
    }
}
