using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PController : MonoBehaviour
{
    [SerializeField, Range(0f, 1000f)]
	float maxSpeed = 10f;
    [SerializeField, Range(0f, 1000f)]
	float maxAcceleration = 10f;
    [SerializeField]
	Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);
    [SerializeField, Range(0f, 1f)]
	float bounciness = 0.5f;
	public RotateCamera rotate_script;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Debug.Log(rotate_script.notRotating);
        if(rotate_script.notRotating){
			Vector2 playerInput;
			
			playerInput.x = 0f;
			playerInput.y = 0f;
			playerInput.x = Input.GetAxis("Horizontal");
			playerInput.y = Input.GetAxis("Vertical");
			Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y);
			playerInput = Vector2.ClampMagnitude(playerInput, 1f);
			Vector3 acceleration = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
			velocity += acceleration * Time.deltaTime;
			Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
			float maxSpeedChange = maxAcceleration * Time.deltaTime;
			velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
			velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
			Vector3 displacement = velocity * Time.deltaTime;
			Vector3 newPosition = transform.localPosition + displacement;
			if (newPosition.x < allowedArea.xMin) {
				newPosition.x = allowedArea.xMin;
				velocity.x = -velocity.x * bounciness;
			}
			else if (newPosition.x > allowedArea.xMax) {
				newPosition.x = allowedArea.xMax;
				velocity.x = -velocity.x * bounciness;
			}
			if (newPosition.z < allowedArea.yMin) {
				newPosition.z = allowedArea.yMin;
				velocity.z = -velocity.z * bounciness;
			}
			else if (newPosition.z > allowedArea.yMax) {
				newPosition.z = allowedArea.yMax;
				velocity.z = -velocity.z * bounciness;
			}
			transform.localPosition = newPosition;
		}
    }
}
