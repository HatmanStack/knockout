using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatGPTController : MonoBehaviour
{
    public GameObject player;
    
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        player.transform.position = player.transform.position + new Vector3(horizontalInput, 0, verticalInput);

        if (Input.GetButtonDown("Fire1"))
        {
            
        }
    }
}