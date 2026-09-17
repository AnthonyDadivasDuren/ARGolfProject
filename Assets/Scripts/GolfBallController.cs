using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Collider ballCollider;
    [SerializeField] private float maxDragDistance = 0.3f;
    [SerializeField] private float maxShotSpeed = 2f;

    private Rigidbody body;
    private Plane aimPlane;
    private Vector3 dragStart;
    private bool aiming;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;

        if (pointer == null || aimCamera == null || ballCollider == null)
            return;

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

        if (aiming && pointer.press.wasReleasedThisFrame)
        {
            aiming = false;

            if (!aimPlane.Raycast(ray, out float distance))
                return;

            Vector3 drag = dragStart - ray.GetPoint(distance);
            drag.y = 0f;

            
            if (drag.magnitude < 0.01f)
                return;

            float power = Mathf.Clamp01(
                drag.magnitude / maxDragDistance);

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            body.AddForce(
                drag.normalized * power * maxShotSpeed,
                ForceMode.VelocityChange);
        }
    }

    private void OnDisable()
    {
        aiming = false;
    }
}