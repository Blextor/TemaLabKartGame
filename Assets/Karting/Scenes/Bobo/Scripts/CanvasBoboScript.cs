using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CanvasBoboScript : MonoBehaviour
{
    public int playerCnt=0;
    public Text[] texts;
    public GameObject panel;
    public Text ipadd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //gameObject.GetComponent<Canvas>().GetComponentInChildren<Panel>().enabled = Input.GetKey(KeyCode.Tab);
        

        GameObject[] players = GameObject.FindGameObjectsWithTag("BoboPlayer");
        panel.SetActive(Input.GetKey(KeyCode.Tab) && players.Length!=0);
        ipadd.enabled=(players.Length == 0);
     
        for (int i=0; i < 4; i++)
        {
            if (players.Length > i && players.Length != 0)
            {
                string temp = "Points: " + (int)(players[i].GetComponent<HelloWorldPlayer>().PointsNet.Value);
                texts[i].text = temp;
                texts[i].color = Color.black;
                texts[i].fontSize = 14;
            }
            else
            {
                string temp = "Offline";
                texts[i].text = temp;
                texts[i].color = Color.grey;
                texts[i].fontSize = 28;
            }
        }
    }
}
