using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    public BoardManager boardManager;
    public Transform firstPosition;
    public Transform secondPosition;// Reference to the transform for rotation direction

    public float rotationSpeed = 5f; // Adjust the rotation speed as needed

    void Update()
    {
        if (boardManager != null)
        {
            if (boardManager.isWhiteTurn)
            {
                transform.position = Vector3.Lerp(transform.position, firstPosition.position, Time.deltaTime * rotationSpeed);

                Quaternion targetRotation = Quaternion.LookRotation(firstPosition.forward, firstPosition.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, secondPosition.position, Time.deltaTime * rotationSpeed);

                Quaternion targetRotation = Quaternion.LookRotation(secondPosition.forward, secondPosition.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            Debug.LogWarning("BoardManager reference not set in CameraToggle script");
        }
    }
}
