using UnityEngine;

namespace DirtDash.Bike
{
    /// <summary>
    /// Configurable stats and handling characteristics for a specific motorbike in Dirt Dash.
    /// Supports multiple bikes in the shop with different speed, grip, mass and agility profiles.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBikeStats", menuName = "Dirt Dash/Bike Stats")]
    public class BikeStats : ScriptableObject
    {
        [Header("Identity & Economy")]
        [Tooltip("Unique ID for saving and inventory lookup")]
        public string bikeId = "bike_starter";

        [Tooltip("Display name shown in menus and shop")]
        public string displayName = "Dirt Runner";

        [TextArea(2, 3)]
        [Tooltip("Flavor text description of this bike")]
        public string description = "A reliable, balanced starter bike with responsive handling.";

        [Tooltip("Unlock cost in coins (0 = free starter bike)")]
        public int coinPrice = 0;

        [Tooltip("Whether this bike is unlocked by default")]
        public bool isDefaultUnlocked = false;

        [Header("Motor & Acceleration")]
        [Tooltip("Target rotational speed of the motor (deg/s or motor joint speed)")]
        public float motorSpeed = 2200f;

        [Tooltip("Maximum torque applied by the rear wheel motor")]
        public float motorTorque = 40f;

        [Tooltip("Maximum forward horizontal speed clamp (m/s)")]
        public float maxSpeed = 32f;

        [Tooltip("Acceleration curve ramp-up rate (higher = snappier throttle)")]
        public float accelerationSmoothing = 8f;

        [Tooltip("Torque applied when braking / reversing")]
        public float brakeTorque = 35f;

        [Header("Air & Ground Rotation (Leaning / Flips)")]
        [Tooltip("Angular torque applied when tilting/leaning in mid-air (enables flips)")]
        public float airRotationTorque = 28f;

        [Tooltip("Angular torque applied when leaning on the ground (for wheelies / balance)")]
        public float groundLeanTorque = 12f;

        [Tooltip("Maximum angular velocity clamp (deg/s) to avoid runaway spin")]
        public float maxAngularVelocity = 720f;

        [Header("Weight & Rigidbody Physics")]
        [Tooltip("Mass of the central bike chassis frame (kg)")]
        public float bodyMass = 1.5f;

        [Tooltip("Mass of each wheel (kg)")]
        public float wheelMass = 0.45f;

        [Tooltip("Linear drag on body in the air")]
        public float linearDrag = 0.2f;

        [Tooltip("Angular drag on body")]
        public float angularDrag = 0.8f;

        [Header("Suspension (WheelJoint2D)")]
        [Tooltip("Suspension spring frequency (Hz / stiffness)")]
        public float suspensionFrequency = 8f;

        [Tooltip("Suspension damping ratio (0 = bouncy, 1 = critical damping)")]
        public float suspensionDamping = 0.7f;

        [Header("Tire Grip & Friction")]
        [Tooltip("Friction coefficient for the tire physics material")]
        public float tireFriction = 1.35f;

        [Tooltip("Bounciness of the tires")]
        public float tireBounciness = 0.1f;
    }
}
