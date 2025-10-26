using UnityEngine;
using Cinemachine;

public class CinemachineSwitcher : MonoBehaviour
{
    // 1인칭 전투용 카메라
    public CinemachineVirtualCamera firstPersonCam;

    // 3인칭 일반용 카메라
    public CinemachineVirtualCamera thirdPersonCam;

    // 이 변수를 PlayerController가 읽어갑니다.
    public bool isThirdPersonView = false;

    void Start()
    {
        // 시작은 1인칭
        firstPersonCam.Priority = 20;
        thirdPersonCam.Priority = 0;
        isThirdPersonView = false;
    }

    void Update()
    {
        // [수정됨] V 키 -> 마우스 휠 버튼(2)
        if (Input.GetMouseButtonDown(2)) // 2 = 마우스 휠 버튼 클릭
        {
            isThirdPersonView = !isThirdPersonView;

            if (isThirdPersonView)
            {
                // 3인칭 켜기
                thirdPersonCam.Priority = 20;
                firstPersonCam.Priority = 0;
            }
            else
            {
                // 1인칭 켜기
                firstPersonCam.Priority = 20;
                thirdPersonCam.Priority = 0;
            }
        }
    }
}