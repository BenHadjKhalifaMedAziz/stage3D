using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class WallGenerator : MonoBehaviour
{
    public float wallHeight = 1f;

    private GameObject wallPrefab;
    private GameObject cornerPrefab;
    private GameObject bossWallPrefab;
    private GameObject bossCornerPrefab;
    private GameObject passagePrefab;

    // Getters and Setters
    public GameObject WallPrefab { get { return wallPrefab; } set { wallPrefab = value; } }
    public GameObject CornerPrefab { get { return cornerPrefab; } set { cornerPrefab = value; } }
    public GameObject BossWallPrefab { get { return bossWallPrefab; } set { bossWallPrefab = value; } }
    public GameObject BossCornerPrefab { get { return bossCornerPrefab; } set { bossCornerPrefab = value; } }
    public GameObject PassagePrefab { get { return passagePrefab; } set { passagePrefab = value; } }


    void Start()
    {
     //   GenerateWalls();
    }

    public void GenerateWalls()
    {
        GameObject[] normalRooms = GameObject.FindGameObjectsWithTag("normalRoom");
        GameObject[] bonusRooms = GameObject.FindGameObjectsWithTag("bonusRoom");
        GameObject[] bossRooms = GameObject.FindGameObjectsWithTag("bossRoom");

        List<GameObject> allRooms = new List<GameObject>(normalRooms);
        allRooms.AddRange(bonusRooms);

        foreach (GameObject room in allRooms)
        {
            GenerateRoomWalls(room, wallPrefab, cornerPrefab);
        }

        foreach (GameObject bossRoom in bossRooms)
        {
            GenerateRoomWalls(bossRoom, bossWallPrefab, bossCornerPrefab);
        }


        GeneratePassageWalls();


    }

    void GenerateRoomWalls(GameObject room, GameObject wallPrefab, GameObject cornerPrefab)
    {
        List<GameObject> corners = new List<GameObject>();
        List<GameObject> walls = new List<GameObject>();
        List<GameObject> doors = new List<GameObject>();

        foreach (Transform child in room.transform)
        {
            if (child.name.Contains("corner"))
            {
                corners.Add(child.gameObject);
            }
            else if (child.CompareTag("wall"))
            {
                walls.Add(child.gameObject);
            }
            else if (child.CompareTag("door"))
            {
                doors.Add(child.gameObject);
            }
        }

        // Create the 3dWalls and 3dCorners parents for this room
        GameObject wallsParent = new GameObject("3dWalls");
        wallsParent.transform.SetParent(room.transform); // Set as a child of the room

        GameObject cornersParent = new GameObject("3dCorners");
        cornersParent.transform.SetParent(room.transform); // Set as a child of the room

        List<List<GameObject>> xGroups = GroupWalls(walls, doors, corners, "X");
        List<List<GameObject>> zGroups = GroupWalls(walls, doors, corners, "Z");

        foreach (List<GameObject> group in xGroups)
        {
            GenerateWall(group, wallsParent, wallPrefab);
        }

        foreach (List<GameObject> group in zGroups)
        {
            GenerateWall(group, wallsParent, wallPrefab);
        }

        GenerateCornerWalls(corners, cornersParent, cornerPrefab);
    }

    List<List<GameObject>> GroupWalls(List<GameObject> walls, List<GameObject> doors, List<GameObject> corners, string axis)
    {
        List<List<GameObject>> groups = new List<List<GameObject>>();

        foreach (GameObject corner in corners)
        {
            List<GameObject> group = new List<GameObject>();
            List<GameObject> tempWalls = new List<GameObject>(walls);

            foreach (GameObject wall in tempWalls)
            {
                if (axis == "X" && Mathf.Approximately(wall.transform.position.x, corner.transform.position.x))
                {
                    group.Add(wall);
                    walls.Remove(wall);
                }
                else if (axis == "Z" && Mathf.Approximately(wall.transform.position.z, corner.transform.position.z))
                {
                    group.Add(wall);
                    walls.Remove(wall);
                }
            }

            List<List<GameObject>> dividedGroups = DivideGroupByDoors(group, doors, axis);
            groups.AddRange(dividedGroups);
        }

        return groups;
    }

    List<List<GameObject>> DivideGroupByDoors(List<GameObject> group, List<GameObject> doors, string axis)
    {
        List<List<GameObject>> dividedGroups = new List<List<GameObject>>();

        if (group.Count == 0)
        {
            return dividedGroups;
        }

        group.Sort((a, b) => axis == "X" ?
            a.transform.position.z.CompareTo(b.transform.position.z) :
            a.transform.position.x.CompareTo(b.transform.position.x));

        List<GameObject> currentGroup = new List<GameObject>();
        Vector3 previousPosition = group[0].transform.position;

        foreach (GameObject wall in group)
        {
            bool isDoorBetween = false;
            foreach (GameObject door in doors)
            {
                if ((axis == "X" && Mathf.Approximately(door.transform.position.x, wall.transform.position.x) &&
                    door.transform.position.z > previousPosition.z && door.transform.position.z < wall.transform.position.z) ||
                    (axis == "Z" && Mathf.Approximately(door.transform.position.z, wall.transform.position.z) &&
                    door.transform.position.x > previousPosition.x && door.transform.position.x < wall.transform.position.x))
                {
                    isDoorBetween = true;
                    break;
                }
            }

            if (isDoorBetween)
            {
                if (currentGroup.Count > 0)
                {
                    dividedGroups.Add(new List<GameObject>(currentGroup));
                    currentGroup.Clear();
                }
            }
            currentGroup.Add(wall);
            previousPosition = wall.transform.position;
        }

        if (currentGroup.Count > 0)
        {
            dividedGroups.Add(currentGroup);
        }

        return dividedGroups;
    }

    void GenerateWall(List<GameObject> walls, GameObject parent, GameObject wallPrefab)
    {
        if (walls.Count == 0) return;

        Vector3 firstWallPos = walls[0].transform.position;
        bool isXAligned = true;
        bool isZAligned = true;

        foreach (GameObject wall in walls)
        {
            if (wall.transform.position.x != firstWallPos.x)
                isXAligned = false;
            if (wall.transform.position.z != firstWallPos.z)
                isZAligned = false;
        }

        if (!isXAligned && !isZAligned)
        {
            Debug.LogError("Walls are not aligned in either X or Z direction");
            return;
        }

        Vector3 midPoint = CalculateMidPoint(walls);

        float initialWallHeight = CalculateCombinedHeight(walls) / walls.Count;
        float wallPosY = (wallHeight / 2) + (initialWallHeight / 2);
        midPoint.y = firstWallPos.y + wallPosY;

        if (isXAligned)
        {
            float combinedLength = CalculateCombinedLength(walls, "X");
            CreateWall(midPoint, new Vector3(1, wallHeight, combinedLength), parent, wallPrefab, "3dWall");
        }
        else if (isZAligned)
        {
            float combinedLength = CalculateCombinedLength(walls, "Z");
            CreateWall(midPoint, new Vector3(combinedLength, wallHeight, 1), parent, wallPrefab, "3dWall");
        }
    }

    void GenerateCornerWalls(List<GameObject> corners, GameObject parent, GameObject cornerPrefab)
    {
        foreach (GameObject corner in corners)
        {
            float wallPosY = (wallHeight / 2) + (corner.transform.localScale.y / 2);
            Vector3 position = new Vector3(corner.transform.position.x, wallPosY, corner.transform.position.z);

            CreateWall(position, new Vector3(corner.transform.localScale.x, wallHeight, corner.transform.localScale.z), parent, cornerPrefab, "3dCorner");
        }
    }

    void GeneratePassageWalls()
    {
        // Find the "PassagesAndWalls" object in the scene
        GameObject passagesAndWallsObject = GameObject.Find("PassagesAndWalls");

        // Check if the object was found
        if (passagesAndWallsObject != null)
        {
            // Get all child objects with the name "passageWall"
            Transform[] passageWallChildren = passagesAndWallsObject.GetComponentsInChildren<Transform>();

            // Iterate through the child objects and log "hello" for each
            foreach (Transform child in passageWallChildren)
            {
                if (child.name == "passageWall")
                {
                    float wallPosY = (wallHeight / 2) + (child.transform.localScale.y / 2);
                    Vector3 position = new Vector3(child.transform.position.x, wallPosY, child.transform.position.z);

                    CreateWall(position, new Vector3(child.transform.localScale.x, wallHeight, child.transform.localScale.z), passagesAndWallsObject, PassagePrefab, "3dPassageWall");

                }
            }
        }
        
    }

    Vector3 CalculateMidPoint(List<GameObject> walls)
    {
        Vector3 midPoint = Vector3.zero;
        foreach (GameObject wall in walls)
        {
            midPoint += wall.transform.position;
        }
        return midPoint / walls.Count;
    }

    float CalculateCombinedHeight(List<GameObject> walls)
    {
        float height = 0;
        foreach (GameObject wall in walls)
        {
            height += wall.transform.localScale.y;
        }
        return height;
    }

    float CalculateCombinedLength(List<GameObject> walls, string axis)
    {
        float length = 0;
        foreach (GameObject wall in walls)
        {
            if (axis == "X")
            {
                length += wall.transform.localScale.z;
            }
            else if (axis == "Z")
            {
                length += wall.transform.localScale.x;
            }
        }
        return length;
    }

    void CreateWall(Vector3 position, Vector3 scale, GameObject parent, GameObject prefab, string name)
    {
        // Instantiate the wall or corner prefab
        GameObject wall = Instantiate(prefab, position, Quaternion.identity);
        wall.transform.localScale = scale;

        // Set the name and parent
        wall.name = name;
        wall.transform.SetParent(parent.transform);
    }
}
