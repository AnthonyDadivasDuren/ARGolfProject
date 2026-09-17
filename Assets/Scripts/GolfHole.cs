using UnityEngine;

public class GolfHole : MonoBehaviour
{
    [SerializeField] private GolfBallController ball;
    [SerializeField] private float settleTime = 0.4f;
    [SerializeField] private float maximumSpeed = 0.15f;

    private Rigidbody ballBody;
    private float settledFor;

    private void Start()
    {
        if (ball != null)
            ballBody = ball.GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (ballBody == null || ball.HoleComplete)
            return;

        if (other.attachedRigidbody != ballBody)
            return;

        float localHeight =
            transform.InverseTransformPoint(ballBody.position).y;

        bool insideCup = localHeight < -0.01f;
        bool movingSlowly =
            ballBody.linearVelocity.magnitude < maximumSpeed;

        if (!insideCup || !movingSlowly)
        {
            settledFor = 0f;
            return;
        }

        settledFor += Time.fixedDeltaTime;

        if (settledFor >= settleTime)
            ball.CompleteHole();
    }

    private void OnTriggerExit(Collider other)
    {
        if (ballBody != null &&
            other.attachedRigidbody == ballBody)
        {
            settledFor = 0f;
        }
    }

    public void ResetDetection()
    {
        settledFor = 0f;
    }
}