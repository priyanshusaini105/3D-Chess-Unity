using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementManager2 : MonoBehaviour
{
    public ARRaycastManager raymanager;
    public GameObject PointerObj;

    void Start()
    {
        raymanager = FindObjectOfType<ARRaycastManager>();
        PointerObj = transform.GetChild(0).gameObject; // Changed this line to use transform.GetChild(0)
        PointerObj.SetActive(false);
    }

    void Update()
    {
        List<ARRaycastHit> hitpoint = new List<ARRaycastHit>();
        raymanager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hitpoint, TrackableType.Planes);
        if (hitpoint.Count > 0)
        {
            transform.position = hitpoint[0].pose.position;
            transform.rotation = hitpoint[0].pose.rotation;
            if (!PointerObj.activeInHierarchy)
            {
                PointerObj.SetActive(true);
            }
        }
    }

    public void DisableIndicator()
    {
        PointerObj.SetActive(false);
    }
}