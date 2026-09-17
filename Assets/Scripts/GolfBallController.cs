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

    [SerializeField] private float touchRadiusFraction = 0.06f;

    private Vector3 startingLocalPosition;
    private Quaternion startingLocalRotation;

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
        {
            SaveSafePosition();

            startingLocalPosition = lastSafeLocalPosition;
            startingLocalRotation = lastSafeLocalRotation;
        }

        if (aimLine != null)
        {
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 2;
            aimLine.loop = false;
            float courseScale = courseRoot != null
                 ? Mathf.Abs(courseRoot.lossyScale.x)
                 : 1f;

            aimLine.startWidth = 0.012f * courseScale;
            aimLine.endWidth = 0.004f * courseScale;
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

            Vector3 ballScreenPosition =
                    aimCamera.WorldToScreenPoint(ballCollider.bounds.center);

            // Don't allow selecting a ball behind the camera.
            if (ballScreenPosition.z <= 0f)
                return;

            float touchRadius =
                Mathf.Min(Screen.width, Screen.height) * touchRadiusFraction;

            Vector2 pointerPosition = pointer.position.ReadValue();
            Vector2 ballPosition = new Vector2(
                ballScreenPosition.x, ballScreenPosition.y);

            bool touchedBall = ballCollider.Raycast(ray, out _, 100f);
            bool touchedNearBall =
                Vector2.Distance(pointerPosition, ballPosition) <= touchRadius;

            if (!touchedBall && !touchedNearBall)
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

        
        if (aimLine != null)
        {
            aimLine.enabled = validShot;

            float courseScale = courseRoot != null
                    ? Mathf.Abs(courseRoot.lossyScale.x)
                    : 1f;

            Vector3 start =
                body.position + Vector3.up * (0.005f * courseScale);
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

    public void RestartHole()
    {
        CancelAim();

        HoleComplete = false;
        StrokeCount = 0;

        body.isKinematic = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        lastSafeLocalPosition = startingLocalPosition;
        lastSafeLocalRotation = startingLocalRotation;

        body.position =
            courseRoot.TransformPoint(startingLocalPosition);

        body.rotation =
            courseRoot.rotation * startingLocalRotation;

        body.WakeUp();
    }
}