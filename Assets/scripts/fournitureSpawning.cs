using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fournitureSpawning : MonoBehaviour
{
    // Reference to the static dictionary from fournitureGrid
    public Dictionary<GameObject, (string roomType, int nbrDoors, Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)>)> RoomsList;

    //[SerializeField] private List<GameObject> wallFrames; // Wall frame prefabs
    [Range(0f, 1f)] public float spawnProbability = 0.05f; // Probability of instantiating a prefab (default 5%)

    public List<WallFrames> wallFrames = new List<WallFrames>();
    public List<WallFournitures> wallFournitures = new List<WallFournitures>(); 
    public List<CenterFournitures> centerFournitures = new List<CenterFournitures>();

    //[SerializeField] private List<GameObject> wallFournitures; // Wall frame prefabs
    [Range(0f, 1f)] public float spawnProbability1 = 0.05f; // Probability of instantiating a prefab (default 5%)

    [Range(0f, 1f)] public float spawnProbability2 = 0.05f; // Probability of instantiating a prefab (default 5%)



    void Start()
    {
        // Reference the static dictionary
        RoomsList = fournitureGrid.RoomsList;

        // Instantiate wall frames based on the dictionary
        InstantiateWallFrames();
        InstantiateLay1();
        InstantiateLay2();

    }

    public void InstantiateWallFrames()
    {
        // Iterate through each room in the RoomsList
        foreach (var room in RoomsList)
        {
          //  Debug.Log("tttttttttttttttttttttttttttt");
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
                    // Choose a random wall frame prefab
                    var randomWallFrame = wallFrames[Random.Range(0, wallFrames.Count)];


                    // Check neighboring cells to see if any are tagged "wall"
                    bool hasNeighboringWall = CheckForNeighboringWalls(cellsDictionary, cellKey, randomWallFrame.width);

                    // Only proceed with instantiation if the neighboring cells are also tagged as "wall"
                    if (hasNeighboringWall)
                    {
                        // Randomly check if we should instantiate the wall frame based on the probability
                        if (Random.value <= spawnProbability) // Random.value returns a float between 0.0 and 1.0
                        {
                        

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
                            Instantiate(randomWallFrame.prefab, spawnPosition, rotation, childObject.transform);

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
                            ReserveNeighboringCells(cellsDictionary, cellKey, randomWallFrame.width);

                          //  CheckForCornerOrNextDoor(cellsDictionary, cellKey);
                        }
                    }
                }
            }
        }
    }



    void ReserveNeighboringCells(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey, int width)
    {
        //Debug.Log("Reserving neighboring cells...");

        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;

        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        //var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        var neighborOffsets = GenerateNeighborOffsets(width);

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

    bool CheckForNeighboringWalls(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey, int width)
    {
        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;




        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        //var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        var neighborOffsets = GenerateNeighborOffsets(width);

        foreach (Vector3 offset in neighborOffsets)
        {
            Debug.Log($"Vector3: {offset}");
        }

        // Iterate through each possible neighboring position and check if any of them are tagged "wall"
        foreach (var offset in neighborOffsets)
        {
            Vector3 neighborPosition = currentPosition + offset;

            // Check if the neighboring cell exists in the dictionary (matching position) and is tagged "wall"
            foreach (var entry in cellsDictionary)
            {
                var neighborData = entry.Value;
                if (neighborData.position == neighborPosition && (neighborData.tag == "corner" || neighborData.tag == "nextDoor") && neighborData.layer == 1 )
                {
                    return false; // A neighboring wall was found
                }
                if (neighborData.position == neighborPosition && neighborData.reserved == true && neighborData.layer == 1)
                {
                    return false; // A neighboring wall was found
                }

            }
        }

        return true; // No neighboring walls found
                       
    }

    bool CheckForNeighboringWalls1(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey, int width)
    {
        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;




        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        //var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        var neighborOffsets = GenerateNeighborOffsets(width);

        foreach (Vector3 offset in neighborOffsets)
        {
            Debug.Log($"Vector3: {offset}");
        }

        // Iterate through each possible neighboring position and check if any of them are tagged "wall"
        foreach (var offset in neighborOffsets)
        {
            Vector3 neighborPosition = currentPosition + offset;

            // Check if the neighboring cell exists in the dictionary (matching position) and is tagged "wall"
            foreach (var entry in cellsDictionary)
            {
                var neighborData = entry.Value;
                if (neighborData.position == neighborPosition && (neighborData.tag == "corner" || neighborData.tag == "nextDoor") && neighborData.layer == 2)
                {
                    return false; // A neighboring wall was found
                }
                if (neighborData.position == neighborPosition && neighborData.reserved==true && neighborData.layer == 2)
                {
                    return false; // A neighboring wall was found
                }


            }
        }

        return true; // No neighboring walls found

    }

    bool CheckForNeighboringWalls2(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey, int width)
    {
        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;




        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        //var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        var neighborOffsets = GenerateNeighborOffsets(width);

        foreach (Vector3 offset in neighborOffsets)
        {
            Debug.Log($"Vector3: {offset}");
        }

        // Iterate through each possible neighboring position and check if any of them are tagged "wall"
        foreach (var offset in neighborOffsets)
        {
            Vector3 neighborPosition = currentPosition + offset;

            // Check if the neighboring cell exists in the dictionary (matching position) and is tagged "wall"
            foreach (var entry in cellsDictionary)
            {
                var neighborData = entry.Value;
                if (neighborData.position == neighborPosition && (neighborData.tag == "corner" || neighborData.tag == "nextDoor") && neighborData.layer == 2)
                {
                    return false; // A neighboring wall was found
                }
                if (neighborData.position == neighborPosition && neighborData.reserved == true && neighborData.layer >3)
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
            if (cellData.tag == "corner" || cellData.wallData.Contains("C"))
                {
                    Debug.Log($"Corner tag found in cell {cellKey} at position {cellData.position}");
                }
            else if (cellData.tag == "nextDoor")
            {
                Debug.Log($"NextDoor tag found in cell {cellKey} at position {cellData.position}");
            }

    }


    public void InstantiateLay1()
    {
        // Iterate through each room in the RoomsList
        foreach (var room in RoomsList)
        {
            //Debug.Log("tttttttttttttttttttttttttttt");
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
                var doorData = cellData.doorData;
                var wallData = cellData.wallData;
                var layer = cellData.layer;
                var reserved = cellData.reserved; // Get the reserved status
                var childObject = cellData.childObject; // Get the GameObject associated with the cell

                // Check if the cell is tagged "wall" and is in layer 1 and not reserved
                if (tag == "Grid" && layer == 2 && !reserved && !doorData && tag != "nextDoor" && !wallData.Contains("C"))
                {
                    // Choose a random wall frame prefab
                    var randomWallFournitures = wallFournitures[Random.Range(0, wallFournitures.Count)];
                    // Check neighboring cells to see if any are tagged "wall"
                    bool hasNeighboringWall = CheckForNeighboringWalls1(cellsDictionary, cellKey, randomWallFournitures.width);

                    // Only proceed with instantiation if the neighboring cells are also tagged as "wall"
                    if (hasNeighboringWall)
                    {
                        // Randomly check if we should instantiate the wall frame based on the probability
                        if (Random.value <= spawnProbability1) // Random.value returns a float between 0.0 and 1.0
                        {
                            

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
                            Instantiate(randomWallFournitures.prefab, spawnPosition, rotation, childObject.transform);

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
                            ReserveNeighboringCells1(cellsDictionary, cellKey, randomWallFournitures.width);

                          //  CheckForCornerOrNextDoor(cellsDictionary, cellKey);
                        }
                    }
                }
            }
        }
    }

    void ReserveNeighboringCells1(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey, int width)
    {
        //Debug.Log("Reserving neighboring cells...");

        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;

        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        //var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        var neighborOffsets = GenerateNeighborOffsets(width);

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
                if (neighborData.position == neighborPosition && neighborData.tag == "Grid" && (neighborData.layer == 2 || neighborData.layer == 3 ) && !neighborData.reserved)
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


    // Helper function to generate neighboring offsets based on width
    List<Vector3> GenerateNeighborOffsets(int width)
    {
        List<Vector3> offsets = new List<Vector3>();

        // Generate offsets from 1 to width in both x and z directions
        for (int i = 0; i <= width; i++)
        {
            offsets.Add(new Vector3(i, 0, 0));  // Positive X
            offsets.Add(new Vector3(-i, 0, 0)); // Negative X
            offsets.Add(new Vector3(0, 0, i));  // Positive Z
            offsets.Add(new Vector3(0, 0, -i)); // Negative Z
        }

        return offsets;
    }

    // Helper function to generate neighboring offsets based on width for props in room center 
    List<Vector3> GenerateNeighborOffsets2(int width)
    {
        List<Vector3> offsets = new List<Vector3>();

        // Generate offsets from 1 to width in both x and z directions
        for (int i = 0; i <= width; i++)
        {
            offsets.Add(new Vector3(i, 0, 0)); offsets.Add(new Vector3(i, 0, i)); offsets.Add(new Vector3(i, 0, -i));  // Positive X
            offsets.Add(new Vector3(-i, 0, 0)); offsets.Add(new Vector3(-i, 0, i)); offsets.Add(new Vector3(-i, 0, -i)); // Negative X
            offsets.Add(new Vector3(0, 0, i));  // Positive Z
            offsets.Add(new Vector3(0, 0, -i)); // Negative Z
        }

        return offsets;

    }

    public void InstantiateLay2()
    {
        // Iterate through each room in the RoomsList
        foreach (var room in RoomsList)
        {
            //Debug.Log("tttttttttttttttttttttttttttt");
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
                var doorData = cellData.doorData;
                var wallData = cellData.wallData;
                var layer = cellData.layer;
                var reserved = cellData.reserved; // Get the reserved status
                var childObject = cellData.childObject; // Get the GameObject associated with the cell

                // Check if the cell is tagged "wall" and is in layer 1 and not reserved
                if (tag == "Grid" && layer >=3 && !reserved && !doorData && tag != "nextDoor" && !wallData.Contains("C"))
                {
                    // Choose a random wall frame prefab
                    var randomWallFournitures = centerFournitures[Random.Range(0, centerFournitures.Count)];
                    // Check neighboring cells to see if any are tagged "wall"
                    bool hasNeighboringWall = CheckForNeighboringWalls2(cellsDictionary, cellKey, randomWallFournitures.width);

                    // Only proceed with instantiation if the neighboring cells are also tagged as "wall"
                    if (hasNeighboringWall)
                    {
                        // Randomly check if we should instantiate the wall frame based on the probability
                        if (Random.value <= spawnProbability2) // Random.value returns a float between 0.0 and 1.0
                        {


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
                            Instantiate(randomWallFournitures.prefab, spawnPosition, rotation, childObject.transform);

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
                            ReserveNeighboringCells2(cellsDictionary, cellKey, randomWallFournitures.width);

                            //  CheckForCornerOrNextDoor(cellsDictionary, cellKey);
                        }
                    }
                }
            }
        }
    }

    void ReserveNeighboringCells2(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer, bool reserved)> cellsDictionary, int currentKey, int width)
    {
        //Debug.Log("Reserving neighboring cells...");

        // Get the position of the current cell
        var currentCellData = cellsDictionary[currentKey];
        Vector3 currentPosition = currentCellData.position;

        // Define possible neighboring offsets in the x and z direction (+1, -1 for both)
        //var neighborOffsets = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };
        var neighborOffsets = GenerateNeighborOffsets2(width);

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
                if (neighborData.position == neighborPosition && neighborData.tag == "Grid" && (neighborData.layer > 3) && !neighborData.reserved)
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

}
[System.Serializable]
public class WallFrames
{
    public GameObject prefab;
    public int width;
}


[System.Serializable]
public class WallFournitures
{
    public GameObject prefab;
    public int width;
}

[System.Serializable]
public class CenterFournitures
{
    public GameObject prefab;
    public int width;
}