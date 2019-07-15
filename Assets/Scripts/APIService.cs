using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;


public class APIService : MonoBehaviour
{
    private Sensor1 sensor1;
    private Sensor2 sensor2;

    public static APIService Instance;

    private static HttpClient _client = new HttpClient();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public async Task<Sensor1> Server1RandNum()
    {
        string url = "HTTP://192.168.1.4";
        string responseString = await _client.GetStringAsync(url);
        Sensor1 i = JsonConvert.DeserializeObject<Sensor1>(responseString);
        return i;
    }

    public async Task<Sensor2> Server2RandNum()
    {
        string url = "HTTP://192.168.1.2";
        string responseString = await _client.GetStringAsync(url);
        Sensor2 i = JsonConvert.DeserializeObject<Sensor2>(responseString);
        return i;
    }
}
