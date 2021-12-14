using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;

public class MovementKart : MonoBehaviour
{

    [System.Serializable]
    public struct Stats
    {
        [Header("Movement Settings")]
        [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
        public float TopSpeed;

        [Tooltip("How quickly the kart reaches top speed.")]
        public float Acceleration;

        [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
        public float ReverseSpeed;

        [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
        public float ReverseAcceleration;

        [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
        [Range(0.2f, 1)]
        public float AccelerationCurve;

        [Tooltip("How quickly the kart slows down when the brake is applied.")]
        public float Braking;

        [Range(0.0f, 1.0f)]
        [Tooltip("The amount of side-to-side friction.")]
        public float Grip;

        [Tooltip("How tightly the kart can turn left or right.")]
        public float Steer;

    }

    public Rigidbody Rigidbody { get; private set; }
    public float AirPercent { get; private set; }
    public float GroundPercent { get; private set; }

    public MovementKart.Stats baseStats = new MovementKart.Stats
    {
        TopSpeed = 10f,
        Acceleration = 5f,
        AccelerationCurve = 4f,
        Braking = 10f,
        ReverseAcceleration = 5f,
        ReverseSpeed = 5f,
        Steer = 5f,
        Grip = .95f,
    };

    [Header("Vehicle Physics")]
    [Tooltip("The transform that determines the position of the kart's mass.")]
    public Transform CenterOfMass;

    [Header("Drifting")]
    [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
    public float DriftGrip = 0.4f;
    [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
    public float DriftAdditionalSteer = 5.0f;
    [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
    public float MinAngleToFinishDrift = 10.0f;
    [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
    public float MinSpeedPercentToFinishDrift = 0.5f;
    [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
    public float DriftControl = 10.0f;
    [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
    public float DriftDampening = 10.0f;

    [Header("VFX")]
    [Tooltip("VFX that will be placed on the wheels when drifting.")]
    public GameObject DriftTrailPrefab;
    [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
    public float DriftTrailVerticalOffset;

    [Header("Physical Wheels")]
    [Tooltip("The physical representations of the Kart's wheels.")]
    public WheelCollider FrontLeftWheel;
    public WheelCollider FrontRightWheel;
    public WheelCollider RearLeftWheel;
    public WheelCollider RearRightWheel;

    [Tooltip("Which layers the wheels will detect.")]
    public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

    Vector3 m_VerticalReference = Vector3.up;

    // Drift params
    public bool WantsToDrift { get; private set; } = false;
    public bool IsDrifting { get; private set; } = false;
    readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();

    MovementKart.Stats m_FinalStats;

    public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.ReverseSpeed);

    private void ActivateDriftVFX(bool active)
    {

        foreach (var trail in m_DriftTrailInstances)
            trail.trail.emitting = active && trail.wheel.GetGroundHit(out WheelHit hit);
    }

    private void UpdateDriftVFXOrientation()
    {

        foreach (var trail in m_DriftTrailInstances)
        {
            trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
            trail.trailRoot.transform.rotation = transform.rotation;
        }
    }


    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();

        if (DriftTrailPrefab != null)
        {
            AddTrailToWheel(FrontLeftWheel);
            AddTrailToWheel(FrontRightWheel);
            AddTrailToWheel(RearLeftWheel);
            AddTrailToWheel(RearRightWheel);
        }

    }

    void AddTrailToWheel(WheelCollider wheel)
    {
        GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
        TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
        trail.emitting = false;
        m_DriftTrailInstances.Add((trailRoot, wheel, trail));
    }

    void FixedUpdate()
    {

        GetInput();

        m_FinalStats = baseStats;

        Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

        int groundedCount = 0;
        if (FrontLeftWheel.isGrounded && FrontLeftWheel.GetGroundHit(out WheelHit hit))
            groundedCount++;
        if (FrontRightWheel.isGrounded && FrontRightWheel.GetGroundHit(out hit))
            groundedCount++;
        if (RearLeftWheel.isGrounded && RearLeftWheel.GetGroundHit(out hit))
            groundedCount++;
        if (RearRightWheel.isGrounded && RearRightWheel.GetGroundHit(out hit))
            groundedCount++;

        GroundPercent = (float)groundedCount / 4.0f;
        AirPercent = 1 - GroundPercent;

        MoveVehicle(m_AccelerateInput, m_BreakInput, m_TurnInput);

        UpdateDriftVFXOrientation();
    }

    private bool m_AccelerateInput;
    private bool m_BreakInput;
    private float m_TurnInput;

    public void GetInput()
    {
        m_AccelerateInput = Input.GetButton("Accelerate");
        m_BreakInput = Input.GetButton("Brake");
        m_TurnInput = Input.GetAxis("Horizontal");

        WantsToDrift =  Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
    }

    public void Reset()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = euler.z = 0f;
        transform.rotation = Quaternion.Euler(euler);
    }

    void MoveVehicle(bool accelerate, bool brake, float turnInput)
    {
        float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

        float accelerationCurveCoeff = 5;
        Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

        bool accelDirectionIsFwd = accelInput >= 0;
        bool localVelDirectionIsFwd = localVel.z >= 0;

        float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
        float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

        float currentSpeed = Rigidbody.velocity.magnitude;
        float accelRampT = currentSpeed / maxSpeed;
        float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * accelerationCurveCoeff;
        float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

        bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

        float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;
        float finalAcceleration = finalAccelPower * accelRamp;

        float turningPower =  turnInput * m_FinalStats.Steer;

        Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
        Vector3 fwd = turnAngle * transform.forward;
        Vector3 movement = fwd * accelInput * finalAcceleration * ((GroundPercent > 0.0f) ? 1.0f : 0.0f);

        bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

        if (wasOverMaxSpeed && !isBraking)
            movement *= 0.0f;

        Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
        newVelocity.y = Rigidbody.velocity.y;

        Rigidbody.velocity = newVelocity;

        // Drift
        if (GroundPercent > 0.0f)
        {

            float angularVelocitySteering = 0.4f;
            float angularVelocitySmoothSpeed = 40f;

            var angularVel = Rigidbody.angularVelocity;
            angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);
            Rigidbody.angularVelocity = angularVel;

            if (!IsDrifting)
            {
                if (WantsToDrift)
                {
                    IsDrifting = true;
                    ActivateDriftVFX(true);
                }
            }

            if (IsDrifting)
            {
                if (Vector3.Angle(Rigidbody.velocity, transform.forward) < 15)
                {
                    IsDrifting = false;
                }
            }
        }

        if (GroundPercent < 0.7f)
        {
            Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
            Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
            finalOrientationDirection.Normalize();
            if (finalOrientationDirection.sqrMagnitude > 0.0f)
            {
                Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(0.1f * Time.fixedDeltaTime)));
            }
        }
        ActivateDriftVFX(IsDrifting && accelDirectionIsFwd);
    }
}
