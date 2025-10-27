using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // (... 변수 선언은 동일 ...)
    public float speed = 5f;
    public float jumpPower = 5f;
    public float gravity = -9.81f;
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    [Header("Object Links")]
    public GameObject playerModel;
    [Header("Camera Links")]
    public CinemachineSwitcher cameraSwitcher;
    public CinemachineVirtualCamera firstPersonCam;
    public CinemachineVirtualCamera thirdPersonCam;
    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CinemachinePOV pov;
    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;

    [Header("HP")]
    public int maxHP = 100;
    private int currentHP;
    public Slider hpSlider;

    private Camera mainCamera;

    void Start()
    {
        Debug.Log("[DEBUG] PlayerController: Start() 함수 *시작*.");
        controller = GetComponent<CharacterController>();

        if (firstPersonCam != null)
            pov = firstPersonCam.GetCinemachineComponent<CinemachinePOV>();

        // --- [강화된 디버깅 1: Null 체크] ---
        if (pov == null)
        {
            Debug.LogError("!!! [오류] PlayerController: 'First Person Cam'에 'CinemachinePOV' 설정이 없습니다! 스크립트를 비활성화합니다!");
            this.enabled = false;
            return;
        }
        if (playerModel == null)
        {
            Debug.LogError("!!! [오류] PlayerController: 'Player Model' 변수가 비어있습니다! 스크립트를 비활성화합니다!");
            this.enabled = false;
            return;
        }
        if (groundCheck == null)
        {
            Debug.LogError("!!! [오류] PlayerController: 'Ground Check' 변수가 비어있습니다! 스크립트를 비활성화합니다!");
            this.enabled = false;
            return;
        }
        if (controller == null)
        {
            Debug.LogError("!!! [오류] PlayerController: 'CharacterController' 컴포넌트가 없습니다! 스크립트를 비활성화합니다!");
            this.enabled = false;
            return;
        }
        if (cameraSwitcher == null) Debug.LogWarning("[주의] PlayerController: 'Camera Switcher'가 연결되지 않았습니다.");
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("!!! [오류] PlayerController: 씬에 'Main Camera' 태그를 가진 카메라가 없습니다!");
        // --- [디버깅 끝] ---

        currentHP = maxHP;
        if (hpSlider != null) hpSlider.value = 1f;

        if (pov != null) pov.enabled = false;
        Debug.Log("[DEBUG] PlayerController: Start() 함수 *종료*. (POV 비활성화 완료)");
    }

    void Update()
    {
        // Debug.Log("[DEBUG] PlayerController: Update() 실행 중..."); // (너무 많이 뜨므로 주석 처리)

        // --- [게임 시작 게이트] ---
        if (GameManager.Instance == null)
        {
            Debug.LogError("[DEBUG] PlayerController: GameManager.Instance가 null입니다! (GameManager가 씬에 없거나 비활성화됨)");
            return; // GameManager 없으면 아무것도 안 함
        }

        if (!GameManager.Instance.isGameStarted)
        {
            // Debug.Log("[DEBUG] PlayerController: 게임 시작 대기 중 (isGameStarted == false)"); // (너무 많이 뜨므로 주석 처리)

            if (pov != null && pov.enabled)
            {
                pov.enabled = false; // (게임 시작 전에는 POV 강제 비활성화)
            }

            // (중력 적용 및 바닥 붙임)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            return; // Update() 함수를 즉시 종료
        }
        // --- [게이트 끝] ---

        // (이하는 isGameStarted == true일 때만 실행)

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpPower;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (cameraSwitcher.isThirdPersonView) // (1인칭)
        {
            if (playerModel.activeSelf) playerModel.SetActive(false);
            if (pov.enabled) pov.enabled = false; // 1인칭일땐 POV 끄기

            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            Vector3 move = (camForward * z + camRight * x).normalized;
            controller.Move(move * speed * Time.deltaTime);
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);
        }
        else // (3인칭)
        {
            if (!playerModel.activeSelf) playerModel.SetActive(true);
            if (!pov.enabled) pov.enabled = true; // 3인칭일땐 POV 켜기

            Vector3 camForward = firstPersonCam.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 camRight = firstPersonCam.transform.right;
            camRight.y = 0;
            camRight.Normalize();
            Vector3 move = (camForward * z + camRight * x).normalized;
            controller.Move(move * speed * Time.deltaTime);
            float cameraYaw = pov.m_HorizontalAxis.Value;
            transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        }

        if (cameraSwitcher.isThirdPersonView && Input.GetKeyDown(KeyCode.Tab))
        {
            if (pov != null)
            {
                pov.m_HorizontalAxis.Value = transform.eulerAngles.y;
                pov.m_VerticalAxis.Value = 0f;
            }
        }
    }

    // (HP/Die 함수는 변경 없음)
    public void TakeDamage(int damage) { /* ... */ }
    void Die() { /* ... */ }
}