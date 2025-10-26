using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
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

    // --- [새 기능] Ground Check (수동 센서) ---
    [Header("Ground Check Settings")]
    public Transform groundCheck; // '발' 위치 (빈 오브젝트)
    public float groundDistance = 0.4f; // 감지 구체(Sphere)의 반경
    public LayerMask groundMask; // '바닥'으로 인식할 레이어
    // --- [새 기능 끝] ---

    private CinemachinePOV pov;
    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded; // (이제 이 변수는 CheckSphere가 제어함)

    [Header("HP")]
    public int maxHP = 100;
    private int currentHP;
    public Slider hpSlider;

    private Camera mainCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (firstPersonCam != null)
            pov = firstPersonCam.GetCinemachineComponent<CinemachinePOV>();

        if (pov == null)
        {
            Debug.LogError("!!! [오류] 'First Person Cam'에 'CinemachinePOV' 설정이 없습니다! Aim을 POV로 바꿔주세요!");
            this.enabled = false;
            return;
        }

        if (playerModel == null)
        {
            Debug.LogError("!!! [오류] 'Player Model' 변수가 비어있습니다! 인스펙터에서 'default' 오브젝트를 연결해주세요!");
            this.enabled = false;
            return;
        }

        // [추가] Ground Check 오브젝트가 없으면 오류 방지
        if (groundCheck == null)
        {
            Debug.LogError("!!! [오류] 'Ground Check' 변수가 비어있습니다! 'player'의 자식으로 'GroundCheck' 빈 오브젝트를 만들어 연결해주세요!");
            this.enabled = false;
            return;
        }

        mainCamera = Camera.main;
        currentHP = maxHP;
        if (hpSlider != null) hpSlider.value = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- [수정] Ground Check ---
        // 1. 'groundCheck' 위치에 'groundDistance' 반경의 구체를 쏴서 'groundMask'와 닿는지 확인
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 2. 바닥에 닿았고, 중력 속도(y)가 0보다 크면 (즉, 떨어지는 중이면)
        if (isGrounded && velocity.y < 0)
        {
            // 3. 중력 속도를 -2f로 리셋 (바닥에 딱 붙어있도록)
            velocity.y = -2f;
        }
        // --- [수정 끝] ---


        // [유지] 점프 로직 (이제 isGrounded가 훨씬 정확해짐)
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpPower; // 점프!
        }

        // [유지] 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // 중력/점프 이동 적용


        // (이후 이동/회전 로직은 동일)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (cameraSwitcher.isThirdPersonView) // (이게 실제로는 1인칭!)
        {
            // [1인칭 조작]
            if (playerModel.activeSelf) playerModel.SetActive(false);
            if (pov.enabled) pov.enabled = false;

            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            Vector3 move = (camForward * z + camRight * x).normalized;
            controller.Move(move * speed * Time.deltaTime); // 이동 적용
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);
        }
        else // (이게 실제로는 3인칭!)
        {
            // [3인칭 조작]
            if (!playerModel.activeSelf) playerModel.SetActive(true);
            if (!pov.enabled) pov.enabled = true;

            Vector3 camForward = firstPersonCam.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            Vector3 camRight = firstPersonCam.transform.right;
            camRight.y = 0;
            camRight.Normalize();
            Vector3 move = (camForward * z + camRight * x).normalized;
            controller.Move(move * speed * Time.deltaTime); // 이동 적용
            float cameraYaw = pov.m_HorizontalAxis.Value;
            transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        }

        // (Tab 키 로직)
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
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (hpSlider != null) hpSlider.value = (float)currentHP / maxHP;
        if (currentHP <= 0) Die();
    }
    void Die()
    {
        // ...
    }
}