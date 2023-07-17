using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

public class HubShop : MonoBehaviour
{
    [SerializeField] bool isActive;
    //[Tooltip("0 = Name, 1 = Speed, 2 = Turn, 3 = Jump, 4 = Flavor")]
    //public Text[] boardInfoText;
    //[Tooltip("0 = Coin, 1 = Bronze, 2 = Silver, 3 = Gold")]
    //public Text[] costs, funds;
    public int currentChoice;
    //public GameObject loadSet, warnSet, soldOutSign;
    public GameObject lazySusan;
    public Board[] specialBoards;
    public Board[] basicBoards;
    //public Slider progressBar;
    bool stickMove;
    SaveData reloadData;
    //public Image fadePanel;
    //bool fadingIn, fadingOut;
    //public float fadeDelay, startTime;
    HubUIBridge uiBridge;
    bool activatedThisFrame;

    public void Activate()
    {
        isActive = true;
        activatedThisFrame = true;
        uiBridge = GetComponent<HubUIBridge>();
        currentChoice = 4;
        basicBoards = Resources.LoadAll<Board>("Objects/Boards/Basic");
        basicBoards = basicBoards.OrderBy(x => x.shopIndex).ToArray();
        specialBoards = Resources.LoadAll<Board>("Objects/Boards/Special");
        specialBoards = specialBoards.OrderBy(x => x.shopIndex).ToArray();

        //fadePanel.gameObject.SetActive(true);
        //StartCoroutine(Fade(true));
    }

    void Update()
    {
        if (!isActive)
            return;

        //lazySusan.transform.SetPositionAndRotation(lazySusan.transform.position, Quaternion.Euler(0, currentChoice * 36f, 0));

        string currentBoardDescription = string.Format("Speed: {0} Control: {1} Jump: {2}\n{3}", specialBoards[currentChoice].speed, specialBoards[currentChoice].turn, specialBoards[currentChoice].jump, specialBoards[currentChoice].description);

        uiBridge.infoName.text = specialBoards[currentChoice].name;
        uiBridge.infoDescription.text = currentBoardDescription;

        if (GameRam.currentSaveFile.ownedBoardID.Contains(specialBoards[currentChoice].boardID))
        {
            uiBridge.infoName.style.color = Color.gray;
            uiBridge.coinCount.text = string.Format(
                "{0} <color=#aaaaaa>- {1}",
                GameRam.currentSaveFile.coins.ToString("N0"),
                specialBoards[currentChoice].boardCost.coins.ToString("N0"));
            uiBridge.goldTick.text = string.Format(
                "{0} <color=#aaaaaa>- {1}",
                GameRam.currentSaveFile.ticketGold.ToString("N0"),
                specialBoards[currentChoice].boardCost.goldTickets.ToString("N0"));
            uiBridge.silvTick.text = string.Format(
                "{0} <color=#aaaaaa>- {1}",
                GameRam.currentSaveFile.ticketSilver.ToString("N0"),
                specialBoards[currentChoice].boardCost.silverTickets.ToString("N0"));
            uiBridge.bronTick.text = string.Format(
                "{0} <color=#aaaaaa>- {1}",
                GameRam.currentSaveFile.ticketBronze.ToString("N0"),
                specialBoards[currentChoice].boardCost.bronzeTickets.ToString("N0"));
        }
        else
        {
            uiBridge.infoName.style.color = Color.white;
            uiBridge.coinCount.text = string.Format(
                "{0} {2}- {1}",
                GameRam.currentSaveFile.coins.ToString("N0"),
                specialBoards[currentChoice].boardCost.coins.ToString("N0"),
                specialBoards[currentChoice].boardCost.coins <= GameRam.currentSaveFile.coins ? "<color=green>" : "<color=red>");
            uiBridge.goldTick.text = string.Format(
                "{0} {2}- {1}",
                GameRam.currentSaveFile.ticketGold,
                specialBoards[currentChoice].boardCost.goldTickets,
                specialBoards[currentChoice].boardCost.goldTickets <= GameRam.currentSaveFile.ticketGold ? "<color=green>" : "<color=red>");
            uiBridge.silvTick.text = string.Format(
                "{0} {2}- {1}",
                GameRam.currentSaveFile.ticketSilver,
                specialBoards[currentChoice].boardCost.silverTickets,
                specialBoards[currentChoice].boardCost.silverTickets <= GameRam.currentSaveFile.ticketSilver ? "<color=green>" : "<color=red>");
            uiBridge.bronTick.text = string.Format(
                "{0} {2}- {1}",
                GameRam.currentSaveFile.ticketBronze,
                specialBoards[currentChoice].boardCost.bronzeTickets,
                specialBoards[currentChoice].boardCost.bronzeTickets <= GameRam.currentSaveFile.ticketBronze ? "<color=green>" : "<color=red>");
        }
    }

    public void OnCancel()
    {
        GetComponent<HubTownControls>().Reactivate();
        uiBridge.infoName.style.color = Color.white;
        uiBridge.coinCount.text = GameRam.currentSaveFile.coins.ToString("N0");
        uiBridge.goldTick.text = GameRam.currentSaveFile.ticketGold.ToString();
        uiBridge.silvTick.text = GameRam.currentSaveFile.ticketSilver.ToString();
        uiBridge.bronTick.text = GameRam.currentSaveFile.ticketBronze.ToString();
        isActive = false;
    }

    public void OnSubmit()
    {
        if (!isActive)
            return;

        if (activatedThisFrame)
        {
            activatedThisFrame = false;
            return;
        }

        ItemCost board = specialBoards[currentChoice].boardCost;
        SaveData save = GameRam.currentSaveFile;
        if (board.coins > save.coins
            || board.bronzeTickets > save.ticketBronze
            || board.silverTickets > save.ticketSilver
            || board.goldTickets > save.ticketGold)
        {
            // Not Enough.
        }
        else if (GameRam.currentSaveFile.ownedBoardID.Contains(specialBoards[currentChoice].boardID))
        {
            // Already Owned.
        }
        else
        {
            GameRam.currentSaveFile.ownedBoardID.Add(specialBoards[currentChoice].boardID);
            GameRam.currentSaveFile.coins -= board.coins;
            GameRam.currentSaveFile.ticketBronze -= board.bronzeTickets;
            GameRam.currentSaveFile.ticketSilver -= board.silverTickets;
            GameRam.currentSaveFile.ticketGold -= board.goldTickets;
            GameRam.currentSaveFile.lastSaved = System.DateTime.Now;
            FileManager.SaveFile(GameRam.currentSaveFile.fileName, GameRam.currentSaveFile, Application.persistentDataPath + "/Saves");
            GameRam.ownedBoards.Clear();
            foreach (int id in GameRam.currentSaveFile.ownedBoardID)
            {
                foreach (Board availableBoard in GameRam.allBoards)
                {
                    if (availableBoard.boardID == id)
                    {
                        GameRam.ownedBoards.Add(availableBoard);
                    }
                }
            }
            // reloadData = LoadFile(GameRam.currentSaveDirectory);
        }
    }

    public void OnNavigate(InputValue val)
    {
        var v = val.Get<Vector2>();

        if (v.x > .5f && !stickMove)
        {
            currentChoice++;
            if (currentChoice > specialBoards.Length - 1) currentChoice = 0;
            stickMove = true;
        }
        else if (v.x < -.5f && !stickMove)
        {
            currentChoice--;
            if (currentChoice < 0) currentChoice = specialBoards.Length - 1;
            stickMove = true;
        }
        else if (v.x > -.5f && v.x < .5f)
            stickMove = false;
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

    //public IEnumerator Fade(bool i)
    //{
    //    if (i) fadingIn = true;
    //    else fadingOut = true;
    //    startTime = Time.time;
    //    yield return new WaitForSeconds(fadeDelay);
    //    if (i) fadingIn = false;
    //    else
    //    {
    //        fadingOut = false;
    //        StartCoroutine(LoadScene("HubTown"));
    //    }
    //}
}