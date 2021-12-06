using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PWUP_Peach : PWUP_Script
{
    [SerializeField] private float effectTime = 10f;

    [SerializeField] private float massMultiplier = 20f;

    private bool isTimerActive = false;

    private float remainingTime = 0;

    private bool pickedUp = false;

    private GameObject player = null;

    public override void PickedUpByPlayer(GameObject player)
    {
        if (pickedUp) return;
        pickedUp = true;

        this.player = player;

        player.GetComponent<Rigidbody>().mass *= massMultiplier;

        isTimerActive = true;
        remainingTime = effectTime;

        Hide();
    }

    override public void Update()
    {
        base.Update();

        if (isTimerActive)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                TimerEnded();
        }
    }

    private void TimerEnded()
    {
        isTimerActive = false;

        player.GetComponent<Rigidbody>().mass /= massMultiplier;

        RemoveItself();
    }
}
