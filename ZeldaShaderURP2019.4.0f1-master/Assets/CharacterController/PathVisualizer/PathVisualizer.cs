using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class PathVisualizer : MonoBehaviour {
    public NavMeshAgent agent;
    public int maxPositions = 100;
    public float lineWidth = 0.2f;
    public Color lineColor = Color.blue;

    private LineRenderer lineRenderer;

    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material.color = lineColor;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                //if (hit.collider.CompareTag("Ground")) // Assuming the ground has a tag "Ground"
                //{
                //    // Set the destination for the NavMeshAgent
                //    agent.SetDestination(hit.point);
                //}
                agent.SetDestination(hit.point);
            }
        }

        if (agent != null && agent.hasPath) {
            NavMeshPath path = agent.path;

            if (path.corners.Length > 0) {
                lineRenderer.positionCount = Mathf.Min(path.corners.Length, maxPositions);

                for (int i = 0; i < lineRenderer.positionCount; i++) {
                    lineRenderer.SetPosition(i, path.corners[i]);
                }
            }
        } else {
            lineRenderer.positionCount = 0;
        }
    }
}
