using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRLineDrawerOpenXR : MonoBehaviour
{
    public ToolMenuFunctionality toolMenu; // Reference to tool menu

    public InputActionProperty triggerAction; 
    public InputActionProperty leftTriggerAction; // Left controller trigger
    public Transform rightController;
    public Transform leftController; // Left controller transform
    public float maxRayDistance = 10f; // Max distance for raycast
    public LayerMask raycastLayerMask; // Layer to interact with (set in the inspector)
    public GameObject pointPrefab; // Red dot prefab to spawn
    public GameObject linePrefab; // Prefab for creating lines between dots
    public LineRenderer lineRenderer; // Reference to the LineRenderer

    // New field to store the drawing quad (drag this into the Inspector)
    public Transform drawingQuadTransform; // Drag your drawing quad here

    private bool isDrawing = false;
    private GameObject spawnedPoint; // Reference to the spawned point
    private Vector3 currentRaycastHitPoint; // To store the current hit point for later point creation

    public Color hoverColor = Color.green;
    public Color selectedColor = Color.red;
    public List<GameObject> selectedPoints = new List<GameObject>();
    private GameObject hoveredPoint = null;
    private Color? hoveredPrevColor = null;
    private bool selectedThisPress = false;

    private List<GameObject> allPoints = new List<GameObject>();
    public float hoverPickRadius = 0.03f;
    
    // List to track order of selected dots for non-point tools
    private List<GameObject> orderedSelectedDots = new List<GameObject>();

 
    void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        triggerAction.action.canceled += OnTriggerReleased;
        
        leftTriggerAction.action.Enable();
        leftTriggerAction.action.performed += OnLeftTriggerPressed;
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
        triggerAction.action.canceled -= OnTriggerReleased;
        
        leftTriggerAction.action.Disable();
        leftTriggerAction.action.performed -= OnLeftTriggerPressed;
    }

    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        selectedThisPress = false;

        if (hoveredPoint != null)
        {
            int currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;
            
            if (currentTool != 1) // If not Point tool
            {
                // Add to ordered selection list (allow duplicates for shapes like triangles)
                orderedSelectedDots.Add(hoveredPoint);
                Debug.Log("Selected dot " + hoveredPoint.name + " (Order: " + orderedSelectedDots.Count + ")");
            }
            
            SetDotColor(hoveredPoint, selectedColor);
            if (!selectedPoints.Contains(hoveredPoint))
                selectedPoints.Add(hoveredPoint);

            selectedThisPress = true;

            if (rightController && lineRenderer)
                DrawRaycastLine(rightController.position, hoveredPoint.transform.position);

            isDrawing = true;
            return;
        }

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

        if (selectedThisPress)
        {
            selectedThisPress = false;
            return;
        }

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

                int currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;
                
                switch (currentTool)
                {
                    case 1: // Point tool
                        CreatePoint(hitInfo.point);
                        break;
                    case 2: // Line tool
                        Debug.Log("Line tool selected - functionality to be implemented");
                        break;
                    case 3: // Arc tool
                        Debug.Log("Arc tool selected - functionality to be implemented");
                        break;
                    case 4: // Circle tool
                        Debug.Log("Circle tool selected - functionality to be implemented");
                        break;
                    case 5: // Rectangle tool
                        Debug.Log("Rectangle tool selected - functionality to be implemented");
                        break;
                    case 6: // Polygon tool
                        Debug.Log("Polygon tool selected - functionality to be implemented");
                        break;
                    default:
                        Debug.Log("No tool selected");
                        break;
                }
            }
            else
            {
                Debug.Log("Ray did not hit the drawing quad → no point spawned");
            }
        }
    }



    private void Update()
    {
        UpdateHover(); // Always update hover regardless of tool or drawing state

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

    private void UpdateHover()
    {
        if (rightController == null) return;

        Ray ray = new Ray(rightController.position, rightController.forward);

        GameObject bestDot = null;
        float bestDist = hoverPickRadius;

        for (int i = allPoints.Count - 1; i >= 0; i--)
        {
            var dot = allPoints[i];
            if (dot == null) { allPoints.RemoveAt(i); continue; }

            Vector3 toDot = dot.transform.position - ray.origin;
            float proj = Vector3.Dot(toDot, ray.direction);
            if (proj < 0f || proj > maxRayDistance) continue;

            Vector3 closest = ray.origin + ray.direction * proj;
            float dist = Vector3.Distance(closest, dot.transform.position);
            if (dist <= bestDist)
            {
                bestDist = dist;
                bestDot = dot;
            }
        }

        if (hoveredPoint == bestDot) return;

        if (hoveredPoint != null && !selectedPoints.Contains(hoveredPoint))
        {
            if (hoveredPrevColor.HasValue) SetDotColor(hoveredPoint, hoveredPrevColor.Value);
            else SetDotColor(hoveredPoint, selectedColor);
        }

        hoveredPoint = bestDot;
        hoveredPrevColor = null;

        if (hoveredPoint != null && !selectedPoints.Contains(hoveredPoint))
        {
            var rend = hoveredPoint.GetComponent<Renderer>();
            if (rend != null) hoveredPrevColor = rend.material.color;
            SetDotColor(hoveredPoint, hoverColor);
        }
    }

    private void SetDotColor(GameObject dot, Color c)
    {
        if (!dot) return;
        var rend = dot.GetComponent<Renderer>();
        if (rend != null) rend.material.color = c;
    }

    private void CreatePoint(Vector3 hitPoint)
    {
        float quadZ = drawingQuadTransform.position.z;
        Vector3 spawnPos = new Vector3(hitPoint.x, hitPoint.y, quadZ);
        
        GameObject newPoint = Instantiate(pointPrefab, spawnPos, Quaternion.identity);
        newPoint.transform.SetParent(drawingQuadTransform, true);
        
        var col = newPoint.GetComponent<Collider>();
        if (!col) newPoint.AddComponent<SphereCollider>();
        
        pointCount++;
        newPoint.name = "sphere_" + pointCount;
        
        allPoints.Add(newPoint);
        
        Debug.Log("Spawned " + newPoint.name + " at " + spawnPos);
    }
    
    public List<GameObject> GetOrderedSelectedDots()
    {
        return new List<GameObject>(orderedSelectedDots);
    }
    
    public void ClearOrderedSelection()
    {
        orderedSelectedDots.Clear();
        Debug.Log("Cleared ordered dot selection");
    }
    
    private void OnLeftTriggerPressed(InputAction.CallbackContext ctx)
    {
        int currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;
        
        if (currentTool == 2 && orderedSelectedDots.Count >= 2) // Line tool and at least 2 dots
        {
            CreateLinesFromSelection();
        }
        else
        {
            Debug.Log("Need Line tool selected and at least 2 dots to create lines");
        }
    }
    
    private void CreateLinesFromSelection()
    {
        for (int i = 0; i < orderedSelectedDots.Count - 1; i++)
        {
            GameObject startDot = orderedSelectedDots[i];
            GameObject endDot = orderedSelectedDots[i + 1];
            
            if (startDot != null && endDot != null)
            {
                CreateLineBetweenDots(startDot, endDot);
            }
        }
        
        Debug.Log("Created " + (orderedSelectedDots.Count - 1) + " lines from selected dots");
        ClearOrderedSelection();
    }
    
    private void CreateLineBetweenDots(GameObject startDot, GameObject endDot)
    {
        if (linePrefab != null)
        {
            GameObject newLine = Instantiate(linePrefab, drawingQuadTransform);
            LineRenderer lr = newLine.GetComponent<LineRenderer>();
            
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, startDot.transform.position);
                lr.SetPosition(1, endDot.transform.position);
                
                Debug.Log("Created line from " + startDot.name + " to " + endDot.name);
            }
        }
        else
        {
            Debug.LogWarning("Line prefab not assigned!");
        }
    }
}
