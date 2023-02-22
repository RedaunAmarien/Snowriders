using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerRaceControls : MonoBehaviour
{

    Rigidbody rigid;
    public int controllerIndex;
    public bool fire1Down, fire2Down, rightStickinUse, jumpPressed;
    public bool lockControls, raceOver, comboAble;
    PlayerUI playerUI;
    RacerCore racerCore;
    // public string lStickH, lStickV, rStickH, rStickV, aBut, bBut, xBut, yBut, stBut, bkBut;
    public Vector2 lStickPos, rStickPos;

    void Start()
    {
        //Find objects.
        playerUI = GetComponent<PlayerUI>();
        racerCore = GetComponent<RacerCore>();

        //Initialize objects.
        rigid = gameObject.GetComponent<Rigidbody>();
        racerCore.spotLock = true;
        lockControls = true;
        rigid.maxAngularVelocity = 0.05f;
    }

    void Update()
    {

        // Control player if not finished.
        if (!racerCore.finished && !lockControls)
        {
            // Steer
            if (racerCore.grounded)
            {
                if (!jumpPressed)
                {
                    if (lStickPos.y >= 0)
                    {
                        float t = lStickPos.x * racerCore.turnSpeed * Time.deltaTime;
                        transform.Rotate(0, t, 0);
                    }
                    else
                    {
                        float t = lStickPos.x * racerCore.turnSpeed * Time.deltaTime;
                        transform.Rotate(0, t + (t * Mathf.Abs(lStickPos.y)), 0);
                    }
                    if (lStickPos.x < 0)
                    {
                        racerCore.animator.SetBool("TurningLeft", true);
                        racerCore.animator.SetBool("TurningRight", false);
                    }
                    else if (lStickPos.x > 0)
                    {
                        racerCore.animator.SetBool("TurningRight", true);
                        racerCore.animator.SetBool("TurningLeft", false);
                    }
                    else
                    {
                        racerCore.animator.SetBool("TurningLeft", false);
                        racerCore.animator.SetBool("TurningRight", false);
                    }
                }
            }
            else
            {
                // Turn slower when not grounded, but not at all when tricking
                if (!racerCore.tricking)
                {
                    var t = lStickPos.x * racerCore.turnSpeed * Time.deltaTime;
                    transform.Rotate(0, t / 2, 0);
                }
            }

            // Do board grabs.
            if (!racerCore.grabbing && !racerCore.grounded && !rightStickinUse)
            {
                if (rStickPos.x < -.2f)
                {
                    StartCoroutine(racerCore.BoardGrab(7, racerCore.lgdr));
                    rightStickinUse = true;
                }
                else if (rStickPos.x > .2f)
                {
                    StartCoroutine(racerCore.BoardGrab(3, racerCore.lgdr));
                    rightStickinUse = true;
                }
                else if (rStickPos.y < -.2f)
                {
                    StartCoroutine(racerCore.BoardGrab(5, racerCore.lgdr));
                    rightStickinUse = true;
                }
                else if (rStickPos.y > .2f)
                {
                    StartCoroutine(racerCore.BoardGrab(1, racerCore.lgdr));
                    rightStickinUse = true;
                }
            }
            if (Mathf.Abs(rStickPos.x) < .2f && Mathf.Abs(rStickPos.y) < .2f) rightStickinUse = false;
            // Use items and attacks, making sure one press only fires one collectItem.
            // if (Input.GetButtonDown(xBut) && !fire2Down) {
            // 	racerCore.Shoot();
            // 	fire2Down = true;
            // }
            // if (Input.GetButtonDown(bBut) && !fire1Down) {
            // 	racerCore.Item();
            // 	fire1Down = true;
            // }
        }
    }

    public void OnLookBack(InputValue val)
    {
        float p = val.Get<float>();
        //Look Backwards.
        if (p == 0) playerUI.camIsReversed = false;
        else playerUI.camIsReversed = true;
    }

    public void OnJump(InputValue val)
    {
        float p = val.Get<float>();
        //Tell scripts A is pressed.
        if (p == 0) jumpPressed = false;
        else jumpPressed = true;

        // Return to Menu
        if (raceOver && jumpPressed) StartCoroutine(racerCore.trackManager.Fade(false));

        // During Race
        if (!racerCore.finished && !lockControls)
        {
            if (racerCore.grounded)
            {
                if (jumpPressed)
                {
                    racerCore.animator.SetBool("Crouching", true);
                }
                else
                {
                    // Jump and perform tricks if touching the ground.
                    racerCore.Jump();
                    if (lStickPos.x > .2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(2, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(4, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(3, racerCore.ltdr));
                    }
                    else if (lStickPos.x < -.2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(8, racerCore.ltdr));
                        else if (lStickPos.y < .2f) StartCoroutine(racerCore.Trick(6, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(7, racerCore.ltdr));
                    }
                    else
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(1, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(5, racerCore.ltdr));
                    }
                }
            }
            else
            {
                // Combo tricks in air
                if (!jumpPressed && comboAble)
                {
                    if (lStickPos.x > .2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(2, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(4, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(3, racerCore.ltdr));
                    }
                    else if (lStickPos.x < -.2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(8, racerCore.ltdr));
                        else if (lStickPos.y < .2f) StartCoroutine(racerCore.Trick(6, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(7, racerCore.ltdr));
                    }
                    else
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(1, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(5, racerCore.ltdr));
                    }
                    comboAble = false;
                }
            }
        }
    }

    public void OnJumpAI(bool down)
    {

        // During Race
        if (!racerCore.finished && !lockControls)
        {
            if (racerCore.grounded)
            {
                if (down)
                {
                    racerCore.animator.SetBool("Crouching", true);
                }
                else
                {
                    // Jump and perform tricks if touching the ground.
                    racerCore.Jump();
                    if (lStickPos.x > .2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(2, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(4, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(3, racerCore.ltdr));
                    }
                    else if (lStickPos.x < -.2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(8, racerCore.ltdr));
                        else if (lStickPos.y < .2f) StartCoroutine(racerCore.Trick(6, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(7, racerCore.ltdr));
                    }
                    else
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(1, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(5, racerCore.ltdr));
                    }
                }
            }
            else
            {
                // Combo tricks in air
                if (!down && comboAble)
                {
                    if (lStickPos.x > .2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(2, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(4, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(3, racerCore.ltdr));
                    }
                    else if (lStickPos.x < -.2f)
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(8, racerCore.ltdr));
                        else if (lStickPos.y < .2f) StartCoroutine(racerCore.Trick(6, racerCore.ltdr));
                        else StartCoroutine(racerCore.Trick(7, racerCore.ltdr));
                    }
                    else
                    {
                        if (lStickPos.y > .2f) StartCoroutine(racerCore.Trick(1, racerCore.ltdr));
                        else if (lStickPos.y < -.2f) StartCoroutine(racerCore.Trick(5, racerCore.ltdr));
                    }
                    comboAble = false;
                }
            }
        }
    }

    public void OnShoot(InputValue val)
    {
        var v = val.Get<float>();
        if (v > 0) fire1Down = true;
        else fire1Down = false;
        if (!racerCore.finished && !lockControls && !fire1Down)
        {
            racerCore.Shoot();
        }
    }

    public void OnShootAI()
    {
        if (!racerCore.finished && !lockControls)
        {
            racerCore.Shoot();
        }
    }

    public void OnItem(InputValue val)
    {
        var v = val.Get<float>();
        if (v > 0) fire2Down = true;
        else fire2Down = false;
        if (!racerCore.finished && !lockControls && !fire2Down)
        {
            racerCore.Item();
        }
    }

    public void OnItemAI()
    {
        if (!racerCore.finished && !lockControls)
        {
            racerCore.Item();
        }
    }

    public void OnTurn(InputValue val)
    {
        lStickPos = val.Get<Vector2>();
    }

    public void OnGrab(InputValue val)
    {
        rStickPos = val.Get<Vector2>();
    }

    public void OnPause(InputValue val)
    {
        if (val.Get<float>() < .5f)
        {
            // Pause or unpause.
            if (!playerUI.paused) playerUI.Pause(0);
            else playerUI.Pause(1);
        }
    }
}
