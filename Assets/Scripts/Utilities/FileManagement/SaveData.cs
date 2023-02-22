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
}