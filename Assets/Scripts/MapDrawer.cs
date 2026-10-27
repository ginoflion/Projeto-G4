using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MapDrawer : MonoBehaviour
{
    public Camera mapCamera; 
    public GameObject linePrefab; 
    public LayerMask mapLayer; 
    public bool isDrawingModeActive = false; 
    
    public enum Tool { Lapis, Borracha }
    public Tool currentTool = Tool.Lapis;
    public float eraserRadius = 0.02f;

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();
    
    private List<LineRenderer> allLines = new List<LineRenderer>();

    void Update()
    {
        if (!isDrawingModeActive) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) currentTool = Tool.Lapis;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) currentTool = Tool.Borracha;

        if (Mouse.current.leftButton.wasPressedThisFrame) StartAction();
        else if (Mouse.current.leftButton.isPressed) ContinueAction();
        else if (Mouse.current.leftButton.wasReleasedThisFrame) EndAction();
    }

    private void StartAction()
    {
        if (currentTool == Tool.Lapis)
        {
            Ray ray = mapCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mapLayer))
            {
                GameObject newLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                currentLine = newLine.GetComponent<LineRenderer>();
                points.Clear();
                AddPoint(hit.point + hit.normal * 0.005f); 
            }
        }
    }

    private void ContinueAction()
    {
        Ray ray = mapCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (currentTool == Tool.Lapis && currentLine != null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mapLayer))
            {
                Vector3 drawPoint = hit.point + hit.normal * 0.005f;
                if (Vector3.Distance(points[points.Count - 1], drawPoint) > 0.005f)
                {
                    AddPoint(drawPoint);
                }
            }
        }
        else if (currentTool == Tool.Borracha)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mapLayer))
            {
                EraseNearPoint(hit.point);
            }
        }
    }

    private void EndAction()
    {
        if (currentTool == Tool.Lapis && currentLine != null)
        {
            allLines.Add(currentLine);
            currentLine = null;
        }
    }

    private void AddPoint(Vector3 point)
    {
        points.Add(point);
        currentLine.positionCount = points.Count;
        currentLine.SetPosition(points.Count - 1, point);
    }

    private void EraseNearPoint(Vector3 eraserPosition)
    {
        for (int i = allLines.Count - 1; i >= 0; i--)
        {
            LineRenderer lr = allLines[i];
            if (lr == null) continue;

            Vector3[] linePoints = new Vector3[lr.positionCount];
            lr.GetPositions(linePoints);

            List<List<Vector3>> newSegments = new List<List<Vector3>>();
            List<Vector3> currentSegment = new List<Vector3>();

            bool wasModified = false;

            for (int p = 0; p < linePoints.Length; p++)
            {
                if (Vector3.Distance(linePoints[p], eraserPosition) > eraserRadius)
                {
                    currentSegment.Add(linePoints[p]);
                }
                else
                {
                    wasModified = true;
                    if (currentSegment.Count > 0)
                    {
                        newSegments.Add(new List<Vector3>(currentSegment));
                        currentSegment.Clear();
                    }
                }
            }

            if (currentSegment.Count > 0)
            {
                newSegments.Add(currentSegment);
            }
            if (wasModified)
            {
                allLines.RemoveAt(i);
                Destroy(lr.gameObject);
                foreach (List<Vector3> segment in newSegments)
                {
                    if (segment.Count > 1) 
                    {
                        GameObject newLineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
                        LineRenderer newLr = newLineObj.GetComponent<LineRenderer>();
                        newLr.positionCount = segment.Count;
                        newLr.SetPositions(segment.ToArray());
                        allLines.Add(newLr);
                    }
                }
            }
        }
    }
}