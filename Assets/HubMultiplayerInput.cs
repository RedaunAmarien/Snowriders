using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HubMultiplayerInput : MonoBehaviour
{
    [SerializeField] GameObject hubController;
    [SerializeField] int myIndex;

    void Start()
    {
        hubController = GameObject.Find("Controller");
        transform.parent = hubController.transform;
        myIndex = GetComponent<PlayerInput>().playerIndex;
    }

    public struct Paras {
        public InputValue val;
        public int index;
    }

    // Update is called once per frame
    void OnNavigate(InputValue val)
    {
        Paras paras = new Paras();
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
