using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Stats")]
    public float maxStamina;
    public float currentStamina;
    public float staminaDrainRate;
    public float staminaRegenRate;
    public float minStaminaToRun;
    public float regenDelay;
    private float regenTimer;

    [Header("UI")]
    public UnityEngine.UI.Image staminaBar;

    [Header("Movement")]
    public float moveSpeed; // Speed of player movement
    public float runningSpeed; // Speed of player movement

    public float airMultiplier; // Multiplier for movement in the air
    public float jumpForce;
    public float jumpColldown;
    public float groundLinearDamping;
    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;


    [Header("Ground Check")]
    public float playerHeigh;
    public LayerMask whatIsGround;
    bool grounded;
    bool isRunning = false;

    [Header("Interaction")]
    public bool isInteracting = false;
    public float rotationSpeed = 5f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

	Coroutine hideCoroutine;

	private void Start()
    {
        if (Instance == null)
            Instance = this;

        currentStamina = maxStamina;

        if (staminaBar != null)
        {
            staminaBar.fillAmount = currentStamina;
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the Rigidbody from rotating due to physics
    }

    private void FixedUpdate()
    {
        MovePlayer();
        checkStamina();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeigh * 0.5f + 0.5f, whatIsGround);

        MyInput();
        //SpeedControl();

        if (grounded)
        {
            rb.linearDamping = groundLinearDamping;
        }
        else
            rb.linearDamping = 2;
    }

    private void MyInput()
    {
        // Get input from keyboard
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpColldown); // Call ResetJump after jumpColldown seconds
        }

        if (Input.GetKey(runKey) && CanRun())
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

    }

	IEnumerator WaitToHideStamina()
	{
		yield return new WaitForSeconds(5f);
		staminaBar.gameObject.SetActive(false);
		hideCoroutine = null; // Libera referência
	}

	private void checkStamina()
    {
		if (isRunning)
		{
			// Se o jogador começou a correr novamente, cancela qualquer tentativa de esconder
			if (hideCoroutine != null)
			{
				StopCoroutine(hideCoroutine);
				hideCoroutine = null;
			}

			staminaBar.gameObject.SetActive(true);
			DrainStamina();
		}
		else
		{
			// Só inicia a coroutine se ainda não estiver rodando
			if (hideCoroutine == null)
			{
				hideCoroutine = StartCoroutine(WaitToHideStamina());
			}

			RegenStamina();
		}

		currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

		if (staminaBar != null)
			staminaBar.fillAmount = currentStamina / maxStamina;
	}

    private void DrainStamina()
    {
        currentStamina -= staminaDrainRate * Time.deltaTime;
        regenTimer = 0f; // reseta o timer de regeneração
    }

    void RegenStamina()
    {
        if (currentStamina < maxStamina)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }
    }

    public bool CanRun()
    {
        return currentStamina > 0;
    }

    private void MovePlayer()
    {
        // Calculate movement direction based on input and orientation
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(isInteracting == false)
        {
            if (isRunning)
            {
                if (grounded)
                {
                    rb.AddForce(moveDirection.normalized * runningSpeed * 10f, ForceMode.Force);

                }
                else if(!grounded)
                {
                    rb.AddForce(moveDirection.normalized * runningSpeed * 10f * airMultiplier, ForceMode.Force);
                }

            }
            else
            {
                if (grounded)
                {
                    rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

                }
                else if (!grounded)
                {
                    rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
                }
            }
        }
    }

    public void RotateTowardsTarget(Transform target)
    {
        Transform playerObj = GetComponentInChildren<Transform>();
        if (playerObj != null && playerObj.name == "PlayerObj")
        {
            Vector3 direction = (target.position - playerObj.position).normalized; // Direção para o alvo
            Quaternion lookRotation = Quaternion.LookRotation(direction); // Calcula a rotação necessária
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
       
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Limit velocity if needed
        if(flatVel.magnitude > moveSpeed && !isRunning)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        } else
        {
            Vector3 limitedVel = flatVel.normalized * runningSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity, revents jump height from increasing if jump button is pressed multiple times in air
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
