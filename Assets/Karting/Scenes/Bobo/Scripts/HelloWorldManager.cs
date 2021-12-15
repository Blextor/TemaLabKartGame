using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UNET;
using System.Threading;
using System.Collections.Generic;
using PathCreation.Examples;

public class HelloWorldManager : NetworkBehaviour
{

    public Text IPAddress;
    string ipAddress = "";
    string stringToEdit = "";
    public UNetTransport script;

    public NetworkObject ExtraPointNetworkObject;
    public NetworkVariable<int> MyPointsNet = new NetworkVariable<int>();
    public int MyPoints;
    public NetworkVariable<bool> ActiveExtraPointNet = new NetworkVariable<bool>();
    public bool activeExtraPoint;
    public GameObject ExtraPointGameObject;
    

    //private

    public void Start()
    {
        //IPAddress = GameObject.FindGameObjectWithTag("IP_address_Tag").GetComponent<Text>();
        script = GameObject.FindGameObjectWithTag("NetworkManagerTag").GetComponent<UNetTransport>();
        if (NetworkManager.Singleton.IsServer)
        {
            StartingUp();
            ExtraPointNetworkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        } else if (NetworkManager.Singleton.IsClient)
        {
            StartServerRpc();
        }
    }

    [ServerRpc]
    public void StartServerRpc()
    {
        StartingUp();
    } 

    private void StartingUp()
    {
        ActiveExtraPointNet.Value = true;
        activeExtraPoint = true;
        MyPointsNet.Value = 0;
        MyPoints = 0;
    }

    private void Provider() // WorkingThread()
    {
        try
        {
            System.Net.IPAddress[] iplist = System.Net.Dns.GetHostAddresses(stringToEdit);
            ipAddress = iplist[0].ToString();
            if (ipAddress != script.ConnectAddress)
                script.ConnectAddress = ipAddress;
        }
        catch { ipAddress = ""; 
            script.ConnectAddress = ipAddress; }
        return;
    }

    void ServerAddressInputField()
    {
        stringToEdit = GUILayout.TextField(stringToEdit, 25);
    }

    private void Setup()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }


    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
            ServerAddressInputField();
        }
        else
        {
            StatusLabels();

            SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Tickrate Up" : "Tickrate Up"))
        {
            NetworkManager.Singleton.NetworkConfig.TickRate *= 5;
        }

        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Tickrate Down" : "Tickrate Down"))
        {
            NetworkManager.Singleton.NetworkConfig.TickRate /= 5;
        }
        GUILayout.Label("TickRate: " + NetworkManager.Singleton.NetworkConfig.TickRate);
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    static void SubmitNewPosition()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start"))
            {
                //var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                GameObject startLine = GameObject.FindGameObjectWithTag("StartLine");
                Vector3 trans = startLine.transform.position;
                float dis = players.Length * -3.0f;
                Debug.Log("Talán itt");
                RoadMeshCreator script = GameObject.FindGameObjectWithTag("RoadCreator").GetComponent<RoadMeshCreator>();
                Vector3[] cpoints = script.CirclePoints;
                Vector3[] rpoints = script.RoadPoints;
                List<GameObject> ugr =  GameObject.FindGameObjectWithTag("RoadCreator").GetComponent<RoadMeshCreator>().ramps;
                Vector3[] ugratok_pos = new Vector3[ugr.Count];
                Quaternion[] ugratok_qua = new Quaternion[ugr.Count];
                for (int i=0; i < ugr.Count; i++)
                {
                    ugratok_pos[i] = ugr[i].transform.position;
                    ugratok_qua[i] = ugr[i].transform.rotation;
                }
                for (int i = 0; i < players.Length; i++)
                {
                    Debug.Log("Talán bitt");
                    Vector3 pos = trans + startLine.transform.right * dis;
                    dis += 6f;
                    Debug.Log("Player: " + i + " " + players.Length);
                    players[i].GetComponent<KartPlayer>().GetReadyClientRpc(pos.x, pos.y + 2, pos.z, startLine.transform.forward);
                    Debug.Log("Player: " + i);
                    players[i].GetComponent<KartPlayer>().UpdateMapClientRpc(cpoints, rpoints, ugratok_pos, ugratok_qua);
                    Debug.Log("Player: " + i);
                }
                Debug.Log("Talán itt");
                //var player = playerObject.GetComponent<HelloWorldPlayer>();
                //player.Move();
            }
        }
    }

    
    [ServerRpc]
    private void SpawnExtrapointServerRpc(ulong netID)
    {
        Debug.Log("Probáljunk meg letenni egy Orb-ot");
        GameObject go = Instantiate(ExtraPointGameObject);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(netID);
        ulong itemNetID = go.GetComponent<NetworkObject>().NetworkObjectId;

        SpawnExtrapointClientRpc(itemNetID);
    }

    [ClientRpc]
    private void SpawnExtrapointClientRpc(ulong itemNetID)
    {
        Debug.Log("Sikerült letenni egy Orb-ot");
        NetworkObject netObj = NetworkManager.SpawnManager.SpawnedObjects[itemNetID];

    }




    public void Update()
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Thread th1 = new Thread(Provider);
            th1.Name = "Provider";      // biztos ami biztos
            th1.Start();
            IPAddress.text = ipAddress;
        } else
        {
            IPAddress.text = "";
        }


        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("M pressed");
            SpawnExtrapointServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            //Vector3[] alma = new Vector3[20], korte = new Vector3[20];
            RoadMeshCreator script = GameObject.FindGameObjectWithTag("RoadCreator").GetComponent<RoadMeshCreator>();
            Vector3[] cpoints = script.CirclePoints, rpoints = script.RoadPoints;
            for (int i=0; i < players.Length; i++)
            {
                //if (players[i].GetComponent<NetworkObject>().NetworkObjectId != this.gameObject.GetComponent<NetworkObject>().NetworkObjectId)
                //{
                    
                //players[i].GetComponent<KartPlayer>().UpdateMapClientRpc(cpoints, rpoints);
                //}
            }
        }

        //if (!activeExtraPoint)
          //  ExtraPointGameObject.GetComponent<PowerUpBobo>().DeActivate();
        //else
          //  ExtraPointGameObject.GetComponent<PowerUpBobo>().DeActivate();


    }

}
