using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaThrow : MonoBehaviour
{
    [SerializeField] private float velocity = 5;
    [SerializeField] private float spawnDistanceOfCar = 5;
    [SerializeField] private float secondsUntilBananaDestroy = 10;

    private int bananaCount = 0;

    private Dictionary<GameObject, float> bananasCountDown = new Dictionary<GameObject, float>();

    // Update is called once per frame
    void Update()
    {
        List<GameObject> bananasToRemove = new List<GameObject>();
        foreach(var banana in new List<GameObject>(bananasCountDown.Keys))
        {
            if (bananasCountDown[banana] > 0)
            {
                bananasCountDown[banana] = bananasCountDown[banana] - Time.deltaTime;
            }
            else
            {
                bananasToRemove.Add(banana);
            }
        }
        foreach (var banana in bananasToRemove)
        {
            bananasCountDown.Remove(banana);
            Destroy(banana);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (bananaCount == 0)
                return;

            --bananaCount;

            Vector3 bananaPos = transform.position - transform.forward * spawnDistanceOfCar;
            Quaternion bananaRot = new Quaternion();
            bananaRot.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y + 70f, 0);

            var prefab = Resources.Load<GameObject>("Powerup/PWUP_ThrowingBanana_Prefab");
            GameObject banana = Instantiate(prefab, bananaPos, bananaRot);
            Vector3 bananaVelocity = GetComponent<Rigidbody>().velocity * -1f;
            bananaVelocity.Normalize();
            bananaVelocity = bananaVelocity * velocity;
            banana.GetComponent<Rigidbody>().velocity = bananaVelocity;

            bananasCountDown[banana] = secondsUntilBananaDestroy;
        }
    }

    public void IncreaseBananaCount()
    {
        ++bananaCount;
    }
}
