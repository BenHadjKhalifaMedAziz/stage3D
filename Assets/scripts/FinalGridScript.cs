using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalGridScript : MonoBehaviour
{
    public Dictionary<int, Vector3> GridToUse = new Dictionary<int, Vector3>();


    private GridManager gridManager;  //to know that the mao has been generated 



    private bool hasCalledFunction = false;   //to call drawgrid only one time , 

    //private bool GridDrawn = false;


    [Header("Test the Grid")]
    public bool ShowGrid = false;
    public GameObject cubePrefab; ///new for final grid verif 


    [SerializeField]
    private bool allset = false ; //<------------------------------------------------------grid mawjoud


    public bool Allset { get { return allset; } set { allset = value; } }//  to verefy check from this getter 



    private void Start()
    {
        gridManager = GetComponent<GridManager>();
        //initialisers :
       
    }


    private void Update()
    {
       
            if (!hasCalledFunction )
            {
                if (allset)
                {
                    if (ShowGrid)
                    {
                        drawGrid();
                        hasCalledFunction = true;

                    }

                }
          
            }
        
    }


    public void AnalyzeScene()
    {
        // Clear the dictionary
        GridToUse.Clear();

        // Look for bossRoom, bonusRoom, or normalRooms tags
        string[] roomTags = { "normalRoom", "bonusRoom", "bossRoom" };
        foreach (string tag in roomTags)
        {
            GameObject[] rooms = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject room in rooms)
            {
                foreach (Transform child in room.transform)
                {
                    if (child.CompareTag("Grid") || child.CompareTag("door"))
                    {
                        // Add to GridToUse dictionary
                        GridToUse.Add(GridToUse.Count, child.position);
                     //   Debug.Log($"Added {child.name} at {child.position} from room {room.name} with tag {tag}");
                    }
                }
            }
        }

        // Look for objects named "passage" under parent named "PassagesAndWalls"
        GameObject passagesParent = GameObject.Find("PassagesAndWalls");
        if (passagesParent != null)
        {
            foreach (Transform child in passagesParent.transform)
            {
                if (child.name == "passage")
                {
                    // Add to GridToUse dictionary
                    GridToUse.Add(GridToUse.Count, child.position);
                  //  Debug.Log($"Added passage {child.name} at {child.position}");
                }
            }
        }
    }

    public void drawGrid()
    {
        foreach (KeyValuePair<int, Vector3> entry in GridToUse)
        {
            Vector3 position = entry.Value;
            position.y = 10; // Set Y position to 10

            Instantiate(cubePrefab, position, Quaternion.identity);
        }
    }


   


}
