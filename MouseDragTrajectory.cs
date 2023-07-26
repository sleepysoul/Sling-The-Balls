using UnityEngine;

public class MouseDragTrajectory : MonoBehaviour
{
    public GameObject trajectoryPointPrefab;
    public Rigidbody projectilePrefab;
    public int numberOfPoints = 20;
    public float timeInterval = 0.1f;
    public float initialVelocity = 10f;

    private GameObject[] trajectoryPoints;
    private bool isDragging;
    private Vector3 dragStartPos;
    private Vector3 dragEndPos;

    private void Start()
    {
        trajectoryPoints = new GameObject[numberOfPoints];
        for (int i = 0;i < numberOfPoints;i++) {
            trajectoryPoints[i] = Instantiate(trajectoryPointPrefab, transform.position, Quaternion.identity);
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
            isDragging = false;
            dragEndPos = GetMouseWorldPosition();
            CalculateTrajectory();
        }

        if (isDragging) {
            dragEndPos = GetMouseWorldPosition();
            CalculateTrajectory();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void CalculateTrajectory()
    {
        Vector3 direction = (dragEndPos - dragStartPos).normalized;
        float magnitude = (dragEndPos - dragStartPos).magnitude;

        float timeStep = timeInterval;
        float velocityMagnitude = initialVelocity * magnitude;

        for (int i = 0;i < numberOfPoints;i++) {
            float time = timeStep * i;
            float x = direction.x * velocityMagnitude * time;
            float y = direction.y * velocityMagnitude * time - 0.5f * Physics.gravity.magnitude * time * time;
            float z = direction.z * velocityMagnitude * time;

            Vector3 newPos = new Vector3(x, y, z) + transform.position;
            trajectoryPoints[i].transform.position = newPos;
            trajectoryPoints[i].SetActive(true);
        }
    }

    private void ShootProjectile()
    {
        Vector3 direction = (dragEndPos - dragStartPos).normalized;
        float magnitude = (dragEndPos - dragStartPos).magnitude;

        Vector3 initialVelocityVector = direction * initialVelocity * magnitude;
        Rigidbody projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.velocity = initialVelocityVector;
    }

    public void OnMouseUp()
    {
        if (isDragging) {
            ShootProjectile();
            isDragging = false;
            ClearTrajectory();
        }
    }

    private void ClearTrajectory()
    {
        foreach (var point in trajectoryPoints) {
            point.SetActive(false);
        }
    }
}
