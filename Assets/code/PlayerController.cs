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

    // --- [추가] 1인칭일 때 끌 모델 ---
    [Header("Object Links")]
    public GameObject playerModel; // 1인칭일 때 비활성화할 3D 모델

    // --- 카메라 연결 ---
    public CinemachineSwitcher cameraSwitcher;
    public CinemachineVirtualCamera firstPersonCam;
    public CinemachineVirtualCamera thirdPersonCam;

    private CinemachinePOV pov;
    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;

    // --- HP ---
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

        // [추가] 모델이 연결되었는지 확인
        if (playerModel == null)
        {
            Debug.LogError("!!! [오류] 'Player Model' 변수가 비어있습니다! 인스펙터에서 'default' 오브젝트를 연결해주세요!");
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
        // (중력/이동 로직은 그대로)
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
        if (isGrounded && Input.GetKeyDown(KeyCode.Space)) velocity.y = jumpPower;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (cameraSwitcher.isThirdPersonView) // (이게 실제로는 1인칭!)
        {
            // [3인칭 조작] -> [실제 1인칭 조작]

            // [수정] 3D 모델을 '끈다' (뚫림 문제 해결!)
            if (playerModel.activeSelf) playerModel.SetActive(false);

            if (pov.enabled) pov.enabled = false; // 1인칭 POV 끄기 (이건 왜 끄는지 헷갈리지만, 기존 로직 유지)

            // (이동/회전 로직은 그대로)
            Vector3 camForward = mainCamera.transform.forward; // (이게 1인칭인데 메인카메라를 쓰네요? 일단 유지)
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            Vector3 move = (camForward * z + camRight * x).normalized;
            controller.Move(move * speed * Time.deltaTime);
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);
        }
        else // (이게 실제로는 3인칭!)
        {
            // [1인칭 조작] -> [실제 3인칭 조작]

            // [수정] 3D 모델을 '켠다' (3인칭에선 보여야 함)
            if (!playerModel.activeSelf) playerModel.SetActive(true);

            if (!pov.enabled) pov.enabled = true; // 1인칭 POV 켜기 (이것도 헷갈리지만, 기존 로직 유지)

            // (이동/회전 로직은 그대로)
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

        // (Tab 키 로직)
        if (!cameraSwitcher.isThirdPersonView && Input.GetKeyDown(KeyCode.Tab)) // (이게 실제 3인칭일때 리셋)
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