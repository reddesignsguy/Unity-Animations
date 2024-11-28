using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform center;
    public Transform playerBody;  // The object the camera is attached to (e.g., the player)
    public float mouseSensitivity = 100f;  // Sensitivity of the mouse
    public float maxVerticalAngle = 80f;   // Maximum up/down rotation to prevent flipping

    private float xRotation = 0f;  // Tracks the camera's vertical rotation

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the camera up/down (vertical rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxVerticalAngle, maxVerticalAngle);  // Clamp the vertical rotation

        // Apply vertical rotation to the camera
        Debug.Log(xRotation);
        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.RotateAround(center.position, center.right, -mouseY);

        // Rotate the player body left/right (horizontal rotation)
        //transform.RotateAround(center.position, center.up, mouseX);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
