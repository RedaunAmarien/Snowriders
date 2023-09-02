using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class AngleMaker : MonoBehaviour
{
    public Vector3 normal;
    public Vector3 normalEuler;
    public float leanAngle;
    public float spinAngle;
    public Vector3 newRotation;
    public Vector3 newForward;
    public GameObject referenceObject;
    public Vector3 referenceObjectRotation;
    public GameObject midObject;

    private void OnDrawGizmosSelected()
    {
        normal = transform.up; //Normal is readonly in a real race.
        referenceObject.transform.rotation = Quaternion.Euler(referenceObjectRotation);

        leanAngle = Vector3.SignedAngle(referenceObject.transform.up, normal, referenceObject.transform.right);

        newRotation = new Vector3(leanAngle, 0, 0);
        midObject.transform.rotation = Quaternion.Euler(newRotation);
        newForward = new Vector3(0, spinAngle, 0);

        //newUp.Normalize();
        //newForward.Normalize();
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, normal);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(referenceObject.transform.position, referenceObject.transform.up);
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.position, newUp);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(referenceObject.transform.position, referenceObject.transform.forward);
    }
}
