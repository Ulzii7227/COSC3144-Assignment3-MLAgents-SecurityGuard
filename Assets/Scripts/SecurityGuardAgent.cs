using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SecurityGuardAgent : Agent
{
    public Transform player;
    private Rigidbody rb;

    private void Start()
    {
        Debug.Log("SecurityGuardAgent script is running.");
    }

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Stop any residual velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset SecurityGuard position
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));

        // Reset Player position if assigned
        if (player != null)
        {
            player.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        }

        Debug.Log("New episode started.");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (player == null)
        {
            sensor.AddObservation(Vector3.zero); // placeholder direction
            sensor.AddObservation(0f);           // placeholder distance
        }
        else
        {
            Vector3 relativePos = player.position - transform.position;

            sensor.AddObservation(relativePos.normalized);                         // direction (3)
            sensor.AddObservation(Vector3.Distance(player.position, transform.position)); // distance (1)
        }

        sensor.AddObservation(rb.linearVelocity); // agent velocity (3)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        rb.AddForce(move * 5f);

        Debug.Log($"Actions received: {move}");

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < 1.5f)
            {
                SetReward(1.0f);  // Success
                Debug.Log("Player caught! Episode ended with reward.");
                EndEpisode();
            }
            else
            {
                AddReward(-0.001f);  // Time penalty
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
