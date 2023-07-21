using UnityEngine;

public class CrosshairShooting : MonoBehaviour
{
	// Reference to the camera
	public Camera Camera;
	// Layer mask for the target objects
	public LayerMask TargetLayer;
	// Maximum distance at which objects can be interacted with
	public float CutDistance = 7f;

	// Reference to the RectTransform component of the crosshair UI
	private RectTransform crosshairUI;

	private void Start()
	{
		// Get the RectTransform component attached to the same GameObject as this script
		crosshairUI = GetComponent<RectTransform>();
	}

	private void Update()
	{
		// Check if the left mouse button is pressed
		if (Input.GetMouseButtonDown(0))
		{
			// Call the ShootRaycast method
			ShootRaycast();
		}
	}

	private void ShootRaycast()
	{
		// Create a ray from the camera to the position of the crosshair UI
		Ray ray = Camera.ScreenPointToRay(crosshairUI.position);

		// Store information about the raycast hit
		RaycastHit hit;

		// Cast the ray and check if it hits an object within the specified distance and on the specified layer
		if (Physics.Raycast(ray, out hit, CutDistance, TargetLayer))
		{
			// Get the hit object's GameObject
			GameObject target = hit.collider.gameObject;

			// Call the Deforest method in the TerrainController instance and pass the hit tree
			TerrainController.Instance.Deforest(target);
		}
	}
}

