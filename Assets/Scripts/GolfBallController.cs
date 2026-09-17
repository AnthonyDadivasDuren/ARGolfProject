using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Collider ballCollider;
    [SerializeField] private LineRenderer aimLine;

    [SerializeField] private float maxDragDistance = 0.3f;
    [SerializeField] private float maxShotSpeed = 2f;
    [SerializeField] private float maxLineLength = 0.6f;

    [SerializeField] private Transform courseRoot;
    [SerializeField] private float fallDepth = 0.25f;

    private Vector3 lastSafeLocalPosition;
    private Quaternion lastSafeLocalRotation;

    private Rigidbody body;
    private Plane aimPlane;
    private Vector3 dragStart;
    private bool aiming;

    public int StrokeCount { get; private set; }

    public bool HoleComplete { get; private set; }

    public void CompleteHole()
    {
        if (HoleComplete)
            return;

        HoleComplete = true;
        CancelAim();

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;

        Debug.Log($"Hole complete! Strokes: {StrokeCount}");
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        if (courseRoot != null)
            SaveSafePosition();

        if (aimLine != null)
        {
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 2;
            aimLine.loop = false;
            aimLine.startWidth = 0.012f;
            aimLine.endWidth = 0.004f;
            aimLine.enabled = false;
        }
    }

    private void Update()
    {
        if (HoleComplete)
            return;

        if (courseRoot != null)
        {
            float heightAboveCourse =
                courseRoot.InverseTransformPoint(body.position).y;

            if (heightAboveCourse < -fallDepth)
            {
                ReturnToSafePosition();
                return;
            }
        }

        Pointer pointer = Pointer.current;

        if (pointer == null || aimCamera == null || ballCollider == null)
        {
            CancelAim();
            return;
        }

        Ray ray = aimCamera.ScreenPointToRay(
            pointer.position.ReadValue());

        if (pointer.press.wasPressedThisFrame)
        {
            if (body.linearVelocity.sqrMagnitude > 0.0004f)
                return;

            if (!ballCollider.Raycast(ray, out _, 100f))
                return;

            aimPlane = new Plane(Vector3.up, body.position);

            if (!aimPlane.Raycast(ray, out float distance))
                return;

            dragStart = ray.GetPoint(distance);
            aiming = true;
        }

        if (!aiming)
            return;

        if (!aimPlane.Raycast(ray, out float dragDistance))
        {
            CancelAim();
            return;
        }

        Vector3 drag = dragStart - ray.GetPoint(dragDistance);
        drag.y = 0f;

        float power = Mathf.Clamp01(
            drag.magnitude / Mathf.Max(maxDragDistance, 0.01f));

        power *= power;

        bool validShot = drag.magnitude >= 0.01f;

        // Preview the same direction and power used for the shot.
        if (aimLine != null)
        {
            aimLine.enabled = validShot;

            Vector3 start = body.position + Vector3.up * 0.005f;
            Vector3 end = start
                + drag.normalized * power * maxLineLength;

            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            CancelAim();

            if (!validShot)
                return;

            if (courseRoot != null)
                SaveSafePosition();

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Vector3 shotVelocity = drag.normalized * power * maxShotSpeed;

            body.linearVelocity = shotVelocity;

            StrokeCount++;
            Debug.Log($"Strokes: {StrokeCount}");

            // Start with matching rolling motion instead of sliding.
            float radius = ballCollider.bounds.extents.y;

            body.angularVelocity =
                Vector3.Cross(Vector3.up, shotVelocity) / Mathf.Max(radius, 0.001f);
        }
        else if (!pointer.press.isPressed)
        {
            CancelAim();
        }
    }

    private void CancelAim()
    {
        aiming = false;

        if (aimLine != null)
            aimLine.enabled = false;
    }

    private void OnDisable()
    {
        CancelAim();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            CancelAim();
    }

    private void SaveSafePosition()
    {
        lastSafeLocalPosition =
            courseRoot.InverseTransformPoint(body.position);

        lastSafeLocalRotation =
            Quaternion.Inverse(courseRoot.rotation) * body.rotation;
    }

    private void ReturnToSafePosition()
    {
        CancelAim();

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        body.position =
            courseRoot.TransformPoint(lastSafeLocalPosition);

        body.rotation =
            courseRoot.rotation * lastSafeLocalRotation;

        body.WakeUp();
    }
}