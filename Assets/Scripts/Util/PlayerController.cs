using MEC;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0, 50)] public float walkSpeed = 4;
    [Range(0, 1)] public float runSpeed = 8;
    [Range(0, 1)] public float gravity = -12;
    [Range(0, 1)] public float jumpHeight = 1;

    [Range(0, 1)] public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    private float turnSmoothVelocity;
    private readonly float speedSmoothTime = 0.15f;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private readonly float standJumpDelay = .85f;
    private bool isJumping = false;
    private float velocityY;
    private float eulerY = 0;
    private bool isRotationFixed = false;
    private CharacterController controller;

    public Animator animator { get; set; }

    private Transform cameraTransform;

    [Header("Test")]
    public float moveMultiplier = 1;
    public bool isPathFinding = false;

    CinemachineTPSController cinemachineTPSController;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        cinemachineTPSController = FindObjectOfType<CinemachineTPSController>();

        cameraTransform = transform.Search("LookTarget");
    }

    private void Update()
    {
        if (cinemachineTPSController.isPanel)
        {
            moveInput = Vector3.zero;

            return;
        }

        if (!isPathFinding)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveMultiplier;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        //print("Current Frame from PlayerController : " + Time.frameCount.ToString());
        Move(moveInput, isRunning);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            isRotationFixed = true;

            eulerY = cameraTransform.eulerAngles.y;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotationFixed = false;
        }

        float movement = isRunning ? currentSpeed / runSpeed : currentSpeed / walkSpeed * 0.5f;

        if (movement < Define.THRESHOLD_MOVEMENT)
        {
            movement = 0f;
        }

        animator.SetFloat(Define.MOVEMENT, movement, speedSmoothTime, Time.deltaTime);
    }

    private void Move( Vector2 inputDir, bool running )
    {
        if (inputDir != Vector2.zero)
        {
            if (!isPathFinding)
            {
                float rotationY = isRotationFixed ? eulerY : cameraTransform.eulerAngles.y;

                float targetRotation = (Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg) + rotationY;

                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
            }
        }

        float targetSpeed = (running ? runSpeed : walkSpeed) * inputDir.magnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));
        //print("currentSpeed : " + currentSpeed);

        velocityY += Time.deltaTime * gravity;
        //print("Current Frame from PlayerController : " + Time.frameCount.ToString() + " " + Time.deltaTime);

        Vector3 velocity = (transform.forward * currentSpeed) + (Vector3.up * velocityY);

        _ = controller.Move(velocity * Time.deltaTime);

        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }

    private void Jump()
    {
        if (isJumping)
        {
            return;
        }

        _ = Timing.KillCoroutines(nameof(Co_Jump));

        _ = Timing.RunCoroutine(Co_Jump(), nameof(Co_Jump));

        isJumping = true;
    }

    private IEnumerator<float> Co_Jump()
    {
        float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
        float movement = animator.GetFloat(Define.MOVEMENT);

        animator.SetInteger(Define.JUMP, Define.JUMPRUN);

        velocityY = jumpVelocity;

        yield return Timing.WaitUntilTrue(() => velocityY == 0);

        animator.SetInteger(Define.JUMP, 0);

        isJumping = false;
    }

    private float GetModifiedSmoothTime( float _smoothTime )
    {
        if (controller.isGrounded)
        {
            return _smoothTime;
        }

        return airControlPercent == 0 ? float.MaxValue : _smoothTime / airControlPercent;
    }

    #region Movement

    public float rotationSpeed = 720f;
    public float stopDistance = 5;
    public Vector2 moveInput;
    private CoroutineHandle handle_move;
    private CoroutineHandle handle_rotate;

    public void FollowTarget( Transform _target )
    {
        _ = Timing.KillCoroutines(handle_move);
        _ = Timing.KillCoroutines(handle_rotate);

        handle_move = Timing.RunCoroutine(Co_FollowTarget(_target));
    }

    private IEnumerator<float> Co_FollowTarget( Transform _target )
    {
        stopDistance = 2f;

        TurnToTarget(_target, true);

        while (Vector3.Distance(transform.position, _target.transform.position) >= stopDistance)
        {
            yield return Timing.WaitForOneFrame;

            Vector2 moveDir = new Vector2(_target.position.x - transform.position.x, _target.position.y - transform.position.y).normalized * walkSpeed;

            moveInput = moveDir;
        }

        transform.position = _target.transform.position;

        moveInput = Vector2.zero;

        yield return Timing.WaitUntilTrue(() => Vector3.Distance(transform.position, _target.transform.position) >= stopDistance);

        yield return Timing.WaitForSeconds(.5f);

        FollowTarget(_target);
    }


    public void MoveToTarget( Transform _target )
    {
        isPathFinding = true;

        _ = Timing.KillCoroutines(handle_move);
        _ = Timing.KillCoroutines(handle_rotate);

        handle_move = Timing.RunCoroutine(Co_MoveToTarget(_target.position));
    }

    public void MoveToTarget( Vector3 _target )
    {
        isPathFinding = true;

        _ = Timing.KillCoroutines(handle_move);
        _ = Timing.KillCoroutines(handle_rotate);

        handle_move = Timing.RunCoroutine(Co_MoveToTarget(_target));
    }

    private IEnumerator<float> Co_MoveToTarget( Vector3 _target )
    {
        //TurnToTarget(_target);

        Vector2 moveDir = new Vector2(_target.x - transform.position.x, _target.y - transform.position.y).normalized * walkSpeed;

        moveInput = moveDir;

        yield return Timing.WaitUntilTrue(() => Vector3.Distance(transform.position, _target) <= stopDistance);

        moveInput = Vector2.zero;

        isPathFinding = false;
    }



    public void TurnToTarget( Transform _target, bool _isLookAt = false )
    {
        _ = Timing.KillCoroutines(handle_rotate);

        handle_rotate = Timing.RunCoroutine(Co_TurnToTarget(_target, _isLookAt));
    }

    private IEnumerator<float> Co_TurnToTarget( Transform _target, bool _isLookAt = false )
    {
        if (!lookTarget)
        {
            while (Quaternion.Angle(transform.rotation, _target.transform.rotation) > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _target.transform.rotation, rotationSpeed * Time.deltaTime);

                yield return Timing.WaitForOneFrame;
            }
        }

        if (!_isLookAt)
        {
            yield break;
        }

        while (true)
        {
            if (!lookTarget)
            {
                lookTarget = true;
            }

            Debug.Log("Looking Forever");

            transform.LookAt(_target);

            yield return Timing.WaitForOneFrame;
        }
    }

    private bool lookTarget = false;


    public void TurnToTarget( Vector3 _target )
    {
        _ = Timing.KillCoroutines(handle_rotate);

        handle_rotate = Timing.RunCoroutine(Co_TurnToTarget(_target));
    }

    private IEnumerator<float> Co_TurnToTarget( Vector3 _target )
    {
        Vector3 targetDir = _target - transform.position;
        Quaternion rotation = Quaternion.LookRotation(targetDir);

        while (Quaternion.Angle(transform.rotation, rotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

            yield return Timing.WaitForOneFrame;
        }
    }

    public void Stop()
    {
        _ = Timing.KillCoroutines(handle_move);
        _ = Timing.KillCoroutines(handle_rotate);

        moveInput = Vector2.zero;

        isPathFinding = false;

        transform.LookAt(null);
    }

	public void KillMovement() => Timing.KillCoroutines(nameof(Co_SetMovement));

	public void SetMovement(float _value = 0f)
	{
		Timing.KillCoroutines(nameof(Co_SetMovement));

		Timing.RunCoroutine(Co_SetMovement(_value), nameof(Co_SetMovement));
	}

	IEnumerator<float> Co_SetMovement(float _target = 0f)
	{
        var elapsedTime = 0f;

        while(!Equals(animator.GetFloat(Define.MOVEMENT), _target))
		{
            animator.SetFloat(Define.MOVEMENT, Mathf.Lerp(animator.GetFloat(Define.MOVEMENT), _target, elapsedTime += Time.deltaTime));

            yield return Timing.WaitForOneFrame;
		}

        animator.SetFloat(Define.MOVEMENT, _target);
    }

    #endregion
}