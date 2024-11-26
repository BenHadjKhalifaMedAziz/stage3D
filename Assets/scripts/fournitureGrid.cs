using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class fournitureGrid : MonoBehaviour
{
    public static Dictionary<GameObject, (string roomType, int nbrDoors, Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer)>)> RoomsList =
        new Dictionary<GameObject, (string, int, Dictionary<int, (GameObject, Vector3, string, bool, string, int)>)>();

    public void RoomsAnalyzer()
    {
        string[] roomTags = { "normalRoom", "bonusRoom", "bossRoom" };

        RoomsList.Clear();

        foreach (string tag in roomTags)
        {
            GameObject[] rooms = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject room in rooms)
            {
                int doorCount = 0;
                Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer)> childData = new();

                foreach (Transform child in room.transform)
                {
                    if (child.name == "Floor" || child.name == "3dCorners" || child.name == "3dWalls")
                        continue;

                    if (child.CompareTag("door"))
                    {
                        doorCount++;
                    }

                    Vector3 childPosition = child.position;
                    string childTag = child.tag;

                    int initialLayer = (child.CompareTag("door") || child.CompareTag("wall")) ? 1 : 0;
                    var detailsComponent = child.GetComponent<details>();
                    if (detailsComponent != null)
                    {
                        detailsComponent.layer = initialLayer;
                    }

                    int childKey = child.gameObject.GetInstanceID();
                    childData[childKey] = (child.gameObject, child.position, child.tag, false, string.Empty, initialLayer);
                }

                // Process layer 1 for wall data assignment
                ProcessLayerOneWallData(childData);

                // Process subsequent layers (layer 2 and beyond)
                ProcessChildLayers(childData, 1);

                // Process layer 2 for wall data assignment
                ProcessLayerTwoWallData(childData);

                ProcessDoorAdjacentLayerTwo(childData);

                RoomsList.Add(room, (tag, doorCount, childData));
                Debug.Log($"Room {room.name} of type {tag} has {doorCount} doors and {childData.Count} child grids.");
            }
        }
    }

    private void ProcessDoorAdjacentLayerTwo(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer)> childData)
    {
        // Find all doors in the child data
        var doors = childData.Values.Where(c => c.tag == "door").ToList();

        // Find all layer 2 children
        var layerTwoChildren = childData.Values.Where(c => c.layer == 2).ToList();

        foreach (var door in doors)
        {
            foreach (var layerTwoCell in layerTwoChildren)
            {
                // Check if the wall data of the door matches the wall data of the layer 2 cell
                if (layerTwoCell.wallData.Contains(door.wallData))
                {
                    // Check if they are adjacent
                    if (IsAdjacent(door.position, layerTwoCell.position))
                    {
                        // Update the door detail or flag layer 2 cell
                        var detailsComponent = layerTwoCell.childObject.GetComponent<details>();
                        if (detailsComponent != null)
                        {
                            detailsComponent.doorData = true; // Flag as adjacent to door
                        }

                        int key = layerTwoCell.childObject.GetInstanceID();
                        childData[key] = (layerTwoCell.childObject, layerTwoCell.position, layerTwoCell.tag, true, layerTwoCell.wallData, layerTwoCell.layer);

                        Debug.Log($"Layer 2 cell at {layerTwoCell.position} flagged as adjacent to door at {door.position}");
                    }
                }
            }
        }
    }

    private void ProcessLayerOneWallData(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer)> childData)
    {
        var layerOneChildren = childData.Values.Where(c => c.layer == 1).ToList();

        float minX = layerOneChildren.Min(c => c.position.x);
        float maxX = layerOneChildren.Max(c => c.position.x);
        float minZ = layerOneChildren.Min(c => c.position.z);
        float maxZ = layerOneChildren.Max(c => c.position.z);

        foreach (var child in layerOneChildren)
        {
            string wallData = AssignWallData(child.position, minX, maxX, minZ, maxZ);

            int key = child.childObject.GetInstanceID();
            childData[key] = (child.childObject, child.position, child.tag, child.doorData, wallData, child.layer);

            var detailsComponent = child.childObject.GetComponent<details>();
            if (detailsComponent != null)
            {
                detailsComponent.wallData = wallData;
            }
        }
    }

    private void ProcessLayerTwoWallData(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer)> childData)
    {
        var layerTwoChildren = childData.Values.Where(c => c.layer == 2).ToList();

        float minX = layerTwoChildren.Min(c => c.position.x);
        float maxX = layerTwoChildren.Max(c => c.position.x);
        float minZ = layerTwoChildren.Min(c => c.position.z);
        float maxZ = layerTwoChildren.Max(c => c.position.z);

        foreach (var child in layerTwoChildren)
        {
            string wallData = AssignWallData(child.position, minX, maxX, minZ, maxZ);

            int key = child.childObject.GetInstanceID();
            childData[key] = (child.childObject, child.position, child.tag, child.doorData, wallData, child.layer);

            var detailsComponent = child.childObject.GetComponent<details>();
            if (detailsComponent != null)
            {
                detailsComponent.wallData = wallData;
            }
        }
    }

    private string AssignWallData(Vector3 position, float minX, float maxX, float minZ, float maxZ)
    {
        string wallData = string.Empty;

        if (Mathf.Approximately(position.x, minX))
            wallData = "L"; // Left
        if (Mathf.Approximately(position.x, maxX))
            wallData = "R"; // Right
        if (Mathf.Approximately(position.z, minZ))
            wallData = "B"; // Back
        if (Mathf.Approximately(position.z, maxZ))
            wallData = "F"; // Front

        if (Mathf.Approximately(position.x, minX) && Mathf.Approximately(position.z, minZ))
            wallData = "CBL"; // Corner Back
        if (Mathf.Approximately(position.x, maxX) && Mathf.Approximately(position.z, minZ))
            wallData = "CBR"; // Corner Right
        if (Mathf.Approximately(position.x, minX) && Mathf.Approximately(position.z, maxZ))
            wallData = "CFL"; // Corner Left
        if (Mathf.Approximately(position.x, maxX) && Mathf.Approximately(position.z, maxZ))
            wallData = "CFR"; // Corner Front

        return wallData;
    }

    private void ProcessChildLayers(Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int layer)> childData, int currentLayer)
    {
        bool foundNewLayer = false;

        var currentLayerChildren = childData.Values.Where(c => c.layer == currentLayer).ToList();
        var updates = new Dictionary<int, (GameObject childObject, Vector3 position, string tag, bool doorData, string wallData, int newLayer)>();

        foreach (var child in currentLayerChildren)
        {
            foreach (var potentialNeighbor in childData.Values.Where(c => c.layer == 0))
            {
                if (IsAdjacent(child.position, potentialNeighbor.position))
                {
                    int neighborKey = potentialNeighbor.childObject.GetInstanceID();
                    updates[neighborKey] = (potentialNeighbor.childObject, potentialNeighbor.position, potentialNeighbor.tag, potentialNeighbor.doorData, potentialNeighbor.wallData, currentLayer + 1);
                    foundNewLayer = true;
                }
            }
        }

        foreach (var update in updates)
        {
            int key = update.Key;
            if (childData.ContainsKey(key))
            {
                childData[key] = (update.Value.childObject, update.Value.position, update.Value.tag, update.Value.doorData, update.Value.wallData, update.Value.newLayer);

                var detailsComponent = update.Value.childObject.GetComponent<details>();
                if (detailsComponent != null)
                {
                    detailsComponent.layer = update.Value.newLayer;
                }
            }
        }

        if (foundNewLayer)
        {
            ProcessChildLayers(childData, currentLayer + 1);
        }
    }

    private bool IsAdjacent(Vector3 position1, Vector3 position2)
    {
        return (Mathf.Approximately(Mathf.Abs(position1.x - position2.x), 1) && Mathf.Approximately(position1.z, position2.z)) ||
               (Mathf.Approximately(Mathf.Abs(position1.z - position2.z), 1) && Mathf.Approximately(position1.x, position2.x));
    }
}
