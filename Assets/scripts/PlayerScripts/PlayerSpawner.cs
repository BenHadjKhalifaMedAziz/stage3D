using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // Assign the player prefab in the Inspector
    private GameObject r1; // We'll find this in the scene
    private bool playerInstantiated = false; // To make sure player is instantiated only once
    public GameObject generator;
    void Update()
    {

        if (generator != null)
        {
            // Access the FinalGridScript from the generator and check if Allset is true
            FinalGridScript finalGridScript = generator.GetComponent<FinalGridScript>();

            if (finalGridScript != null && finalGridScript.Allset && !playerInstantiated)
            {
                InstantiatePlayer();
            }
        }
    }

    void InstantiatePlayer()
    {
        // Find the r1 object in the scene
        r1 = GameObject.Find("r1");

        if (r1 != null)
        {
            // Find the "floor" child under r1
            Transform floorTransform = r1.transform.Find("Floor");

            if (floorTransform != null)
            {
                // Get the position of the floor and adjust Y by +1
                Vector3 spawnPosition = floorTransform.position + new Vector3(0f, 1f, 0f);

                // Instantiate the player prefab at the adjusted position
                Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

                playerInstantiated = true; // Ensure the player is instantiated only once
            }
            else
            {
                Debug.LogError("Floor not found in r1");
            }
        }
        else
        {
            Debug.LogError("r1 not found in the scene");
        }
    }
}
