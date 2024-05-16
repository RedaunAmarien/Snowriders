using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sun : MonoBehaviour {
    public float timeOfDayPercent, rotator;
    public Vector3 offsetRoot, offsetLight;
    GameObject child;
    public GameObject[] sceneNightLights;
    [Range(0,1)]
    public float sceneSunrise, sceneSunset;
    public bool timeStatic;
    [Range(0,23)]
    public int staticHour;
    [Range(0,59)]
    public int staticMinute;

    DateTime staticTime; 
    void Start() {
        child = transform.GetChild(0).gameObject;
        if (child == null) Debug.LogErrorFormat("Sun child object was not found. Make sure scene sun has been replaced with LightRoot prefab.");
        Reset();
    }

    void Reset() {
        staticTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, staticHour, staticMinute, 0);
        for (int i = 0; i < sceneNightLights.Length; i++) {
            if (timeOfDayPercent > sceneSunrise && timeOfDayPercent < sceneSunset)
                sceneNightLights[i].SetActive(false);
            else
                sceneNightLights[i].SetActive(true);
        }
    }

    void LateUpdate() {
        if (timeStatic)
            timeOfDayPercent = (float)staticTime.TimeOfDay.TotalMinutes/1440f;
        else
            timeOfDayPercent = (float)DateTime.Now.TimeOfDay.TotalMinutes/1440f;
        rotator = (timeOfDayPercent*360f) + offsetLight.x;
        child.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(rotator, offsetLight.y, offsetLight.z));
    }
}
