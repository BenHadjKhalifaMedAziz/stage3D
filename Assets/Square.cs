using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Square : MonoBehaviour
{
   
    private Material outlineMaterial;
    private Material neighborMaterial;
    private Material doorMaterial;
    private GameObject passagePrefab;
    private GameObject passageWallPrefab;

    // Getters and Setters
    public Material OutlineMaterial { get { return outlineMaterial; } set { outlineMaterial = value; } }
    public Material NeighborMaterial { get { return neighborMaterial; } set { neighborMaterial = value; } }
    public Material DoorMaterial { get { return doorMaterial; } set { doorMaterial = value; } }
    public GameObject PassagePrefab { get { return passagePrefab; } set { passagePrefab = value; } }
    public GameObject PassageWallPrefab { get { return passageWallPrefab; } set { passageWallPrefab = value; } }




    void Update()
    { /////////////////// door generation tester 
        /*        if (Input.GetKeyDown(KeyCode.X))
                {
                    DestroyPassagesAndWallsParent();

                    ChangeGridOutlines();
                }*/

        // Door generation tester
        if (Input.GetKeyDown(KeyCode.X))
        {
            DestroyPassagesAndWallsParent(ChangeGridOutlines);
        }


    }

    public void ChangeGridOutlines()
    {
        GameObject[] normalRooms = GameObject.FindGameObjectsWithTag("normalRoom");
        GameObject[] bossRooms = GameObject.FindGameObjectsWithTag("bossRoom");
        GameObject[] bonusRooms = GameObject.FindGameObjectsWithTag("bonusRoom");

        // Combine the arrays
        GameObject[] gridParents = normalRooms.Concat(bossRooms).Concat(bonusRooms).ToArray();
        Dictionary<string, GameObject> roomDictionary = gridParents.ToDictionary(room => room.name, room => room);

        foreach (GameObject gridParent in gridParents)
        {
            Transform[] children = gridParent.GetComponentsInChildren<Transform>();
            List<Transform> outermostChildren = new List<Transform>();
            List<Transform> cornerChildren = new List<Transform>();
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            // Find the min and max x and z positions
            foreach (Transform child in children)
            {
                if (child == gridParent.transform) continue; // Skip the parent itself
                Vector3 position = child.position;
                if (position.x < minX) minX = position.x;
                if (position.x > maxX) maxX = position.x;
                if (position.z < minZ) minZ = position.z;
                if (position.z > maxZ) maxZ = position.z;
            }

            // Collect the outermost and corner children
            foreach (Transform child in children)
            {
                if (child == gridParent.transform) continue; // Skip the parent itself
                Vector3 position = child.position;
                if (position.x == minX || position.x == maxX || position.z == minZ || position.z == maxZ)
                {
                    outermostChildren.Add(child);
                    // Identify corners
                    if ((position.x == minX || position.x == maxX) && (position.z == minZ || position.z == maxZ))
                    {
                        cornerChildren.Add(child);
                    }
                }
            }

            // Change the material and tag of the outermost children
            foreach (Transform child in outermostChildren)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = outlineMaterial;
                }
                BoxCollider boxCollider = child.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.enabled = true;
                }
                // Tag as wall
                child.gameObject.tag = "wall";
            }

            // Name the corner children as "corner"
            foreach (Transform child in cornerChildren)
            {
                child.gameObject.name = "corner";
            }

            // Handle neighbor walls
            HandleNeighborWalls(gridParent, roomDictionary);
        }

        GroupPassagesAndWalls();
    }

    private void HandleNeighborWalls(GameObject currentRoom, Dictionary<string, GameObject> roomDictionary)
    {
        string currentRoomName = currentRoom.name;
        if (!currentRoomName.Contains("p")) return; // Skip the first room

        string[] parts = currentRoomName.Split(' ');
        string previousRoomName = parts[1].Substring(1); // Extract the previous room number
        string direction = parts[2].Substring(1); // Extract the direction

        GameObject previousRoom = roomDictionary.Values.FirstOrDefault(room => room.name.Contains($"r{previousRoomName}"));
        if (previousRoom == null) return;

        List<Transform> currentRoomWalls = GetRoomWalls(currentRoom);
        List<Transform> previousRoomWalls = GetRoomWalls(previousRoom);

        // Change the material of the walls based on the direction and tag them as doors
        List<Transform> currentRoomTargetWalls = ChangeWallMaterialAndTag(currentRoomWalls, direction, neighborMaterial);
        List<Transform> previousRoomTargetWalls = ChangeWallMaterialAndTag(previousRoomWalls, GetOppositeDirection(direction), neighborMaterial);

        // Tag the first matching pair of walls as doors
        TagWallsAsDoors(currentRoomTargetWalls, previousRoomTargetWalls);
    }

    private List<Transform> GetRoomWalls(GameObject room)
    {
        return room.GetComponentsInChildren<Transform>()
                   .Where(child => child.CompareTag("wall") && child.name != "corner")
                   .ToList();
    }

    private List<Transform> ChangeWallMaterialAndTag(List<Transform> walls, string direction, Material material)
    {
        List<Transform> targetWalls = new List<Transform>();
        foreach (Transform wall in walls)
        {
            Vector3 position = wall.position;
            bool isTargetWall = false;
            switch (direction)
            {
                case "F": // Forward (formerly Up)
                    isTargetWall = position.z == walls.Min(w => w.position.z);
                    break;
                case "B": // Backward (formerly Down)
                    isTargetWall = position.z == walls.Max(w => w.position.z);
                    break;
                case "L": // Left
                    isTargetWall = position.x == walls.Max(w => w.position.x);
                    break;
                case "R": // Right
                    isTargetWall = position.x == walls.Min(w => w.position.x);
                    break;
            }
            if (isTargetWall)
            {
                Renderer renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
                targetWalls.Add(wall);
            }
        }
        return targetWalls;
    }


    private string GetOppositeDirection(string direction)
    {
        switch (direction)
        {
            case "F": return "B"; // Forward to Backward
            case "B": return "F"; // Backward to Forward
            case "L": return "R"; // Left to Right
            case "R": return "L"; // Right to Left
            default: return "";
        }
    }


    private void TagWallsAsDoors(List<Transform> currentRoomWalls, List<Transform> previousRoomWalls)
    {
        List<Transform> matchingCurrentWalls = new List<Transform>();
        List<Transform> matchingPreviousWalls = new List<Transform>();

        foreach (Transform currentWall in currentRoomWalls)
        {
            foreach (Transform previousWall in previousRoomWalls)
            {
                if (Mathf.Approximately(currentWall.position.x, previousWall.position.x) ||
                    Mathf.Approximately(currentWall.position.z, previousWall.position.z)) // Changed y to z
                {
                    matchingCurrentWalls.Add(currentWall);
                    matchingPreviousWalls.Add(previousWall);
                }
            }
        }

        if (matchingCurrentWalls.Count > 0 && matchingPreviousWalls.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, matchingCurrentWalls.Count);
            Transform randomCurrentWall = matchingCurrentWalls[randomIndex];
            Transform randomPreviousWall = matchingPreviousWalls[randomIndex];

            randomCurrentWall.tag = "door";
            randomCurrentWall.name = "door";
            Renderer renderer = randomCurrentWall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = doorMaterial;
            }

            randomPreviousWall.tag = "door";
            randomPreviousWall.name = "door";
            Renderer renderer1 = randomPreviousWall.GetComponent<Renderer>();
            if (renderer1 != null)
            {
                renderer1.material = doorMaterial;
            }

            // Instantiate the passage prefab
            Vector3 passagePosition = (randomCurrentWall.position + randomPreviousWall.position) / 2;
            GameObject passage = Instantiate(passagePrefab, passagePosition, Quaternion.identity);
            passage.name = "passage";

            // Instantiate the passage wall prefabs based on the direction
            Vector3 offset1, offset2;
            if (Mathf.Approximately(randomCurrentWall.position.z, randomPreviousWall.position.z)) // Changed y to z
            {
                // Left or right direction
                offset1 = new Vector3(0,0, 1); // Changed y to z
                offset2 = new Vector3(0, 0, -1); // Changed y to z
            }
            else
            {
                // Forward or backward direction
                offset1 = new Vector3(1, 0, 0);
                offset2 = new Vector3(-1, 0, 0);
            }
            GameObject passageWall1 = Instantiate(passageWallPrefab, passagePosition + offset1, Quaternion.identity);
            passageWall1.name = "passageWall";
            GameObject passageWall2 = Instantiate(passageWallPrefab, passagePosition + offset2, Quaternion.identity);
            passageWall2.name = "passageWall";
        }
    }


    private void GroupPassagesAndWalls()
    {
        // Create a new parent GameObject
        GameObject parentObject = new GameObject("PassagesAndWalls");

        // Find all passages and passage walls by name
        GameObject[] passages = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == "passage").ToArray();
        GameObject[] passageWalls = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == "passageWall").ToArray();

        // Set the parent of each passage and passage wall to the new parent GameObject
        foreach (GameObject passage in passages)
        {
            passage.transform.SetParent(parentObject.transform);
        }

        foreach (GameObject passageWall in passageWalls)
        {
            passageWall.transform.SetParent(parentObject.transform);
        }
    }

    /// //foor generation showcase
    private void DestroyPassagesAndWallsParent(System.Action callback)
    {
        GameObject parentObject = GameObject.Find("PassagesAndWalls");
        if (parentObject != null)
        {
            Destroy(parentObject);
            StartCoroutine(WaitForDestruction(callback));
        }
        else
        {
            callback?.Invoke();
        }
    }
    private IEnumerator WaitForDestruction(System.Action callback)
    {
        yield return new WaitForEndOfFrame(); // Wait for the end of the frame to ensure destruction is complete
        callback?.Invoke();
    }


}
