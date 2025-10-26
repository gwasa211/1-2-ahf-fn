using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// [클래스 1] 기물 정보 (변경 없음)
[System.Serializable]
public class BuildItem
{
    public string itemName;
    public GameObject actualPrefab;
    public GameObject previewPrefab;
    public Sprite itemIcon;
}

// [클래스 2] 핵심 상호작용 스크립트
public class PlayerInteraction : MonoBehaviour
{
    public enum PlayerMode { Combat, Build }
    public PlayerMode currentMode = PlayerMode.Combat;

    [Header("Build Settings")]
    public List<BuildItem> buildItems;
    public UIManager buildUIManager;
    public LayerMask groundLayerMask;

    [Header("Grid & Distance")]
    public float gridSize = 0.11f;
    public float buildDistanceInTiles = 3f;

    private GameObject currentPreviewObject;
    private int currentBuildIndex = 0;
    private Vector3 buildPosition;
    private bool canBuild = false;

    private Camera mainCamera;
    private float gridOffset;
    private float maxBuildDistance;

    void Start()
    {
        mainCamera = Camera.main;
        gridOffset = gridSize / 2f;
        maxBuildDistance = gridSize * buildDistanceInTiles;

        if (buildUIManager != null)
            buildUIManager.ShowBuildPanel(false);
    }

    void Update()
    {
        // [수정됨] 휠 클릭 -> V 키
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (currentMode == PlayerMode.Combat)
            {
                // [빌드 모드 진입]
                currentMode = PlayerMode.Build;
                if (buildUIManager != null)
                    buildUIManager.ShowBuildPanel(true);
                SelectItem(currentBuildIndex);
            }
            else // currentMode == PlayerMode.Build
            {
                // [전투 모드 진입]
                currentMode = PlayerMode.Combat;
                if (buildUIManager != null)
                    buildUIManager.ShowBuildPanel(false);

                if (currentPreviewObject != null)
                {
                    Destroy(currentPreviewObject);
                    currentPreviewObject = null;
                }
                canBuild = false;
            }
        }

        // 2. 현재 모드에 따라 행동 실행
        if (currentMode == PlayerMode.Build)
        {
            HandleItemSelection();
            HandleBuildMode();
        }
        else // currentMode == PlayerMode.Combat
        {
            HandleCombatMode();
        }
    }

    // (이하 함수들은 변경 없음)

    void HandleItemSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectItem(3);
    }

    void SelectItem(int index)
    {
        if (index < 0 || index >= buildItems.Count) return;
        currentBuildIndex = index;
        if (currentPreviewObject != null) Destroy(currentPreviewObject);
        currentPreviewObject = Instantiate(buildItems[currentBuildIndex].previewPrefab);
        currentPreviewObject.SetActive(false);
        if (buildUIManager != null) buildUIManager.UpdateSelection(currentBuildIndex);
    }

    void HandleBuildMode()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, maxBuildDistance, groundLayerMask))
        {
            TileInfo tile = hitInfo.collider.GetComponent<TileInfo>();
            if (tile != null && (tile.type == TileInfo.TileType.PlayerMove || tile.type == TileInfo.TileType.EnemyMove))
            {
                canBuild = true;
                if (currentPreviewObject != null) currentPreviewObject.SetActive(true);
                buildPosition = new Vector3(
                    Mathf.Floor(hitInfo.point.x / gridSize) * gridSize + gridOffset,
                    hitInfo.transform.position.y + 0.01f,
                    Mathf.Floor(hitInfo.point.z / gridSize) * gridSize + gridOffset
                );
                if (currentPreviewObject != null) currentPreviewObject.transform.position = buildPosition;
                if (Input.GetMouseButtonDown(0))
                {
                    Instantiate(buildItems[currentBuildIndex].actualPrefab, buildPosition, Quaternion.identity);
                }
            }
            else
            {
                canBuild = false;
                if (currentPreviewObject != null) currentPreviewObject.SetActive(false);
            }
        }
        else
        {
            canBuild = false;
            if (currentPreviewObject != null) currentPreviewObject.SetActive(false);
        }
    }

    void HandleCombatMode()
    {
        // (전투 로직)
    }
}