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
using UnityEngine.UIElements;

public class HubFileSelect : MonoBehaviour
{
    [SerializeField] bool isActive;
    [SerializeField] int selectedFile;
    [SerializeField] int newFileNum;
    [SerializeField] private SaveData[] saveData;
    private SaveData newSaveData;
    [SerializeField] string saveFileVersion;
    string savesFolder;
    bool navHasReset;
    bool activatedThisFrame;
    bool navLocked;
    HubUIBridge uiBridge;

    private void Start()
    {
        uiBridge = GetComponent<HubUIBridge>();
        UpdateFileList();
    }

    public void Activate()
    {
        Debug.Log("Save list activated.");
        isActive = true;
        activatedThisFrame = true;
        uiBridge.RevealWindow(HubUIBridge.WindowSet.FileSelect);

        // Find Save Files.
        savesFolder = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(savesFolder))
        {
            Directory.CreateDirectory(savesFolder);
            //firstTime = true;
        }
        UpdateFileList();
        selectedFile = 0;
        uiBridge.HighlightSave(0);
        navHasReset = true;
    }

    public void OnCancel()
    {
        GetComponent<HubTownControls>().Reactivate();
        uiBridge.HideWindow(HubUIBridge.WindowSet.FileSelect);
        isActive = false;
    }

    public void OnSubmit()
    {
        if (!isActive || navLocked)
            return;

        if(activatedThisFrame)
        {
            activatedThisFrame = false;
            return;
        }

        bool isNew = true;
        for (int i = 0; i < saveData.Length; i++)
        {
            if (saveData[i].saveSlot == selectedFile)
            {
                Load(selectedFile);
                isNew = false;
                OnCancel();
                break;
            }
        }

        if (isNew)
        {
            NewFile();
        }
    }

    public void OnNavigate(InputValue val)
    {
        if (!isActive || navLocked)
            return;

        Vector2 v = val.Get<Vector2>();
        if (v.magnitude < 0.5f)
        {
            navHasReset = true;
            return;
        }

        if (!navHasReset)
            return;

        uiBridge.HighlightSave(selectedFile);
        if ( v.x > 0.5f)
        {
            selectedFile += 1;
            if (selectedFile == 3)
            {
                selectedFile = 0;
            }
            if (selectedFile >= 6)
            {
                selectedFile = 3;
            }
            navHasReset = false;
        }
        if (v.x < -0.5f)
        {
            selectedFile -= 1;
            if (selectedFile == 2)
            {
                selectedFile = 5;
            }    
            if (selectedFile <= -1)
            {
                selectedFile = 2;
            }
            navHasReset = false;
        }
        if (v.y > 0.5f)
        {
            selectedFile += 3;
            if (selectedFile >= 6)
            {
                selectedFile -= 6;
            }
            navHasReset = false;
        }
        if (v.y < -0.5f)
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

    //public void OnVolumeChange(Slider slider)
    //{
    //    GameRam.masterVolume = slider.value;
    //}

    void UpdateFileList()
    {
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
        uiBridge.UpdateSaveDisplay(saveData);
    }

    public void NewFile()
    {
        uiBridge.CreateNewFile(selectedFile);
        Debug.LogFormat("Creating new save file in slot {0}.", selectedFile);
        GameRam.currentSaveFile = newSaveData;
        navLocked = true;
    }

    public void NameNewSave(string name)
    {
        Debug.LogFormat("Creating new save file \"{0}\".", name);
        navLocked = false;
        newSaveData = new(selectedFile, name, saveFileVersion);
        FileManager.SaveFile(newSaveData.fileName, newSaveData, Path.Combine(Application.persistentDataPath, "Saves"));
        Debug.LogFormat("Successfully created new save file \"{0}\".", newSaveData.fileName);
        UpdateFileList();
        Load(selectedFile);
        OnCancel();
    }

    public void Load(int slot)
    {
        SaveData dataToLoad = new();
        foreach (SaveData data in saveData)
        {
            if (data.saveSlot == slot)
                dataToLoad = data;
        }
        GameRam.currentSaveFile = dataToLoad;
        GameRam.currentSaveDirectory = Path.Combine(savesFolder, dataToLoad.fileName + ".srd");
        // GameRam.currentSaveFile.saveSlot = slot;
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
        uiBridge.UpdateFileDisplay();
        uiBridge.HighlightSave(slot);
    }

    //public void DeleteFile()
    //{
    //    string path = Application.persistentDataPath + "/Saves/" + deleteBox.text + ".srd";
    //    bool succeeded = FileManager.DeleteFile(path);
    //    if (succeeded)
    //        Debug.LogWarningFormat("Successfully deleted file {0}", path);
    //    else
    //        Debug.LogErrorFormat("Failed to delete file at {0}", path);
    //    UpdateFileList();
    //}

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
