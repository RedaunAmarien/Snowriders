using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EpisodeSlot : MonoBehaviour
{
    public string trackName;
    public int playerRank, trackNumber;
    public TextMeshProUGUI nameUI, rankUI, numUI;
    public RacePrep menu;
    public Button button;

    public void Start() {
        nameUI.text = trackName;
        rankUI.text = string.Format("<sprite=\"Medals\" index={0}>", playerRank);
        numUI.text = trackNumber.ToString();
    }

    public void OnButtonPress(ChosenCourse choice) {
        menu.ChooseCourse(choice);
    }

}
