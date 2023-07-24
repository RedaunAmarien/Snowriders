using System.Collections.Generic;
using System;

[Serializable]
public class SaveData
{
    public enum CourseGrade { None, Glass, Bronze, Silver, Gold };
    public string fileName;
    public string version;
    public int saveSlot;
    public int coins;
    public List<int> completedChallenge;
    public int ticketBronze;
    public int ticketSilver;
    public int ticketGold;
    public CourseGrade[] courseGrade;
    public List<int> ownedBoardID;
    public List<int> ownedItemID;
    public List<bool> unlockedBoard;
    public bool storyStarted;
    public DateTime lastSaved;

    public SaveData()
    {
        fileName = string.Empty;
        version = string.Empty;
        saveSlot = -1;
        ownedBoardID = new();
        unlockedBoard = new();
        ownedItemID = new();
        completedChallenge = new();
        courseGrade = new CourseGrade[12];
    }

    public SaveData(int slot, string name, string dataVersion)
    {
        fileName = name;
        version = dataVersion;
        saveSlot = slot;
        ownedItemID = new();
        completedChallenge = new();
        ownedBoardID = new()
        {
            0,
            3,
            6,
            9
        };
        unlockedBoard = new();
        ticketBronze = 1;
        courseGrade = new CourseGrade[12];
        lastSaved = System.DateTime.Now;
    }
}