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

        //createButton = uiDoc.rootVisualElement.Q<Button>("CreateButton");
        //deleteButton = uiDoc.rootVisualElement.Q<Button>("DeleteButton");
        //backButton = uiDoc.rootVisualElement.Q<Button>("ReturnButton");
        //listItemTemplate = uiDoc.rootVisualElement.Q<VisualElement>("ListItemTemplate");
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

    public void AddListItems(SaveData[] data)
    {
        fileList.makeItem = () =>
        {
            var newListEntry = listItemTemplate.Instantiate();
            var newListEntryLogic = new FileListEntryController();
            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);

            return newListEntry;
        };

        fileList.bindItem = (item, index) =>
        {
            (item.userData as FileListEntryController).SetFileData(data[index]);
        };

        fileList.fixedItemHeight = 100;
        fileList.itemsSource = data;
    }

    //private void Update()
    //{
    //    //createButton.clickable.clicked += () =>
    //    //{
    //    //    GetComponent<HubFileSelect>().NewFile();
    //    //};
    //}

    //// Update is called once per frame
    //public void AddFiles(SaveData[] newData)
    //{
    //    //FileListController controller = new();
    //    //controller.InitializeFileList(newData, uiDoc.rootVisualElement, listItemTemplate);
    //}
}

public class FileListEntryController
{
    Label nameLabel;

    //This function retrieves a reference to the 
    //character name label inside the UI element.

    public void SetVisualElement(VisualElement visualElement)
    {
        nameLabel = visualElement.Q<Label>("FileName");
    }

    //This function receives the character whose name this list 
    //element displays. Since the elements listed 
    //in a `ListView` are pooled and reused, it's necessary to 
    //have a `Set` function to change which character's data to display.

    public void SetFileData(SaveData saveData)
    {
        nameLabel.text = saveData.fileName;
    }
}

//public class FileListController
//{
//    // UXML template for list entries
//    VisualTreeAsset listEntryTemplate;

//    // UI element references
//    ListView fileList;
//    Label fileName;
//    Label coinCount;
//    Label goldMed;
//    Label silvMed;
//    Label bronMed;
//    Label goldTick;
//    Label silvTick;
//    Label bronTick;
//    //Label coinCount;

//    List<SaveData> allFiles;

//    public void InitializeFileList(SaveData[] data, VisualElement root, VisualTreeAsset template)
//    {
//        allFiles = new List<SaveData>();
//        allFiles.AddRange(data);

//        // Store a reference to the template for the list entries
//        listEntryTemplate = template;

//        // Store a reference to the character list element
//        fileList = root.Q<ListView>("FileList");

//        // Store references to the selected character info elements
//        fileName = root.Q<Label>("FileName");
//        coinCount = root.Q<Label>("CoinCount");
//        goldMed = root.Q<Label>("GoldMed");
//        silvMed = root.Q<Label>("SilvMed");
//        bronMed = root.Q<Label>("BronMed");
//        goldTick = root.Q<Label>("GoldTick");
//        silvTick = root.Q<Label>("SilvTick");
//        bronTick = root.Q<Label>("BronTick");

//        FillFileList();

//        // Register to get a callback when an item is selected
//        fileList.onSelectionChange += OnFileSelected;
//    }

//    void FillFileList()
//    {
//        // Set up a make item function for a list entry
//        fileList.makeItem = () =>
//        {
//            // Instantiate the UXML template for the entry
//            var newListEntry = listEntryTemplate.Instantiate();

//            // Instantiate a controller for the data
//            var newListEntryLogic = new FileListEntryController();

//            // Assign the controller script to the visual element
//            newListEntry.userData = newListEntryLogic;

//            // Initialize the controller script
//            newListEntryLogic.SetVisualElement(newListEntry);

//            // Return the root of the instantiated visual tree
//            return newListEntry;
//        };

//        // Set up bind function for a specific list entry
//        fileList.bindItem = (item, index) =>
//        {
//            (item.userData as FileListEntryController).SetFileData(allFiles[index]);
//        };

//        // Set a fixed item height
//        fileList.fixedItemHeight = 45;

//        // Set the actual item's source list/array
//        fileList.itemsSource = allFiles;
//    }

//    void OnFileSelected(IEnumerable<object> selectedItems)
//    {
//        // Get the currently selected item directly from the ListView
//        var selectedFile = fileList.selectedItem as SaveData;

//        // Handle none-selection (Escape to deselect everything)
//        if (selectedFile == null)
//        {
//            // Clear
//            fileName.text = "";
//            coinCount.text = "";
//            goldMed.text = "";
//            silvMed.text = "";
//            bronMed.text = "";
//            goldTick.text = "";
//            silvTick.text = "";
//            bronTick.text = "";

//            return;
//        }

//        // Fill in character details
//        fileName.text = selectedFile.fileName;
//        coinCount.text = selectedFile.coins.ToString();
//        goldMed.text = selectedFile.courseGrade.Count(y => y == SaveData.CourseGrade.Gold).ToString();
//        silvMed.text = selectedFile.courseGrade.Count(y => y == SaveData.CourseGrade.Silver).ToString();
//        bronMed.text = selectedFile.courseGrade.Count(y => y == SaveData.CourseGrade.Bronze).ToString();
//        goldTick.text = selectedFile.ticketGold.ToString();
//        silvTick.text = selectedFile.ticketSilver.ToString();
//        bronTick.text = selectedFile.ticketBronze.ToString();
//    }
//}