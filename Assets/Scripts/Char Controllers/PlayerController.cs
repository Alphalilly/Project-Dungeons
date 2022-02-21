using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum MovementDirection
    {
        Forward,
        Backward,
        Left,
        Right
    }

    public enum MovementMode
    {
        Idle,
        Running,
        Sprinting,
        Jumping,
        Falling
    }

    public Animator animator;
    public GameObject gameCamera;
    public LayerMask body;
    public LayerMask interactable;
    public int runSpeed = 5;
    public int sprintSpeed = 10;

    private enum BlendState
    {
        Idle_Running_Sprinting,
        Jumping,
        Falling
    }

    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 7f;

    private float maxInensity;
    private MovementMode movementMode;
    private Rigidbody rb;
    private float moveIntensity = 0.0f;
    private float velocityAcceleration = 8.0f;
    private float velocityDeceleration = 20.0f;
    private float playerRotationSpeed = 650;
    private float jumpHeight = 4f;
    private KeyCode forwardInput = KeyCode.W;
    private KeyCode backwardInput = KeyCode.S;
    private KeyCode leftInput = KeyCode.A;
    private KeyCode rightInput = KeyCode.D;
    private KeyCode jumpInput = KeyCode.Space;
    private KeyCode sprintInput = KeyCode.LeftShift;
    private KeyCode interactInput = KeyCode.E;
    private float attackBlend;
    private float attackBlendAcceleration = 10.0f;
    private float attackBlendDeceleration = 3.5f;
    private float attackTimer;
    private Vector3 moveDirection;
    private float jumpTimeDuration = 1.34f;
    private float jumpTimer;
    private float rayRange = 0.85f;
    private RaycastHit rayHit;
    private PlayerStats playerStats;
    Collider[] hitColInteraction;
    private bool canMove;

    public bool CanMove { set{ canMove = value; } } 

    void Start()
    {
        playerStats = transform.GetComponent<PlayerStats>();
        rb = this.GetComponent<Rigidbody>();
        movementMode = MovementMode.Idle;
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        //player rotation
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, playerRotationSpeed * Time.deltaTime);
        }

        //Cam movement/placement
        gameCamera.transform.localEulerAngles = new Vector3(50, -45, 0);
        gameCamera.transform.position = new Vector3(transform.position.x + 8, transform.position.y + 15, transform.position.z - 8);

        //animation movement controller
        {
            CheckPlayerInputandPerformPlayerActions();
            if (Time.time > jumpTimer && isGrounded() == false) { movementMode = MovementMode.Falling; }
            animator.SetFloat("Velocity", moveIntensity);
            animator.SetLayerWeight(1, attackBlend);

            switch (movementMode)
            {
                case MovementMode.Idle:
                    animator.SetFloat("AnimState", (int)BlendState.Idle_Running_Sprinting);
                    UpdateMoveIntensity(movementMode);
                    break;
                case MovementMode.Running:
                    animator.SetFloat("AnimState", (int)BlendState.Idle_Running_Sprinting);
                    maxInensity = runSpeed;
                    UpdateMoveIntensity(movementMode);
                    break;
                case MovementMode.Sprinting:
                    animator.SetFloat("AnimState", (int)BlendState.Idle_Running_Sprinting);
                    maxInensity = sprintSpeed;
                    UpdateMoveIntensity(movementMode);
                    break;
                case MovementMode.Jumping:
                    animator.SetFloat("AnimState", (int)BlendState.Jumping);
                    break;
                case MovementMode.Falling:
                    animator.SetFloat("AnimState", (int)BlendState.Falling);
                    break;
            }
        }

        EnableInteractionFeedbackWithinRange();
    }

    private void OnDrawGizmosSelected()
    {
        //interaction range
        { 
            float r = interactionRadius;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, r);
        }
    }

    private void CheckPlayerInputandPerformPlayerActions()
    {
        if (Input.GetKey(forwardInput) == true) { Move(MovementDirection.Forward); }
        if (Input.GetKey(backwardInput) == true) { Move(MovementDirection.Backward); }
        if (Input.GetKey(rightInput) == true) { Move(MovementDirection.Right); }
        if (Input.GetKey(leftInput) == true) { Move(MovementDirection.Left); }
        moveDirection.Normalize();
        transform.Translate(moveDirection * moveIntensity * Time.deltaTime, Space.World);

        //attacking
        if (Input.GetMouseButton(0)){ ActivateAttack(); }
        if(IsAttacking() == false) { attackBlend -= Time.deltaTime * attackBlendDeceleration; StopAttacking(); }

        //blocking
        if (Input.GetMouseButton(1)) { ActivateBlock(); }
        if (IsBlocking() == false) { StopBlocking(); }

        //movemonet/sprinting
        if (Input.GetKey(sprintInput)) { Sprint(); }

        //jumping
        if (Input.GetKey(jumpInput)) { Jump(); }

        //checking to be idle
        else if (IsMoving() == false) { moveDirection = Vector3.zero; movementMode = MovementMode.Idle; }

        //interact with object
        if (Input.GetKeyDown(interactInput) && canMove)
        {
            //Debug.LogWarning("call Interact");

            if (hitColInteraction.Length != 0)
            {
                //Debug.LogError($"trying Interaction with {hitColInteraction.Length} object(s)");

                foreach (Collider col in hitColInteraction)
                {
                    Interactable interactable = col.gameObject.GetComponentInParent<Interactable>();
                    //Debug.LogWarning($"trying Interaction with {interactable.gameObject.name}");

                    if (interactable.InteractableEnabled == true)
                    {
                        Debug.LogWarning($"{gameObject.name} interacting with {interactable.gameObject.name}");
                        interactable.Interact(this.gameObject);
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && !canMove)
        {
            canMove = true;
            GameManager.manager.levelManager.StopReadingNote();
        }
    }

    private void EnableInteractionFeedbackWithinRange()
    {
        ///[errcontrol] make sure interactable LayerMask is set in PlayerController inspector Interactable
        hitColInteraction = Physics.OverlapSphere(transform.position, // setting detection range
                interactionRadius, interactable.value, QueryTriggerInteraction.Ignore);

        //Only follow through if interactable is hit
        if (hitColInteraction.Length == 0) { return; }
        
        float r = interactionRadius - .1f; // MN#: -.1 due to radius encompasing hitColInteraction enough to not miss turning off the light
        

            foreach (Collider col in hitColInteraction)
            {

                float distance = Vector3.Distance(transform.position, col.transform.position);
                //Debug.LogError($"hit {col.gameObject.name}");

                if (col.gameObject.GetComponent<Interactable>() != null)
                {
                    Interactable interactable = col.gameObject.GetComponent<Interactable>();

                    if (distance >= r && interactable.FeedbackEnabled)
                    {
                        interactable.DisableFeedback();
                    }
                    else if (distance <= r && !interactable.FeedbackEnabled)
                    {
                        Physics.IgnoreCollision(this.transform.GetChild(0).GetComponent<Collider>(), col, true);

                        interactable.EnableFeedback();
                    }
                }
                else { Debug.LogError("INTERACTABLE LAYER BEING USED BY NON-INTERACTABLE, CHECK \"Debug.LogError(hit { col.gameObject.name} );\""); }
            }
        
    }

    private void UpdateMoveIntensity(MovementMode movementMode)
    {
        if (movementMode == MovementMode.Idle)
        {
            if (moveIntensity > 0.0f) { moveIntensity -= Time.deltaTime * velocityDeceleration; }
        }
        else if (movementMode != MovementMode.Idle)
        {
            if (moveIntensity < maxInensity) { moveIntensity += Time.deltaTime * velocityAcceleration; }
            else { moveIntensity -= Time.deltaTime * velocityDeceleration; }
        }
    }

    private void Move(MovementDirection direction)
    {
        if (canMove == false) { return; }

        if (direction == MovementDirection.Forward) { moveDirection += new Vector3(-1, 0, 1); }
        if (direction == MovementDirection.Backward) { moveDirection += new Vector3(1, 0, -1); }
        if (direction == MovementDirection.Right) { moveDirection += new Vector3(1, 0, 1); }
        if (direction == MovementDirection.Left) { moveDirection += new Vector3(-1, 0, -1); }

        if (Input.GetKey(sprintInput) == true) { return; }
        if (movementMode == MovementMode.Jumping) { return; }
        if (isGrounded() == false) { return; }

        movementMode = MovementMode.Running;
    }

    private bool IsMoving()
    {
        if (Input.GetKey(forwardInput) == true) { return true; }
        if (Input.GetKey(backwardInput) == true) { return true; }
        if (Input.GetKey(rightInput) == true) { return true; }
        if (Input.GetKey(leftInput) == true) { return true; }
        if (movementMode == MovementMode.Jumping) { return true; }
        if (isGrounded() == false) { return true; }

        return false;
    }

    private void Sprint()
    {
        if (movementMode == MovementMode.Jumping) { return; }
        if (isGrounded() == false) { return; }

        movementMode = MovementMode.Sprinting;
    }

    private void Jump()
    {
        if (isGrounded() == false) { return; }
        if (Time.time <= jumpTimer) { return; }

        movementMode = MovementMode.Jumping;
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jumpHeight, rb.velocity.z);
        jumpTimer = Time.time + jumpTimeDuration;
    }

    private void ActivateAttack()
    {
        if (attackBlend >= 1) { return; }
        if (Time.time <= attackTimer) { return; }
        if (movementMode == MovementMode.Sprinting) { return; }
        if (movementMode == MovementMode.Falling) { return; }
        if (movementMode == MovementMode.Jumping) { return; }
        if (Input.GetMouseButton(1) == true) { return; }

        attackBlend = 1;
        attackTimer = Time.time + playerStats.AttackSpeed;
        Attack();
    }
    public void Attack() 
    { 
        if (playerStats.Shield.activeSelf == false && 
            playerStats.WeaponHitArea.activeSelf == false) 
        { playerStats.WeaponHitArea.SetActive(true); } 
    }

    public void StopAttacking() 
    {
        if (playerStats.WeaponHitArea.activeSelf == true) 
        { playerStats.WeaponHitArea.SetActive(false); } 
    }

    private bool IsAttacking()
    {
        if (Time.time <= attackTimer) { return true; }
        if (attackBlend <= 0) { return true; }

        return false;
    }

    private void ActivateBlock()
    {
        if (Time.time <= attackTimer) { return; }
        if (movementMode == MovementMode.Sprinting) { return; }
        if (movementMode == MovementMode.Falling) { return; }

        Block();
    }

    public void Block() 
    { 
        if (playerStats.Shield.activeSelf == false) 
        { playerStats.Shield.SetActive(true); } 
    }

    public void StopBlocking() 
    { 
        if (playerStats.Shield.activeSelf == true) 
        { playerStats.Shield.SetActive(false); } 
    }
    
    private bool IsBlocking()
    {
        if (Input.GetMouseButton(1) == true) { return true; }

        return false;
    }

    private bool isGrounded()
    {
        /// Make sure body Layermask is set in PlayerController Inspector Body
        if (Physics.Raycast(this.transform.position, -this.transform.up, out rayHit, rayRange, ~body)) 
        {
            return true;
        }
        return false;
    }

    private void OnCollisionStay(Collision other)
    {
        if (isGrounded() == true && movementMode == MovementMode.Jumping || isGrounded() == true && movementMode == MovementMode.Falling) 
        { 
            movementMode = MovementMode.Idle; 
        }
    }
}
