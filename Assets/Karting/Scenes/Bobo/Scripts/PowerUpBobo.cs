using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpBobo : MonoBehaviour
{
    public Collider coli;
    public MeshRenderer meshrend;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");
        DeActivate();
        HelloWorldPlayer script = other.gameObject.GetComponent<HelloWorldPlayer>();
        script.GetExtraPoint(GetComponent<NetworkObject>().NetworkObjectId);
    }
    public void DeActivate()
    {
        coli.enabled = false;
        meshrend.enabled = false;
    }

    public void Activate()
    {
        coli.enabled = true;
        meshrend.enabled = true;
    }
}
