using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UNET;
using System.Threading;

public class HelloWorldManager : MonoBehaviour
{

    public Text IPAddress;
    string ipAddress = "";
    string stringToEdit = "";
    public UNetTransport script;

    public void Start()
    {
        //IPAddress = GameObject.FindGameObjectWithTag("IP_address_Tag").GetComponent<Text>();
        script = GameObject.FindGameObjectWithTag("NetworkManagerTag").GetComponent<UNetTransport>();
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
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var player = playerObject.GetComponent<HelloWorldPlayer>();
            player.Move();
        }
        
    }

    public void Update()
    {
        Thread th1 = new Thread(Provider);
        th1.Name = "Provider";      // biztos ami biztos
        th1.Start();
        IPAddress.text = ipAddress;

    }

}
