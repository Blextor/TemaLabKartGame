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

        [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
        public float CoastingDrag;

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
        CoastingDrag = 4f,
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

    const float k_NullInput = 0.01f;
    const float k_NullSpeed = 0.01f;
    Vector3 m_VerticalReference = Vector3.up;

    // Drift params
    public bool WantsToDrift { get; private set; } = false;
    public bool IsDrifting { get; private set; } = false;
    float m_CurrentGrip = 1.0f;
    float m_DriftTurningPower = 0.0f;
    float m_PreviousGroundPercent = 1.0f;
    readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();

    MovementKart.Stats m_FinalStats;

    Quaternion m_LastValidRotation;
    Vector3 m_LastValidPosition;
    Vector3 m_LastCollisionNormal;
    bool m_HasCollision;
    bool m_InAir = false;

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

        m_CurrentGrip = baseStats.Grip;

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
        m_FinalStats.Grip = Mathf.Clamp(m_FinalStats.Grip, 0, 1);

        // apply our physics properties
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

        // calculate how grounded and airborne we are
        GroundPercent = (float)groundedCount / 4.0f;
        AirPercent = 1 - GroundPercent;

        // apply vehicle physics

        MoveVehicle(m_AccelerateInput, m_BreakInput, m_TurnInput);

        m_PreviousGroundPercent = GroundPercent;

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

        WantsToDrift =  m_BreakInput && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;

    }


    public void Reset()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = euler.z = 0f;
        transform.rotation = Quaternion.Euler(euler);
    }

    public float LocalSpeed()
    {

        float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
        if (Mathf.Abs(dot) > 0.1f)
        {
            float speed = Rigidbody.velocity.magnitude;
            return dot < 0 ? -(speed / m_FinalStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
        }
        return 0f;

    }

    void OnCollisionEnter(Collision collision) => m_HasCollision = true;
    void OnCollisionExit(Collision collision) => m_HasCollision = false;

    void OnCollisionStay(Collision collision)
    {
        m_HasCollision = true;
        m_LastCollisionNormal = Vector3.zero;
        float dot = -1.0f;

        foreach (var contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                m_LastCollisionNormal = contact.normal;
        }
    }

    void MoveVehicle(bool accelerate, bool brake, float turnInput)
    {
        float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

        // manual acceleration curve coefficient scalar
        float accelerationCurveCoeff = 5;
        Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

        bool accelDirectionIsFwd = accelInput >= 0;
        bool localVelDirectionIsFwd = localVel.z >= 0;

        // use the max speed for the direction we are going--forward or reverse.
        float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
        float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

        float currentSpeed = Rigidbody.velocity.magnitude;
        float accelRampT = currentSpeed / maxSpeed;
        float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * accelerationCurveCoeff;
        float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

        bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

        // if we are braking (moving reverse to where we are going)
        // use the braking accleration instead
        float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;

        float finalAcceleration = finalAccelPower * accelRamp;

        // apply inputs to forward/backward
        float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * m_FinalStats.Steer;

        Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
        Vector3 fwd = turnAngle * transform.forward;
        Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

        // forward movement
        bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

        // if over max speed, cannot accelerate faster.
        if (wasOverMaxSpeed && !isBraking)
            movement *= 0.0f;

        Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
        newVelocity.y = Rigidbody.velocity.y;

        //  clamp max speed if we are on ground
        if (GroundPercent > 0.0f && !wasOverMaxSpeed)
        {
            newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
        }

        // coasting is when we aren't touching accelerate
        if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
        {
            newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag);
        }

        Rigidbody.velocity = newVelocity;

        // Drift
        if (GroundPercent > 0.0f)
        {
            if (m_InAir)
            {
                m_InAir = false;
            }

            // manual angular velocity coefficient
            float angularVelocitySteering = 0.4f;
            float angularVelocitySmoothSpeed = 20f;

            // turning is reversed if we're going in reverse and pressing reverse
            if (!localVelDirectionIsFwd && !accelDirectionIsFwd)
                angularVelocitySteering *= -1.0f;

            var angularVel = Rigidbody.angularVelocity;

            // move the Y angular velocity towards our target
            angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

            // apply the angular velocity
            Rigidbody.angularVelocity = angularVel;

            // If the karts lands with a forward not in the velocity direction, we start the drift
            if (GroundPercent >= 0.0f && m_PreviousGroundPercent < 0.1f)
            {
                Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, m_VerticalReference).normalized;
                if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad))
                {
                    IsDrifting = true;
                    m_CurrentGrip = DriftGrip;
                    m_DriftTurningPower = 0.0f;
                }
            }

            // Drift Management
            if (!IsDrifting)
            {
                if ((WantsToDrift || isBraking) && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                {
                    IsDrifting = true;
                    m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                    m_CurrentGrip = DriftGrip;

                    ActivateDriftVFX(true);
                }
            }

            if (IsDrifting)
            {
                float turnInputAbs = Mathf.Abs(turnInput);
                if (turnInputAbs < k_NullInput)
                    m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

                // Update the turning power based on input
                float driftMaxSteerValue = m_FinalStats.Steer + DriftAdditionalSteer;
                m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnInput * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                bool facingVelocity = Vector3.Dot(Rigidbody.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

                bool canEndDrift = true;
                if (isBraking)
                    canEndDrift = false;
                else if (!facingVelocity)
                    canEndDrift = false;
                else if (turnInputAbs >= k_NullInput && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                    canEndDrift = false;

                if (canEndDrift || currentSpeed < k_NullSpeed)
                {
                    // No Input, and car aligned with speed direction => Stop the drift
                    IsDrifting = false;
                    m_CurrentGrip = m_FinalStats.Grip;
                }
            }
        }
        else
        {
            m_InAir = true;
        }


        // Airborne / Half on ground management
        if (GroundPercent < 0.7f)
        {
            Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
            Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
            finalOrientationDirection.Normalize();
            if (finalOrientationDirection.sqrMagnitude > 0.0f)
            {
                Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(5 * Time.fixedDeltaTime)));
            }
        }
        ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);
    }
}
