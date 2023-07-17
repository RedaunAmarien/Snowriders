using System.Collections.Generic;
using System;

[Serializable]
public class SaveData
{
    private static SaveData _current;
    public static SaveData Current
    {
        get
        {
            _current ??= new SaveData();
            return _current;
        }
        set
        {
            if (value != null)
            {
                _current = value;
            }
        }
    }

    public enum CourseGrade { None, Black, Bronze, Silver, Gold };
    public string fileName, version;
    public int saveSlot;
    public int coins;
    public List<int> completedChallenge;
    public int ticketBronze, ticketSilver, ticketGold;
    public CourseGrade[] courseGrade;
    public List<int> ownedBoardID, ownedItemID;
    public bool storyStarted;
    public DateTime lastSaved;

    public SaveData()
    {
        fileName = string.Empty;
        version = string.Empty;
        saveSlot = -1;
        ownedBoardID = new();
        ownedItemID = new();
        completedChallenge = new();
        courseGrade = new CourseGrade[12];
    }

    public SaveData(int slot, string name, string dataVersion)
    {
        fileName = name;
        version = dataVersion;
        saveSlot = slot;
        ownedBoardID = new();
        ownedItemID = new();
        completedChallenge = new();
        ownedBoardID.Add(0);
        ownedBoardID.Add(3);
        ownedBoardID.Add(6);
        ownedBoardID.Add(9);
        ticketBronze = 1;
        courseGrade = new CourseGrade[12];
        lastSaved = System.DateTime.Now;
    }
}