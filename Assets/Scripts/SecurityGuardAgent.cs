using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SecurityGuardAgent : Agent
{
    public Transform player;
    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Temporarily use fixed positions to ensure visibility
        transform.localPosition = new Vector3(0f, 0.5f, -2f); // SecurityGuard
        player.localPosition = new Vector3(0f, 0.5f, 2f);     // Player
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 relativePos = player.position - transform.position;

        sensor.AddObservation(relativePos.normalized); // direction to player (3 floats)
        sensor.AddObservation(Vector3.Distance(player.position, transform.position)); // distance (1 float)
        sensor.AddObservation(rb.linearVelocity); // velocity (3 floats)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        rb.AddForce(move * 5f);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 1.5f)
        {
            SetReward(1.0f);  // Success: capture
            EndEpisode();
        }
        else
        {
            AddReward(-0.001f);  // Time penalty
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }
}
