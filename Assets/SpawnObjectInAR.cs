using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class SpawnObjectInAR : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private PlacementManager2 placementManager; // Reference to PlacementManager2

    private bool hasSpawned = false;
    private bool isPlacementMode = true;

    void Start()
    {
        // Ensure that the ARRaycastManager, ARPlaneManager, and PlacementManager2 are assigned in the Unity Editor.
        if (arRaycastManager == null || arPlaneManager == null || placementManager == null)
        {
            Debug.LogError("AR Raycast Manager, AR Plane Manager, or Placement Manager2 is not assigned!");
        }
    }

    void Update()
    {
        // Check if in placement mode and the user taps the screen
        if (isPlacementMode && !hasSpawned && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Check if the touch is over a UI element
            if (IsPointerOverUI(Input.GetTouch(0)))
            {
                return; // Do not proceed with spawning if touch is over UI
            }

            // Raycast from the screen point touched by the user
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            // Perform the raycast
            if (arRaycastManager.Raycast(ray, hits, TrackableType.Planes))
            {
                // Get the pose of the hit
                Pose hitPose = hits[0].pose;

                // Update the crosshair position and rotation only during placement mode
                UpdateCrosshair(hitPose.position);

                // Spawn the object on the detected AR plane
                SpawnObjectOnPlane(hitPose.position, hitPose.rotation, hits[0].trackableId);

                // Set the flag to true to indicate that an object has been spawned
                hasSpawned = true;

                // Set placement mode to false after placement
                isPlacementMode = false;

                // Disable the indicator in the PlacementManager2
                placementManager.DisableIndicator();
            }
        }
    }

    void UpdateCrosshair(Vector3 position)
    {
        // Update the crosshair position to point to the floor
        crosshair.transform.position = new Vector3(position.x, 0, position.z);

        // Rotate the crosshair to face the camera only during placement mode
        if (isPlacementMode)
        {
            crosshair.transform.LookAt(Camera.main.transform);
            crosshair.transform.eulerAngles = new Vector3(90, crosshair.transform.eulerAngles.y, 0);
        }
        else
        {
            // Make the crosshair invisible after placement
            crosshair.SetActive(false);
        }
    }

    void SpawnObjectOnPlane(Vector3 position, Quaternion rotation, TrackableId planeId)
    {
        // Find the ARPlane associated with the detected plane ID
        ARPlane arPlane = arPlaneManager.GetPlane(planeId);

        if (arPlane != null)
        {
            // Spawn the object on the ARPlane
            GameObject spawnedObject = Instantiate(objectToSpawn, position, rotation);

            // Set the object's position to be slightly above the plane to avoid z-fighting
            spawnedObject.transform.position = arPlane.transform.position + Vector3.up * 0.01f;

            crosshair.SetActive(false);
        }
    }

    bool IsPointerOverUI(Touch touch)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
