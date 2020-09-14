using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FileButton : MonoBehaviour {

    public int fileSlot;
    public string fileName, fileStats;
    public TextMeshProUGUI fileNameText, fileStatsText;
    public MainMenu mainMenu;
    public Button fileButton;

    void Start() {
        fileNameText.text = fileName;
        fileStatsText.text = fileStats;
    }

    public void OnButtonPress() {
        mainMenu.Load(fileSlot);
    }
}
