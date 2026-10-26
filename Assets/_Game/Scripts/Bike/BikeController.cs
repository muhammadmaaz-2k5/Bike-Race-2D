using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using DirtDash.Core;

namespace DirtDash.Bike
{
    /// <summary>
    /// Core 2D physics motorbike controller for Dirt Dash.
    /// Handles throttle smoothing, brake resistance, suspension dynamics,
    /// torque-reaction wheelies, mid-air leaning/rotation, and visual wheel sync.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BikeController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BikeStats stats;

        [Header("Physics Bodies & Joints")]
        [SerializeField] private Rigidbody2D bikeBody;
        [SerializeField] private Rigidbody2D frontWheel;
        [SerializeField] private Rigidbody2D backWheel;
        [SerializeField] private WheelJoint2D frontWheelJoint;
        [SerializeField] private WheelJoint2D backWheelJoint;
        [SerializeField] private CircleCollider2D frontWheelCollider;
        [SerializeField] private CircleCollider2D backWheelCollider;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayerMask = ~0;
        [SerializeField] private float groundCheckRadius = 0.35f;

        [Header("Visual Child References (Optional 3D or 2D)")]
        [SerializeField] private Transform visualBody;
        [SerializeField] private Transform visualFrontWheel;
        [SerializeField] private Transform visualBackWheel;

        [Header("Current Runtime State")]
        [SerializeField] private bool isControllable = true;
        [SerializeField] private bool isGrounded = false;
        [SerializeField] private bool isFrontGrounded = false;
        [SerializeField] private bool isBackGrounded = false;
        [SerializeField] private float currentSpeedKmh = 0f;
        [SerializeField] private float throttleInput = 0f;
        [SerializeField] private float brakeInput = 0f;
        [SerializeField] private float leanInput = 0f;

        // Internal physics working variables
        private float _currentMotorSpeed = 0f;
        private bool _externalInputActive = false; // Set to true when mobile touch/tilt feeds inputs
        private bool _isCrashed = false;

        #region Public Properties

        public BikeStats Stats => stats;
        public Rigidbody2D BodyRigidbody => bikeBody;
        public Rigidbody2D FrontWheelRigidbody => frontWheel;
        public Rigidbody2D BackWheelRigidbody => backWheel;
        public bool IsGrounded => isGrounded;
        public bool IsFrontGrounded => isFrontGrounded;
        public bool IsBackGrounded => isBackGrounded;
        public float CurrentSpeedKmh => currentSpeedKmh;
        public bool IsCrashed => _isCrashed;
        public bool IsControllable => isControllable && !_isCrashed;

        public event Action<float> OnSpeedChanged;

        #endregion

        #region Initialization

        private void Awake()
        {
            if (bikeBody == null)
            {
                bikeBody = GetComponent<Rigidbody2D>();
            }

            ApplyStats(stats);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<BikeCrashedEvent>(HandleBikeCrashed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BikeCrashedEvent>(HandleBikeCrashed);
        }

        /// <summary>
        /// Applies or swaps the active BikeStats ScriptableObject configuration.
        /// </summary>
        public void ApplyStats(BikeStats newStats)
        {
            if (newStats == null) return;
            stats = newStats;

            if (bikeBody != null)
            {
                bikeBody.mass = stats.bodyMass;
                bikeBody.linearDamping = stats.linearDrag;
                bikeBody.angularDamping = stats.angularDrag;
            }

            if (frontWheel != null)
            {
                frontWheel.mass = stats.wheelMass;
            }

            if (backWheel != null)
            {
                backWheel.mass = stats.wheelMass;
            }

            // Apply suspension parameters
            ApplySuspensionSettings(frontWheelJoint);
            ApplySuspensionSettings(backWheelJoint);
        }

        private void ApplySuspensionSettings(WheelJoint2D joint)
        {
            if (joint == null || stats == null) return;

            JointSuspension2D suspension = joint.suspension;
            suspension.frequency = stats.suspensionFrequency;
            suspension.dampingRatio = stats.suspensionDamping;
            joint.suspension = suspension;
        }

        #endregion

        #region Input Handling

        private void Update()
        {
            if (!IsControllable)
            {
                throttleInput = 0f;
                brakeInput = 0f;
                leanInput = 0f;
                return;
            }

            // Read keyboard input if not controlled by mobile touch/tilt
            if (!_externalInputActive)
            {
                ReadDesktopInput();
            }

            // Sync visual transforms (especially if using 3D meshes as visual children)
            SyncVisualWheels();
        }

        /// <summary>
        /// Reads keyboard controls for Editor / Desktop testing.
        /// Right Arrow / D = Run (Accelerate)
        /// Left Arrow / A = Brake
        /// Up/Down or W/S or Left/Right for lean rotation
        /// </summary>
        private void ReadDesktopInput()
        {
            float throttle = 0f;
            float brake = 0f;
            float lean = 0f;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) throttle = 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) brake = 1f;

                // Leaning: Up / W / Left arrow leans back (clockwise/counter-clockwise in 2D)
                // Left Arrow / A leans back (pull up), Right Arrow / D leans forward (push down) in air
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) lean += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) lean -= 1f;

                // When in air, A/D or Left/Right also act as lean
                if (!isGrounded)
                {
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) lean += 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) lean -= 1f;
                }
            }
#else
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) throttle = 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) brake = 1f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) lean += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) lean -= 1f;

            if (!isGrounded)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) lean += 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) lean -= 1f;
            }
#endif

            throttleInput = Mathf.Clamp01(throttle);
            brakeInput = Mathf.Clamp01(brake);
            leanInput = Mathf.Clamp(lean, -1f, 1f);
        }

        /// <summary>
        /// Programmatic hook called by Mobile Touch & Tilt input components.
        /// </summary>
        public void SetMobileInputs(float throttle, float brake, float lean)
        {
            _externalInputActive = true;
            throttleInput = Mathf.Clamp01(throttle);
            brakeInput = Mathf.Clamp01(brake);
            leanInput = Mathf.Clamp(lean, -1f, 1f);
        }

        /// <summary>
        /// Clears mobile control mode, restoring desktop keyboard input.
        /// </summary>
        public void ReleaseMobileInputLock()
        {
            _externalInputActive = false;
        }

        #endregion

        #region Physics Simulation (FixedUpdate)

        private void FixedUpdate()
        {
            CheckGroundStatus();

            if (bikeBody != null)
            {
                currentSpeedKmh = bikeBody.linearVelocity.magnitude * 3.6f;
                OnSpeedChanged?.Invoke(currentSpeedKmh);
            }

            if (!IsControllable)
            {
                DisableMotor();
                return;
            }

            SimulateMotor();
            SimulateLeaning();
            ClampMaxSpeed();
        }

        /// <summary>
        /// Checks whether front and rear wheels are currently in contact with the ground.
        /// </summary>
        private void CheckGroundStatus()
        {
            isFrontGrounded = frontWheelCollider != null && frontWheelCollider.IsTouchingLayers(groundLayerMask);
            isBackGrounded = backWheelCollider != null && backWheelCollider.IsTouchingLayers(groundLayerMask);
            isGrounded = isFrontGrounded || isBackGrounded;
        }

        /// <summary>
        /// Applies smoothed motor torque to the back wheel and handles brake resistance.
        /// </summary>
        private void SimulateMotor()
        {
            if (backWheelJoint == null || stats == null) return;

            JointMotor2D motor = backWheelJoint.motor;

            if (throttleInput > 0.01f)
            {
                // Smooth throttle ramp-up
                float targetSpeed = -stats.motorSpeed * throttleInput;
                _currentMotorSpeed = Mathf.MoveTowards(
                    _currentMotorSpeed,
                    targetSpeed,
                    stats.motorSpeed * stats.accelerationSmoothing * Time.fixedDeltaTime
                );

                motor.motorSpeed = _currentMotorSpeed;
                motor.maxMotorTorque = stats.motorTorque;
                backWheelJoint.motor = motor;
                backWheelJoint.useMotor = true;

                // Real torque reaction: accelerating the rear wheel forward exerts an anti-clockwise reaction on the chassis,
                // causing a natural wheelie pop!
                if (isBackGrounded)
                {
                    float wheelieReaction = stats.motorTorque * 0.12f * throttleInput;
                    bikeBody.AddTorque(wheelieReaction, ForceMode2D.Force);
                }
            }
            else if (brakeInput > 0.01f)
            {
                // Active braking: if stationary, allow slow reverse backing up; if moving forward, act as strong disc brake.
                bool movingForward = bikeBody.linearVelocity.x > 0.5f;

                if (movingForward)
                {
                    // Full brake resistance (speed 0, high torque)
                    motor.motorSpeed = 0f;
                    motor.maxMotorTorque = stats.brakeTorque * brakeInput;
                    _currentMotorSpeed = 0f;
                }
                else
                {
                    // Slow reverse crawl
                    float reverseSpeed = stats.motorSpeed * 0.35f * brakeInput;
                    motor.motorSpeed = reverseSpeed;
                    motor.maxMotorTorque = stats.brakeTorque * 0.5f;
                }

                backWheelJoint.motor = motor;
                backWheelJoint.useMotor = true;
            }
            else
            {
                // Coasting / Freewheeling
                _currentMotorSpeed = Mathf.MoveTowards(_currentMotorSpeed, 0f, stats.motorSpeed * 4f * Time.fixedDeltaTime);
                backWheelJoint.useMotor = false;
            }
        }

        /// <summary>
        /// Applies rotational torque to the bike frame based on tilt input.
        /// High torque in air (enables flips), moderate torque on ground (for wheelies / balance).
        /// </summary>
        private void SimulateLeaning()
        {
            if (bikeBody == null || stats == null || Mathf.Abs(leanInput) < 0.01f) return;

            float torqueFactor = isGrounded ? stats.groundLeanTorque : stats.airRotationTorque;
            float torque = leanInput * torqueFactor;

            bikeBody.AddTorque(torque, ForceMode2D.Force);

            // Clamp max angular velocity to prevent runaway rotation
            if (Mathf.Abs(bikeBody.angularVelocity) > stats.maxAngularVelocity)
            {
                bikeBody.angularVelocity = Mathf.Sign(bikeBody.angularVelocity) * stats.maxAngularVelocity;
            }
        }

        /// <summary>
        /// Clamps maximum forward speed to keep gameplay stable and high-speed jumps predictable.
        /// </summary>
        private void ClampMaxSpeed()
        {
            if (bikeBody == null || stats == null) return;

            Vector2 vel = bikeBody.linearVelocity;
            if (vel.magnitude > stats.maxSpeed)
            {
                bikeBody.linearVelocity = vel.normalized * stats.maxSpeed;
            }
        }

        private void DisableMotor()
        {
            if (backWheelJoint != null)
            {
                backWheelJoint.useMotor = false;
            }
        }

        #endregion

        #region Visual Synchronization

        /// <summary>
        /// Synchronizes visual wheel meshes/sprites with 2D physics wheel rotations.
        /// Essential when using 3D rigged models or detached wheel sprites in 2.5D.
        /// </summary>
        private void SyncVisualWheels()
        {
            if (visualFrontWheel != null && frontWheel != null)
            {
                if (visualBody != null && visualFrontWheel.IsChildOf(visualBody))
                {
                    // 3D rigged wheel bone inside FBX rig: roll around local X axle
                    visualFrontWheel.localRotation = Quaternion.Euler(frontWheel.rotation, 0f, 0f);
                }
                else
                {
                    // Standalone 2D sprite / transform
                    visualFrontWheel.position = frontWheel.position;
                    visualFrontWheel.rotation = Quaternion.Euler(0f, 0f, frontWheel.rotation);
                }
            }

            if (visualBackWheel != null && backWheel != null)
            {
                if (visualBody != null && visualBackWheel.IsChildOf(visualBody))
                {
                    // 3D rigged wheel bone inside FBX rig: roll around local X axle
                    visualBackWheel.localRotation = Quaternion.Euler(backWheel.rotation, 0f, 0f);
                }
                else
                {
                    // Standalone 2D sprite / transform
                    visualBackWheel.position = backWheel.position;
                    visualBackWheel.rotation = Quaternion.Euler(0f, 0f, backWheel.rotation);
                }
            }

            if (visualBody != null && visualBody.parent != transform && bikeBody != null)
            {
                visualBody.position = bikeBody.position;
                visualBody.rotation = Quaternion.Euler(0f, 90f, bikeBody.rotation);
            }
        }

        #endregion

        #region Crash & State Control

        private void HandleBikeCrashed(BikeCrashedEvent evt)
        {
            _isCrashed = true;
            isControllable = false;
            DisableMotor();
        }

        /// <summary>
        /// Resets the bike position and state for quick respawn.
        /// </summary>
        public void Respawn(Vector2 spawnPosition, float spawnAngle = 0f)
        {
            _isCrashed = false;
            isControllable = true;
            _currentMotorSpeed = 0f;

            if (bikeBody != null)
            {
                bikeBody.position = spawnPosition;
                bikeBody.rotation = spawnAngle;
                bikeBody.linearVelocity = Vector2.zero;
                bikeBody.angularVelocity = 0f;
            }

            if (frontWheel != null && frontWheelJoint != null)
            {
                frontWheel.position = spawnPosition + frontWheelJoint.anchor;
                frontWheel.rotation = spawnAngle;
                frontWheel.linearVelocity = Vector2.zero;
                frontWheel.angularVelocity = 0f;
            }

            if (backWheel != null && backWheelJoint != null)
            {
                backWheel.position = spawnPosition + backWheelJoint.anchor;
                backWheel.rotation = spawnAngle;
                backWheel.linearVelocity = Vector2.zero;
                backWheel.angularVelocity = 0f;
            }

            SyncVisualWheels();
        }

        public void SetControllable(bool controllable)
        {
            isControllable = controllable;
            if (!isControllable)
            {
                DisableMotor();
            }
        }

        #endregion
    }
}
