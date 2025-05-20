using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SecurityGuardAgent : Agent
{
    public Transform player;
    private Rigidbody rb;
    private float previousDistance;
    public float moveSpeed = 10f;

    private void Start()
    {
        Debug.Log("✅ SecurityGuardAgent script is running.");
    }

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("❌ Rigidbody is missing from SecurityGuardAgent.");
        }
    }

    public override void OnEpisodeBegin()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));

        if (player != null)
        {
            player.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
            previousDistance = Vector3.Distance(transform.position, player.position);
        }
        else
        {
            Debug.LogWarning("⚠️ Player Transform is not assigned!");
        }

        Debug.Log("🌀 New episode started.");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (player == null)
        {
            sensor.AddObservation(Vector3.zero); // direction placeholder
            sensor.AddObservation(0f);           // distance placeholder
        }
        else
        {
            Vector3 relativePos = player.position - transform.position;
            sensor.AddObservation(relativePos.normalized);                               // direction to player (3)
            sensor.AddObservation(Vector3.Distance(transform.position, player.position)); // distance to player (1)
        }

        sensor.AddObservation(rb != null ? rb.linearVelocity : Vector3.zero); // velocity (3)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);

        if (rb != null)
        {
            rb.AddForce(move * moveSpeed);

            // Optional: Clamp velocity to prevent sliding
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, moveSpeed);
        }

        if (player != null)
        {
            float currentDistance = Vector3.Distance(transform.position, player.position);

            if (currentDistance < previousDistance)
            {
                AddReward(0.002f);  // reward for getting closer
            }
            else
            {
                AddReward(-0.001f); // penalty for moving away
            }

            previousDistance = currentDistance;

            if (currentDistance < 1.5f)
            {
                SetReward(1.0f);
                Debug.Log("🏁 Player caught! Reward granted. Ending episode.");
                EndEpisode();
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }
}
