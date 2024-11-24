using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class fournitureGrid : MonoBehaviour
{
    public static Dictionary<GameObject, (string roomType, int nbrDoors, List<(GameObject childObject, Vector3 position, string tag, string doorData, string wallData, int layer)> childData)> RoomsList =
         new Dictionary<GameObject, (string, int, List<(GameObject, Vector3, string, string, string, int)>)>();

    public void RoomsAnalyzer()
    {
        // Define the tags for the rooms
        string[] roomTags = { "normalRoom", "bonusRoom", "bossRoom" };

        // Clear the dictionary in case it's being reused
        RoomsList.Clear();

        // First pass: Add rooms and their children to the dictionary
        foreach (string tag in roomTags)
        {
            // Find all GameObjects with the current tag
            GameObject[] rooms = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject room in rooms)
            {
                int doorCount = 0; // Initialize door count for this room
                List<(GameObject childObject, Vector3 position, string tag, string doorData, string wallData, int layer)> childData = new();

                // Collect all children and count doors
                foreach (Transform child in room.transform)
                {
                    if (child.name == "Floor" || child.name == "3dCorners" || child.name == "3dWalls")
                        continue;

                    // Count doors
                    if (child.CompareTag("door"))
                    {
                        doorCount++;
                    }

                    // Add child data if it's not skipped
                    Vector3 childPosition = child.position;
                    string childTag = child.tag;

                    // Get the 'details' component if available and assign the layer
                    int initialLayer = (child.CompareTag("door") || child.CompareTag("wall")) ? 1 : 0;
                    var detailsComponent = child.GetComponent<details>();
                    if (detailsComponent != null)
                    {
                        detailsComponent.layer = initialLayer;
                    }

                    // Store the GameObject and its associated data
                    childData.Add((child.gameObject, child.position, child.tag, string.Empty, string.Empty, initialLayer));
                }

                // After the first pass, process child layers
                ProcessChildLayers(childData, 1);

                // Add the room and its child data to the dictionary
                RoomsList.Add(room, (tag, doorCount, childData));

                Debug.Log($"Room {room.name} of type {tag} has {doorCount} doors and {childData.Count} child grids.");
            }
        }
    }

    private void ProcessChildLayers(List<(GameObject childObject, Vector3 position, string tag, string doorData, string wallData, int layer)> childData, int currentLayer)
    {
        bool foundNewLayer = false;

        // Get children of the current layer
        var currentLayerChildren = childData.Where(c => c.layer == currentLayer).ToList();

        // Temporary list to store updates
        var updates = new List<(GameObject childObject, Vector3 position, string tag, string doorData, string wallData, int newLayer)>();

        foreach (var child in currentLayerChildren)
        {
            foreach (var potentialNeighbor in childData.Where(c => c.layer == 0))
            {
                if (IsAdjacent(child.position, potentialNeighbor.position))
                {
                    // Add the neighbor to the updates list
                    updates.Add((potentialNeighbor.childObject, potentialNeighbor.position, potentialNeighbor.tag, potentialNeighbor.doorData, potentialNeighbor.wallData, currentLayer + 1));
                    foundNewLayer = true;
                }
            }
        }

        // Apply updates after iteration
        foreach (var update in updates)
        {
            int index = childData.IndexOf((update.childObject, update.position, update.tag, update.doorData, update.wallData, 0));
            if (index >= 0)
            {
                childData[index] = (update.childObject, update.position, update.tag, update.doorData, update.wallData, update.newLayer);

                // Update the 'details' component
                var detailsComponent = update.childObject.GetComponent<details>();
                if (detailsComponent != null)
                {
                    detailsComponent.layer = update.newLayer;
                }
            }
        }

        // Recurse if new layers were found
        if (foundNewLayer)
        {
            ProcessChildLayers(childData, currentLayer + 1);
        }
    }

    private bool IsAdjacent(Vector3 position1, Vector3 position2)
    {
        return (Mathf.Abs(position1.x - position2.x) == 1 && position1.z == position2.z) ||
               (Mathf.Abs(position1.z - position2.z) == 1 && position1.x == position2.x);
    }

}
