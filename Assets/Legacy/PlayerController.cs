using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    public float playerSpeed = 2.0f;
    
    
    private CharacterController m_Controller;
    private Animator m_Animator;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;

    private void Start()
    {
        m_Controller = gameObject.GetComponent<CharacterController>();
        m_Animator = gameObject.GetComponentInChildren<Animator>();
        
    }

    void Update()
    {

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("space"))
        {
            m_Animator.SetTrigger("Block");
        }
        if (Input.GetButtonDown("e"))
        {
            m_Animator.SetTrigger("right_dodge");
        }
        if (Input.GetButtonDown("q"))
        {
            m_Animator.SetTrigger("left_dodge");
        }
        if (Input.GetButtonDown("u"))
        {
            m_Animator.SetTrigger("Left_Jab");
        }
        if (Input.GetButtonDown("j"))
        {
            m_Animator.SetTrigger("left_uppercut");
        }
        if (Input.GetButtonDown("i"))
        {
            m_Animator.SetTrigger("left_hook");
        }
        if (Input.GetButtonDown("o"))
        {
            m_Animator.SetTrigger("Right_cross");
        }
        if (Input.GetButtonDown("l"))
        {
            m_Animator.SetTrigger("Right_uppercut");
        }
        if (Input.GetButtonDown("k"))
        {
            m_Animator.SetTrigger("Right_cross");
        }

        playerVelocity.y += gravityValue * Time.deltaTime;

        m_Controller.Move(playerVelocity * Time.deltaTime);
    }
}
