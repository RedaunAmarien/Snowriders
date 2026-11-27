using System.Collections;
using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    public int respawnTime;
    public float spinSpeed;
    public Vector2 spinRange;
    bool spinCCW, noSpawn;

    void Start()
    {
        float choice = Random.value;
        if (choice > .5f)
            spinCCW = false;
        else
            spinCCW = true;
        spinSpeed = Random.Range(spinRange.x, spinRange.y);
    }

    void Update()
    {
        if (spinCCW)
            transform.Rotate(new Vector3(0, -spinSpeed, 0) * Time.deltaTime);
        else 
            transform.Rotate(new Vector3(0, spinSpeed, 0) * Time.deltaTime);
    }

    public IEnumerator Respawn()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnTime);
        gameObject.SetActive(true);
    }
}
