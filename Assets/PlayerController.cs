using UnityEngine;

public class PlayerController : MonoBehaviour
{
	// Movement speed of the player
	public float Speed = 6f;
	// Time it takes for the player to smoothly turn
	public float TurnSmoothTime = 0.1f; 
	// Reference to the camera transform
	public Transform Camera; 

	// Reference to the CharacterController component
	private CharacterController characterController; 
	// Reference to the Animator component
	private Animator animator; 

	// Smooth velocity variable for turning (not used, has to be here because of function's signature)
	private float turnSmoothVelocity; 

	void Start()
	{
		// Get the CharacterController component
		characterController = GetComponent<CharacterController>(); 
		// Get the Animator component
		animator = GetComponent<Animator>(); 

		// Lock cursor to the center of the screen
		Cursor.lockState = CursorLockMode.Locked; 
	}

	void Update()
	{
		// Get horizontal input (-1, 0, or 1)
		float horizontal = Input.GetAxisRaw("Horizontal"); 
		// Get vertical input (-1, 0, or 1)
		float vertical = Input.GetAxisRaw("Vertical"); 

		// Create a normalized direction vector
		Vector3 direction = new Vector3(horizontal, 0, vertical).normalized; 

		// If the player is moving
		if (direction.magnitude >= 0.1f) 
		{
			// Calculate the target angle for the player to face
			float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.eulerAngles.y; 
			// Smoothly interpolate the current angle to the target angle
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); 
			// Set the player's rotation to the calculated angle
			transform.rotation = Quaternion.Euler(0f, angle, 0f); 

			// Calculate the movement direction based on the target angle
			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; 
			// Move the character controller in the calculated direction and speed
			characterController.Move(moveDir.normalized * Speed * Time.deltaTime);

			animator.SetBool("Running", true);
		} else
		{
			animator.SetBool("Running", false);
		}
	}

	private void OnFootstep()
	{
		// Event from animator, can be used to trigger footstep sounds or other related actions
	}
}
