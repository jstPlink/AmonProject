using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
//#if ENABLE_INPUT_SYSTEM
//[RequireComponent(typeof(PlayerInput))]
//#endif
public class ThirdPersonControllerEsplorations : MonoBehaviour {


    [Header("Player")]
    public bool charActive = true;




    [Header("Player")]
    public bool yourTurn = false;
    //public bool clickToMove = false;
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

//#if ENABLE_INPUT_SYSTEM
    //public PlayerInput _playerInput;
//#endif
    private Animator _animator;
    private CharacterController _controller;
    CharacterManagerSingle characterManagerSingle;
    //private StarterAssetsInputs _input;

    private InputSystemCustom inputSystemCustom;

    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    // AI variables
    public NavMeshAgent thisAgent;
    [Tooltip("Target destination for Nav Mesh Agent as Transform")]
    public Transform Target;
    [Tooltip("If the AI is sprinting or not.")]
    public bool Sprinting = false;
    [Tooltip("If the AI will start a Jump or not.")]
    public bool Jump = false;
    public bool destinationArrived = true;
    public Vector3 targherDestinationAI;
    public float stoppingDistance = 5f;
    private bool IsCurrentDeviceMouse {
        get {
         return PlayerInputInstance.Instance._playerInput.currentControlScheme == "KeyboardMouse";
        }
    }


    private void Awake() {
        // get a reference to our main camera
        if (_mainCamera == null) {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        inputSystemCustom = new InputSystemCustom();
        characterManagerSingle = GetComponent<CharacterManagerSingle>();
    }

    private void Start() {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        //_input = GetComponent<StarterAssetsInputs>();
//#if ENABLE_INPUT_SYSTEM
        //_playerInput = GetComponent<PlayerInput>();
//#else
        	//Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
//#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        thisAgent = GetComponent<NavMeshAgent>();
        thisAgent.updateRotation = false;

        if (Sprinting) thisAgent.speed = SprintSpeed;
        else thisAgent.speed = MoveSpeed;
    }

    private void Update() {
        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
        if(characterManagerSingle.characterControllerMode == CharacterControllerMode.ThirdPersonController) {
            if (thisAgent.enabled) thisAgent.enabled = false;
            Move();
        } else if (characterManagerSingle.characterControllerMode == CharacterControllerMode.ClickToMove) {
            if (!thisAgent.enabled) thisAgent.enabled = true;

            if(yourTurn) thisAgent.stoppingDistance = 0f;
            else thisAgent.stoppingDistance = stoppingDistance;
            if (characterManagerSingle.isFollowingPlayer) {
                thisAgent.SetDestination(CharacterManager.Instance.currentCharacterManagerSingle.gameObject.transform.position);

                if (thisAgent.remainingDistance > thisAgent.stoppingDistance) {
                    thisAgent.speed = MoveSpeed;
                    Move(thisAgent.desiredVelocity.normalized, thisAgent.desiredVelocity.magnitude);
                } else {
                    Move(thisAgent.desiredVelocity.normalized, 0f);
                    destinationArrived = true;
                    thisAgent.speed = 0f;
                }
            } else {
                if (yourTurn) {
                    if (destinationArrived) {
                        ClickToMove();
                    }
                }
            }
            if (thisAgent.enabled && charActive) 
            {
                if (thisAgent.remainingDistance > thisAgent.stoppingDistance) {
                    Move(thisAgent.desiredVelocity.normalized, thisAgent.desiredVelocity.magnitude);
                } else {
                    Move(thisAgent.desiredVelocity.normalized, 0f);
                    destinationArrived = true;
                    thisAgent.speed = 0f;
                }
            }
        }
    }

    private void LateUpdate() {
        if (inputSystemCustom.asset.enabled && yourTurn) {
            CameraRotation();
        }         
    }
    private void OnEnable() {
        inputSystemCustom.Enable();
    }
    private void OnDisable() {
        inputSystemCustom.Disable();
    }
    public void ResetCameraRoot() {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _cinemachineTargetPitch = 0;
        CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(0.0f,
        0.0f, 0.0f);
        Debug.Log("CameraRootResettata");
    }
    private void AssignAnimationIDs() {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck() {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator) {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation() {
            if (inputSystemCustom.Player.Look.ReadValue<Vector2>().sqrMagnitude >= _threshold && !LockCameraPosition) {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                //float deltaTimeMultiplier = 1.0f;

                _cinemachineTargetYaw += inputSystemCustom.Player.Look.ReadValue<Vector2>().x * deltaTimeMultiplier;
                _cinemachineTargetPitch += inputSystemCustom.Player.Look.ReadValue<Vector2>().y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);

    }

    private void Move() {

        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = inputSystemCustom.Player.Sprint.IsPressed() ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (inputSystemCustom.Player.Move.ReadValue<Vector2>() == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        //float inputMagnitude = _input.analogMovement ? inputSystemCustom.Player.Move.ReadValue<Vector2>().magnitude : 1f;
        float inputMagnitude = inputSystemCustom.Player.Move.ReadValue<Vector2>().magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset) {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        } else {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(inputSystemCustom.Player.Move.ReadValue<Vector2>().x, 0.0f, inputSystemCustom.Player.Move.ReadValue<Vector2>().y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (inputSystemCustom.Player.Move.ReadValue<Vector2>() != Vector2.zero && yourTurn) {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        if (!yourTurn) {
            _speed = 0;
            inputMagnitude = 0;
            _animationBlend = 0;
        }
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator) {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
        
    }
    private void Move(Vector3 AgentDestination, float AgentSpeed) {
        if (AgentSpeed > 0f) {
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < AgentSpeed - speedOffset || currentHorizontalSpeed > AgentSpeed + speedOffset) {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, AgentSpeed, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            } else {
                _speed = AgentSpeed;
            }
            _animationBlend = Mathf.Lerp(_animationBlend, AgentSpeed, Time.deltaTime * SpeedChangeRate);

            // rotate player when the player is moving
            if (_speed != 0f) {
                _targetRotation = Mathf.Atan2(AgentDestination.x, AgentDestination.z) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            float theMagnitude = 1f;
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, theMagnitude);

        } else {
            _animationBlend = Mathf.Lerp(_animationBlend, 0f, Time.deltaTime * SpeedChangeRate);
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
        }
    }
    private void ClickToMove() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit);
        if (hasHit) {
            thisAgent.SetDestination(hit.point);
            if (Input.GetMouseButtonDown(0)) {
                if (Sprinting) thisAgent.speed = SprintSpeed;
                else thisAgent.speed = MoveSpeed;
                destinationArrived = false;
            };
        }
    }
    private void JumpAndGravity() {
        if (Grounded) {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator) {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f) {
                _verticalVelocity = -2f;
            }
            // Jump
            if (yourTurn && characterManagerSingle.characterControllerMode == CharacterControllerMode.ThirdPersonController) {
                if (inputSystemCustom.Player.Jump.IsPressed() && _jumpTimeoutDelta <= 0.0f) {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator) {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
        } else {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f) {
                _fallTimeoutDelta -= Time.deltaTime;
            } else {
                // update animator if using character
                if (_hasAnimator) {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            //_input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity) {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected() {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent) {
        if (animationEvent.animatorClipInfo.weight > 0.5f) {
            if (FootstepAudioClips.Length > 0) {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent) {
        if (animationEvent.animatorClipInfo.weight > 0.5f) {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}
