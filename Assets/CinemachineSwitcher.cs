using UnityEngine;
using Cinemachine;

public class CinemachineSwitcher : MonoBehaviour
{
    // �⺻ TPS ī�޶�
    public CinemachineVirtualCamera virtualCam;
    // ���� ȸ�� TPS ī�޶�
    public CinemachineFreeLook freelookCam;

    public bool usingFreelook = false;

    void Start()
    {
        // ������ Virtual Camera Ȱ��ȭ
        virtualCam.Priority = 10;
        freelookCam.Priority = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // ��Ŭ��
        {
            usingFreelook = !usingFreelook;

            if (usingFreelook)
            {
                freelookCam.Priority = 20; // Freelook Ȱ��ȭ
                virtualCam.Priority = 0;
            }
            else
            {
                virtualCam.Priority = 20; // Virtual Camera Ȱ��ȭ
                freelookCam.Priority = 0;
            }
        }
    }
}