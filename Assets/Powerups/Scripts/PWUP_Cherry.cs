using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PWUP_Cherry : PWUP_Script
{
    [SerializeField] private float effectTime = 10f;

    [SerializeField] private Text counterText;

    private bool isTimerActive = false;

    private float remainingTime = 0;

    private bool pickedUp = false;

    private GameObject player = null;

    public override void PickedUpByPlayer(GameObject player)
    {
        if (pickedUp) return;
        pickedUp = true;

        this.player = player;

        foreach (var otherPlayer in GetOtherPlayers(player))
        {
            IgnoreOtherPlayerCollision(player, otherPlayer, true);
        }

        isTimerActive = true;
        remainingTime = effectTime;

        Hide();
    }

    private List<GameObject> GetOtherPlayers(GameObject ignorable)
    {
        List<GameObject> otherPlayers = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));

        otherPlayers.Remove(ignorable);

        return otherPlayers;
    }

    private void IgnoreOtherPlayerCollision(GameObject player, GameObject otherPlayer, bool ignore)
    {
        Collider[] playerColliders = player.transform.GetComponentsInChildren<Collider>();
        Collider[] otherPlayerColliders = otherPlayer.transform.GetComponentsInChildren<Collider>();

        foreach (var playerCollider in playerColliders)
        {
            foreach (var otherPlayerCollider in otherPlayerColliders)
            {
                Physics.IgnoreCollision(playerCollider, otherPlayerCollider, ignore);
            }
        }
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

        foreach (var otherPlayer in GetOtherPlayers(player))
        {
            IgnoreOtherPlayerCollision(player, otherPlayer, false);
        }

        RemoveItself();
    }
}
