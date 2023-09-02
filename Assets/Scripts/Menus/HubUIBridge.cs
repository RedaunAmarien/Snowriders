using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class HubUIBridge : MonoBehaviour
{
    [SerializeField] UIDocument uiDoc;
    VisualElement fader;
    VisualElement fileSelectRoot;
    VisualElement racePrepRoot;
    VisualElement courseSelectRoot;
    VisualElement loadedFileRoot;
    public VisualElement leftArrow;
    public VisualElement rightArrow;
    public Label fileName, coinCount, goldMed, silvMed, bronMed, goldTick, silvTick, bronTick;
    public Label infoName;
    public Label infoDescription;
    public Label pressStart;
    [SerializeField] Sprite blackSprite;
    [SerializeField] Sprite bronzeSprite;
    [SerializeField] Sprite silverSprite;
    [SerializeField] Sprite goldSprite;
    [SerializeField] Sprite blackTicket;
    [SerializeField] Sprite bronzeTicket;
    [SerializeField] Sprite silverTicket;
    [SerializeField] Sprite goldTicket;
    TextField newNameField;
    int newFileSlot;
    bool elementFlashing;
    public float flashDelay;

    void Awake()
    {
        //File References
        fader = uiDoc.rootVisualElement.Q<VisualElement>("Fader");
        fileSelectRoot = uiDoc.rootVisualElement.Q<VisualElement>("FileSelect");
        racePrepRoot = uiDoc.rootVisualElement.Q<VisualElement>("RacePrep");
        courseSelectRoot = uiDoc.rootVisualElement.Q<VisualElement>("CourseSelect");
        loadedFileRoot = uiDoc.rootVisualElement.Q<VisualElement>("Stats");
        infoName = uiDoc.rootVisualElement.Q<Label>("SubjectName");
        infoDescription = uiDoc.rootVisualElement.Q<Label>("SubjectDescription");
        pressStart = uiDoc.rootVisualElement.Q<Label>("PressStart");
        leftArrow = uiDoc.rootVisualElement.Q<VisualElement>("LeftArrow");
        rightArrow = uiDoc.rootVisualElement.Q<VisualElement>("RightArrow");
        StartCoroutine(FlashingArrows());

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

    public void FirstLoad()
    {
        
    }

    public enum WindowSet { FileSelect, RacePrep, CourseSelect };

    public void RevealWindow(WindowSet set)
    {
        switch (set)
        {
            case WindowSet.FileSelect:
                fileSelectRoot.RemoveFromClassList("hidden");
                break;
            case WindowSet.RacePrep:
                racePrepRoot.RemoveFromClassList("hidden");
                break;
            case WindowSet.CourseSelect:
                courseSelectRoot.RemoveFromClassList("hidden");
                break;
        }
    }

    public void HideWindow(WindowSet set)
    {
        switch (set)
        {
            case WindowSet.FileSelect:
                fileSelectRoot.AddToClassList("hidden");
                break;
            case WindowSet.RacePrep:
                racePrepRoot.AddToClassList("hidden");
                break;
            case WindowSet.CourseSelect:
                courseSelectRoot.AddToClassList("hidden");
                break;
        }
    }

    public void FadeIn()
    {
        fader.AddToClassList("fader-hide");
        pressStart.AddToClassList("fader-hide");
        elementFlashing = false;
    }   
    public void FadeOut()
    {
        fader.RemoveFromClassList("fader-hide");
        elementFlashing = true;
    }

    public void StartFlash(VisualElement element)
    {
        elementFlashing = true;
        StartCoroutine(ContinuousFlash(element));
    }
    public void StopFlash(VisualElement element)
    {
        elementFlashing = false;
    }

    IEnumerator FlashingArrows()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(flashDelay);
            leftArrow.ToggleInClassList("flash-on");
            rightArrow.ToggleInClassList("flash-on");
        }
    }

    IEnumerator ContinuousFlash(VisualElement element)
    {
        while (elementFlashing)
        {
            yield return new WaitForSecondsRealtime(flashDelay);
            element.ToggleInClassList("flash-on");
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
        dataRoot.Q<Button>("SubmitName").clickable.clicked += NewNameSubmit;
    }

    void NewNameSubmit()
    {
        if (newNameField.value == "")
        {
            Debug.Log("File name cannot be blank.");
            return;
        }
        VisualElement dataRoot = uiDoc.rootVisualElement.Q<VisualElement>("File" + newFileSlot);
        dataRoot.Q<Label>("FileName").text = newNameField.value;
        dataRoot.Q<Label>("FileName").ToggleInClassList("hidden");
        newNameField = dataRoot.Q<TextField>("NewName");
        dataRoot.Q<VisualElement>("NamingRoot").ToggleInClassList("hidden");
        GetComponent<HubFileSelect>().NameNewSave(newNameField.value);
    }

    public void UpdatePlayer(int index)
    {
        VisualElement root = uiDoc.rootVisualElement.Q<VisualElement>("P" + index);
        root.RemoveFromClassList("hidden");
        VisualElement portrait = root.Q<VisualElement>("Portrait");
        Label charName = root.Q<Label>("CharName");

        VisualElement sRoot = root.Q<VisualElement>("SpeedStars");
        VisualElement cRoot = root.Q<VisualElement>("ControlStars");
        VisualElement jRoot = root.Q<VisualElement>("JumpStars");
        List<VisualElement> sStars = sRoot.Query(name: "Star").ToList();
        List<VisualElement> cStars = cRoot.Query(name: "Star").ToList();
        List<VisualElement> jStars = jRoot.Query(name: "Star").ToList();

        portrait.style.backgroundImage = new StyleBackground(GameRam.allCharacters[GameRam.charForP[index]].charSprite);
        charName.text = GameRam.allCharacters[GameRam.charForP[index]].characterName;

        if (GameRam.allCharacters[GameRam.charForP[index]].speed > 7)
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(goldSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].speed > 4)
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(silverSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].speed > 1)
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(bronzeSprite));
        else
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(blackSprite));

        if (GameRam.allCharacters[GameRam.charForP[index]].turn > 7)
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(goldSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].turn > 4)
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(silverSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].turn > 1)
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(bronzeSprite));
        else
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(blackSprite));

        if (GameRam.allCharacters[GameRam.charForP[index]].jump > 7)
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(goldSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].jump > 4)
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(silverSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].jump > 1)
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(bronzeSprite));
        else
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(blackSprite));


        for (int i = 0; i < 10; i++)
        {
            if (GameRam.allCharacters[GameRam.charForP[index]].speed <= i)
                sStars[i].AddToClassList("hidden");
            else
                sStars[i].RemoveFromClassList("hidden");

            if (GameRam.allCharacters[GameRam.charForP[index]].turn <= i)
                cStars[i].AddToClassList("hidden");
            else
                cStars[i].RemoveFromClassList("hidden");

            if (GameRam.allCharacters[GameRam.charForP[index]].jump <= i)
                jStars[i].AddToClassList("hidden");
            else
                jStars[i].RemoveFromClassList("hidden");
        }
    }

    public void DeactivatePlayer(int index)
    {
        VisualElement root = uiDoc.rootVisualElement.Q<VisualElement>("P" + index);
        root.AddToClassList("hidden");
    }

    public void UpdateBoard(int index)
    {
        VisualElement root = uiDoc.rootVisualElement.Q<VisualElement>("P" + index);
        Label boardName = root.Q<Label>("BoardName");
        boardName.RemoveFromClassList("hidden");

        VisualElement sRoot = root.Q<VisualElement>("SpeedStars");
        VisualElement cRoot = root.Q<VisualElement>("ControlStars");
        VisualElement jRoot = root.Q<VisualElement>("JumpStars");
        List<VisualElement> sStars = sRoot.Query(name: "Star").ToList();
        List<VisualElement> cStars = cRoot.Query(name: "Star").ToList();
        List<VisualElement> jStars = jRoot.Query(name: "Star").ToList();

        boardName.text = GameRam.ownedBoards[GameRam.boardForP[index]].boardName;

        if (GameRam.allCharacters[GameRam.charForP[index]].speed + GameRam.ownedBoards[GameRam.boardForP[index]].speed > 7)
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(goldSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].speed + GameRam.ownedBoards[GameRam.boardForP[index]].speed > 4)
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(silverSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].speed + GameRam.ownedBoards[GameRam.boardForP[index]].speed > 1)
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(bronzeSprite));
        else
            sStars.ForEach(x => x.style.backgroundImage = new StyleBackground(blackSprite));

        if (GameRam.allCharacters[GameRam.charForP[index]].turn + GameRam.ownedBoards[GameRam.boardForP[index]].turn > 7)
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(goldSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].turn + GameRam.ownedBoards[GameRam.boardForP[index]].turn > 4)
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(silverSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].turn + GameRam.ownedBoards[GameRam.boardForP[index]].turn > 1)
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(bronzeSprite));
        else
            cStars.ForEach(x => x.style.backgroundImage = new StyleBackground(blackSprite));

        if (GameRam.allCharacters[GameRam.charForP[index]].jump + GameRam.ownedBoards[GameRam.boardForP[index]].jump > 7)
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(goldSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].jump + GameRam.ownedBoards[GameRam.boardForP[index]].jump > 4)
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(silverSprite));
        else if (GameRam.allCharacters[GameRam.charForP[index]].jump + GameRam.ownedBoards[GameRam.boardForP[index]].jump > 1)
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(bronzeSprite));
        else
            jStars.ForEach(x => x.style.backgroundImage = new StyleBackground(blackSprite));


        for (int i = 0; i < 10; i++)
        {
            if (GameRam.allCharacters[GameRam.charForP[index]].speed + GameRam.ownedBoards[GameRam.boardForP[index]].speed <= i)
                sStars[i].AddToClassList("hidden");
            else
                sStars[i].RemoveFromClassList("hidden");

            if (GameRam.allCharacters[GameRam.charForP[index]].turn + GameRam.ownedBoards[GameRam.boardForP[index]].turn <= i)
                cStars[i].AddToClassList("hidden");
            else
                cStars[i].RemoveFromClassList("hidden");

            if (GameRam.allCharacters[GameRam.charForP[index]].jump + GameRam.ownedBoards[GameRam.boardForP[index]].jump <= i)
                jStars[i].AddToClassList("hidden");
            else
                jStars[i].RemoveFromClassList("hidden");
        }
    }
    public void DeactivateBoard(int index)
    {
        VisualElement root = uiDoc.rootVisualElement.Q<VisualElement>("P" + index);
        Label boardName = root.Q<Label>("BoardName");
        boardName.AddToClassList("hidden");
    }

    public void UpdateCourseSelect(Course course)
    {
        Label courseName = courseSelectRoot.Q<Label>("CourseName");
        VisualElement grade = courseSelectRoot.Q<VisualElement>("CourseGrade");
        VisualElement preview = courseSelectRoot.Q<VisualElement>("CoursePreview");
        Label lapCount = courseSelectRoot.Q<Label>("LapCount");
        Label courseLength = courseSelectRoot.Q<Label>("Length");
        Label prize1 = courseSelectRoot.Q<Label>("Prize1");
        Label prize2 = courseSelectRoot.Q<Label>("Prize2");
        Label prize3 = courseSelectRoot.Q<Label>("Prize3");
        Label prize4 = courseSelectRoot.Q<Label>("Prize4");

        courseName.text = course.courseName;
        switch (GameRam.currentSaveFile.courseGrade[course.courseIndex])
        {
            case SaveData.CourseGrade.None:
                grade.style.backgroundImage = null;
                break;
            case SaveData.CourseGrade.Glass:
                grade.style.backgroundImage = new StyleBackground(blackSprite);
                break;
            case SaveData.CourseGrade.Bronze:
                grade.style.backgroundImage = new StyleBackground(bronzeSprite);
                break;
            case SaveData.CourseGrade.Silver:
                grade.style.backgroundImage = new StyleBackground(silverSprite);
                break;
            case SaveData.CourseGrade.Gold:
                grade.style.backgroundImage = new StyleBackground(goldSprite);
                break;
        }
        preview.style.backgroundImage = course.preview;
        lapCount.text = string.Format("{0} laps", course.defaultLapCount.ToString("N0"));
        courseLength.text = string.Format("{0}m", course.courseLength.ToString("N0"));
        prize1.text = string.Format("1st: {0}", course.prizeMoney[0].ToString("N0"));
        prize2.text = string.Format("2nd: {0}", course.prizeMoney[1].ToString("N0"));
        prize3.text = string.Format("3rd: {0}", course.prizeMoney[2].ToString("N0"));
        prize4.text = string.Format("4th: {0}", course.prizeMoney[3].ToString("N0"));
    }

    public void UpdateChallengeSelect(Challenge challenge)
    {
        Label courseName = courseSelectRoot.Q<Label>("CourseName");
        VisualElement grade = courseSelectRoot.Q<VisualElement>("CourseGrade");
        VisualElement preview = courseSelectRoot.Q<VisualElement>("CoursePreview");
        Label lapCount = courseSelectRoot.Q<Label>("LapCount");
        Label courseLength = courseSelectRoot.Q<Label>("Length");
        Label prize1 = courseSelectRoot.Q<Label>("Prize1");
        Label prize2 = courseSelectRoot.Q<Label>("Prize2");
        Label prize3 = courseSelectRoot.Q<Label>("Prize3");
        Label prize4 = courseSelectRoot.Q<Label>("Prize4");

        courseName.text = challenge.challengeName;
        switch (challenge.challengeLevel)
        {
            case Challenge.TicketLevel.Glass:
                grade.style.backgroundImage = new StyleBackground(blackTicket);
                break;
            case Challenge.TicketLevel.Bronze:
                grade.style.backgroundImage = new StyleBackground(bronzeTicket);
                break;
            case Challenge.TicketLevel.Silver:
                grade.style.backgroundImage = new StyleBackground(silverTicket);
                break;
            case Challenge.TicketLevel.Gold:
                grade.style.backgroundImage = new StyleBackground(goldTicket);
                break;
        }
        preview.style.backgroundImage = challenge.challengeCourse.preview;
        lapCount.text = string.Format("{0} laps", challenge.challengeCourse.defaultLapCount.ToString("N0"));
        courseLength.text = string.Format("{0}m", challenge.challengeCourse.courseLength.ToString("N0"));
        prize1.text = "Requirements:";
        prize2.text = challenge.boardRule ? string.Format("Use the {0}", challenge.requiredBoard.boardName) : "Any Board";
        prize3.text = challenge.coinRule ? string.Format("Finish with at least {0:N0} points", challenge.requiredCoinCount) : "No Point Requirement";
        prize4.text = challenge.timeRule ? string.Format("Finish under {0} seconds", challenge.timeLimitInSeconds) : "No Time Limit";
    }
}