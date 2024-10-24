using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float mouseSensitivity = 100f; // Sensitivity for mouse movement
    private Transform playerBody;         // The player's body transform

    void Start()
    {
        // Lock the cursor in the game window
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // If playerBody is not assigned, try to find the player
        if (playerBody == null)
        {
            // Find the player object in the scene by tag
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                playerBody = player.transform; // Get the player's transform
            }
        }

        // If the playerBody has been found, control the camera
        if (playerBody != null)
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

            // Rotate the player body on the x-axis (left/right)
            playerBody.Rotate(Vector3.up * mouseX);

            // Set the camera's position to the player's position for first-person view
            transform.position = playerBody.position;
            transform.rotation = playerBody.rotation;
        }
    }
}
