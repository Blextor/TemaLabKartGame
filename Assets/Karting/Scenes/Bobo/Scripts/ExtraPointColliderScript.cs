using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraPointColliderScript : MonoBehaviour
{

    public GameObject Parent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {

        Physics.IgnoreLayerCollision(15, 15);

        if (collision.collider.gameObject.layer >= 0)
            Debug.Log("SCollide");
        if (collision.collider.gameObject.layer == 15)
            Debug.Log("ExtraPoint");
       
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
