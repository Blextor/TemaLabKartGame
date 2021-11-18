using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.UI;

public class PWUP_Apple : PWUP_Script
{
    [SerializeField] private float accelerationBoost = 1.2f;
    [SerializeField] private float topSpeedBoost = 1.2f;
    [SerializeField] private float velocityBoost = 1.2f;

    [SerializeField] private float effectTime = 10f;

    [SerializeField] private Text counterText;

    private bool isTimerActive = false;

    private float remainingTime = 0;

    private GameObject player = null;

    private bool pickedUp = false;

    public override void PickedUpByPlayer(GameObject player)
    {
        if (pickedUp) return;
        pickedUp = true;

        this.player = player;

        ArcadeKart kart = player.GetComponent<ArcadeKart>();
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
            counterText.text = $"Counter: {remainingTime}";

            if (remainingTime <= 0)
                TimerEnded();
        }
    }

    private void TimerEnded()
    {
        isTimerActive = false;

        ArcadeKart kart = player.GetComponent<ArcadeKart>();
        kart.baseStats.Acceleration /= accelerationBoost;
        kart.baseStats.TopSpeed /= topSpeedBoost;

        counterText.text = "Counter: 0";

        RemoveItself();
    }
}
