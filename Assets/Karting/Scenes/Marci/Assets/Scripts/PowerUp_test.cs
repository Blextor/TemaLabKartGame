using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.UI;

public class PowerUp_test : PWUP_Script
{
    [SerializeField] private float accelerationBoost = 1.2f;
    [SerializeField] private float topSpeedBoost = 1.2f;
    [SerializeField] private float velocityBoost = 1.2f;

    [SerializeField] private float effectTime = 10f;

    private bool isTimerActive = false;

    private float remainingTime = 0;

    private GameObject player = null;

    private bool pickedUp = false;

    public override void PickedUpByPlayer(GameObject player)
    {
        if (pickedUp) return;
        pickedUp = true;

        this.player = player;

        MovementKart kart = player.GetComponent<MovementKart>();
        Rigidbody rigidbody = player.GetComponent<Rigidbody>();

        kart.baseStats.Acceleration *= accelerationBoost;
        kart.baseStats.TopSpeed *= topSpeedBoost;
        rigidbody.velocity *= velocityBoost;

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

        MovementKart kart = player.GetComponent<MovementKart>();
        kart.baseStats.Acceleration /= accelerationBoost;
        kart.baseStats.TopSpeed /= topSpeedBoost;

        RemoveItself();
    }
}
