using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    public BoardManager boardManager;
    public Transform firstPosition;
    public Transform secondPosition;

    public float rotationSpeed = 5f; // Adjust the rotation speed as needed

    void Update()
    {
        if (boardManager != null)
        {
            if (boardManager.isWhiteTurn)
            {
                transform.position = Vector3.Lerp(transform.position, firstPosition.position, Time.deltaTime * rotationSpeed);
                transform.rotation = Quaternion.Euler(40.382f, 0f, 0f); // Set rotation to 0 degrees
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, secondPosition.position, Time.deltaTime * rotationSpeed);
                transform.rotation = Quaternion.Euler(40.382f, 180f, 0f); // Set rotation to 180 degrees
            }
        }
        else
        {
            Debug.LogWarning("BoardManager reference not set in CameraToggle script");
        }
    }
}
