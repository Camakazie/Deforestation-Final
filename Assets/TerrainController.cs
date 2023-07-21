using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using TMPro;

public class TerrainController : MonoBehaviour
{
	// Reference to the instance of TerrainController (singleton pattern)
	public static TerrainController Instance; 

	[Header("Prefabs")]
	// Array of tree prefabs
	[SerializeField] private GameObject[] TreePrefabs; 
	// Array of rock prefabs
	[SerializeField] private GameObject[] RockPrefabs; 

	[Header("Coordinates")]
	// Maximum X coordinate for object placement
	public float maxX; 
	// Minimum X coordinate for object placement
	public float minX; 
	// Maximum Z coordinate for object placement
	public float maxZ; 
	// Minimum Z coordinate for object placement
	public float minZ; 
	// Y coordinate of the terrain plane
	public float planeY; 

	[Header("Settings")]
	// Number of rocks to generate
	public int RockCount = 50; 
	// Multiplier for the number of trees to generate
	public int TreeMultiplier = 1; 

	// Maximum number of attempts to find a valid position for an object
	public int PositionLoopTimes = 100; 
	// Minimum distance between objects to avoid collision
	public float CollisionDistance = 1.5f; 

	// Maximum year for simulation
	public int MaxYear = 2023; 
	// Minimum year for simulation
	public int MinYear = 1920; 

	[Header("References")]
	// Parent transform for tree objects
	[SerializeField] private Transform TreesParent; 
	// Parent transform for rock objects
	[SerializeField] private Transform RocksParent; 
	// Text component to display the current year
	[SerializeField] private TMP_Text YearLabel; 

	// List to store references to spawned trees
	private List<GameObject> spawnedTrees = new(); 
	// List to store references to all spawned objects
	private List<GameObject> spawnedObjects = new(); 
	// Current year in the simulation
	private int currentYear; 

	// Flag to indicate if the automated simulation is currently running
	private bool isSimulating = false; 

	void Start()
	{
		// Set the instance reference to this TerrainController object
		Instance = this; 

		// Generate the initial terrain based on the year difference
		GenerateTerrain(MaxYear - MinYear); 

		// Set the current year to the minimum year
		currentYear = MinYear; 
		// Update the year label text
		YearLabel.text = $"Year {currentYear}"; 
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.K) && !isSimulating)
		{
			isSimulating = true;
			// Start the automated simulation when the 'K' key is pressed and
			// simulation is not already running
			SimulateCuttingTrees(); 
		}
	}

	public void GenerateTerrain(int treeCount)
	{
		// Adjust the tree count based on the multiplier
		treeCount *= TreeMultiplier; 
		// Generate tree objects
		int treesSpawned = GenerateObjects(TreePrefabs, treeCount, TreesParent); 

		if (treesSpawned < treeCount)
		{
			Debug.LogError($"Only {treesSpawned} trees were spawned out of {treeCount}. Change tree multiplier value, year difference, or make a bigger space for objects to spawn!");
		}

		// Make a seperate list just for spawned trees
		spawnedTrees.AddRange(spawnedObjects); 

		// Generate rock objects
		GenerateObjects(RockPrefabs, RockCount, RocksParent); 
	}

	private int GenerateObjects(GameObject[] objectArray, int count, Transform parent)
	{
		int objectsSpawned = 0;

		for (int i = 0; i < count; i++)
		{
			// Randomly select a prefab from the array
			int prefabIndex = Random.Range(0, objectArray.Length); 

			// Find a valid position for the object
			bool success = FindValidPosition(out Vector3 position);

			if (!success)
			{
				// If a valid position couldn't be found, skip this iteration
				continue; 
			}

			// Instantiate the object
			var spawnedObject = Instantiate(objectArray[prefabIndex], parent); 
			// Set the object's position
			spawnedObject.transform.position = new Vector3(position.x, planeY, position.z); 
			// Set a random rotation
			spawnedObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f); 

			// Add the spawned object to the list
			spawnedObjects.Add(spawnedObject); 
			objectsSpawned++;
		}

		return objectsSpawned;
	}

	private bool FindValidPosition(out Vector3 position)
	{
		// Random X coordinate within the specified range
		float randomX = Random.Range(minX, maxX); 
		// Random Z coordinate within the specified range
		float randomZ = Random.Range(minZ, maxZ); 
		int loopCount = 0;

		while (!IsPositionValid(new Vector3(randomX, planeY, randomZ)) && loopCount <= PositionLoopTimes)
		{
			// Retry with a new random X coordinate
			randomX = Random.Range(minX, maxX); 
			// Retry with a new random Z coordinate
			randomZ = Random.Range(minZ, maxZ); 
			loopCount++;
		}

		if (loopCount > PositionLoopTimes)
		{
			position = Vector3.zero;
			// If a valid position couldn't be found within the maximum number of attempts, return false
			return false; 
		}

		position = new Vector3(randomX, planeY, randomZ);
		// Valid position found
		return true; 
	}

	private bool IsPositionValid(Vector3 position)
	{
		foreach (GameObject spawnedObject in spawnedObjects)
		{
			if (Vector3.Distance(spawnedObject.transform.position, position) < CollisionDistance)
			{
				// If the position is too close to any existing object, return false
				return false; 
			}
		}

		// Position is valid
		return true; 
	}

	public void Deforest(GameObject originalTree)
	{
		// Call a method on the original tree object to simulate cutting
		bool success = originalTree.GetComponent<TreeFalling>().CutTree(); 

		if (!success)
		{
			// If specified tree cannot be cut, don't do anything
			return; 
		}

		// Remove the original tree from the list
		spawnedTrees.Remove(originalTree); 
		// Increment the year
		AddYear(); 

		for (int i = 0; i < TreeMultiplier - 1; i++)
		{
			// Randomly select another tree from the list
			int randomTreeIndex = Random.Range(0, spawnedTrees.Count); 
			var tree = spawnedTrees[randomTreeIndex];

			spawnedTrees.Remove(tree);
			// Cut the selected tree
			tree.GetComponent<TreeFalling>().CutTree(); 
		}
	}

	private void AddYear()
	{
		// Current year
		currentYear++; 
		// Update the year label text
		YearLabel.text = $"Year {currentYear}"; 
	}

	private async void SimulateCuttingTrees()
	{
		Debug.Log("Timbeeeer! (Simulation started!)");

		// Destroy the crosshair shooting component
		Destroy(FindObjectOfType<CrosshairShooting>()); 

		// Counter for calculating how many years have passed
		int treeCounter = 0;
		// using spawnedTrees.ToList() to make a copy of the
		// spawned trees list, because you can't mutate a list
		// in a foreach loop directly
		foreach (var tree in spawnedTrees.ToList())
		{
			// Remove the tree from the list
			spawnedTrees.Remove(tree); 
			// Cut the tree
			tree.GetComponent<TreeFalling>().CutTree(); 
			treeCounter++;

			if (treeCounter % TreeMultiplier == 0)
			{
				treeCounter = 0;
				// Increment the year when the tree counter reaches the TreeMultiplier
				AddYear(); 
			}

			// Delay for a random amount of time
			await Task.Delay(Random.Range(100, 500));
		}

		Debug.Log("Simulation finished.");
	}
}