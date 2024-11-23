using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fournitureGrid : MonoBehaviour
{

    public static Dictionary<GameObject, (string roomType, int nbrDoors)> RoomsList =
    new Dictionary<GameObject, (string, int)>();

    public Dictionary<GameObject, List<(Vector3 position, string tag, string doorData, string wallData)>> childObjectDictionary =
       new Dictionary<GameObject, List<(Vector3, string, string, string)>>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RoomsAnalyzer()
    {
        // Define the tags for the rooms
        string[] roomTags = { "normalRoom", "bonusRoom", "bossRoom" };

        // Clear the dictionary in case it's being reused
        RoomsList.Clear();

        foreach (string tag in roomTags)
            {
                // Find all GameObjects with the current tag
                GameObject[] rooms = GameObject.FindGameObjectsWithTag(tag);

                foreach (GameObject room in rooms)
                    {
                        int doorCount = 0; // Initialize door count for this room

                        foreach (Transform child in room.transform)
                        {
                            // Check if the child has the "door" tag
                            if (child.CompareTag("door"))
                            {
                                doorCount++; // Increment the door count
                            }
                        }

                        // Add the room to the dictionary with its tag and door count
                        RoomsList.Add(room, (tag, doorCount));

                      //  Debug.Log($"Room {room.name} of type {tag} has {doorCount} doors.");
                    }
        }
    }


    // Populate the childObjectDictionary based on RoomsList
    public void PopulateChildGrid()
    {
        foreach (var roomEntry in RoomsList)
        {
            GameObject room = roomEntry.Key;
            List<(Vector3 position, string tag, string doorData, string wallData)> childData = new List<(Vector3, string, string, string)>();

            foreach (Transform child in room.transform)
            {
                // Skip certain objects
                if (child.name == "Floor" || child.name == "3dCorners" || child.name == "3dWalls")
                    continue;

                Vector3 childPosition = child.position;
                string childTag = child.tag;
                string doorData = string.Empty; // Default as empty
                string wallData = string.Empty; // Default as empty

                // Process door proximity
                if (childTag == "Grid")
                {
                    doorData = ProcessDoorProximity(childPosition);
                }

                // Process wall proximity
                if (childTag == "Grid")
                {
                    wallData = ProcessWallProximity(childPosition);
                }

                // If there is no connection to door or wall, skip adding this child
                if (string.IsNullOrEmpty(doorData) && string.IsNullOrEmpty(wallData))
                    continue;

                // Add the child to the grid data list
                childData.Add((childPosition, childTag, doorData, wallData));

                // Populate the details component for each child
                details childDetails = child.GetComponent<details>();
                if (childDetails != null)
                {
                    childDetails.doorData = doorData;
                    childDetails.wallData = wallData;
                }
            }

            childObjectDictionary.Add(room, childData);
        }
    }

    // Determine door proximity and return corresponding data
    private string ProcessDoorProximity(Vector3 gridPosition)
    {
        foreach (var roomEntry in RoomsList)
        {
            GameObject room = roomEntry.Key;

            foreach (Transform child in room.transform)
            {
                if (child.CompareTag("door"))
                {
                    Vector3 doorPosition = child.position;
                    Vector3 direction = gridPosition - doorPosition;

                    // Check if grid position is near the door (adjust the proximity threshold as needed)
                    if (Mathf.Abs(direction.x) <= 1 && Mathf.Abs(direction.z) <= 1)
                    {
                        if (Mathf.Abs(direction.x) == 1) return "door L or R";
                        if (Mathf.Abs(direction.z) == 1) return "door F or B";
                        return "door center";
                    }

                    // Diagonal proximity to the door
                    if (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.z) == 1)
                    {
                        return "door diagL or diagR";
                    }
                }
            }
        }
        return string.Empty; // No door proximity
    }

    // Determine wall proximity and return corresponding data
    private string ProcessWallProximity(Vector3 gridPosition)
    {
        foreach (var roomEntry in RoomsList)
        {
            GameObject room = roomEntry.Key;

            foreach (Transform child in room.transform)
            {
                if (child.CompareTag("wall"))
                {
                    Vector3 wallPosition = child.position;
                    Vector3 direction = gridPosition - wallPosition;

                    // Check if grid position is near the wall (adjust the proximity threshold as needed)
                    if (Mathf.Abs(direction.x) <= 1 && Mathf.Abs(direction.z) <= 1)
                    {
                        if (Mathf.Abs(direction.x) == 1) return "wall L or R";
                        if (Mathf.Abs(direction.z) == 1) return "wall F or B";
                        return "wall center";
                    }
                }
            }
        }
        return string.Empty; // No wall proximity
    }
}
