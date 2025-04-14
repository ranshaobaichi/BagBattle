using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public uint cam_height = 10;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 cam_pos = player.transform.position;
            cam_pos.z = -cam_height;
            transform.position = cam_pos;
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }
}
