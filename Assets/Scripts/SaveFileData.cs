using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class SaveFileData {
    public string fileName, version;
    public int coins;
    public int[] courseGrade;
    public bool[] boardOwned;
}