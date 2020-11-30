using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EpisodeSlot : MonoBehaviour
{
    public int trackNumber;
    public TextMeshProUGUI nameUI, rankUI, numUI;
    public RacePrep menu;

    public void OnButtonPress()
    {
        menu.ChooseCourse(trackNumber);
    }

}
