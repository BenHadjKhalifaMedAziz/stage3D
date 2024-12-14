using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{ 

    [Header("Rooms Dimentions ")]
    public int roomMinWidth = 5;
    public int roomMaxWidth = 8;
    public int roomMinHeight = 5;
    public int roomMaxHeight = 8;

    [Header("Number of Main ROoms ")]
    public int numberOfRooms = 8;
    private int bossRoomParent;

    private GameObject roomPrefab;
    private GameObject outlinePrefab;
    private Dictionary<Vector3, CellInfo> gridDictionary = new Dictionary<Vector3, CellInfo>();


    private Square gridOutlineChanger;
    private WallGenerator wallGenerator;
    private PrefabManager prefabManager;
    private FinalGridScript finalGridScript;
    private fournitureGrid fournitureGridScript;
    private fournitureSpawning fournitureSpawningScript;

    // Getters and Setters
    public GameObject RoomPrefab { get { return roomPrefab; } set { roomPrefab = value; } }
    public GameObject OutlinePrefab { get { return outlinePrefab; } set { outlinePrefab = value; } }




    //for final grid tester 
    [HideInInspector]
    public bool canDraw = false;

    [HideInInspector]
    public bool AllSet = false;     //<-----------------------------map has been generated 

    void Start()
    {

        gridOutlineChanger = GetComponent<Square>();
        wallGenerator = GetComponent<WallGenerator>();
        prefabManager = GetComponent<PrefabManager>();
        finalGridScript = GetComponent<FinalGridScript>();

        fournitureGridScript = GetComponent<fournitureGrid>();
        fournitureSpawningScript = GetComponent<fournitureSpawning>();


        mapInitializer();


        fournitureGridScript.RoomsAnalyzer();
        if (fournitureSpawningScript != null)
        {
           
            Debug.Log("not nulll  ");

           // fournitureSpawningScript.InstantiateWallFrames();
        }
       



    }

    public void mapInitializer ()
    {
        bool roomsAreSet = false;

        while (!roomsAreSet)
        {
            prefabManager.InitializeAllPrefabsAndMaterials();
            // Select BossRoom's parent
            bossRoomParent = Random.Range(2, numberOfRooms);

            // Initialize the grid and generate rooms when the script starts
            GenerateRooms();

            // Clear grid dictionary if needed
            // gridDictionary.Clear();

            // Change grid outlines
            gridOutlineChanger.ChangeGridOutlines(); // Call the method to change the outlines

            // Check if rooms are set
            roomsAreSet = CheckRooms();

            Debug.Log("all set : " + roomsAreSet);

            wallGenerator.GenerateWalls();

            

            finalGridScript.AnalyzeScene();
            AllSet = true;
            canDraw = true;
            finalGridScript.Allset = true;




          
        }
    }

   

    void GenerateRooms()
    {
        // Start from the middle of the grid
        Vector3 startPosition = new Vector3(0, 0, 0);

        // Create the first room at the start position
        int firstRoomWidth = Random.Range(roomMinWidth, roomMaxWidth + 1);
        int firstRoomHeight = Random.Range(roomMinHeight, roomMaxHeight + 1); // This can be considered as depth in 3D
        CreateRoom(startPosition, 1, -1, "r1", firstRoomWidth, firstRoomHeight, "normalRoom");

        // List to keep track of room positions, dimensions, and parent room numbers
        List<RoomInfo> roomInfos = new List<RoomInfo>();
        roomInfos.Add(new RoomInfo(startPosition, firstRoomWidth, firstRoomHeight, 1));

        // Generate additional rooms
        for (int i = 2; i <= numberOfRooms; i++)
        {
            bool roomCreated = false;
            Vector3 newRoomPosition = Vector3.zero;
            int newRoomWidth = 0;
            int newRoomHeight = 0;
            int parentRoomNumber = -1;
            Vector3 parentRoomPosition = Vector3.zero;

            // Try to create a new room, backtracking if necessary
            for (int j = roomInfos.Count - 1; j >= 0; j--)
            {
                RoomInfo roomInfo = roomInfos[j];
                newRoomWidth = Random.Range(roomMinWidth, roomMaxWidth + 1);
                newRoomHeight = Random.Range(roomMinHeight, roomMaxHeight + 1); // This can be considered as depth in 3D
                newRoomPosition = GetRandomAdjacentPosition(roomInfo.Position, roomInfo.Width, roomInfo.Height, newRoomWidth, newRoomHeight);

                if (newRoomPosition != Vector3.zero)
                {
                    roomCreated = true;
                    parentRoomNumber = roomInfo.RoomNumber;
                    parentRoomPosition = roomInfo.Position;
                    break;
                }
            }

            // If a valid position is found, create the room and update the list
            if (roomCreated)
            {
                CreateRoom(newRoomPosition, i, parentRoomNumber, $"r{i} p{parentRoomNumber} d{GetDirection(parentRoomPosition, newRoomPosition)}", newRoomWidth, newRoomHeight, "normalRoom");
                roomInfos.Add(new RoomInfo(newRoomPosition, newRoomWidth, newRoomHeight, i));

                // Check for additional rooms
                if (i == bossRoomParent)
                {
                    GenerateBossRoom(newRoomPosition, newRoomWidth, newRoomHeight, i);
                }
                GenerateAdditionalRooms(newRoomPosition, newRoomWidth, newRoomHeight, i);
            }
            else
            {
                Debug.LogWarning("No valid position found for room " + i);

                // Try to generate the room from a bonus room
                bool bonusRoomCreated = false;
                for (int j = roomInfos.Count - 1; j >= 0; j--)
                {
                    RoomInfo roomInfo = roomInfos[j];
                    newRoomWidth = Random.Range(roomMinWidth, roomMaxWidth + 1);
                    newRoomHeight = Random.Range(roomMinHeight, roomMaxHeight + 1); // This can be considered as depth in 3D
                    newRoomPosition = GetRandomAdjacentPosition(roomInfo.Position, roomInfo.Width, roomInfo.Height, newRoomWidth, newRoomHeight);

                    if (newRoomPosition != Vector3.zero)
                    {
                        CreateRoom(newRoomPosition, i, roomInfo.RoomNumber, $"r{i} p{roomInfo.RoomNumber} d{GetDirection(roomInfo.Position, newRoomPosition)}", newRoomWidth, newRoomHeight, "normalRoom");
                        Debug.Log("current room: " + i + " parent room: " + roomInfo.RoomNumber + " direction " + GetDirection(roomInfo.Position, newRoomPosition));
                        roomInfos.Add(new RoomInfo(newRoomPosition, newRoomWidth, newRoomHeight, i));
                        bonusRoomCreated = true;
                        break;
                    }
                }

                if (!bonusRoomCreated)
                {
                    Debug.LogWarning("No valid position found for room " + i + " even from bonus rooms.");
                    break;
                }
            }
        }
    }


    //tiktak

    void GenerateAdditionalRooms(Vector3 parentRoomPosition, int parentRoomWidth, int parentRoomHeight, int parentRoomNumber)
    {
        float extraRoomProbability = Random.value;
        int extraRooms = 0;
        if (extraRoomProbability < 0.19f) // Probability of 2 bonus rooms from a main room
        {
            extraRooms = 2;
        }
        else if (extraRoomProbability < 0.37f) // Probability for a bonus room from a main room
        {
            extraRooms = 1;
        }

        for (int j = 1; j <= extraRooms; j++)
        {
            // Get random dimensions for the new room
            int newRoomWidth = Random.Range(roomMinWidth, roomMaxWidth + 1);
            int newRoomHeight = Random.Range(roomMinHeight, roomMaxHeight + 1); // In 3D, this can be considered as depth

            // Get a random adjacent position for the new room
            Vector3 newRoomPosition = GetRandomAdjacentPosition(parentRoomPosition, parentRoomWidth, parentRoomHeight, newRoomWidth, newRoomHeight);

            // If a valid position is found, create the room
            if (newRoomPosition != Vector3.zero)
            {
                CreateRoom(newRoomPosition, parentRoomNumber * 100 + j, parentRoomNumber, $"bonus{j} p{parentRoomNumber} d{GetDirection(parentRoomPosition, newRoomPosition)}", newRoomWidth, newRoomHeight, "bonusRoom");
            }
        }
    }

    void GenerateBossRoom(Vector3 parentRoomPosition, int parentRoomWidth, int parentRoomHeight, int parentRoomNumber)
    {
        // Get random dimensions for the new room
        int newRoomWidth = 20; // Random.Range(roomMinWidth, roomMaxWidth + 1);
        int newRoomHeight = 20; // Random.Range(roomMinHeight, roomMaxHeight + 1);

        // Get a random adjacent position for the new room
        Vector3 newRoomPosition = GetRandomAdjacentPosition(parentRoomPosition, parentRoomWidth, parentRoomHeight, newRoomWidth, newRoomHeight);

        // If a valid position is found, create the room
        if (newRoomPosition != Vector3.zero)
        {
            CreateRoom(newRoomPosition, parentRoomNumber * 100, parentRoomNumber, $"Bossroom p{parentRoomNumber} d{GetDirection(parentRoomPosition, newRoomPosition)}", newRoomWidth, newRoomHeight, "bossRoom");
        }
    }


    void CreateRoom(Vector3 startPosition, int roomNumber, int parentRoomNumber, string roomName, int roomWidth, int roomHeight, string roomType)
    {
        // Create a new parent GameObject for the room
        GameObject roomParent = new GameObject(roomName);
        // Assign the tag based on the room type
        roomParent.tag = roomType;
        int prefabCounter = 1; // Counter for naming prefabs

        // Loop through each cell in the room dimensions
        for (int x = 0; x < roomWidth; x++)
        {
            for (int z = 0; z < roomHeight; z++) // Use 'z' for depth
            {
                // Calculate the cell position within the room
                Vector3 cellPosition = new Vector3(startPosition.x + x, startPosition.y, startPosition.z + z);

                // Check if the cell is within the grid and not reserved
                if (gridDictionary.ContainsKey(cellPosition) && !gridDictionary[cellPosition].reserved)
                {
                    // Mark the cell as reserved
                    gridDictionary[cellPosition].reserved = true;

                    // Instantiate the room prefab at the cell position and parent it to the room parent
                    GameObject roomInstance = Instantiate(roomPrefab, cellPosition, Quaternion.identity);
                    roomInstance.name = prefabCounter.ToString(); // Name the prefab with its number
                    roomInstance.transform.parent = roomParent.transform;
                    roomInstance.tag = "Grid"; // Set the desired tag
                    prefabCounter++;
                }

                if (!gridDictionary.ContainsKey(cellPosition))
                {
                    CellInfo cellInfo = new CellInfo
                    {
                        location = cellPosition,
                        reserved = true
                    };

                    // Add the cell information to the dictionary
                    gridDictionary.Add(cellPosition, cellInfo);

                    // Instantiate the room prefab at the cell position and parent it to the room parent
                    GameObject roomInstance = Instantiate(roomPrefab, cellPosition, Quaternion.identity);
                    roomInstance.name = prefabCounter.ToString(); // Name the prefab with its number
                    roomInstance.transform.parent = roomParent.transform;
                    roomInstance.tag = "Grid"; // Set the desired tag

                    prefabCounter++;
                }
            }
        }

        // Call to CreateOutlineCells with Vector3 parameters
        CreateOutlineCells(startPosition, roomWidth, roomHeight);
    }



    void CreateOutlineCells(Vector3 startPosition, int roomWidth, int roomHeight)
    {
        // Loop through each cell around the room to create the outline
        for (int x = -1; x <= roomWidth; x++)
        {
            for (int z = -1; z <= roomHeight; z++) // Use 'z' for depth
            {
                // Calculate the cell position for the outline
                Vector3 cellPosition = new Vector3(startPosition.x + x, startPosition.y, startPosition.z + z);

                // Skip the cells that are part of the room
                if (x >= 0 && x < roomWidth && z >= 0 && z < roomHeight)
                {
                    continue;
                }

                // Check if the cell is within the grid and not reserved
                if (gridDictionary.ContainsKey(cellPosition))
                {
                    // We're good, the cell already exists
                    continue;
                }

                // If the cell position is not in the dictionary, add it
                if (!gridDictionary.ContainsKey(cellPosition))
                {
                    CellInfo cellInfo = new CellInfo
                    {
                        location = cellPosition,
                        reserved = false
                    };

                    // Add the cell information to the dictionary
                    gridDictionary.Add(cellPosition, cellInfo);

                    // Instantiate the outline prefab at the cell position
                    // Instantiate(outlinePrefab, cellPosition, Quaternion.identity);
                }
            }
        }
    }


    Vector3 GetRandomAdjacentPosition(Vector3 currentRoomPosition, int currentRoomWidth, int currentRoomHeight, int newRoomWidth, int newRoomHeight)
    {
        // List of possible positions for the new room, ensuring a gap of one cell
        List<Vector3> possiblePositions = new List<Vector3>
    {
        new Vector3(currentRoomPosition.x + currentRoomWidth + 1, currentRoomPosition.y, currentRoomPosition.z), // Right
        new Vector3(currentRoomPosition.x - newRoomWidth - 1, currentRoomPosition.y, currentRoomPosition.z), // Left
        new Vector3(currentRoomPosition.x, currentRoomPosition.y, currentRoomPosition.z + currentRoomHeight + 1), // Forward
        new Vector3(currentRoomPosition.x, currentRoomPosition.y, currentRoomPosition.z - newRoomHeight - 1) // Backward
    };

        // Shuffle the list of possible positions to randomize the selection
        possiblePositions.Shuffle();

        // Check each possible position for validity
        foreach (Vector3 position in possiblePositions)
        {
            if (IsPositionValid(position, newRoomWidth, newRoomHeight))
            {
                return position;
            }
        }

        // Return Vector3.zero if no valid position is found
        return Vector3.zero;
    }

    bool IsPositionValid(Vector3 position, int roomWidth, int roomHeight)
    {
        // Loop through each cell in the room dimensions
        for (int x = 0; x < roomWidth; x++)
        {
            for (int z = 0; z < roomHeight; z++) // Use 'z' for depth
            {
                // Calculate the cell position within the room
                Vector3 cellPosition = new Vector3(position.x + x, position.y, position.z + z);

                // Check if the cell is within the grid and not reserved
                if (gridDictionary.ContainsKey(cellPosition) && gridDictionary[cellPosition].reserved)
                {
                    return false;
                }
            }
        }
        return true;
    }


    string GetDirection(Vector3 from, Vector3 to)
    {
        if (to.x > from.x) return "R";
        if (to.x < from.x) return "L";
        if (to.z > from.z) return "F"; // Forward for Z axis
        if (to.z < from.z) return "B"; // Backward for Z axis
        return "";
    }




    public bool CheckRooms()
    {
        // Find all objects with the tag "normalRoom"
        GameObject[] normalRooms = GameObject.FindGameObjectsWithTag("normalRoom");

        // Check if the count of normalRooms equals numberOfRooms
        if (normalRooms.Length == numberOfRooms)
        {
            // Look for an object with the tag "bossRoom"
            GameObject bossRoom = GameObject.FindGameObjectWithTag("bossRoom");

            // If bossRoom is found, return true
            if (bossRoom != null)
            {
                return true;
            }
        }

        // If conditions are not met, return false
        return false;
    }

    // Class to store cell information
    public class CellInfo
    {
        public Vector3 location; // Use Vector3 instead of Vector2
        public bool reserved;
    }


    // Helper class to store room information
    class RoomInfo
    {
        public Vector3 Position { get; }
        public int Width { get; }
        public int Height { get; }
        public int RoomNumber { get; }

        public RoomInfo(Vector3 position, int width, int height, int roomNumber)
        {
            Position = position;
            Width = width;
            Height = height;
            RoomNumber = roomNumber;
        }

    }
}

// Extension method to shuffle a list
public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}