using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRLineDrawerOpenXR : MonoBehaviour
{
    private Dictionary<double, string> tools = new Dictionary<double, string>();

    public InputActionProperty triggerAction; 
    public Transform rightController;
    public float maxRayDistance = 10f; // Max distance for raycast
    public LayerMask raycastLayerMask; // Layer to interact with (set in the inspector)
    public GameObject pointPrefab; // Red dot prefab to spawn
    public LineRenderer lineRenderer; // Reference to the LineRenderer

    // New field to store the drawing quad (drag this into the Inspector)
    public Transform drawingQuadTransform; // Drag your drawing quad here

    private bool isDrawing = false;
    private GameObject spawnedPoint; // Reference to the spawned point
    private Vector3 currentRaycastHitPoint; // To store the current hit point for later point creation
    private void Start()
    {
        //add subtooms denoted by key x.y
        tools.Add(0, "Point");
        tools.Add(1, "Line");
        tools.Add(2, "Arc");
        tools.Add(3, "Circle");
        tools.Add(4, "Rectangle");
        tools.Add(5, "Polygon");
    } 
    void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        triggerAction.action.canceled += OnTriggerReleased;
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
        triggerAction.action.canceled -= OnTriggerReleased;
    }

    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        isDrawing = true;
        Debug.Log("Trigger Pressed!");

        // Perform the raycast from the right controller
        Ray ray = new Ray(rightController.position, rightController.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, maxRayDistance, raycastLayerMask))
        {
            // Log the name of the object hit by the raycast
            Debug.Log("Hit Object: " + hitInfo.transform.name);

            // Store the current raycast hit point for point spawning later
            currentRaycastHitPoint = hitInfo.point;

            // Draw the line from the controller to the hit point (continuously updating the line)
            DrawRaycastLine(rightController.position, hitInfo.point);
        }
        else
        {
            // If the raycast doesn't hit anything, draw a line to the max distance
            DrawRaycastLine(rightController.position, ray.GetPoint(maxRayDistance));
        }
    }
    private int pointCount = 0;   // counter for placed points

    private void OnTriggerReleased(InputAction.CallbackContext ctx)
    {
        if (!isDrawing) return;
        isDrawing = false;

        lineRenderer.positionCount = 0;

        if (currentRaycastHitPoint != Vector3.zero && drawingQuadTransform != null)
        {
            // make sure we actually hit the drawing quad
            Ray ray = new Ray(rightController.position, rightController.forward);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxRayDistance, raycastLayerMask) &&
                hitInfo.transform == drawingQuadTransform)
            {
                float quadZ = drawingQuadTransform.position.z;

                // snap the point’s Z to the quad
                Vector3 spawnPos = new Vector3(currentRaycastHitPoint.x,
                                            currentRaycastHitPoint.y,
                                            quadZ);

                GameObject newPoint = Instantiate(pointPrefab, spawnPos, Quaternion.identity);
                newPoint.transform.SetParent(drawingQuadTransform, true);

                // increment counter and rename
                pointCount++;
                newPoint.name = "sphere_" + pointCount;

                Debug.Log("Spawned " + newPoint.name + " at " + spawnPos);
            }
            else
            {
                Debug.Log("Ray did not hit the drawing quad → no point spawned");
            }
        }
    }



    private void Update()
    {
        if (isDrawing)
        {
            // Continuously update the line while holding the trigger
            Ray ray = new Ray(rightController.position, rightController.forward);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, maxRayDistance, raycastLayerMask))
            {
                // Update the line's end position to the raycast hit point
                currentRaycastHitPoint = hitInfo.point;
                DrawRaycastLine(rightController.position, hitInfo.point);
            }
            else
            {
                // If no hit, update the line to the max ray distance
                DrawRaycastLine(rightController.position, ray.GetPoint(maxRayDistance));
            }
        }
    }

    private void DrawRaycastLine(Vector3 startPoint, Vector3 endPoint)
    {
        // Set the line's position count to 2 (start and end points)
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint); // Start position (controller position)
        lineRenderer.SetPosition(1, endPoint);   // End position (hit point or max distance)
    }
}
