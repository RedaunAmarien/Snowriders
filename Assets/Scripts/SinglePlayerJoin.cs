using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SinglePlayerJoin : MonoBehaviour
{
    public BattleMenu battleMenu;
    public GameObject prefab;
    // Start is called before the first frame update
    public void OnPlayerJoined(PlayerInput player)
    {
        battleMenu.OnPlayerJoined(player);
        Instantiate(prefab);
    }

    // Update is called once per frame
    public void OnplayerLeft(PlayerInput player)
    {
        battleMenu.OnPlayerLeft(player);
    }
}
