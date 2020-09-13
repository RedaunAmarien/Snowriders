using System.Collections.Generic;

[System.Serializable]
public class SaveData {
    
    private static SaveData _current;
    public static SaveData current {
        get {
            if (_current == null) {
                _current = new SaveData();
            }
            return _current;
        }
        set
        {
            if (value != null) {
                _current = value;
            }
        }
    }

    public string fileName, version;
    public int saveSlot;
    public int coins;
    public bool[] challengeBronze, challengeSilver, challengeGold;
    public int ticketBronze, ticketSilver, ticketGold;
    public int[] courseGrade;
    public bool[] boardOwned;
    public bool storyStarted;
}