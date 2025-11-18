using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//Attach this script to a GameObject to rotate around the target position.
public class RotateCamera : MonoBehaviour
{
    //Assign a GameObject in the Inspector to rotate around
    public GameObject target;
    public GameObject camera;
    public GameObject player;
    public GameObject[] parts;
    public Vector3 cameraOriginalPos;
    public Quaternion cameraOriginalRot;
    public Vector3 playerOriginalPos;
    public Quaternion playerOriginalRot;
    public Timer timer_script;
    public bool notRotating;

    void Start() {
        cameraOriginalPos = camera.transform.position;
        cameraOriginalRot = camera.transform.rotation;
        playerOriginalPos = player.transform.position;
        playerOriginalRot = player.transform.rotation;
        notRotating = false;
    }

    public void Restart(){
        //Change Music to Restart Intro Tune
        notRotating = false;
        timer_script.timeRemaining = timer_script.roundTime;
        timer_script.restartButton.gameObject.SetActive(false);
        //Need to ensure camera/player are square to ring when being reset
        camera.transform.position = cameraOriginalPos;
        camera.transform.rotation = cameraOriginalRot;
        player.transform.position = playerOriginalPos;
        player.transform.rotation = playerOriginalRot;
    }

    public void Rotate()
    {
        notRotating = false;
        StartCoroutine("rotateCamera");
    }

    public void Update(){
        
        if(notRotating){
            StopAllCoroutines();
            camera.transform.position = player.transform.position + new Vector3(0,10,-10);
            camera.transform.rotation = player.transform.rotation * Quaternion.Euler(17,-5,0);
            for(int i=0; i< 6; i++){
                
                var col = parts[i].GetComponent<Renderer>().material.color;
                col.a = .01f;
            } 
        }
    }
    

    IEnumerator rotateCamera()
    {
        float holder = 0;
        while (holder < 5)
        {
            holder += 1 * Time.deltaTime;
            camera.transform.RotateAround(target.transform.position, Vector3.up, 70 * Time.deltaTime);
            camera.transform.position += new Vector3(0, -3 * Time.deltaTime, 0);
            camera.transform.position +=  camera.transform.forward * 8 * Time.deltaTime;
            yield return 0;
        }
        camera.transform.position = new Vector3(0,15,-10);
        camera.transform.rotation = Quaternion.Euler(17, 0, 0);
        notRotating = true;
        yield return true;
    }
 
}
