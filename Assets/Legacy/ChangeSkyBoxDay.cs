using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChangeSkyBoxDay : MonoBehaviour
{
    public Material mat1;
    public Material mat2;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeDay()
    {
        if(RenderSettings.skybox == mat1)
        {
            RenderSettings.skybox = mat2;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight; 
        } else
        {
            RenderSettings.skybox = mat1;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        }

    }
}