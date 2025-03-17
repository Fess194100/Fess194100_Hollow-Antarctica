using Cinemachine;
using UnityEngine;

namespace SimpleCharController
{
    public class CharController : MonoBehaviour
    {
        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Permission")]
        public bool canControl = true;
        public bool canJump = true;
        public bool LockCameraPosition = false;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Move")]
        public float MoveSpeedForward = 3.0f;
        public float MoveSpeedBack = 2.0f;
        public float SprintSpeed = 5.0f;
        public AnimationCurve curveMoveSpeedForward;
        public float speedChengedCurveMoveSpeed = 7.0f;

        [Space(6)]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        //public float SpeedChangeRate = 7f;

        [Space(10)] //-------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Jump&Gravity")]
        public float _verticalVelocity;
        public float jumpHeight = 1.2f;
        public float jumpForce = 2f;
        public float jumpTimeout = 0.4f;
        public float fallTimeout = 1.2f;

        [Space(6)]
        public float gravity = -10.0f;        
        //public float gravityCharInGround = -2;        

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
        public bool isClimbingLadder = false;
        public bool isClimbingRope = false;

        #region Private Variable

        // Player
        private float _currentSpeed;
        private float currentValueEvaluate;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private Vector3 targetDirectional;
        private int _currentTargetDirection_Z = 1;
        private int _currentTargetDirection_X = 0;

        //Jump&Gravity
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float currentFOV;
        private float velocityFOV = 0.0f;
        private float FOV = 60;

        //Ower
        private SimpleInputActions _input;
        private CharacterController _controller;

        #endregion

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<SimpleInputActions>();
        }

        void Update()
        {

        }

        private void FixedUpdate()
        {
            GroundedCheck();
            JumpAndGravity();
            Move();
            CameraRotation();
        }

        private void GroundedCheck()
        {
            isGrounded = _controller.isGrounded;
        }

        private void JumpAndGravity()
        {
            if (isGrounded)
            {
                _fallTimeoutDelta = fallTimeout;
                isJumping = false;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) _verticalVelocity = gravity;

                // Jump
                if (!isClimbingLadder && canJump && _input.jump && _jumpTimeoutDelta <= 0.0f && canControl)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
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

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.fixedDeltaTime;
                }

                if (_verticalVelocity > gravity) _verticalVelocity += -jumpForce * 2 * Time.fixedDeltaTime;
            }
        }
        private void Move()
        {
            Vector3 inputDirection = new Vector3();
            float targetSpeed;
            float inputMagnitude = _input.move.magnitude == 0 ? 0 : 1;

            // Get direction input
            if (canControl)
            {
                inputDirection.x = _input.move.x;
                inputDirection.z = _input.move.y;
                inputDirection.Normalize();
            }

            // Get Target Speed
            if (inputDirection.z < 0) targetSpeed = MoveSpeedBack;
            else targetSpeed = _input.sprint ? SprintSpeed : MoveSpeedForward;

            //Get Current Speed
            currentValueEvaluate = Mathf.Lerp(currentValueEvaluate, targetSpeed * inputMagnitude, Time.fixedDeltaTime * speedChengedCurveMoveSpeed);
            _currentSpeed = curveMoveSpeedForward.Evaluate(currentValueEvaluate);

            if (_input.move != Vector2.zero && canControl)
            {
                if (inputDirection.z < 0) { inputDirection.x *= -1; }

                _targetRotation = Mathf.Atan2(inputDirection.x, Mathf.Abs(inputDirection.z)) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            targetDirectional = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (inputDirection.z < 0)
            {
                targetDirectional = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.back;
                _currentTargetDirection_Z = -1;
                _currentTargetDirection_X = 0;
            }
            if (inputDirection.z > 0)
            {
                _currentTargetDirection_Z = 1;
            }
            if (inputDirection.z == 0)
            {
                if (_currentTargetDirection_Z < 0 & inputDirection.x == 0)
                {
                    if (_currentTargetDirection_X == 0)
                    {
                        targetDirectional = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.back;
                    }
                    else
                    {
                        targetDirectional = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                    }
                }
                if (_currentTargetDirection_Z < 0 & inputDirection.x != 0)
                {
                    targetDirectional = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                    _currentTargetDirection_X = 1;
                }
            }

            Vector3 Moved = targetDirectional.normalized * (_currentSpeed * Time.fixedDeltaTime);
            Vector3 Velocity = new Vector3(0, _verticalVelocity, 0);
            //Vector3 JumpOff = (-transform.forward * _speedOffLadder) + DirectionOffRope();
            _controller.Move(Moved + Velocity);
        }
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
    }
}

