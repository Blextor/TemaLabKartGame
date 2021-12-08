using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PWUP_Banana : PWUP_Script
{
    public override void PickedUpByPlayer(GameObject player)
    {
        player.GetComponent<BananaThrow>().IncreaseBananaCount();

        RemoveItself();
    }
}
