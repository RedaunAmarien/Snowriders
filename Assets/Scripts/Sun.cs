using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {
    public float time1, time2, time3, timetick, rotator, offsetX, offsetY;
    void LateUpdate() {
        time1 = System.DateTime.Now.Hour;
        time2 = System.DateTime.Now.Minute;
        time3 = System.DateTime.Now.Second;
        timetick = ((time1*60*60) + (time2*60) + (time3));
        rotator = (timetick/86400*360) + offsetX;
        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(rotator,offsetY,0));
        // if (time1 > 6 && time1 < 18) nightLight.SetActive(false);
        // else nightLight.SetActive(true);
    }
}
