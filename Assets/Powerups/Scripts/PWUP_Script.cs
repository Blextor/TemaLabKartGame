using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PWUP_Script : MonoBehaviour
{
    private float startHeight;

    private const float ANIM_DISTANCE = 0.3f;

    private bool goUp = true;

    private void Start()
    {
        startHeight = transform.position.y;
    }

    /**
     * Little animation for fruits
     */
    virtual public void Update()
    {
        transform.Rotate(new Vector3(0, 0, 150f) * Time.deltaTime);

        transform.position += new Vector3(0, 1 / 500f, 0) * (goUp ? 1f : -1f);

        if (transform.position.y >= startHeight + ANIM_DISTANCE)
            goUp = false;
        else if (transform.position.y <= startHeight)
            goUp = true;
    }

    /**
     * Handling collision with players
     */
    private void OnTriggerEnter(Collider other)
    {
        if (other == null || other.gameObject == null || other.gameObject.tag != "Player")
            return;

        PickedUpByPlayer(other.gameObject);
    }

    public abstract void PickedUpByPlayer(GameObject player);

    /**
     * Remove itself from the map, and from the server.
     */
    public void RemoveItself()
    {
        Destroy(gameObject);

        // TODO remove from server
    }

    public void Hide()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}
