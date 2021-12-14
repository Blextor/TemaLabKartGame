using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePowerups : MonoBehaviour
{
    [SerializeField] private int powerupsOnMapCount = 5;

    private System.Random randomObj = new System.Random();

    private Dictionary<GameObject, Vector3> powerupPositions = new Dictionary<GameObject, Vector3>();

    private string[] powerupPrefabs = new string[] { "PWUP_Prefab_Apple", "PWUP_Prefab_Banana", "PWUP_Prefab_Cherry", "PWUP_Prefab_Peach" };
    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> randomMapPoints = GetRandomPoints(powerupsOnMapCount);
        
        for (int i = 0; i < powerupsOnMapCount; ++i)
        {
            GeneratePowerup(randomMapPoints[i]);
        }
    }

    private List<Vector3> GetRandomPoints(int n)
    {
        RoadMeshCreator pathCreator = GameObject.Find("RoadCreator").GetComponent<RoadMeshCreator>();
        List<Vector3> list = new List<Vector3>(pathCreator.RoadPoints);
        List<Vector3> randomList = new List<Vector3>();

        for (int i = 0; i < n; ++i)
        {
            int idx = randomObj.Next(1, list.Count);

            randomList.Add(list[idx]);
            list.RemoveAt(idx);
        }

        return randomList;
    }

    private void GeneratePowerup(Vector3 mapPosition)
    {
        var prefab = Resources.Load<GameObject>($"Powerup/{powerupPrefabs[randomObj.Next(powerupPrefabs.Length)]}");
        Vector3 powerupPos = mapPosition + new Vector3(0, 1f, 0);
        Vector3 powerupRot = new Vector3(-90f, 0, 0);
        GameObject powerup = Instantiate(prefab, powerupPos, Quaternion.Euler(powerupRot));
        powerup.transform.localScale = new Vector3(100f, 100f, 100f);

        powerupPositions[powerup] = mapPosition;
    }

    public void RemovePowerup(GameObject gameObject)
    {
        powerupPositions.Remove(gameObject);

        List<Vector3> randomMapPoints = GetRandomPoints(powerupsOnMapCount + 1);

        int i = 0;
        while(powerupPositions.ContainsValue(randomMapPoints[i])) {
            ++i;
        }

        GeneratePowerup(randomMapPoints[i]);
    }
}
