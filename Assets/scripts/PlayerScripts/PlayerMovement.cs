using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of player movement
    private Transform cameraTransform;   // To get camera's rotation

    void Start()
    {
        // Find the main camera
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        // Get input from WASD or arrow keys
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Create a movement vector relative to the camera's direction
        Vector3 movement = cameraTransform.right * moveX + cameraTransform.forward * moveY;
        movement.y = 0f; // Prevent movement in the y-axis (only move on the ground)

        // Apply movement based on speed and time
        transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.World);
    }
}
