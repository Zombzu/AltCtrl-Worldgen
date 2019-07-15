using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public Transform sunPos;
    private void FixedUpdate()
    {
        transform.LookAt(new Vector3(sunPos.position.x,0f,sunPos.position.z));
    }
}
