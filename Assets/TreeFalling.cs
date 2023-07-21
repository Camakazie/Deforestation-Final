using System.Collections;
using UnityEngine;

public class TreeFalling : MonoBehaviour
{
	[Header("Settings")]
	// The speed at which the tree falls
	public float fallSpeed = 2f;

	[Header("References")]
	// The material used for fading out the tree
	public Material FadeMaterial;

	// Flag indicating if the tree is currently falling
	private bool isFalling = false; 
	// Flag indicating if the tree has fallen down
	private bool isDown = false; 

	// The current alpha value for fading out
	private float currentAlpha = 1f; 
	// The speed at which the tree fades out
	private float fadeSpeed = 0.5f;

	void Update()
	{
		if (isFalling)
		{
			// Calculate the rotation angle based on the fall speed and delta time (independent of framerate)
			float angle = fallSpeed * Time.deltaTime; 
			// Rotate the tree around (make it fall)
			transform.Rotate(Vector3.right, angle); 
			// Increase the fall speed over time
			fallSpeed *= 1.005f; 
			// Clamp the fall speed to a maximum value of 100
			fallSpeed = Mathf.Clamp(fallSpeed, 0, 100); 

			// Checking if tree has fallen either to 90 degrees in x axis,
			// or it's z axis value changed, because Unity calculates rotation vector
			// differently under the hood, so sometimes it might not reach 90 in x axis, but
			// will be 180 in z axis (both means, that tree has fallen)
			if (Mathf.Abs(transform.localRotation.eulerAngles.x) >= 90 || 
				Mathf.Abs(transform.localRotation.eulerAngles.z) >= 1)
			{
				isDown = true;
				isFalling = false;

				// Start the coroutine for fading out the tree
				StartCoroutine(FadeOutCoroutine()); 
			}
		}
	}

	private IEnumerator FadeOutCoroutine()
	{
		// Get all renderers attached to the tree
		var renderers = GetComponentsInChildren<Renderer>(); 

		while (renderers[0].material.color.a > 0)
		{
			foreach (var renderer in renderers)
			{
				// Decrease the current alpha value over time
				currentAlpha -= fadeSpeed * Time.deltaTime; 

				// Clamp the current alpha value between 0 and 1
				currentAlpha = Mathf.Clamp01(currentAlpha); 

				// Create a new material with the faded alpha value
				Material fadedMaterial = new Material(FadeMaterial)
				{
					color = new Color(
						renderer.sharedMaterial.color.r,
						renderer.sharedMaterial.color.g,
						renderer.sharedMaterial.color.b,
						currentAlpha
					)
				};

				// Assign the faded material to the renderer
				renderer.material = fadedMaterial; 
			}

			// Wait for the next frame
			yield return null;
		}

		// Destroy the tree game object when it's fully faded
		Destroy(gameObject); 
	}

	public bool CutTree()
	{
		if (!isDown && !isFalling)
		{
			// Set the flag to indicate that the tree is falling
			isFalling = true; 
			// Return true to indicate that the tree was successfully started to cut
			return true; 
		}

		// Return false if the tree is already down or currently falling
		return false; 
	}
}
