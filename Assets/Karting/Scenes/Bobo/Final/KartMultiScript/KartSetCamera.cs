using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KartSetCamera : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            CinemachineVirtualCamera settings = GameObject.FindGameObjectWithTag("MainCameraController").GetComponent<CinemachineVirtualCamera>();
            settings.Follow = transform;
            settings.LookAt = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
