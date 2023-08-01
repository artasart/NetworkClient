using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadReckoning : MonoBehaviour
{
	private Vector3 targetPosition;
	private float moveSpeed = 5f;

    [Header("Network")]
    private float sendInterval = 1f;
	private float lastSendTime;

	[Header("DeadReckoning")]
	private bool isMoving;
	private bool isDirectionChanged;
	private float directionChangeTime;

	private void Start()
	{
		targetPosition = transform.position;
		isMoving = false;
		isDirectionChanged = false;
		lastSendTime = Time.time;
    }

    private void Update()
    {
        if (isMoving)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }

        if (Time.time - lastSendTime >= sendInterval)
        {
            SendTransform();

            lastSendTime = Time.time;
        }
    }

    private void SendTransform()
    {        

    }

    public void HandleInput(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W:
                StartMoving();
                break;
            case KeyCode.S:
                StartMoving();
                break;
            case KeyCode.A:
                StartMoving();
                break;
            case KeyCode.D:
                StartMoving();
                break;
            case KeyCode.Space:
                isDirectionChanged = true;
                directionChangeTime = Time.time;
                break;
            default:
                StopMoving();
                break;
        }
    }

    private void StartMoving()
    {
        if (!isMoving)
        {
            isMoving = true;
            targetPosition = transform.position + transform.forward;
        }
    }

    private void StopMoving()
    {
        if (isMoving)
        {
            isMoving = false;
            targetPosition = transform.position;
        }
    }

    public void HandlePositionAndDirection(Vector3 newPosition, bool directionChanged)
    {
        // write Set Transform here.


        if (isMoving)
        {
            float timeSinceDirectionChange = Time.time - directionChangeTime;
            float lerpFactor = timeSinceDirectionChange / sendInterval;
            Vector3 predictedPosition = Vector3.Lerp(transform.position, newPosition, lerpFactor);
            transform.position = predictedPosition;
        }

        else
        {
            transform.position = newPosition;
        }

        isDirectionChanged = directionChanged;
        directionChangeTime = Time.time;
    }
}
