using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class GolfRollingResistance : MonoBehaviour
{
    [SerializeField] private LayerMask grassLayers;
    [SerializeField] private float deceleration = 0.8f;
    [SerializeField] private float stopSpeed = 0.02f;

    private Rigidbody body;
    private SphereCollider sphere;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();

        body.maxAngularVelocity = 150f;
    }

    private void FixedUpdate()
    {
        Vector3 centre = sphere.bounds.center;
        float radius = sphere.bounds.extents.y;

        bool onGrass = Physics.Raycast(
            centre,
            Vector3.down,
            out RaycastHit hit,
            radius + 0.003f,
            grassLayers,
            QueryTriggerInteraction.Ignore);

        if (!onGrass || Vector3.Dot(hit.normal, Vector3.up) < 0.9f)
            return;

        Vector3 velocity = body.linearVelocity;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float speed = horizontal.magnitude;

        if (speed == 0f)
            return;

        float newSpeed = Mathf.Max(
            0f, speed - deceleration * Time.fixedDeltaTime);

        if (newSpeed < stopSpeed)
            newSpeed = 0f;

        float ratio = newSpeed / speed;
        horizontal *= ratio;

        body.linearVelocity = new Vector3(
            horizontal.x, velocity.y, horizontal.z);

        body.angularVelocity *= ratio;
    }
}