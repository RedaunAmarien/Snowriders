using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class DoorOpener : MonoBehaviour
{
    public List<GameObject> doors;
    public Vector3 slideFactor;
    public float swingFactor;
    public float slideSpeed;
    public float swingSpeed;
    Vector3[] doorStartPosition;
    Vector3[] doorTargetPosition;
    Quaternion[] doorStartRotation;
    Quaternion[] doorTargetRotation;
    bool isOpen = false;

    void Start()
    {
        doorStartPosition = new Vector3[doors.Count];
        doorTargetPosition = new Vector3[doors.Count];
        doorStartRotation = new Quaternion[doors.Count];
        doorTargetRotation = new Quaternion[doors.Count];
        for (int i = 0; i < doors.Count; i++)
        {
            doorStartPosition[i] = doors[i].transform.localPosition;
            doorTargetPosition[i] = doors[i].transform.localPosition;
            doorStartRotation[i] = doors[i].transform.localRotation;
            doorTargetRotation[i] = doors[i].transform.localRotation;
        }
    }

    private void Update()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].transform.localPosition = Vector3.MoveTowards(doors[i].transform.localPosition, doorTargetPosition[i], slideSpeed * Time.deltaTime);
            doors[i].transform.localRotation = Quaternion.RotateTowards(doors[i].transform.localRotation, doorTargetRotation[i], swingSpeed * Time.deltaTime);
        }
    }

    public void ActivateDoor()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            if (!isOpen)
            {
                doorTargetPosition[i] = doorStartPosition[i] + (i % 2 == 0 ? slideFactor : -slideFactor);
                doorTargetRotation[i].eulerAngles = doorStartRotation[i].eulerAngles + new Vector3(0, (i % 2 == 0 ? swingFactor : -swingFactor), 0);
            }
            else
            {
                doorTargetPosition[i] = doorStartPosition[i];
                doorTargetRotation[i] = doorStartRotation[i];
            }

            Debug.LogFormat("{2} moving from {0} to {1}.", doors[i].transform.localPosition, doorTargetPosition[i], doors[i].name);
        }
        isOpen = !isOpen;
    }
}
