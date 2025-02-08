using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultipler;
    public bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode turretKey = KeyCode.F;
    public KeyCode debugTurretKey = KeyCode.X;
    [Header("GroundCheck")]
    bool grounded;
    [Header("ObjectReferences")]
    public Camera fpsCam;
    public GameObject turret;

    private Rigidbody playerRb;
    public Transform orientation;
    private GameManager gameManager;
    public TextMeshProUGUI moreMoneyText;
    public float zBound;

    //various private variables
    float horizontalInput;
    float verticalInput;
    float groundedBackupTimer;

    public MovementState state;

    Vector3 moveDirection;

    public enum MovementState
    {
        walking,
        sprinting,
        air
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        readyToJump = true;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();
        SpeedControl();
        ConstrainPlayerPosition();
        StateHandler();
        groundedBackupTimer += Time.deltaTime;
        if (groundedBackupTimer > 2)
        {
            playerRb.linearDamping = groundDrag;
            grounded = true;
            groundedBackupTimer = 0f;
        }
        if (gameManager.isGameOver == true)
        {
            playerRb.angularVelocity = Vector3.zero;
            playerRb.linearVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            playerRb.linearDamping = groundDrag;
            grounded = true;
            groundedBackupTimer = 0f;
        }
        else
        {
            playerRb.linearDamping = 0;
            grounded = false;
        }
    }
    void MyInput()
    {
        if (!gameManager.isGameOver)
        {
            //reading the input:
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            if (Input.GetKey(jumpKey) && grounded && readyToJump && !gameManager.paused && !gameManager.isGameOver)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
            
            //FOR TESTING ONLY!
            /*
            if (Input.GetKeyDown(debugTurretKey))
            {
                Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Middle of screen
                RaycastHit hit;
                //check if ray hits something
                Vector3 targetPoint;
                if (Physics.Raycast(ray, out hit)) //hit an enemy or wall
                    targetPoint = hit.point;
                else
                    targetPoint = ray.GetPoint(5); //air
                targetPoint.y = 0;
                Instantiate(turret, targetPoint, orientation.rotation);
            }
            */
            
            if (Input.GetKeyDown(turretKey) && gameManager.money < 100 && !gameManager.paused && !gameManager.isGameOver)
            {
                moreMoneyText.text = (100 - gameManager.money) + " more money needed!";
                moreMoneyText.gameObject.SetActive(true);
                Invoke("DeactivateMoreMoneyText", 2f);
            }
            else if (Input.GetKeyDown(turretKey) && gameManager.money >= 100 && !gameManager.paused && !gameManager.isGameOver)
            {
                Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Middle of screen
                RaycastHit hit;
                //check if ray hits something
                Vector3 targetPoint;
                if (Physics.Raycast(ray, out hit)) //hit an enemy or wall
                    targetPoint = hit.point;
                else
                    targetPoint = ray.GetPoint(5); //air
                targetPoint.y = 0;
                Instantiate(turret, targetPoint, orientation.rotation);
                StatTrackerScript.Instance.TurretsPlaced++;
                gameManager.UpdateMoney(-100);
            }
        }
    }


    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //on ground
        if(grounded)
        {
            playerRb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            playerRb.AddForce(moveDirection.normalized * moveSpeed * airMultipler * 10f, ForceMode.Force);
        }
        //in air

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            playerRb.linearVelocity = new Vector3(limitedVel.x, playerRb.linearVelocity.y, limitedVel.z);
        }    
    }

    private void StateHandler()
    {
        //mode - Sprinting
        if (Input.GetKey(sprintKey) && grounded)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        //mode - walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        //mode - air
        else
        {
            state = MovementState.air;
        }
    }

    private void Jump()
    {
        //reset y velocity
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);

        playerRb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        grounded = false;
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    void ConstrainPlayerPosition()
    {
        if (transform.position.z > zBound)
        {
            //don't let them through the invisible wall
            transform.position = new Vector3(transform.position.x, transform.position.y, zBound);
        }
    }

    public void TakeDamage(int damage)
    {
        gameManager.playerHealth -= damage;
        gameManager.ResetHealthDelay();
    }

    void DeactivateMoreMoneyText()
    {
        moreMoneyText.gameObject.SetActive(false);
    }
}
