using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class Movement : MonoBehaviour
{
    /* ────────── Inspector fields ────────── */
    [Header("Input Actions")]
    [SerializeField] private InputAction thrust;     // Button
    [SerializeField] private InputAction rotation;   // Value (‑1 … 1)

    [Header("Tuning")]
    [SerializeField] private float thrustStrength = 100f;
    [SerializeField] private float rotationStrength = 100f;
    [SerializeField] private float touchSensitivity = 1f;

    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Audio")]
    [SerializeField] private AudioClip mainEngineSFX;

    [Header("Particles")]
    [SerializeField] private ParticleSystem mainEngineParticles;
    [SerializeField] private ParticleSystem leftThrusterParticles;
    [SerializeField] private ParticleSystem rightThrusterParticles;

    /* ────────── runtime refs ────────── */
    private Rigidbody rb;
    private AudioSource audioSource;

    /* ────────── state flags ────────── */
    private bool isMobile;
    private bool thrustHeld;
    private bool rotateLeftHeld;
    private bool rotateRightHeld;

    /* ─────────────────────────────────────────────────────────────────── */
    void Awake()
    {
        if (uiDocument == null)
            uiDocument = FindAnyObjectByType<UIDocument>();
    }

    void OnEnable()
    {
        thrust.Enable();
        rotation.Enable();

        isMobile = Application.isMobilePlatform;
        SetupUIButtons();
    }

    void OnDisable()
    {
        thrust.Disable();
        rotation.Disable();
    }

    /* ─────────────────────────────────────────────────────────────────── */
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        ProcessThrust();
        ProcessRotation();
    }

    /* ───────────── UI BUTTONS ────────────────────────────────────────── */
    void SetupUIButtons()
    {
        var root = uiDocument?.rootVisualElement;
        if (root == null) return;

#if UNITY_EDITOR
        root.style.display = DisplayStyle.Flex;                // visible in Play Mode
#else
        root.style.display = isMobile ? DisplayStyle.Flex : DisplayStyle.None;
#endif

        // ── thrust ────────────────────────────────────────────────
        RegisterHold(
            root.Q<Button>("thrust-button"),
            () => { thrustHeld = true; },
            () => { thrustHeld = false; });

        // ── rotate left ───────────────────────────────────────────
        RegisterHold(
            root.Q<Button>("rotate-left-button"),
            () => { rotateLeftHeld = true; rotateRightHeld = false; },
            () => { rotateLeftHeld = false; });

        // ── rotate right ──────────────────────────────────────────
        RegisterHold(
            root.Q<Button>("rotate-right-button"),
            () => { rotateRightHeld = true; rotateLeftHeld = false; },
            () => { rotateRightHeld = false; });
    }

    /// <summary>
    /// Register pointer events in the capture (trickle‑down) phase so they fire
    /// before Button.Clickable swallows them.
    /// </summary>
    static void RegisterHold(Button btn, System.Action onDown, System.Action onUp)
    {
        // — start —
        btn.RegisterCallback<PointerDownEvent>(
            _ => onDown?.Invoke(),
            TrickleDown.TrickleDown);          // <‑‑ Phase changed here

        // — stop —
        btn.RegisterCallback<PointerUpEvent>(
            _ => onUp?.Invoke(),
            TrickleDown.TrickleDown);

        btn.RegisterCallback<PointerLeaveEvent>(
            _ => onUp?.Invoke(),
            TrickleDown.TrickleDown);

        btn.RegisterCallback<PointerCancelEvent>(
            _ => onUp?.Invoke(),
            TrickleDown.TrickleDown);
    }

    /* ───────────── PHYSICS ───────────────────────────────────────────── */
    void ProcessThrust()
    {
        bool shouldThrust = thrust.IsPressed() || thrustHeld;

        if (shouldThrust)
        {
            StartThrusting();
        }
        else
        {
            StopThrusting();
        }
    }

    void StartThrusting()
    {
        rb.AddRelativeForce(Vector3.up * (Time.fixedDeltaTime * thrustStrength));

        if (!audioSource.isPlaying)          // <‑‑ start once
        {
            audioSource.PlayOneShot(mainEngineSFX);
        }
        if (!mainEngineParticles.isPlaying)  // <‑‑ play once
        {
            mainEngineParticles.Play();
        }
    }

    void StopThrusting()
    {
        audioSource.Stop();
        mainEngineParticles.Stop();
    }

    void ProcessRotation()
    {
        float rotInput = rotation.ReadValue<float>();

        if (isMobile || Application.isEditor)
        {
            if (rotateLeftHeld) rotInput -= 1f;
            if (rotateRightHeld) rotInput += 1f;
            rotInput *= touchSensitivity;
        }

        if (rotInput < 0f)
        {
            RotateLeft();
        }
        else if (rotInput > 0f)
        {
            RotateRight();
        }
        else
        {
            StopRotating();
        }
    }

    void RotateLeft()
    {
        ApplyRotation(rotationStrength);
        if (!leftThrusterParticles.isPlaying)
        {
            rightThrusterParticles.Stop();
            leftThrusterParticles.Play();
        }
    }

    void RotateRight()
    {
        ApplyRotation(-rotationStrength);
        if (!rightThrusterParticles.isPlaying)
        {
            leftThrusterParticles.Stop();
            rightThrusterParticles.Play();
        }
    }

    void StopRotating()
    {
        leftThrusterParticles.Stop();
        rightThrusterParticles.Stop();
    }

    void ApplyRotation(float rotationPerFixedUpdate)
    {
        rb.freezeRotation = true;
        transform.Rotate(Vector3.forward * (Time.fixedDeltaTime * rotationPerFixedUpdate));
        rb.freezeRotation = false;
    }
}
