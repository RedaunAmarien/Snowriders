using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;
using UnityEditor.Animations;

public class HubFileSelect : MonoBehaviour
{
    [SerializeField] bool isActive;
    //public GameObject mainSet, loadSet, newFileSet, chooseSet, firstTimeSet, firstLoad, fileSelectDefault;
    public TMP_InputField saveBox, deleteBox;
    // public Text[] fileName, fileCoins, bTickCount, sTickCount, gTickCount, nonCount, bronzeMedalDisplay, silverMedalDisplay, goldMedalDisplay;
    // public Image[] file0Medal, file1Medal, file2Medal, file0Icon, file1Icon, file2Icon;
    public Button[] fileButton;
    public FileButton[] fileScript;
    public TextMeshProUGUI[] fileName;
    public GameObject[] fileContainer;
    // public Sprite[] medalSource;
    public RectTransform scrollContent;
    public Scrollbar scrollbar;
    public GameObject filePrefab;
    bool firstTime;
    public Image fadePanel;
    public bool fadingIn, fadingOut;
    public float fadeDelay, startTime;
    string[] names, charFileCust, charFilePerm, boardFile;
    public SaveData[] saveData;
    public SaveData newSaveData;
    public string saveFileVersion;
    string savesFolder;
    int newFileNum;
    HubUIBridge uiBridge;
    bool navHasReset;
    int selectedFile;

    public void Activate()
    {
        isActive = true;
        uiBridge = GetComponent<HubUIBridge>();
        uiBridge.RevealWindow(HubUIBridge.WindowSet.FileSelect);

        // Find Save Files.
        savesFolder = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(savesFolder))
        {
            Directory.CreateDirectory(savesFolder);
            firstTime = true;
        }
        UpdateFileList();
        selectedFile = 0;
        uiBridge.HighlightSave(0);
        navHasReset = true;
    }

    private void Update()
    {
        if (!isActive)
            return;


    }

    //void Assign(int con)
    //{
    //    GameRam.controlp[0] = con;
    //    GameRam.playerCount = 1;
    //    //firstLoad.SetActive(false);
    //    if (firstTime)
    //    {
    //        //firstTimeSet.SetActive(true);
    //    }
    //    else
    //    {
    //        //mainSet.SetActive(true);
    //    }
    //}

    public void OnCancel()
    {
        GetComponent<HubTownControls>().Reactivate();
        uiBridge.HideWindow(HubUIBridge.WindowSet.FileSelect);
        isActive = false;
    }

    public void OnNavigate(InputValue val)
    {
        if (!isActive)
            return;

        Vector2 v = val.Get<Vector2>();
        if (v == Vector2.zero)
        {
            navHasReset = true;
            return;
        }

        uiBridge.HighlightSave(selectedFile);

        if (navHasReset && v.x > 0.5f)
        {
            selectedFile += 1;
            if (selectedFile >= 6)
            {
                selectedFile = 0;
            }
            navHasReset = false;
        }
        if (navHasReset && v.x < -0.5f)
        {
            selectedFile -= 1;
            if (selectedFile <= -1)
            {
                selectedFile = 5;
            }
            navHasReset = false;
        }
        if (navHasReset && v.y > 0.5f)
        {
            selectedFile += 3;
            if (selectedFile >= 6)
            {
                selectedFile -= 6;
            }
            navHasReset = false;
        }
        if (navHasReset && v.y < -0.5f)
        {
            selectedFile -= 3;
            if (selectedFile <= -1)
            {
                selectedFile += 6;
            }
            navHasReset = false;
        }
        uiBridge.HighlightSave(selectedFile);
        GameRam.currentSaveSlot = selectedFile;
    }

    public void OnVolumeChange(Slider slider)
    {
        GameRam.masterVol = slider.value;
    }

    void UpdateFileList()
    {
        //for (int i = 0; i < fileContainer.Length; i++)
        //{
        //    Destroy(fileContainer[i]);
        //}
        string[] saveFiles = Directory.GetFiles(savesFolder, "*.srd");
        saveData = new SaveData[saveFiles.Length];
        Debug.LogFormat("Found {0} save files.", saveFiles.Length);

        for (int i = 0; i < saveFiles.Length; i++)
        {
            saveData[i] = (SaveData)FileManager.LoadFile(saveFiles[i]);
            if (saveData[i] == null)
            {
                Debug.LogErrorFormat("File {0} not read.", i);
            }
        }
        //saveData = saveData.OrderByDescending(x => x.saveSlot).ToArray();

        uiBridge.UpdateSaveDisplay(saveData);
    }

    public void NewFile()
    {
        //newFileSet.SetActive(true);
        //chooseSet.SetActive(false);
    }

    public void NameNewSave()
    {
        if (saveBox.text == "")
        {
            // Do nothing.
        }
        else
        {
            newSaveData.fileName = saveBox.text;
            newSaveData.version = saveFileVersion;
            newSaveData.saveSlot = selectedFile;
            newSaveData.ownedBoardID.Add(0);
            newSaveData.ownedBoardID.Add(3);
            newSaveData.ownedBoardID.Add(6);
            newSaveData.ownedBoardID.Add(9);
            newSaveData.ticketBronze = 1;
            newSaveData.courseGrade = new SaveData.CourseGrade[12];
            newSaveData.lastSaved = System.DateTime.Now;
            FileManager.SaveFile(newSaveData.fileName, newSaveData, Path.Combine(Application.persistentDataPath, "Saves"));
            Debug.LogFormat("Created new save file \"{0}\".", newSaveData.fileName);
            UpdateFileList();
        }
    }

    public void Load(int fileIndex)
    {
        if (saveData[fileIndex] != null)
        {
            GameRam.currentSaveFile = saveData[fileIndex];
            GameRam.currentSaveDirectory = Path.Combine(savesFolder, saveData[fileIndex].fileName + ".srd");
            // GameRam.currentSaveFile.saveSlot = fileIndex;
            Debug.LogFormat("File \"{0}\" loaded. Last saved on {1}.", GameRam.currentSaveFile.fileName, GameRam.currentSaveFile.lastSaved);
            if (GameRam.currentSaveFile.ownedBoardID == null) GameRam.currentSaveFile.ownedBoardID = new List<int>();
            foreach (int pin in GameRam.currentSaveFile.ownedBoardID)
            {
                foreach (Board board in GameRam.allBoards)
                {
                    if (board.boardID == pin)
                    {
                        GameRam.ownedBoards.Add(board);
                        // Debug.Log("Board " + board.name + " owned.");
                    }
                }
            }
            GameRam.ownedBoards = GameRam.ownedBoards.OrderBy(x => x.shopIndex).ToList();
            GameRam.currentSaveDirectory = FileManager.SaveFile(GameRam.currentSaveFile.fileName, GameRam.currentSaveFile, savesFolder);
            Debug.LogFormat("Save directory set to {0}", GameRam.currentSaveDirectory);
            //StartCoroutine(Fade(false));
        }
        else
        {
            //newFileSet.SetActive(true);
            //chooseSet.SetActive(false);
            newFileNum = fileIndex;
        }
    }

    public void DeleteFile()
    {
        string path = Application.persistentDataPath + "/Saves/" + deleteBox.text + ".srd";
        bool test = FileManager.DeleteFile(path);
        if (test) Debug.LogWarningFormat("Successfully deleted file {0}", path);
        else Debug.LogErrorFormat("Failed to delete file at {0}", path);
        UpdateFileList();
    }

    IEnumerator LoadScene(string sceneToLoad)
    {
        GameRam.nextSceneToLoad = sceneToLoad;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    static Character LoadChar(string path)
    {
        using (StreamReader streamReader = File.OpenText(path))
        {
            string jsonString = streamReader.ReadToEnd();
            return JsonUtility.FromJson<Character>(jsonString);
        }
    }

    static void SaveChar(Character charData, string path)
    {
        string jsonString = JsonUtility.ToJson(charData, true);
        using (StreamWriter streamWriter = File.CreateText(path))
        {
            streamWriter.Write(jsonString);
        }
    }

    static Board LoadBoard(string path)
    {
        using (StreamReader streamReader = File.OpenText(path))
        {
            string jsonString = streamReader.ReadToEnd();
            return JsonUtility.FromJson<Board>(jsonString);
        }
    }
}
