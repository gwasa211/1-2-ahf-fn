using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Build Settings")]
    public GameObject[] actualBlockPrefabs;
    public GameObject[] previewBlockPrefabs;
    public float buildDistance = 5f;

    [Header("Links")]
    public Camera mainCamera;
    public LayerMask buildableLayer;

    private bool isBuildMode = false;

    private List<GameObject> currentPreviews = new List<GameObject>();
    private int currentBuildIndex = 0;

    private Vector3 snappedPosition;
    private bool canBuild = false;

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

        isBuildMode = false;
        HideAllPreviews();
        SelectBuildObject(0);
    }

    void Update()
    {
        // B키로 빌드 모드 토글
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildMode = !isBuildMode;

            if (isBuildMode)
            {
                Debug.Log("빌드 모드 ON");
            }
            else
            {
                Debug.Log("빌드 모드 OFF");
                HideAllPreviews();
            }
        }

        if (!isBuildMode)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectBuildObject(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectBuildObject(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectBuildObject(2);

        HandleBuildPreview();
        HandleBuildActions();
    }

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

            // [핵심] MapGenerator에게 "여기 설치 가능?" 물어보기
            MapGenerator.TileState state = MapGenerator.Instance.GetTileStateAtWorld(snappedPosition);

            bool isOccupied = Physics.CheckBox(
                snappedPosition + new Vector3(0, MapGenerator.Instance.tileSize / 2f, 0),
                Vector3.one * MapGenerator.Instance.tileSize * 0.45f,
                Quaternion.identity
            );

            // "Buildable" 상태(0번, 3번)이고 + "비어있으면" 설치 가능
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