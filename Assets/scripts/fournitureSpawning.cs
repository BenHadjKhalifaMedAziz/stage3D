using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fournitureSpawning : MonoBehaviour
{
    // Reference to the static dictionary from fournitureGrid
    public Dictionary<GameObject, (string roomType, int nbrDoors, Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)>)> RoomsList;

    [SerializeField] private List<GameObject> wallFrames; // Wall frame prefabs
    [Range(0f, 1f)] public float spawnProbability = 0.05f; // Probability of instantiating a prefab (default 5%)

    void Start()
    {
        // Reference the static dictionary
        RoomsList = fournitureGrid.RoomsList;

        // Instantiate wall frames based on the dictionary
      //  InstantiateWallFrames();
    }

    public void InstantiateWallFrames()
    {
        // Iterate through each room in the RoomsList
        foreach (var room in RoomsList)
        {
            var roomData = room.Value; // Unpack room data
            var cellsDictionary = roomData.Item3; // Access the inner dictionary (cells)

            // Create a copy of the dictionary keys to avoid modification during iteration
            var keys = new List<int>(cellsDictionary.Keys);

            // Iterate through each cell in the inner dictionary (cells of the room)
            foreach (var cellKey in keys)
            {
                var cellData = cellsDictionary[cellKey]; // Unpack cell data
                var position = cellData.position;
                var tag = cellData.tag;
                var wallData = cellData.wallData;
                var layer = cellData.layer;
                var reserved = cellData.reserved; // Get the reserved status
                var childObject = cellData.childObject; // Get the GameObject associated with the cell

                // Check if the cell is tagged "wall" and is in layer 1 and not reserved
                if (tag == "wall" && layer == 1 && !reserved)
                {
                    // Check neighboring cells to see if any are tagged "wall"
                    bool hasNeighboringWall = CheckForNeighboringWalls(cellsDictionary, cellKey);

                    // Only proceed with instantiation if the neighboring cells are also tagged as "wall"
                    if (hasNeighboringWall)
                    {
                        // Randomly check if we should instantiate the wall frame based on the probability
                        if (Random.value <= spawnProbability) // Random.value returns a float between 0.0 and 1.0
                        {
                            // Choose a random wall frame prefab
                            var randomWallFrame = wallFrames[Random.Range(0, wallFrames.Count)];

                            // Determine rotation based on wallData
                            Quaternion rotation = Quaternion.identity; // Default rotation
                            switch (wallData)
                            {
                                case "L": // Left
                                    rotation = Quaternion.Euler(0, 90, 0);
                                    break;
                                case "R": // Right
                                    rotation = Quaternion.Euler(0, 270, 0);
                                    break;
                                case "B": // Back
                                    rotation = Quaternion.Euler(0, 0, 0);
                                    break;
                                case "F": // Front
                                    rotation = Quaternion.Euler(0, 180, 0);
                                    break;
                            }

                            // Instantiate the wall frame at the cell's position +2 in Y
                            var spawnPosition = position + Vector3.up * 2;
                            Instantiate(randomWallFrame, spawnPosition, rotation, childObject.transform);

                            if (childObject != null)
                            {
                                var detailsScript = childObject.GetComponent<details>();
                                if (detailsScript != null)
                                {
                                    detailsScript.reserved = true; // Set the reserved flag to true
                                    detailsScript.frameON = true; // Set the frameOn flag to true
                                }
                            }

                            // Mark the cell as reserved
                            cellsDictionary[cellKey] = (cellData.childObject, cellData.position, cellData.tag, cellData.doorData, cellData.wallData, cellData.layer, true);

                            // Reserve neighboring cells in range +1/-1 in all directions
                            ReserveNeighboringCells(cellsDictionary, cellKey);

                            CheckForCornerOrNextDoor(cellsDictionary, cellKey);
                        }
                    }
                }
            }
        }
    }


    void ReserveNeighboringCells(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey)
    {
        //Debug.Log("Reserving neighboring cells...");

        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;

        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };

        // Create a list to store keys of cells that need to be updated
        List<int> cellsToUpdate = new List<int>();

        // Iterate through each possible neighboring position
        foreach (var offset in neighborOffsets)
        {
            Vector3 neighborPosition = currentPosition + offset;

            // Check if the neighboring cell exists in the dictionary (matching position)
            foreach (var entry in cellsDictionary)
            {
                var neighborData = entry.Value;
                if (neighborData.position == neighborPosition && neighborData.tag == "wall" && neighborData.layer == 1 && !neighborData.reserved)
                {
                   // Debug.Log($"Neighbor found at {neighborPosition}, preparing to reserve.");

                    // Access the details script of the neighboring cell's GameObject and set reserved to true
                    if (neighborData.childObject != null)
                    {
                        var detailsScript = neighborData.childObject.GetComponent<details>();
                        if (detailsScript != null)
                        {
                            detailsScript.reserved = true; // Set the reserved flag to true
                        }
                    }

                    // Add the current key to the update list (to be updated after iteration)
                    cellsToUpdate.Add(entry.Key);
                }
            }
        }

        // After iterating through all neighbors, update the dictionary
        foreach (var key in cellsToUpdate)
        {
            var neighborData = cellsDictionary[key];
            cellsDictionary[key] = (neighborData.childObject, neighborData.position, neighborData.tag, neighborData.doorData, neighborData.wallData, neighborData.layer, true);
        }
    }

    bool CheckForNeighboringWalls(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey)
    {
        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;

        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };

        // Iterate through each possible neighboring position and check if any of them are tagged "wall"
        foreach (var offset in neighborOffsets)
        {
            Vector3 neighborPosition = currentPosition + offset;

            // Check if the neighboring cell exists in the dictionary (matching position) and is tagged "wall"
            foreach (var entry in cellsDictionary)
            {
                var neighborData = entry.Value;
                if (neighborData.position == neighborPosition && (neighborData.tag == "corner" || neighborData.tag == "nextDoor") && neighborData.layer == 1)
                {
                    return false; // A neighboring wall was found
                }
               
            }
        }

        return true; // No neighboring walls found

    }

    // New function to check for children with the tags "corner" or "nextDoor" and log them
    private void CheckForCornerOrNextDoor(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int cellKey)
    {
        var cellData = cellsDictionary[cellKey];


        // Check if childObject is not null
      //  Debug.Log($"tag cellkey  {cellKey} at position {cellData.tag}");
        // Check if childData's tag is "corner" or "nextDoor"
        if (cellData.tag == "corner")
            {
                Debug.Log($"Corner tag found in cell {cellKey} at position {cellData.position}");
            }
            else if (cellData.tag == "nextDoor")
            {
                Debug.Log($"NextDoor tag found in cell {cellKey} at position {cellData.position}");
            }
          
        
    }

}
