using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HubUIBridge : MonoBehaviour
{
    public VisualElement fileSelectRoot;
    public UIDocument uiDoc;
    public Button createButton;
    public Button deleteButton;
    public Button backButton;
    public ListView fileList;
    public VisualTreeAsset listItemTemplate;
    public Label fileName, coinCount, goldMed, silvMed, bronMed, goldTick, silvTick, bronTick;
    public Label infoName;
    public Label infoDescription;

    void Start()
    {
        //File References
        fileSelectRoot = uiDoc.rootVisualElement.Q<VisualElement>("FileSelect");
        fileList = uiDoc.rootVisualElement.Q<ListView>("FileList");
        fileName = uiDoc.rootVisualElement.Q<Label>("FileName");
        coinCount = uiDoc.rootVisualElement.Q<Label>("CoinCount");
        goldMed = uiDoc.rootVisualElement.Q<Label>("GoldMed");
        silvMed = uiDoc.rootVisualElement.Q<Label>("SilvMed");
        bronMed = uiDoc.rootVisualElement.Q<Label>("BronMed");
        goldTick = uiDoc.rootVisualElement.Q<Label>("GoldTick");
        silvTick = uiDoc.rootVisualElement.Q<Label>("SilvTick");
        bronTick = uiDoc.rootVisualElement.Q<Label>("BronTick");
        infoName = uiDoc.rootVisualElement.Q<Label>("SubjectName");
        infoDescription = uiDoc.rootVisualElement.Q<Label>("SubjectDescription");
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
        //fileList.makeItem = () =>
        //{
        //    var newListEntry = listItemTemplate.Instantiate();
        //    var newListEntryLogic = new FileListEntryController();
        //    newListEntry.userData = newListEntryLogic;
        //    newListEntryLogic.SetVisualElement(newListEntry);

        //    return newListEntry;
        //};

        //fileList.bindItem = (item, index) =>
        //{
        //    (item.userData as FileListEntryController).SetFileData(data[index]);
        //};

        //fileList.fixedItemHeight = 128;
        //fileList.itemsSource = data;
    }
}

public class FileListEntryController
{
    Label nameLabel;
    Label coinCount;
    Label golds;
    Label silvers;
    Label bronzes;
    Label goldTicks;
    Label silverTicks;
    Label bronzeTicks;
    Button loadButton;

    public void SetVisualElement(VisualElement visualElement)
    {
        nameLabel = visualElement.Q<Label>("FileName");
        coinCount = visualElement.Q<Label>("CoinCount");
        golds = visualElement.Q<Label>("GoldMed");
        silvers = visualElement.Q<Label>("SilvMed");
        bronzes = visualElement.Q<Label>("BronMed");
        goldTicks = visualElement.Q<Label>("GoldTick");
        silverTicks = visualElement.Q<Label>("SilvTick");
        bronzeTicks = visualElement.Q<Label>("BronTick");
        loadButton = visualElement.Q<Button>("LoadButton");
    }

    public void SetFileData(SaveData saveData)
    {
        nameLabel.text = saveData.fileName;
        coinCount.text = saveData.coins.ToString("N0");
        golds.text = saveData.courseGrade.Count(y => y == SaveData.CourseGrade.Gold).ToString();
        silvers.text = saveData.courseGrade.Count(y => y == SaveData.CourseGrade.Silver).ToString();
        bronzes.text = saveData.courseGrade.Count(y => y == SaveData.CourseGrade.Bronze).ToString();
        goldTicks.text = saveData.ticketGold.ToString();
        silverTicks.text = saveData.ticketSilver.ToString();
        bronzeTicks.text = saveData.ticketBronze.ToString();
        //loadButton.RegisterCallback<ClickEvent>(gameObject.GetComponent<HubFileSelect>().Load(saveData.fileName);
    }
}