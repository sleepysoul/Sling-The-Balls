using UnityEngine;

public class MouseDragTrajectory : MonoBehaviour
{
    public GameObject trajectoryPointPrefab;
    public Transform launchPoint;
    public int numberOfPoints = 20;
    public float timeInterval = 0.1f;

    private GameObject[] trajectoryPoints;
    private bool isDragging;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private Vector2 launchDirection;

    private void Start()
    {
        trajectoryPoints = new GameObject[numberOfPoints];
        for (int i = 0;i < numberOfPoints;i++) {
            trajectoryPoints[i] = Instantiate(trajectoryPointPrefab, launchPoint.position, Quaternion.identity);
            trajectoryPoints[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            isDragging = true;
            dragStartPos = GetMouseWorldPosition();
        }
        else if (Input.GetMouseButtonUp(0)) {
            if (isDragging) {
                isDragging = false;
                dragEndPos = GetMouseWorldPosition();
                CalculateTrajectory();
                ClearTrajectory();
            }
        }

        if (isDragging) {
            dragStartPos = GetMouseWorldPosition();
            CalculateTrajectory();
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void CalculateTrajectory()
    {
        Vector2 direction = (dragStartPos - dragEndPos).normalized;
        float magnitude = (dragStartPos - dragEndPos).magnitude;
        launchDirection = direction;

        float timeStep = timeInterval;

        for (int i = 0;i < numberOfPoints;i++) {
            float time = timeStep * i;
            float x = direction.x * magnitude * time;
            float y = direction.y * magnitude * time - 0.5f * Physics2D.gravity.magnitude * time * time;

            Vector2 newPos = new Vector2(x, y) + (Vector2)launchPoint.position;
            trajectoryPoints[i].transform.position = newPos;
            trajectoryPoints[i].SetActive(true);
        }
    }

    private void ClearTrajectory()
    {
        foreach (var point in trajectoryPoints) {
            point.SetActive(false);
        }
    }
}
