using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HubUIBridge : MonoBehaviour
{
    public VisualElement fileSelectRoot;
    public VisualElement loadedFileRoot;
    public UIDocument uiDoc;
    public Button createButton;
    public Button deleteButton;
    public Button backButton;
    public ListView fileList;
    public VisualTreeAsset listItemTemplate;
    public Label fileName, coinCount, goldMed, silvMed, bronMed, goldTick, silvTick, bronTick;
    public Label infoName;
    public Label infoDescription;
    TextField newNameField;
    int newFileSlot;

    void Awake()
    {
        //File References
        fileSelectRoot = uiDoc.rootVisualElement.Q<VisualElement>("FileSelect");
        loadedFileRoot = uiDoc.rootVisualElement.Q<VisualElement>("Stats");
        fileList = uiDoc.rootVisualElement.Q<ListView>("FileList");
        infoName = uiDoc.rootVisualElement.Q<Label>("SubjectName");
        infoDescription = uiDoc.rootVisualElement.Q<Label>("SubjectDescription");

        //Current File Stats
        fileName = loadedFileRoot.Q<Label>("FileName");
        coinCount = loadedFileRoot.Q<Label>("CoinCount");
        goldMed = loadedFileRoot.Q<Label>("GoldMed");
        silvMed = loadedFileRoot.Q<Label>("SilvMed");
        bronMed = loadedFileRoot.Q<Label>("BronMed");
        goldTick = loadedFileRoot.Q<Label>("GoldTick");
        silvTick = loadedFileRoot.Q<Label>("SilvTick");
        bronTick = loadedFileRoot.Q<Label>("BronTick");
    }

    public enum WindowSet { FileSelect, Options, Boards, Clothes, Stats, Quit };

    public void RevealWindow(WindowSet set)
    {
        switch (set)
        {
            case WindowSet.FileSelect:
                fileSelectRoot.style.translate = new StyleTranslate(new Translate(0, 0, 0));
                break;
            case WindowSet.Options:
                break;
            case WindowSet.Boards:
                break;
            case WindowSet.Clothes:
                break;
            case WindowSet.Stats:
                break;
            case WindowSet.Quit:
                break;
        }
    }

    public void HideWindow(WindowSet set)
    {
        switch (set)
        {
            case WindowSet.FileSelect:
                fileSelectRoot.style.translate = new StyleTranslate(new Translate(0, -800, 0));
                break;
            case WindowSet.Options:
                break;
            case WindowSet.Boards:
                break;
            case WindowSet.Clothes:
                break;
            case WindowSet.Stats:
                break;
            case WindowSet.Quit:
                break;
        }
    }

    public void HighlightSave(int slot)
    {
        VisualElement slotElement = uiDoc.rootVisualElement.Q<VisualElement>("File" + slot);
        slotElement.ToggleInClassList("highlighted");
    }

    public void UpdateSaveDisplay(SaveData[] data)
    {
        List<int> emptySlots = new List<int> { 0, 1, 2, 3, 4, 5 };
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != null)
            {
                VisualElement dataRoot = uiDoc.rootVisualElement.Q<VisualElement>("File" + data[i].saveSlot);
                emptySlots.Remove(data[i].saveSlot);
                dataRoot.Q<Label>("FileName").text = data[i].fileName;
                dataRoot.Q<Label>("FileName").style.color = Color.white;
                dataRoot.Q<Label>("CoinCount").text = data[i].coins.ToString("N0");
                dataRoot.Q<Label>("GoldMed").text = data[i].courseGrade.Count(y => y == SaveData.CourseGrade.Gold).ToString();
                dataRoot.Q<Label>("SilvMed").text = data[i].courseGrade.Count(y => y == SaveData.CourseGrade.Silver).ToString();
                dataRoot.Q<Label>("BronMed").text = data[i].courseGrade.Count(y => y == SaveData.CourseGrade.Bronze).ToString();
                dataRoot.Q<Label>("GoldTick").text = data[i].ticketGold.ToString();
                dataRoot.Q<Label>("SilvTick").text = data[i].ticketSilver.ToString();
                dataRoot.Q<Label>("BronTick").text = data[i].ticketBronze.ToString();
            }
        }

        for (int i = 0; i < emptySlots.Count; i++)
        {
            VisualElement dataRoot = uiDoc.rootVisualElement.Q<VisualElement>("File" + emptySlots[i]);
            dataRoot.Q<Label>("FileName").text = "No Data";
            dataRoot.Q<Label>("FileName").style.color = Color.gray;
            dataRoot.Q<Label>("CoinCount").text = "0";
            dataRoot.Q<Label>("GoldMed").text = "0";
            dataRoot.Q<Label>("SilvMed").text = "0";
            dataRoot.Q<Label>("BronMed").text = "0";
            dataRoot.Q<Label>("GoldTick").text = "0";
            dataRoot.Q<Label>("SilvTick").text = "0";
            dataRoot.Q<Label>("BronTick").text = "0";
        }
    }

    public void UpdateFileDisplay()
    {
        //Fill in UI elements from save data.
        int bronzeMedals = 0;
        int silverMedals = 0;
        int goldMedals = 0;
        for (int c = 0; c < GameRam.currentSaveFile.courseGrade.Length; c++)
        {
            if (GameRam.currentSaveFile.courseGrade[c] == SaveData.CourseGrade.Bronze) bronzeMedals++;
            if (GameRam.currentSaveFile.courseGrade[c] == SaveData.CourseGrade.Silver) silverMedals++;
            if (GameRam.currentSaveFile.courseGrade[c] == SaveData.CourseGrade.Gold) goldMedals++;
        }
        fileName.text = GameRam.currentSaveFile.fileName;
        bronMed.text = bronzeMedals.ToString();
        silvMed.text = silverMedals.ToString();
        goldMed.text = goldMedals.ToString();
        bronTick.text = GameRam.currentSaveFile.ticketBronze.ToString();
        silvTick.text = GameRam.currentSaveFile.ticketSilver.ToString();
        goldTick.text = GameRam.currentSaveFile.ticketGold.ToString();
        coinCount.text = GameRam.currentSaveFile.coins.ToString("N0");
    }

    public void CreateNewFile(int slot)
    {
        newFileSlot = slot;
        VisualElement dataRoot = uiDoc.rootVisualElement.Q<VisualElement>("File" + slot);
        dataRoot.Q<Label>("FileName").text = "";
        dataRoot.Q<Label>("FileName").ToggleInClassList("hidden");
        newNameField = dataRoot.Q<TextField>("NewName");
        newNameField.Focus();
        dataRoot.Q<VisualElement>("NamingRoot").ToggleInClassList("hidden");
        dataRoot.Q<Button>("SubmitName").clickable.clicked += OnButtonClicked;
    }

    void OnButtonClicked()
    {
        if (newNameField.value == "")
        {
            Debug.Log("Name cannot be blank.");
            return;
        }
        VisualElement dataRoot = uiDoc.rootVisualElement.Q<VisualElement>("File" + newFileSlot);
        dataRoot.Q<Label>("FileName").text = newNameField.value;
        dataRoot.Q<Label>("FileName").ToggleInClassList("hidden");
        newNameField = dataRoot.Q<TextField>("NewName");
        dataRoot.Q<VisualElement>("NamingRoot").ToggleInClassList("hidden");
        GetComponent<HubFileSelect>().NameNewSave(newNameField.text);
    }
}