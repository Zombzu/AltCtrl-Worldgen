using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{

    public float cycleSpeed;
    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.right, cycleSpeed * Time.deltaTime);
        transform.LookAt(Vector3.zero);
    }
}
