using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {
    public float time0, time1, time2, time3, timetick, rotator;
    public Vector2 offset = new Vector2 (-90, -30);
    void LateUpdate() {
        time0 = System.DateTime.Now.DayOfYear;
        time1 = System.DateTime.Now.Hour;
        time2 = System.DateTime.Now.Minute;
        time3 = System.DateTime.Now.Second;
        timetick = ((time1*60f*60f) + (time2*60f) + (time3));
        rotator = (timetick/86400f*360f) + offset.x;
        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(rotator,offset.y,0));
        // if (time1 > 6 && time1 < 18) nightLight.SetActive(false);
        // else nightLight.SetActive(true);
    }
}
