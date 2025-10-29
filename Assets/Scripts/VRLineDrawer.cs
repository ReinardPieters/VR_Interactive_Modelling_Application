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
    public float maxRayDistance = 30f; // Max distance for raycast
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
    private double previousTool = -1; // Track previous tool to detect changes
    
    // Arc parameters
    public float arcHeight = 0.5f; // How high the arc curves
    public int arcResolution = 20; // Number of points in the arc
    
    // Circle timing for semicircle direction
    private float leftTriggerPressTime = 0f;
    private bool leftTriggerPressed = false;

    //Trigger pressed to select Dot
    private bool dotselected = false;
    
    // Debounce variables to prevent multiple selections
    private float lastTriggerTime = 0f;
    private const float triggerDebounceTime = 0.3f; // 300ms debounce

 
    void OnEnable()
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        triggerAction.action.canceled += OnTriggerReleased;
        
        leftTriggerAction.action.Enable();
        leftTriggerAction.action.performed += OnLeftTriggerPressed;
        leftTriggerAction.action.canceled += OnLeftTriggerReleased;
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
        triggerAction.action.performed -= OnTriggerPressed;
        triggerAction.action.canceled -= OnTriggerReleased;
        
        leftTriggerAction.action.Disable();
        leftTriggerAction.action.performed -= OnLeftTriggerPressed;
        leftTriggerAction.action.canceled -= OnLeftTriggerReleased;
    }

    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        // Debounce check - prevent rapid trigger presses
        if (Time.time - lastTriggerTime < triggerDebounceTime)
        {
            return;
        }
        
        if (!dotselected)
        {
            lastTriggerTime = Time.time;
            selectedThisPress = false;

            if (hoveredPoint != null)
            {
                double currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;
                
                if (currentTool != 1) // If not Point tool
                {
                    if (currentTool == 3) // Arc tool - no duplicates
                    {
                        if (!orderedSelectedDots.Contains(hoveredPoint))
                        {
                            orderedSelectedDots.Add(hoveredPoint);
                            Debug.Log("Selected dot " + hoveredPoint.name + " (Order: " + orderedSelectedDots.Count + ")");
                        }
                        else
                        {
                            Debug.Log("Dot " + hoveredPoint.name + " already selected for arc");
                        }
                    }
                    else // Other tools - allow duplicates for shapes like triangles
                    {
                        orderedSelectedDots.Add(hoveredPoint);
                        Debug.Log("Selected dot " + hoveredPoint.name + " (Order: " + orderedSelectedDots.Count + ")");
                    }
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

            dotselected = true;
        }
    }
    private int pointCount = 0;   // counter for placed points

    private void OnTriggerReleased(InputAction.CallbackContext ctx)
    {
        if (!isDrawing) return;
        isDrawing = false;

        dotselected = false;

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

                double currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;
                
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
        // Check for tool changes and clear selection if tool changed
        double currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;
        if (currentTool != previousTool)
        {
            if (orderedSelectedDots.Count > 0)
            {
                ClearOrderedSelection();
                Debug.Log("Tool changed - cleared dot selection");
            }
            previousTool = currentTool;
        }
        
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

        if (hoveredPoint != null)
        {
            if (hoveredPrevColor.HasValue) 
            {
                SetDotColor(hoveredPoint, hoveredPrevColor.Value);
            }
            else if (selectedPoints.Contains(hoveredPoint))
            {
                SetDotColor(hoveredPoint, selectedColor);
            }
            else
            {
                // Restore to default color (white or whatever the original dot color was)
                SetDotColor(hoveredPoint, Color.white);
            }
        }

        hoveredPoint = bestDot;
        hoveredPrevColor = null;

        if (hoveredPoint != null)
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
    
    private void OnLeftTriggerReleased(InputAction.CallbackContext ctx)
    {
        if (leftTriggerPressed)
        {
            leftTriggerPressed = false;
            float holdTime = Time.time - leftTriggerPressTime;
            bool createUpperSemicircle = holdTime < 1f; // Short press = upper/right, long press = lower/left
            
            CreateCircleFromSelection(createUpperSemicircle);
        }
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
        double currentTool = toolMenu != null ? toolMenu.GetCurrentTool() : 1;

        if (currentTool == 4 && orderedSelectedDots.Count == 2) // Circle tool
        {
            leftTriggerPressed = true;
            leftTriggerPressTime = Time.time;
        }
        else if (currentTool == 2.1 && orderedSelectedDots.Count >= 2) // Line tool
        {
            CreateLinesFromSelection();
        }
        else if (currentTool == 2.2 && orderedSelectedDots.Count == 3) // Parallel line sub-tool
        {
            CreateParallelLine();
        }
        else if (currentTool == 3 && orderedSelectedDots.Count >= 2) // Arc tool
        {
            CreateArcsFromSelection();
        }
		else if (currentTool == 5.1 && orderedSelectedDots.Count == 2) // Rectangle from center
		{
			CreateRectangleFromCenter();
		}
		else if (currentTool == 5.2 && orderedSelectedDots.Count == 2) // Rectangle from corner
		{
			CreateRectangleFromCorner();
		}
		else 
        {
            Debug.Log("Need appropriate tool selected and enough dots to create shapes");
        }
    }

	private void CreateRectangleFromCenter()
	{
		GameObject centerDot = orderedSelectedDots[0];
		GameObject cornerDot = orderedSelectedDots[1];

		Vector3 center = centerDot.transform.position;
		Vector3 corner = cornerDot.transform.position;

		Vector3 halfExtents = corner - center;

		// Calculate the other 3 corners
		Vector3 corner1 = center + new Vector3(halfExtents.x, -halfExtents.y, 0); // bottom-right
		Vector3 corner2 = center + new Vector3(-halfExtents.x, -halfExtents.y, 0); // bottom-left
		Vector3 corner3 = center + new Vector3(-halfExtents.x, halfExtents.y, 0); // top-left

		// Create dots
		GameObject dot1 = CreatePointAtRectPosition(corner);
		GameObject dot2 = CreatePointAtRectPosition(corner1);
		GameObject dot3 = CreatePointAtRectPosition(corner2);
		GameObject dot4 = CreatePointAtRectPosition(corner3);

		// Connect lines in order
		CreateLineBetweenSpecificPoints(dot1.transform.position, dot2.transform.position);
		CreateLineBetweenSpecificPoints(dot2.transform.position, dot3.transform.position);
		CreateLineBetweenSpecificPoints(dot3.transform.position, dot4.transform.position);
		CreateLineBetweenSpecificPoints(dot4.transform.position, dot1.transform.position);

		Debug.Log("Created rectangle (from center) using " + centerDot.name + " and " + cornerDot.name);
		ClearOrderedSelection();
	}

	private void CreateRectangleFromCorner()
	{
		GameObject corner1 = orderedSelectedDots[0];
		GameObject corner2 = orderedSelectedDots[1];

		Vector3 p1 = corner1.transform.position;
		Vector3 p2 = corner2.transform.position;

		// Other corners share the same x or y
		Vector3 corner3Pos = new Vector3(p1.x, p2.y, p1.z);
		Vector3 corner4Pos = new Vector3(p2.x, p1.y, p1.z);

		// Create missing corner dots
		GameObject dot3 = CreatePointAtRectPosition(corner3Pos);
		GameObject dot4 = CreatePointAtRectPosition(corner4Pos);

		// Connect all four corners
		CreateLineBetweenSpecificPoints(p1, corner3Pos);
		CreateLineBetweenSpecificPoints(corner3Pos, p2);
		CreateLineBetweenSpecificPoints(p2, corner4Pos);
		CreateLineBetweenSpecificPoints(corner4Pos, p1);

		Debug.Log("Created rectangle (from corner) using " + corner1.name + " and " + corner2.name);
		ClearOrderedSelection();
	}

	private GameObject CreatePointAtRectPosition(Vector3 position)
	{
		float quadZ = drawingQuadTransform.position.z;
		Vector3 spawnPos = new Vector3(position.x, position.y, quadZ);

		GameObject newPoint = Instantiate(pointPrefab, spawnPos, Quaternion.identity);
		newPoint.transform.SetParent(drawingQuadTransform, true);

		var col = newPoint.GetComponent<Collider>();
		if (!col) newPoint.AddComponent<SphereCollider>();

		pointCount++;
		newPoint.name = "sphere_" + pointCount;

		allPoints.Add(newPoint);

		Debug.Log("Spawned " + newPoint.name + " at " + spawnPos);
		return newPoint;
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
    
    private void CreateArcsFromSelection()
    {
        if (orderedSelectedDots.Count < 2)
        {
            Debug.Log("Need at least 2 dots for arc");
            return;
        }
        
        CreateSingleArcThroughPoints();
        Debug.Log("Created arc through " + orderedSelectedDots.Count + " points");
        ClearOrderedSelection();
    }
    
    private void CreateSingleArcThroughPoints()
    {
        if (linePrefab != null)
        {
            GameObject newArc = Instantiate(linePrefab, drawingQuadTransform);
            LineRenderer lr = newArc.GetComponent<LineRenderer>();
            
            if (lr != null)
            {
                Vector3[] controlPoints = new Vector3[orderedSelectedDots.Count];
                for (int i = 0; i < orderedSelectedDots.Count; i++)
                {
                    controlPoints[i] = orderedSelectedDots[i].transform.position;
                }
                
                lr.positionCount = arcResolution;
                
                // Create smooth curve through all control points
                for (int i = 0; i < arcResolution; i++)
                {
                    float t = i / (float)(arcResolution - 1);
                    Vector3 curvePoint = CalculateBezierCurve(controlPoints, t);
                    lr.SetPosition(i, curvePoint);
                }
                
                Debug.Log("Created arc from " + orderedSelectedDots[0].name + " to " + orderedSelectedDots[orderedSelectedDots.Count-1].name + " through " + (orderedSelectedDots.Count-2) + " control points");
            }
        }
        else
        {
            Debug.LogWarning("Line prefab not assigned!");
        }
    }
    
    private Vector3 CalculateBezierCurve(Vector3[] points, float t)
    {
        if (points.Length == 2)
        {
            // Linear interpolation for 2 points
            return Vector3.Lerp(points[0], points[1], t);
        }
        else if (points.Length == 3)
        {
            // Quadratic Bezier for 3 points
            float u = 1 - t;
            return u * u * points[0] + 2 * u * t * points[1] + t * t * points[2];
        }
        else
        {
            // Use De Casteljau's algorithm for higher order curves
            Vector3[] temp = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                temp[i] = points[i];
            }
            
            for (int level = points.Length - 1; level > 0; level--)
            {
                for (int i = 0; i < level; i++)
                {
                    temp[i] = Vector3.Lerp(temp[i], temp[i + 1], t);
                }
            }
            
            return temp[0];
        }
    }
    
    private void CreateCircleFromSelection(bool upperSemicircle)
    {
        if (orderedSelectedDots.Count != 2)
        {
            Debug.Log("Circle tool needs exactly 2 points on the circle");
            return;
        }
        
        Vector3 point1 = orderedSelectedDots[0].transform.position;
        Vector3 point2 = orderedSelectedDots[1].transform.position;
        
        CreateSemicircle(point1, point2, upperSemicircle);
        Debug.Log("Created " + (upperSemicircle ? "upper" : "lower") + " semicircle through " + orderedSelectedDots[0].name + " and " + orderedSelectedDots[1].name);
        ClearOrderedSelection();
    }
    
    private void CreateSemicircle(Vector3 point1, Vector3 point2, bool upperSemicircle)
    {
        if (linePrefab != null)
        {
            GameObject newSemicircle = Instantiate(linePrefab, drawingQuadTransform);
            LineRenderer lr = newSemicircle.GetComponent<LineRenderer>();
            
            if (lr != null)
            {
                Vector3 center = (point1 + point2) / 2f;
                float radius = Vector3.Distance(point1, point2) / 2f;
                
                int semicircleResolution = 32;
                lr.positionCount = semicircleResolution + 1;
                
                // Calculate start and end angles
                Vector3 direction = (point2 - point1).normalized;
                float startAngle = Mathf.Atan2(direction.y, direction.x);
                float endAngle = startAngle + Mathf.PI;
                
                if (!upperSemicircle)
                {
                    // Flip for lower semicircle
                    startAngle += Mathf.PI;
                    endAngle += Mathf.PI;
                }
                
                for (int i = 0; i <= semicircleResolution; i++)
                {
                    float t = i / (float)semicircleResolution;
                    float angle = Mathf.Lerp(startAngle, endAngle, t);
                    Vector3 circlePoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                    lr.SetPosition(i, circlePoint);
                }
            }
        }
        else
        {
            Debug.LogWarning("Line prefab not assigned!");
        }
    }
    
    private void CreateParallelLine()
    {
        Vector3 point1 = orderedSelectedDots[0].transform.position;
        Vector3 point2 = orderedSelectedDots[1].transform.position;
        Vector3 point3 = orderedSelectedDots[2].transform.position;
        
        // Calculate the direction vector from point1 to point2
        Vector3 lineDirection = (point2 - point1);
        
        // Calculate point4 by adding the same direction vector to point3
        Vector3 point4 = point3 + lineDirection;
        
        // Create the original line (1 to 2)
        CreateLineBetweenDots(orderedSelectedDots[0], orderedSelectedDots[1]);
        
        // Create the parallel line (3 to 4)
        CreateLineBetweenSpecificPoints(point3, point4);
        
        // Create point 4
        CreatePointAtPosition(point4);
        
        Debug.Log("Created parallel line from " + orderedSelectedDots[2].name + " with same direction as " + orderedSelectedDots[0].name + "-" + orderedSelectedDots[1].name);
        ClearOrderedSelection();
    }
    
    private Vector3 GetClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;
        lineDirection.Normalize();
        
        Vector3 toPoint = point - lineStart;
        float projectionLength = Vector3.Dot(toPoint, lineDirection);
        
        // Clamp to line segment
        projectionLength = Mathf.Clamp(projectionLength, 0f, lineLength);
        
        return lineStart + lineDirection * projectionLength;
    }
    
    private void CreateLineBetweenSpecificPoints(Vector3 startPoint, Vector3 endPoint)
    {
        if (linePrefab != null)
        {
            GameObject newLine = Instantiate(linePrefab, drawingQuadTransform);
            LineRenderer lr = newLine.GetComponent<LineRenderer>();
            
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, startPoint);
                lr.SetPosition(1, endPoint);
            }
        }
    }
    
    private void CreatePointAtPosition(Vector3 position)
    {
        GameObject newPoint = Instantiate(pointPrefab, position, Quaternion.identity);
        newPoint.transform.SetParent(drawingQuadTransform, true);
        
        var col = newPoint.GetComponent<Collider>();
        if (!col) newPoint.AddComponent<SphereCollider>();
        
        pointCount++;
        newPoint.name = "sphere_" + pointCount;
        
        allPoints.Add(newPoint);
    }
}
