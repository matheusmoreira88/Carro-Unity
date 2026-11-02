
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(WheelCollider))]
[DisallowMultipleComponent]
public class WheelDebugVisual : MonoBehaviour
{
    #region Settings

    [Header("Visibility")]
    [SerializeField] private bool startVisible = false;

    [Header("Wheel Rings")]
    [Min(0.01f)]
    [SerializeField] private float wheelWidth = 0.26f;

    [Min(0.005f)]
    [SerializeField] private float ringThickness = 0.04f;

    [Header("Steering Arrow")]
    [Min(0.1f)]
    [SerializeField] private float directionLength = 1.6f;

    [Min(0.01f)]
    [SerializeField] private float arrowWidth = 0.045f;

    [Header("Suspension Ball")]
    [Min(0.05f)]
    [SerializeField] private float suspensionBallSize = 0.28f;

    [Header("Load Visualization")]
    [Min(1.01f)]
    [SerializeField] private float fullRedLoad = 1.8f;

    [Header("Slip Visualization")]
    [SerializeField] private bool showSlip = true;
    [SerializeField] private float slipScale = 0.7f;

    #endregion

    #region Components

    private WheelCollider wheel;
    private Rigidbody vehicleBody;

    private LineRenderer leftRing;
    private LineRenderer rightRing;
    private LineRenderer directionLine;
    private LineRenderer arrowHead;
    private LineRenderer forwardSlipLine;
    private LineRenderer sideSlipLine;

    private GameObject suspensionBall;
    private GameObject contactMarker;

    private readonly List<Material> materials =
        new List<Material>();

    private bool isVisible;

    #endregion

    #region Colors

    private static readonly Color LowLoad =
        new Color(0.15f, 1f, 0.2f);

    private static readonly Color NormalLoad =
        Color.yellow;

    private static readonly Color HighLoad =
        Color.red;

    private static readonly Color SuspensionColor =
        Color.cyan;

    private static readonly Color DirectionColor =
        new Color(0.1f, 0.5f, 1f);

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        CreateVisuals();
        SetVisible(startVisible);
    }

    private void LateUpdate()
    {
        if (isVisible)
            UpdateVisuals();
    }

    private void OnDestroy()
    {
        foreach (Material material in materials)
        {
            if (material != null)
                Destroy(material);
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        wheel = GetComponent<WheelCollider>();
        vehicleBody = GetComponentInParent<Rigidbody>();
    }

    private void CreateVisuals()
    {
        leftRing = CreateLine(
            "LeftRing", 48, true, ringThickness, LowLoad);

        rightRing = CreateLine(
            "RightRing", 48, true, ringThickness, LowLoad);

        directionLine = CreateLine(
            "Direction", 2, false, arrowWidth, DirectionColor);

        arrowHead = CreateLine(
            "ArrowHead", 3, false, arrowWidth, DirectionColor);

        forwardSlipLine = CreateLine(
            "ForwardSlip", 2, false, 0.025f, Color.yellow);

        sideSlipLine = CreateLine(
            "SideSlip", 2, false, 0.025f, Color.red);

        suspensionBall = CreateSphere(
            "SuspensionBall", SuspensionColor);

        contactMarker = CreateSphere(
            "ContactMarker", Color.green);
    }

    #endregion

    #region Materials

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("HDRP/Unlit");

        if (shader == null)
            shader = Shader.Find(
                "Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError(
                "WheelDebugVisual: Shader nao encontrado!",
                this
            );
            return null;
        }

        Material material = new Material(shader);
        SetMaterialColor(material, color);

        materials.Add(material);
        return material;
    }

    private void SetMaterialColor(
        Material material,
        Color color)
    {
        if (material == null)
            return;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_UnlitColor"))
            material.SetColor("_UnlitColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
    }

    #endregion

    #region Visual Creation

    private LineRenderer CreateLine(
        string name,
        int points,
        bool loop,
        float width,
        Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform, false);

        LineRenderer line = obj.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.loop = loop;
        line.positionCount = points;
        line.startWidth = width;
        line.endWidth = width;
        line.numCapVertices = 4;
        line.numCornerVertices = 3;
        line.alignment = LineAlignment.View;

        line.sharedMaterial = CreateMaterial(color);

        return line;
    }

    private GameObject CreateSphere(
        string name,
        Color color)
    {
        GameObject sphere =
            GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere.name = name;
        sphere.transform.SetParent(transform, false);

        Collider collider = sphere.GetComponent<Collider>();

        if (collider != null)
            Destroy(collider);

        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial(color);

        return sphere;
    }

    #endregion

    #region Visibility

    public void SetVisible(bool visible)
    {
        isVisible = visible;

        SetLineVisible(leftRing, visible);
        SetLineVisible(rightRing, visible);
        SetLineVisible(directionLine, visible);
        SetLineVisible(arrowHead, visible);

        SetLineVisible(forwardSlipLine, false);
        SetLineVisible(sideSlipLine, false);

        if (suspensionBall != null)
            suspensionBall.SetActive(visible);

        if (contactMarker != null)
            contactMarker.SetActive(false);

        if (visible && wheel != null)
            UpdateVisuals();
    }

    private void SetLineVisible(
        LineRenderer line,
        bool visible)
    {
        if (line != null)
            line.enabled = visible;
    }

    #endregion

    #region Visual Updates

    private void UpdateVisuals()
    {
        wheel.GetWorldPose(
            out Vector3 wheelCenter,
            out _
        );

        // A direcao nao pode acompanhar o giro do pneu.
        Quaternion steeringRotation =
            transform.rotation *
            Quaternion.Euler(0f, wheel.steerAngle, 0f);

        bool grounded = wheel.GetGroundHit(
            out WheelHit hit);

        float loadRatio = CalculateLoadRatio(
            grounded, hit);

        UpdateWheelRings(
            wheelCenter, steeringRotation, grounded, loadRatio);

        UpdateDirectionArrow(
            wheelCenter, steeringRotation);

        UpdateSuspensionBall(wheelCenter, loadRatio);
        UpdateContactMarker(grounded, hit);
        UpdateSlipVectors(grounded, hit);
    }

    #endregion

    #region Load Visualization

    private float CalculateLoadRatio(
        bool grounded,
        WheelHit hit)
    {
        if (!grounded)
            return 0f;

        float massPerWheel = wheel.sprungMass;

        if (massPerWheel <= 0f && vehicleBody != null)
            massPerWheel = vehicleBody.mass / 4f;

        float referenceForce = Mathf.Max(
            massPerWheel * Physics.gravity.magnitude,
            1f
        );

        return Mathf.Max(0f, hit.force) / referenceForce;
    }

    private Color GetLoadColor(
        float ratio,
        bool grounded)
    {
        if (!grounded)
            return Color.gray;

        if (ratio <= 1f)
        {
            return Color.Lerp(
                LowLoad,
                NormalLoad,
                Mathf.Clamp01(ratio)
            );
        }

        float t = Mathf.InverseLerp(
            1f,
            fullRedLoad,
            ratio
        );

        return Color.Lerp(
            NormalLoad,
            HighLoad,
            t
        );
    }

    #endregion

    #region Wheel Rings

    private void UpdateWheelRings(
        Vector3 center,
        Quaternion rotation,
        bool grounded,
        float loadRatio)
    {
        Color color = GetLoadColor(loadRatio, grounded);

        SetMaterialColor(leftRing.sharedMaterial, color);
        SetMaterialColor(rightRing.sharedMaterial, color);

        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        Vector3 forward = rotation * Vector3.forward;

        float halfWidth = wheelWidth * 0.5f;

        DrawRing(
            leftRing,
            center - right * halfWidth,
            up,
            forward
        );

        DrawRing(
            rightRing,
            center + right * halfWidth,
            up,
            forward
        );
    }

    private void DrawRing(
        LineRenderer line,
        Vector3 center,
        Vector3 up,
        Vector3 forward)
    {
        int segments = line.positionCount;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 point = center + wheel.radius *
                (up * Mathf.Cos(angle) +
                 forward * Mathf.Sin(angle));

            line.SetPosition(i, point);
        }
    }

    #endregion

    #region Steering Direction

    private void UpdateDirectionArrow(
        Vector3 center,
        Quaternion rotation)
    {
        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        // A seta comeca na frente do pneu.
        Vector3 start = center
            + forward * (wheel.radius + 0.08f)
            + up * 0.08f;

        Vector3 end = start + forward * directionLength;

        directionLine.SetPosition(0, start);
        directionLine.SetPosition(1, end);

        float headLength = Mathf.Min(
            0.25f,
            directionLength * 0.3f
        );

        float headWidth = headLength * 0.6f;

        Vector3 left = end
            - forward * headLength
            - right * headWidth;

        Vector3 rightPoint = end
            - forward * headLength
            + right * headWidth;

        arrowHead.SetPosition(0, left);
        arrowHead.SetPosition(1, end);
        arrowHead.SetPosition(2, rightPoint);
    }

    #endregion

    #region Suspension Ball

    private void UpdateSuspensionBall(
        Vector3 wheelCenter,
        float loadRatio)
    {
        if (suspensionBall == null)
            return;

        Vector3 mount = transform.TransformPoint(wheel.center);

        // A esfera acompanha a posicao da suspensao.
        suspensionBall.transform.position = Vector3.Lerp(
            mount,
            wheelCenter,
            0.55f
        );

        float sizeMultiplier = Mathf.Lerp(
            0.85f,
            1.25f,
            Mathf.Clamp01(loadRatio / fullRedLoad)
        );

        suspensionBall.transform.localScale =
            Vector3.one * suspensionBallSize * sizeMultiplier;
    }

    #endregion

    #region Ground Contact

    private void UpdateContactMarker(
        bool grounded,
        WheelHit hit)
    {
        if (contactMarker == null)
            return;

        contactMarker.SetActive(grounded);

        if (!grounded)
            return;

        contactMarker.transform.position = hit.point;
        contactMarker.transform.localScale =
            Vector3.one * 0.09f;
    }

    #endregion

    #region Slip Visualization

    private void UpdateSlipVectors(
        bool grounded,
        WheelHit hit)
    {
        bool active = showSlip && grounded;

        SetLineVisible(forwardSlipLine, active);
        SetLineVisible(sideSlipLine, active);

        if (!active)
            return;

        DrawSlip(
            forwardSlipLine,
            hit.point,
            hit.forwardDir,
            hit.forwardSlip
        );

        DrawSlip(
            sideSlipLine,
            hit.point,
            hit.sidewaysDir,
            hit.sidewaysSlip
        );
    }

    private void DrawSlip(
        LineRenderer line,
        Vector3 start,
        Vector3 direction,
        float slip)
    {
        float length = Mathf.Clamp(
            slip * slipScale,
            -1.5f,
            1.5f
        );

        line.SetPosition(0, start);
        line.SetPosition(1, start + direction * length);
    }

    #endregion
}
