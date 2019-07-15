using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    public Terrain[] terrains;
    public GameObject wisp;
    public List<GameObject> objectToPlace = new List<GameObject>();
    public Camera wispyCam;
    public Camera mainCam;
    public int maxItemsPerSpwn;
    public int minItemsPerSpwn;
    public int objectDisplacementX;
    public int objectDisplacementY;
    public int objectDisplacementZ;
    public int objectDisplacementX2;
    public int objectDisplacementZ2;
    public Canvas menu;
    public Canvas confirm;


    private Terrain currTerrain;
    private Vector3 newWispLocation;
    private List<int> currentObjects = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private String[] objectNames;
    private Vector3[] objectLocation;
    private Vector3 newPos;
    private string currentWisp;
    private int displacementX;
    private int numberOfObjects;
    private int objectCount;
    private int count;
    private int objectsToSpawn;
    private int currTerrainIndex;
    private int terrainWidth;
    private int terrainLength;
    private int terrainPosX;
    private int terrainPosZ;
    private int i = 1;
    private bool isWispMoving = false;
    private List<string> objectsInWorld = new List<string>();
    private List<Vector3> transformsInWorld = new List<Vector3>();
    private List<Vector3> orbsInWorld = new List<Vector3>();
    private GameObject newWisp;
    private bool bol;
    private GameObject tempObject;

    public void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            SpawnWisp();
        }
        if (Input.GetKey(KeyCode.P))
        {
            menu.enabled = true;
        }
    }
    private void Start()
    {
        loadData("World");
       InvokeRepeating("SensorSpawn",0.01f,0.5f);
    }

    public async void SensorSpawn()
    {

        Sensor1 m = await APIService.Instance.Server1RandNum();
        int oldTriggers1 = m.sensor1;
        Debug.Log(m);

        Sensor2 o = await APIService.Instance.Server2RandNum();
        int oldTriggers2 = o.sensor2;
        Debug.Log(0);

        StartCoroutine(timer());

        Sensor1 n = await APIService.Instance.Server1RandNum();
        int newTriggers1 = n.sensor1;

        Sensor2 p = await APIService.Instance.Server2RandNum();
        int newTriggers2 = p.sensor2;

        if (oldTriggers1 != newTriggers1 || oldTriggers2 != newTriggers2)
        {
            SpawnWisp();
        }


    }


    public void SpawnWisp()
    {
        
        objectCount = currentObjects.Min();
        currTerrainIndex = currentObjects.IndexOf(objectCount);
        terrainWidth = (int)terrains[currTerrainIndex].terrainData.size.x;
        terrainLength = (int)terrains[currTerrainIndex].terrainData.size.z;
        terrainPosX = (int)terrains[currTerrainIndex].transform.position.x;
        terrainPosZ = (int)terrains[currTerrainIndex].transform.position.z;
        int posX = UnityEngine.Random.Range(terrainPosX, terrainPosX + terrainWidth);
        int posZ = UnityEngine.Random.Range(terrainPosZ, terrainPosZ + terrainLength);
        float posY = Terrain.activeTerrain.SampleHeight(new Vector3(posX, 50, posZ));
        Vector3 orbPlace = new Vector3(posX,50,posZ);
         newWisp = (GameObject)Instantiate(wisp,orbPlace, Quaternion.identity);
      
        orbsInWorld.Add(orbPlace);
        // GameObject.Find("WispCam").GetComponent<MultipleTargetCamera>().targets.Add(wisp.transform);
        // wispyCam.enabled = true;
        //  mainCam.enabled = false;

        // currentObjects[currTerrainIndex]++;// i need to change this code , tis checking wisps not object

        SpawnObjects();
    }




    void SpawnObjects()
    {
        if (!isWispMoving)
        {
            int tempIndex = 0;
            objectsToSpawn = UnityEngine.Random.Range(minItemsPerSpwn, maxItemsPerSpwn + 1);
            for (int i = 0; i < objectsToSpawn; i++)
            {
                tempIndex = UnityEngine.Random.Range(0, objectToPlace.Count + 1);
                Vector3 tempPlace = new Vector3(newWisp.transform.position.x + UnityEngine.Random.Range(-15, 15),
                 5f, newWisp.transform.position.z + UnityEngine.Random.Range(-15, 15));
              GameObject newObject = (GameObject)Instantiate(objectToPlace[tempIndex], tempPlace, Quaternion.identity);
                mainCam.enabled = false;
                wispyCam.enabled = true;
                wispyCam.transform.LookAt(newObject.transform);
                StartCoroutine(waitForCam());
                count++;
                objectsInWorld.Add(objectToPlace[tempIndex].name);
                transformsInWorld.Add(tempPlace);
                Objectinfo objectinfo = new Objectinfo();
                objectinfo.objectNames.AddRange(objectsInWorld);
                objectinfo.objectTransforms.AddRange(transformsInWorld);
                objectinfo.orbTransforms.AddRange(orbsInWorld);
                string jsonData = JsonUtility.ToJson(objectinfo);
                saveData("World", jsonData);
            }
            currentObjects[currTerrainIndex] = currentObjects[currTerrainIndex] + objectsToSpawn;
        }
    }



    //
    // Menu
    //

    public void Clear()
    {
        menu.enabled = false;
        confirm.enabled = true;
    }

    public void Decline()
    {
        menu.enabled = true;
        confirm.enabled = false;
    }

    public void Accept()
    {
        menu.enabled = false;
        confirm.enabled = false;
        deleteData("World");
        SceneManager.LoadScene(0);
        wispyCam.enabled = false;
        mainCam.enabled = true;
    }

    public void Exit()
    {
        menu.enabled = false;
        confirm.enabled = false;
    }




    //
    //Save Data
    //


    void saveData(string saveName, string jsonData)
    {
        //saveData to HDD
        string tempPath = Path.Combine(Application.persistentDataPath, "data");
        tempPath = Path.Combine(tempPath, saveName + ".zombdata");
        byte[] jsonByte = Encoding.ASCII.GetBytes(jsonData);

        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
        }

        try
        {
            File.WriteAllBytes(tempPath, jsonByte);
            Debug.Log("Saved To: " + tempPath.Replace("/", "\\"));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed To PlayerInfo Data to: " + tempPath.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    void loadData(string dataFileName)
    {
        string tempPath = Path.Combine(Application.persistentDataPath, "data");
        tempPath = Path.Combine(tempPath, dataFileName + ".zombdata");

        //Exit if Directory or File does not exist
        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            Debug.LogWarning("Directory does not exist");

        }

        else if (!File.Exists(tempPath))
        {
            Debug.Log("File does not exist");

        }

        else
        {
            byte[] jsonByte = null;
            try
            {
                jsonByte = File.ReadAllBytes(tempPath);
                Debug.Log("Loaded Data from: " + tempPath.Replace("/", "\\"));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed To Load Data from: " + tempPath.Replace("/", "\\"));
                Debug.LogWarning("Error: " + e.Message);
            }
            string jsonData = Encoding.ASCII.GetString(jsonByte);
            Objectinfo loadedData = JsonUtility.FromJson<Objectinfo>(jsonData);
            //spawn/load objects
            objectsInWorld = loadedData.objectNames;
            transformsInWorld = loadedData.objectTransforms;
            orbsInWorld = loadedData.orbTransforms;
            for (int i = 0; i < orbsInWorld.Count; i++)
          {
              Instantiate(wisp, orbsInWorld[i], Quaternion.identity);
            }
            for (int x = 0; x < objectsInWorld.Count + 1; x++)
            {
                for (int y = 0; y < objectToPlace.Count; y++)
                {
                    if (objectToPlace[y].name == objectsInWorld[x])
                    {
                         tempObject = objectToPlace[y];
                        Debug.Log(tempObject.name);
                        break;
                    }
                    Instantiate(tempObject, transformsInWorld[x], Quaternion.identity);
                }
            }
        }


    }
    void deleteData(string dataFileName)
    {
        string tempPath = Path.Combine(Application.persistentDataPath, "data");
        tempPath = Path.Combine(tempPath, dataFileName + ".zombdata");

        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            Debug.LogWarning("Directory does not exist");
        }

        if (!File.Exists(tempPath))
        {
            Debug.Log("File does not exist");
        }

        try
        {
            File.Delete(tempPath);
            Debug.Log("Data deleted from: " + tempPath.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Delete Data: " + e.Message);
        }
    }

    public class Objectinfo
    {
        public List<Vector3> objectTransforms = new List<Vector3>();
        public List<string> objectNames = new List<string>();
        public List<Vector3> orbTransforms = new List<Vector3>();
    }

    IEnumerator timer()
    {
        Debug.Log("");
        yield return new WaitForSecondsRealtime(6f);
        bol = true;
    }

    IEnumerator waitForCam()
    {
        yield return new WaitForSecondsRealtime(3f);
        mainCam.enabled = true;
        wispyCam.enabled = false;

    }

}
