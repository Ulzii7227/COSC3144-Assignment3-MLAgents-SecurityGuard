using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SecurityGuardAgent : Agent
{
    public Transform playerTransform;
    public float moveSpeed = 5f;
    public float rotationSpeed = 300f;
    private Rigidbody rb;
    private Vector3 startPos;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        // Reset positions randomly
        transform.position = startPos + new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));
        rb.linearVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        playerTransform.position = startPos + new Vector3(Random.Range(5f, 8f), 0, Random.Range(5f, 8f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 toPlayer = playerTransform.position - transform.position;

        // Normalize to keep vector magnitude < 1
        sensor.AddObservation(toPlayer.normalized);
        sensor.AddObservation(Vector3.Distance(transform.position, playerTransform.position));
        sensor.AddObservation(transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Vector3 moveDir = new Vector3(moveX, 0, moveZ);

        rb.MovePosition(transform.position + moveDir * moveSpeed * Time.deltaTime);

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        AddReward(-0.001f);  // time penalty to encourage speed

        if (distToPlayer < 1.5f)
        {
            SetReward(1f);   // successfully caught player
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxis("Horizontal");
        contActions[1] = Input.GetAxis("Vertical");
    }
}
