using UnityEngine;
using Cinemachine;

public class CinemachineSwitcher : MonoBehaviour
{
    // 기본 TPS 카메라
    public CinemachineVirtualCamera virtualCam;
    // 자유 회전 TPS 카메라
    public CinemachineFreeLook freelookCam;

    public bool usingFreelook = false;

    void Start()
    {
        // 시작은 Virtual Camera 활성화
        virtualCam.Priority = 10;
        freelookCam.Priority = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            usingFreelook = !usingFreelook;

            if (usingFreelook)
            {
                freelookCam.Priority = 20; // Freelook 활성화
                virtualCam.Priority = 0;
            }
            else
            {
                virtualCam.Priority = 20; // Virtual Camera 활성화
                freelookCam.Priority = 0;
            }
        }
    }
}