using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
// using UnityEngine.UI; // [삭제] UI 변수 안 씀

public class PlayerInteraction : MonoBehaviour
{
    [Header("Build Settings")]
    public GameObject[] actualBlockPrefabs;
    public GameObject[] previewBlockPrefabs;
    public float buildDistance = 5f;

    [Header("Links")]
    public Camera mainCamera;
    public LayerMask buildableLayer;

    // --- [새 기능] 빌드 모드 ---
    // public GameObject crosshairUI; // [삭제]
    private bool isBuildMode = false;
    // --- [새 기능 끝] ---

    private List<GameObject> currentPreviews = new List<GameObject>();
    private int currentBuildIndex = 0;

    private Vector3 snappedPosition;
    private bool canBuild = false;

    // (OnEnable, OnDisable, OnSceneLoaded 함수는 변경 없음)
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetupPreviews();
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupPreviews();
    }

    void SetupPreviews()
    {
        // (기존 미리보기 청소/생성 로직은 동일)
        foreach (GameObject oldPreview in currentPreviews)
        {
            if (oldPreview != null) Destroy(oldPreview);
        }
        currentPreviews.Clear();

        foreach (GameObject prefab in previewBlockPrefabs)
        {
            if (prefab != null)
            {
                GameObject preview = Instantiate(prefab);
                preview.SetActive(false);
                currentPreviews.Add(preview);
            }
        }

        if (mainCamera == null)
        {
            Debug.LogError("PlayerInteraction: 'Main Camera'가 연결되지 않았습니다!", this);
            this.enabled = false;
        }

        // [삭제] 십자선 UI 관련 코드 모두 삭제
        // if (crosshairUI == null) { ... }

        isBuildMode = false;
        // if (crosshairUI != null) crosshairUI.SetActive(false); // [삭제]
        HideAllPreviews();

        SelectBuildObject(0);
    }

    void Update()
    {
        // --- B키로 빌드 모드 토글 ---
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildMode = !isBuildMode;

            if (isBuildMode)
            {
                Debug.Log("빌드 모드 ON");
                // if (crosshairUI != null) crosshairUI.SetActive(true); // [삭제]
            }
            else
            {
                Debug.Log("빌드 모드 OFF");
                // if (crosshairUI != null) crosshairUI.SetActive(false); // [삭제]
                HideAllPreviews();
            }
        }
        // --- [수정 끝] ---


        if (!isBuildMode)
        {
            return;
        }

        // (기물 선택 로직은 동일)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectBuildObject(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectBuildObject(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectBuildObject(2);

        HandleBuildPreview();
        HandleBuildActions();
    }

    // (이하 SelectBuildObject, HideAllPreviews, HandleBuildPreview, HandleBuildActions 함수는 변경 없음)

    void SelectBuildObject(int index)
    {
        if (index < 0 || index >= actualBlockPrefabs.Length || index >= currentPreviews.Count)
        {
            return;
        }
        currentBuildIndex = index;
        HideAllPreviews();
    }

    void HideAllPreviews()
    {
        foreach (GameObject preview in currentPreviews)
        {
            if (preview != null)
            {
                preview.SetActive(false);
            }
        }
    }

    void HandleBuildPreview()
    {
        if (mainCamera == null) return;
        if (currentPreviews.Count == 0 || currentPreviews.Count <= currentBuildIndex) return;

        GameObject currentPreview = currentPreviews[currentBuildIndex];
        if (currentPreview == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, buildDistance, buildableLayer))
        {
            snappedPosition = new Vector3(
                Mathf.Round(hit.point.x / MapGenerator.Instance.tileSize) * MapGenerator.Instance.tileSize,
                0.01f,
                Mathf.Round(hit.point.z / MapGenerator.Instance.tileSize) * MapGenerator.Instance.tileSize
            );

            currentPreview.transform.position = snappedPosition;
            currentPreview.SetActive(true);

            MapGenerator.TileState state = MapGenerator.Instance.GetTileStateAtWorld(snappedPosition);
            bool isOccupied = Physics.CheckBox(
                snappedPosition + new Vector3(0, MapGenerator.Instance.tileSize / 2f, 0),
                Vector3.one * MapGenerator.Instance.tileSize * 0.45f,
                Quaternion.identity
            );

            canBuild = (state == MapGenerator.TileState.Buildable && !isOccupied);
        }
        else
        {
            canBuild = false;
            currentPreview.SetActive(false);
        }
    }

    void HandleBuildActions()
    {
        if (!canBuild) return;

        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(actualBlockPrefabs[currentBuildIndex], snappedPosition, Quaternion.identity);
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out RaycastHit hit, buildDistance))
            {
                if (hit.transform.gameObject.CompareTag("Block"))
                {
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }
}