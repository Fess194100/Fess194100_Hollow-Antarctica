using System.Collections;
using UnityEngine;
using Cinemachine;

namespace SimpleCharController
{
    public class CharController : MonoBehaviour
    {
        public GameObject modelOrigin;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Permission")]
        public bool canControl = true;
        public bool canJump = true;
        public bool canClimb = true;
        public bool LockCameraPosition = false;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Move")]
        public float MoveSpeedForward = 3.0f;
        public float MoveSpeedBack = 2.0f;
        public float SprintSpeed = 5.0f;
        public AnimationCurve curveMoveSpeedForward;
        public float speedChengedCurveMoveSpeed = 5.0f;        

        [Space(6)]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float speedTransitionToTarget = 5.0f;
        public float distanceToOffTarget = 0.05f;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Jump&Gravity")]
        public float gravity = -15.0f;
        public float jumpHeight = 1.2f;
        public float jumpForce = 2f;
        public float jumpOffClimb = 5f;

        [Space(6)]
        public float jumpTimeout = 0.4f;
        public float fallTimeout = 1.2f;

        [HideInInspector]
        public float jumpForceOffClimb = 0.2f;

        [Space(10)]//-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Climbing")]
        public bool debugClimbing = true;
        public float climbCooldownTime = 0.3f;
        public float climbingSpeed = 1.5f;

        [HideInInspector]
        public float boostClimbSpeed = 1.0f;
        [HideInInspector]
        public ClimbingType climbingType;
        [HideInInspector]
        public Transform currentTargetClimb;
        [HideInInspector]
        public Transform offTargetClimb;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Cinemachine")]
        public CinemachineBrain cinemachineBrain;
        public GameObject CinemachineCameraTarget;
        public Transform mainCamera;

        [Space(6)]
        public float TopClamp = 75.0f;
        public float BottomClamp = -75.0f;
        public float sensativeCam = 50f;

        [Space(6)]
        public CinemachineFollowZoom cinemachineFollowZoom;
        public float smoothTimeFOV = 0.3f;
        public AnimationCurve FOV_AtSpeed;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Current State Player")]
        public bool isGrounded = false;
        public bool isJumping = false;
        public bool isFalling = false;
        public bool isClimbing = false;
        public bool isOffClimb = false;

        #region Private Variable

        // Move
        private float _currentSpeed;
        private float currentValueEvaluate;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        //private Vector3 targetDirectional;
        //private int _currentTargetDirection_Z = 1;
        //private int _currentTargetDirection_X = 0;

        //Jump&Gravity
        private float _verticalVelocity;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        //Climb
        private bool wasGroundedOnClimb = false;
        private bool canClimbAgain = true;
        private float _speedOffClimbObj;
        //private float distanceToClimbObj;

        // Cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float currentFOV;
        private float velocityFOV = 0.0f;
        private float FOV = 60;

        //Animation Parametrs
        private bool _hasAnimator;
        private Animator _animator;
        private int _animParamMoveSpeed;   
        private int _animParamIsJump;
        private int _animParamIsGround;
        private int _animParamIsFailing;
        private int _animParamTypeClimb; 
        //private int _animParamIsOffClimb;

        //Ower
        private SimpleInputActions _input;
        private CharacterController _controller;
        bool charInTargetPosition = false; // Для MoveToTarget()
        bool charInTargetRotation = false; // Для RotateTowardsTarget()
        #endregion

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<SimpleInputActions>();

            GetAnimator();
            AssignAnimationIDs();
        }

        void Update()
        {
            AnimationUpdate();
        }

        private void FixedUpdate()
        {
            GroundedCheck();
            PermissionCheck();

            JumpAndGravity();

            //if (isOffClimb) ExitModeClimb();

            if (!isClimbing)
            {
                if (!isOffClimb)
                {
                    Move();
                }
                isOffClimb = false;
            } 
            else
            {
                MoveClimbing();
            }

            MoveOffLadder();

            CameraRotation();
            UpdateFOV();
        }        

        private void GroundedCheck()
        {
            isGrounded = _controller.isGrounded;
        }

        private void PermissionCheck()
        {
            if (!canClimb || !canClimbAgain) ExitModeClimb();
            else
            {
                if (climbingType == ClimbingType.ropeLadder && isGrounded)
                {
                    ExitModeClimb();
                }
            }
        }
        private void JumpAndGravity()
        {

            if (isGrounded)
            {
                _fallTimeoutDelta = fallTimeout;
                isJumping = false;
                isFalling = false;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity != -0.15) _verticalVelocity = -0.15f;

                // Jump
                if (!isClimbing && canJump && _input.jump && _jumpTimeoutDelta <= 0.0f && canControl)
                {
                    _verticalVelocity = jumpHeight * jumpForce;
                    isJumping = true;
                    _input.jump = false;

                    /*// update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, isJumping);
                    }*/
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.fixedDeltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

                if (!isJumping && !isClimbing)
                {
                    if (!isFalling)
                    {
                        isFalling = true;
                        _verticalVelocity = -0.1f;
                    }
                }
                if (isJumping) _input.jump = false;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.fixedDeltaTime;
                }

                if (_verticalVelocity > gravity) _verticalVelocity += -jumpForce * 2 * Time.fixedDeltaTime;

                // Jump off Climb
                if (isClimbing && canControl && canJump && _input.jump)
                {
                    _verticalVelocity = (jumpHeight / 2) * jumpForce;
                    isJumping = true;
                    _input.jump = false;
                    _speedOffClimbObj = jumpForceOffClimb;

                    ExitModeClimb();

                    /*// update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }*/
                }
            }
        }

        #region Move

        private void Move()
        {
            Vector3 inputDirection = new Vector3();
            float targetSpeed;
            float inputMagnitude = 0;
            wasGroundedOnClimb = isGrounded;

            // Get direction input
            if (canControl)
            {
                inputDirection.x = _input.move.x;
                inputDirection.z = _input.move.y;
                inputDirection.Normalize();
                inputMagnitude = _input.move.magnitude == 0 ? 0 : 1;
            }

            // Get Target Speed
            if (inputDirection.z < 0) targetSpeed = MoveSpeedBack;
            else targetSpeed = _input.sprint ? SprintSpeed : MoveSpeedForward;

            //Get Current Speed
            currentValueEvaluate = Mathf.Lerp(currentValueEvaluate, targetSpeed * inputMagnitude, Time.fixedDeltaTime * speedChengedCurveMoveSpeed);
            _currentSpeed = curveMoveSpeedForward.Evaluate(currentValueEvaluate);

            // Rotation Character
            if (_input.move != Vector2.zero && canControl)
            {
                if (inputDirection.z < 0) { inputDirection.x *= -1; }

                _targetRotation = Mathf.Atan2(inputDirection.x, Mathf.Abs(inputDirection.z)) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 Moved = transform.forward * (_currentSpeed * Time.fixedDeltaTime);
            Vector3 Velocity = new Vector3(0, _verticalVelocity, 0);
            Vector3 JumpOff = DirectionOffClimb() * SpeedOffClimb();
            _controller.Move(Moved + Velocity + JumpOff);
        }

        private void MoveClimbing()
        {
            _input.jump = false;
            isJumping = false;
            isFalling = false;

            if (canControl)
            {
                //Determining the type of climbing
                switch (climbingType)
                {
                    case ClimbingType.climbLadder:
                        if (!charInTargetPosition) MoveToTarget(currentTargetClimb, 1);
                        if (!charInTargetRotation) RotateTowardsTarget(currentTargetClimb, 5f);
                        break;

                    case ClimbingType.ropeLadder:
                        MoveToTarget(currentTargetClimb, 1);
                        RotateTowardsTarget(mainCamera, RotationSmoothTime * 50f);
                        break;

                    default:
                        Debug.LogWarning("Unknown climbing type.");
                        break;
                }

                //Get Speed
                _currentSpeed = _input.move.y * climbingSpeed * boostClimbSpeed;

                //Movement
                Vector3 Moved = transform.up * _currentSpeed * Time.fixedDeltaTime;
                _controller.Move(Moved);

                //ExitMode
                if (isGrounded)
                {
                    if (!wasGroundedOnClimb) ExitModeClimb();
                    else if (_input.move.y < 0) ExitModeClimb();
                }
                else if (wasGroundedOnClimb) wasGroundedOnClimb = false;
            }
        }

        private void MoveOffLadder()
        {
            if (isOffClimb)
            {
                MoveToTarget(offTargetClimb, 0);
            }
        }

        private void MoveToTarget(Transform targetObject, byte typeTransition)
        {
            if (targetObject == null)
                return;

            switch (typeTransition)
            {
                case 0:
                    MoveToTargetDirect(targetObject, targetObject.position.y); // С учетом высоты целевого объекта
                    break;
                case 1:
                    MoveToTargetDirect(targetObject, transform.position.y); // Без учета высоты целевого объекта
                    break;
                /*case 2:
                    MoveToTargetParabola(targetObject, speedTransition);
                    break;*/
                default:
                    Debug.LogWarning("Unknown transition type.");
                    break;
            }
        }

        // Движение по прямой до целевого объекта
        private void MoveToTargetDirect(Transform targetObject, float currentPositionY)
        {            
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(targetObject.position.x, currentPositionY, targetObject.position.z);
            float distanceToClimbObj = Vector3.Distance(currentPosition, targetPosition);
            float speedAtDistance = speedTransitionToTarget * distanceToClimbObj;

            if (distanceToClimbObj <= distanceToOffTarget)
            {
                charInTargetPosition = true;

                if (isOffClimb)
                {
                    isOffClimb = false;
                    //ExitModeClimb();
                }
                
                return;
            }

            Vector3 direction = (targetPosition - currentPosition).normalized;
            Vector3 movement = direction * speedAtDistance * Time.fixedDeltaTime;

            _controller.Move(movement);
        }

        /*// Движение по параболе до целевого объекта
        private void MoveToTargetParabola(Transform targetObject, float speedTransition)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = targetObject.position;

            if (Vector3.Distance(currentPosition, targetPosition) <= 0.1)
                return;

            // Рассчитываем направление по горизонтали
            Vector3 horizontalDirection = new Vector3(targetPosition.x - currentPosition.x, 0, targetPosition.z - currentPosition.z).normalized;
            Vector3 horizontalMovement = horizontalDirection * speedTransition * Time.deltaTime;

            // Рассчитываем вертикальное движение (парабола)
            if (isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Начальная скорость для прыжка
            }

            _velocity.y += gravity * Time.deltaTime; // Применяем гравитацию
            Vector3 verticalMovement = Vector3.up * _velocity.y * Time.deltaTime;

            // Общее движение
            characterController.Move(horizontalMovement + verticalMovement);
        }*/

        void RotateTowardsTarget(Transform targetObject, float rotationSpeed)
        {
            if (targetObject == null)
                return;

            if (transform.rotation.y != targetObject.rotation.y + 1 || transform.rotation.y != targetObject.rotation.y - 1)
            {
                Quaternion targetRotation = Quaternion.Euler(0, targetObject.eulerAngles.y, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
            else charInTargetRotation = true;
        }
        #endregion

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= 0.01 && !LockCameraPosition)
            {
                _cinemachineTargetYaw += _input.look.x * Time.fixedDeltaTime * sensativeCam;
                _cinemachineTargetPitch += _input.look.y * Time.fixedDeltaTime * sensativeCam;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void UpdateFOV()
        {
            currentFOV = Mathf.SmoothDamp(currentFOV, _currentSpeed , ref velocityFOV, smoothTimeFOV);
            FOV = FOV_AtSpeed.Evaluate(currentFOV);
            cinemachineFollowZoom.m_MaxFOV = FOV;

            if (currentFOV <= 0.01f) currentFOV = velocityFOV = 0;
        }

        #region Climb
        private float SpeedOffClimb()
        {
            if (_speedOffClimbObj > 0)
            {
                _speedOffClimbObj = Mathf.Lerp(_speedOffClimbObj, 0, Time.fixedDeltaTime * jumpOffClimb);
            }
            return _speedOffClimbObj;
        }

        private Vector3 DirectionOffClimb() 
        {
            Vector3 directionOffClimb = Vector3.zero;
            Vector2 directionInput = _input.move;

            if (currentTargetClimb != null)
            {
                switch (climbingType)
                {
                    case ClimbingType.climbLadder:
                        if (directionInput.x == 0) directionOffClimb = -currentTargetClimb.transform.forward;
                        else directionOffClimb = currentTargetClimb.transform.right * directionInput.x;
                        break;

                    case ClimbingType.ropeLadder:
                        directionOffClimb = transform.forward;
                        break;

                    default:
                        Debug.LogWarning("Unknown climbing type.");
                        break;
                }
            }
            
            return directionOffClimb;
        }

        private void ExitModeClimb()
        {
            isClimbing = false;
            canClimbAgain = false;
            charInTargetPosition = false;
            charInTargetRotation = false;
            StartCoroutine(ClimbCooldown()); // Запускаем корутину для задержки
        }

        private IEnumerator ClimbCooldown()
        {
            yield return new WaitForSeconds(climbCooldownTime);
            canClimbAgain = true; // Позволяем повторое карабканье
        }

        #endregion

        #region Animation

        public void GetAnimator()
        {
            _hasAnimator = modelOrigin.TryGetComponent(out _animator);
        }
        private void AssignAnimationIDs()
        {
            _animParamMoveSpeed = Animator.StringToHash("MoveSpeed");
            _animParamIsJump = Animator.StringToHash("IsJump");
            _animParamIsGround = Animator.StringToHash("IsGround");
            _animParamIsFailing = Animator.StringToHash("IsFailing");
            _animParamTypeClimb = Animator.StringToHash("TypeClimb");
            //_animParamIsOffClimb = Animator.StringToHash("IsOffClimb");
        }

        private void AnimationUpdate()
        {
            if (_animator != null)
            {
                if (_input.move.y < 0)
                {
                    _animator.SetFloat(_animParamMoveSpeed, _currentSpeed);
                }
                else
                {
                    _animator.SetFloat(_animParamMoveSpeed, _currentSpeed);
                }

                _animator.SetBool(_animParamIsJump, isJumping);
                _animator.SetBool(_animParamIsGround, isGrounded);
                _animator.SetBool(_animParamIsFailing, isFalling);

                if (isClimbing)
                {
                    switch (climbingType)
                    {
                        case ClimbingType.climbLadder:
                            _animator.SetInteger(_animParamTypeClimb, 1);
                            break;

                        case ClimbingType.ropeLadder:
                            _animator.SetInteger(_animParamTypeClimb, 2);
                            break;

                        default:
                            _animator.SetInteger(_animParamTypeClimb, 0);
                            break;
                    }
                }
                else
                {
                    _animator.SetInteger(_animParamTypeClimb, 0);
                }

                }
            else Debug.Log("_animator - null :(");
        }
        #endregion TypeClimb
    }
}

