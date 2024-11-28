using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimBehavior : MonoBehaviour
{
    public Transform aimGoal;

    // The maximum distance of the ray
    public float rayDistance = 10f;

    // LayerMask to filter objects the ray can hit
    public LayerMask hitLayers;

    // Update is called once per frame
    void Update()
    {
        // Start position of the ray (the object's position)
        Vector3 rayOrigin = transform.position;

        // Direction of the ray (forward from the object's perspective)
        Vector3 rayDirection = transform.forward;

        Vector3 lookAtPosition = rayOrigin + rayDirection * rayDistance;

        int layerMask = ~LayerMask.GetMask("Player");

        aimGoal.position = lookAtPosition;
    }
}
