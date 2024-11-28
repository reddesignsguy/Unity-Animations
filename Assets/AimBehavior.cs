using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimBehavior : MonoBehaviour
{
    public Transform aimGoal;

    // The maximum distance of the ray
    public float rayDistance = 100f;

    // LayerMask to filter objects the ray can hit
    public LayerMask hitLayers;

    // Update is called once per frame
    void Update()
    {
        // Start position of the ray (the object's position)
        Vector3 rayOrigin = transform.position;

        // Direction of the ray (forward from the object's perspective)
        Vector3 rayDirection = transform.forward;

        int layerMask = ~LayerMask.GetMask("Player");


        // Perform the raycast
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, rayDistance, layerMask))
        {
            Debug.Log(hitInfo.collider.gameObject);
            // If it hits something, draw a green line and log the object
            Debug.DrawLine(rayOrigin, hitInfo.point, Color.green);
            aimGoal.position = hitInfo.point;
        }
    }
}
