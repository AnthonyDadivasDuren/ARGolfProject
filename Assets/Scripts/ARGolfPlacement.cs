using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARGolfPlacement : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    [SerializeField] private Transform courseRoot;
    [SerializeField] private GameObject golfUI;
    [SerializeField] private GameObject placementUI;
    [SerializeField] private TMPro.TMP_Text statusText;
    private int tapCount;

    private readonly List<ARRaycastHit> hits = new();
    private bool placed;

    private void Update()
    {
        if (!placed && statusText != null)
        {
            statusText.text =
                $"Tracking: {ARSession.state}\n" +
                $"Reason: {ARSession.notTrackingReason}\n" +
                $"Surfaces: {planeManager.trackables.count}\n" +
                $"Taps: {tapCount}\n" +
                "Scan the floor, then tap.";
        }

        if (placed || ARSession.state != ARSessionState.SessionTracking)
            return;

        Pointer pointer = Pointer.current;

        if (pointer == null || !pointer.press.wasReleasedThisFrame)
            return;
        tapCount++;

        if (!raycastManager.Raycast(
                pointer.position.ReadValue(),
                hits,
                TrackableType.PlaneWithinPolygon))
        {
            return;
        }

        ARRaycastHit hit = hits[0];
        ARPlane plane = planeManager.GetPlane(hit.trackableId);

        if (plane == null ||
            plane.alignment != PlaneAlignment.HorizontalUp)
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(
            arCamera.transform.forward, Vector3.up);

        if (forward.sqrMagnitude < 0.01f)
            return;

        Quaternion rotation = Quaternion.LookRotation(
            forward.normalized, Vector3.up);

        courseRoot.SetPositionAndRotation(
            hit.pose.position, rotation);

        planeManager.enabled = false;

        foreach (ARPlane detectedPlane in planeManager.trackables)
            detectedPlane.gameObject.SetActive(false);

        Physics.SyncTransforms();

        placed = true;
        courseRoot.gameObject.SetActive(true);
        golfUI.SetActive(true);
        placementUI.SetActive(false);
    }
}