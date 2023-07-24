using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class HubMultiplayerInput : MonoBehaviour
{
    [SerializeField] GameObject hubController;
    [SerializeField] int myIndex;

    void Start()
    {
        hubController = GameObject.Find("Controller");
        transform.parent = hubController.transform;
        PlayerInput input = GetComponent<PlayerInput>();
        myIndex = input.playerIndex;
        GameRam.inputDevice[input.playerIndex] = input.user.pairedDevices[0];
        GameRam.inputUser[input.playerIndex] = input.user;
        gameObject.SendMessageUpwards("InputStart", myIndex);
    }

    public struct Parameters {
        public InputValue val;
        public int index;
    }

    // Update is called once per frame
    void OnNavigate(InputValue val)
    {
        Parameters paras = new();
        paras.val = val;
        paras.index = myIndex;
        gameObject.SendMessageUpwards("OnNavigateCustom", paras);
    }

    void OnCancel()
    {
        gameObject.SendMessageUpwards("OnCancelCustom", myIndex);
    }

    void OnSubmit()
    {
        gameObject.SendMessageUpwards("OnSubmitCustom", myIndex);
    }
}
