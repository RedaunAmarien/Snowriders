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
    [Tooltip("0 = Name, 1 = Speed, 2 = Turn, 3 = Jump, 4 = Flavor")]
    public Text[] boardInfoText;
    [Tooltip("0 = Coin, 1 = Bronze, 2 = Silver, 3 = Gold")]
    public Text[] costs, funds;
    public int currentChoice;
    //public GameObject loadSet, warnSet, soldOutSign;
    public GameObject lazySusan;
    public Board[] specialBoards;
    public Board[] basicBoards;
    public Slider progressBar;
    bool stickMove;
    SaveData reloadData;
    //public Image fadePanel;
    bool fadingIn, fadingOut;
    public float fadeDelay, startTime;
    HubUIBridge uiBridge;

    public void Activate()
    {
        isActive = true;
        uiBridge = GetComponent<HubUIBridge>();
        currentChoice = 4;
        basicBoards = Resources.LoadAll<Board>("Objects/Boards/Basic");
        basicBoards = basicBoards.OrderBy(x => x.shopIndex).ToArray();
        specialBoards = Resources.LoadAll<Board>("Objects/Boards/Special");
        specialBoards = specialBoards.OrderBy(x => x.shopIndex).ToArray();

        //fadePanel.gameObject.SetActive(true);
        StartCoroutine(Fade(true));
    }

    void Update()
    {
        if (!isActive)
            return;

        //lazySusan.transform.SetPositionAndRotation(lazySusan.transform.position, Quaternion.Euler(0, currentChoice * 36f, 0));

        string currentBoardDescription = string.Format("Sp: {0} Tn: {1} Jp: {2}\n{3}", specialBoards[currentChoice].speed, specialBoards[currentChoice].turn, specialBoards[currentChoice].jump, specialBoards[currentChoice].description);

        uiBridge.infoName.text = specialBoards[currentChoice].name;
        uiBridge.infoDescription.text = currentBoardDescription;

        if (GameRam.ownedBoards.Contains(specialBoards[currentChoice]))
        {
            for (int i = 0; i < 4; i++)
            {
                boardInfoText[i].color = Color.gray;
            }
            //soldOutSign.SetActive(true);
        }
        else
        {
            costs[0].text = GameRam.allBoards[currentChoice].boardCost.coins.ToString("N0");
            costs[1].text = GameRam.allBoards[currentChoice].boardCost.bronzeTickets.ToString();
            costs[2].text = GameRam.allBoards[currentChoice].boardCost.silverTickets.ToString();
            costs[3].text = GameRam.allBoards[currentChoice].boardCost.goldTickets.ToString();
            for (int i = 0; i < 4; i++)
            {
                boardInfoText[i].color = Color.white;
            }
            //soldOutSign.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (fadingIn)
        {
            float t = (Time.time - startTime) / fadeDelay;
            //fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(1f, 0f, t));
        }
        else if (fadingOut)
        {
            float t = (Time.time - startTime) / fadeDelay;
            //fadePanel.color = new Color(0, 0, 0, Mathf.SmoothStep(0f, 1f, t));
        }

    }
    public void OnCancel()
    {
        GetComponent<HubTownControls>().Reactivate();
        isActive = false;
    }

    public void OnSubmit()
    {
        if (!isActive) return;
        ItemCost board = GameRam.allBoards[currentChoice].boardCost;
        SaveData save = GameRam.currentSaveFile;
        if (board.coins > save.coins
            || board.bronzeTickets > save.ticketBronze
            || board.silverTickets > save.ticketSilver
            || board.goldTickets > save.ticketGold)
        {
            // Not Enough.
        }
        else if (GameRam.ownedBoards.Contains(GameRam.allBoards[currentChoice]))
        {
            // Already Owned.
        }
        else
        {
            GameRam.currentSaveFile.ownedBoardID.Add(GameRam.allBoards[currentChoice].boardID);
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

    //public void OnCancel()
    //{
    //    StartCoroutine(Fade(false));
    //}

    public void OnNavigate(InputValue val)
    {
        var v = val.Get<Vector2>();

        if (v.x > .5f && !stickMove)
        {
            currentChoice++;
            if (currentChoice > GameRam.allBoards.Count - 1) currentChoice = 4;
            stickMove = true;
        }
        else if (v.x < -.5f && !stickMove)
        {
            currentChoice--;
            if (currentChoice < 4) currentChoice = GameRam.allBoards.Count - 1;
            stickMove = true;
        }
        else if (v.x > -.5f && v.x < .5f) stickMove = false;
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

    public IEnumerator Fade(bool i)
    {
        if (i) fadingIn = true;
        else fadingOut = true;
        startTime = Time.time;
        yield return new WaitForSeconds(fadeDelay);
        if (i) fadingIn = false;
        else
        {
            fadingOut = false;
            StartCoroutine(LoadScene("HubTown"));
        }
    }
}