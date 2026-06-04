using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HubTownOption
{
    public string optionName;
    [Multiline]
    public string optionDescription;
    public bool isAvailable = true;
    //public int currentChoice;
    public CinemachineCamera camera;
    public DoorOpener door;
    public UnityEvent events;
    public List<HubTownOption> subOptions;
}
