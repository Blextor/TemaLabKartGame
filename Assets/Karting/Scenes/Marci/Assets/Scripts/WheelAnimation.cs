using System;
using UnityEngine;

    [DefaultExecutionOrder(100)]
    public class WheelAnimation : MonoBehaviour
    {
        [Serializable] public class Wheel
        {
            [Tooltip("A reference to the transform of the wheel.")]
            public Transform wheelTransform;
            [Tooltip("A reference to the WheelCollider of the wheel.")]
            public WheelCollider wheelCollider;


        }


        [Space]
        [Tooltip("The maximum angle in degrees that the front wheels can be turned away from their default positions, when the Steering input is either 1 or -1.")]
        public float maxSteeringAngle;

        [Tooltip("Information referring to the front left wheel of the kart.")]
        public Wheel frontLeftWheel;
        [Tooltip("Information referring to the front right wheel of the kart.")]
        public Wheel frontRightWheel;
        [Tooltip("Information referring to the rear left wheel of the kart.")]
        public Wheel rearLeftWheel;
        [Tooltip("Information referring to the rear right wheel of the kart.")]
        public Wheel rearRightWheel;


        float m_SmoothedSteeringInput;

        void FixedUpdate()
        {

        float m_TurnInput = Input.GetAxis("Horizontal");
        m_SmoothedSteeringInput = Mathf.MoveTowards(m_SmoothedSteeringInput, m_TurnInput,
                10.0f * Time.deltaTime);

        // Steer front wheels
        float rotationAngle = m_SmoothedSteeringInput * maxSteeringAngle;

            frontLeftWheel.wheelCollider.steerAngle = rotationAngle;
            frontRightWheel.wheelCollider.steerAngle = rotationAngle;

        // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(frontLeftWheel);
            UpdateWheelFromCollider(frontRightWheel);
            UpdateWheelFromCollider(rearLeftWheel);
            UpdateWheelFromCollider(rearRightWheel);
        }

        void LateUpdate()
        {
            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(frontLeftWheel);
            UpdateWheelFromCollider(frontRightWheel);
            UpdateWheelFromCollider(rearLeftWheel);
            UpdateWheelFromCollider(rearRightWheel);
        }

        void UpdateWheelFromCollider(Wheel wheel)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheel.wheelTransform.position = position;
            wheel.wheelTransform.rotation = rotation;
        }
    }
