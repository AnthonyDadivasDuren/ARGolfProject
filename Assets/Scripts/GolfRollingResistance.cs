using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class GolfRollingResistance : MonoBehaviour
{
    [SerializeField] private LayerMask grassLayers;
    [SerializeField] private float deceleration = 0.8f;
    [SerializeField] private float stopSpeed = 0.02f;

    private Rigidbody body;
    private bool touchingGrass;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.maxAngularVelocity = 150f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckGrassContact(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckGrassContact(collision);
    }

    private void CheckGrassContact(Collision collision)
    {
        int layer = collision.gameObject.layer;

        if ((grassLayers.value & (1 << layer)) == 0)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (Vector3.Dot(contact.normal, Vector3.up) > 0.9f)
            {
                touchingGrass = true;
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        bool supported = touchingGrass;
        touchingGrass = false;

        if (body.IsSleeping() || !supported)
            return;

        Vector3 velocity = body.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        float speed = horizontal.magnitude;

        float newSpeed = Mathf.Max(
            0f, speed - deceleration * Time.fixedDeltaTime);

        if (newSpeed <= stopSpeed)
        {
            body.linearVelocity = new Vector3(0f, velocity.y, 0f);
            body.angularVelocity = Vector3.zero;

            if (Mathf.Abs(velocity.y) < 0.05f)
            {
                body.linearVelocity = Vector3.zero;
                body.Sleep();
            }

            return;
        }

        float ratio = newSpeed / speed;

        body.linearVelocity = new Vector3(
            horizontal.x * ratio,
            velocity.y,
            horizontal.z * ratio);

        body.angularVelocity *= ratio;
    }

    private void OnDisable()
    {
        touchingGrass = false;
    }
}