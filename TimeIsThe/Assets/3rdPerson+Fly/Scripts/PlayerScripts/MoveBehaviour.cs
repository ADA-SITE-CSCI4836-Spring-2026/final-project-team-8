using UnityEngine;

public class MoveBehaviour : GenericBehaviour
{
    public float walkSpeed = 0.15f;
    public float runSpeed = 1.0f;
    public float sprintSpeed = 2.0f;
    public float speedDampTime = 0.1f;
    public string jumpButton = "Jump";
    public float jumpHeight = 1.5f;

    [Header("Better Movement")]
    public float walkVelocity = 2f;
    public float runVelocity = 4f;
    public float sprintVelocity = 6f;
    public float airControl = 0.6f;

    private float speed, speedSeeker;
    private int jumpBool;
    private int groundedBool;
    private bool jump;
    private bool isColliding;

    void Start()
    {
        jumpBool = Animator.StringToHash("Jump");
        groundedBool = Animator.StringToHash("Grounded");
        behaviourManager.GetAnim.SetBool(groundedBool, true);

        behaviourManager.SubscribeBehaviour(this);
        behaviourManager.RegisterDefaultBehaviour(this.behaviourCode);
        speedSeeker = runSpeed;
    }

    void Update()
    {
        if (!jump && Input.GetButtonDown(jumpButton) &&
            behaviourManager.IsCurrentBehaviour(this.behaviourCode) &&
            !behaviourManager.IsOverriding())
        {
            jump = true;
        }
    }

    public override void LocalFixedUpdate()
    {
        MovementManagement(behaviourManager.GetH, behaviourManager.GetV);
        JumpManagement();
    }

    void JumpManagement()
    {
        if (jump && !behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.IsGrounded())
        {
            behaviourManager.LockTempBehaviour(this.behaviourCode);
            behaviourManager.GetAnim.SetBool(jumpBool, true);

            if (behaviourManager.GetAnim.GetFloat(speedFloat) > 0.1f)
            {
                GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
                GetComponent<CapsuleCollider>().material.staticFriction = 0f;

                RemoveVerticalVelocity();

                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);

                behaviourManager.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
            else
            {
                RemoveVerticalVelocity();

                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);

                behaviourManager.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
        }
        else if (behaviourManager.GetAnim.GetBool(jumpBool))
        {
            if ((behaviourManager.GetRigidBody.velocity.y < 0) && behaviourManager.IsGrounded())
            {
                behaviourManager.GetAnim.SetBool(groundedBool, true);

                GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
                GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;

                jump = false;
                behaviourManager.GetAnim.SetBool(jumpBool, false);
                behaviourManager.UnlockTempBehaviour(this.behaviourCode);
            }
        }
    }

    void MovementManagement(float horizontal, float vertical)
    {
        if (behaviourManager.IsGrounded())
        {
            behaviourManager.GetRigidBody.useGravity = true;
        }
        else if (!behaviourManager.GetAnim.GetBool(jumpBool) &&
                 behaviourManager.GetRigidBody.velocity.y > 0)
        {
            RemoveVerticalVelocity();
        }

        Vector3 targetDirection = Rotating(horizontal, vertical);

        Vector2 input = new Vector2(horizontal, vertical);
        float inputMagnitude = Vector2.ClampMagnitude(input, 1f).magnitude;

        float currentMoveSpeed = runVelocity;

        if (speedSeeker <= walkSpeed + 0.01f)
            currentMoveSpeed = walkVelocity;

        if (behaviourManager.IsSprinting())
            currentMoveSpeed = sprintVelocity;

        if (inputMagnitude < 0.1f)
            currentMoveSpeed = 0f;

        Vector3 targetVelocity = targetDirection.normalized * currentMoveSpeed * inputMagnitude;
        Vector3 currentVelocity = behaviourManager.GetRigidBody.velocity;

        float control = behaviourManager.IsGrounded() ? 1f : airControl;

        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        Vector3 newHorizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            targetVelocity,
            control
        );

        behaviourManager.GetRigidBody.velocity = new Vector3(
            newHorizontalVelocity.x,
            currentVelocity.y,
            newHorizontalVelocity.z
        );

        speed = inputMagnitude;

        if (behaviourManager.IsSprinting())
            speed = sprintSpeed;
        else
            speed *= speedSeeker;

        behaviourManager.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
    }

    private void RemoveVerticalVelocity()
    {
        Vector3 horizontalVelocity = behaviourManager.GetRigidBody.velocity;
        horizontalVelocity.y = 0;
        behaviourManager.GetRigidBody.velocity = horizontalVelocity;
    }

    Vector3 Rotating(float horizontal, float vertical)
    {
        Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);

        forward.y = 0.0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 targetDirection = forward * vertical + right * horizontal;

        if (behaviourManager.IsMoving() && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Quaternion newRotation = Quaternion.Slerp(
                behaviourManager.GetRigidBody.rotation,
                targetRotation,
                behaviourManager.turnSmoothing
            );

            behaviourManager.GetRigidBody.MoveRotation(newRotation);
            behaviourManager.SetLastDirection(targetDirection);
        }

        if (!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
        {
            behaviourManager.Repositioning();
        }

        return targetDirection;
    }

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;

        if (behaviourManager.IsCurrentBehaviour(this.GetBehaviourCode()) &&
            collision.GetContact(0).normal.y <= 0.1f)
        {
            GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
            GetComponent<CapsuleCollider>().material.staticFriction = 0f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;

        GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
        GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
    }
}