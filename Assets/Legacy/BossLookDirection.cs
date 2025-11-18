using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLookDirection : MonoBehaviour
{

    //public float speed;
    private CapsuleCollider enemyCap;
    public Animator bossAnimator;
    public Transform target;
    private float count;
    private bool isBlockPlaying;
    private bool isRightCrossPlaying;
    

    // Start is called before the first frame update
    void Start()
    {
        enemyCap = GetComponent<CapsuleCollider>();
        //bossAnimator = GetComponent<Animator>();
        //RuntimeAnimatorController animatorController = animator.runtimeAnimatorController;
        //player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(bossAnimator.GetBool("Block"));
        isBlockPlaying = bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Block");
        isRightCrossPlaying = bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Right_cross");
        if (count%100 == 0 && isBlockPlaying == false && isRightCrossPlaying == false)
        {
            bossAnimator.SetBool("Block", true);
        }
        else if (count%50 == 0 && isBlockPlaying == false && isRightCrossPlaying == false)
        {
            bossAnimator.SetBool("Right_cross", true);
            //Debug.Log("working");
        }
        else
        {
            Debug.Log(count + " ");
        }  
        if (bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("knockout_V1") ||
            bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("Knockouts_Countdown_V1"))
        {
            //enemyCap.direction = 0;
            enemyCap.center = new Vector3(0, 2, 0);
        }else
        {
            //enemyCap.direction = 1;
            enemyCap.center = new Vector3(0, 1, 0);
        }

            if (target != null)
        {
            transform.LookAt(target);
        }
        count ++;
        /**Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        enemyRb.AddForce(lookDirection);
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }**/
    }
    /**function punchEnemy(){
      if(target.distanceFromEnemy < 10){
        enemy.punch();
      }
    }
    function blockEnemy(){
      if(target.distanceFromEnemy < 10){
        enemy.block();
      }
    }

    /**function moveEnemy(){
      let direction = Math.random();
      if(direction < 0.25){
        enemy.moveLeft();
      } else if (direction < 0.5){
        enemy.moveRight();
      } else if (direction < 0.75){
        enemy.moveUp();
      } else {
        enemy.moveDown();
      }
    }**/
}
