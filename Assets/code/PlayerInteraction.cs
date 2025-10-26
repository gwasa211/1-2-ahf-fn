using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // [1. 씬 관리자 추가]

public class PlayerInteraction : MonoBehaviour
{
    [Header("Build Settings")]
    public GameObject[] actualBlockPrefabs;
    public GameObject[] previewBlockPrefabs;
    public float buildDistance = 5f;

    [Header("Links")]
    public Camera mainCamera;
    public LayerMask buildableLayer;

    private List<GameObject> currentPreviews = new List<GameObject>();
    private int currentBuildIndex = 0;

    private Vector3 snappedPosition;
    private bool canBuild = false;

    // --- [2. Start() 대신 OnEnable() 사용] ---
    // OnEnable은 스크립트가 활성화될 때 + (DontDestroyOnLoad라면) 씬이 로드될 때마다 호출됩니다.
    void OnEnable()
    {
        // 씬 로드 이벤트를 구독(subscribe)
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 씬이 처음 켜졌을 때도 일단 한 번 실행
        SetupPreviews();
    }

    // --- [3. OnDisable() 추가] ---
    // 스크립트가 비활성화되거나 파괴될 때
    void OnDisable()
    {
        // 구독을 해제(unsubscribe) (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- [4. 씬 로드 완료 시 실행될 함수] ---
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.name + " 씬 로드 완료. 미리보기를 재생성합니다.");
        // 씬이 새로 로드되었으니, 미리보기 블록들을 다시 만듭니다.
        SetupPreviews();
    }

    // --- [5. Start()의 내용을 이 함수로 이동] ---
    void SetupPreviews()
    {
        // [중요] 기존에 있던 (아마도 유령이 된) 미리보기 리스트 청소
        foreach (GameObject oldPreview in currentPreviews)
        {
            if (oldPreview != null)
            {
                Destroy(oldPreview);
            }
        }
        currentPreviews.Clear(); // 리스트 비우기

        // [중요] 프리팹에서 새로 생성
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

        // 0번 기물을 기본으로 선택
        SelectBuildObject(0);
    }

    void Update()
    {
        // (기물 선택 로직은 동일)
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
            // [수정] 유령 객체 조작 방지
            if (preview != null)
            {
                preview.SetActive(false);
            }
        }
    }

    void HandleBuildPreview()
    {
        if (mainCamera == null) return;

        // [수정] 리스트에 기물이 없으면(SetupPreviews 전) 중단
        if (currentPreviews.Count == 0 || currentPreviews.Count <= currentBuildIndex) return;

        GameObject currentPreview = currentPreviews[currentBuildIndex];

        // --- [오류 발생 지점 FIX] ---
        // currentPreview가 (어떤 이유로든) 파괴되었다면, 함수를 즉시 중단
        if (currentPreview == null)
        {
            Debug.LogWarning("현재 미리보기 오브젝트가 Missing (null)입니다. SetupPreviews를 기다립니다.");
            return;
        }
        // --- [FIX 끝] ---

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, buildDistance, buildableLayer))
        {
            // (그리드 스냅 로직은 동일)
            snappedPosition = new Vector3(
                Mathf.Round(hit.point.x / MapGenerator.Instance.tileSize) * MapGenerator.Instance.tileSize,
                0.01f, // (Y=0.01f로 고정했던 것 유지)
                Mathf.Round(hit.point.z / MapGenerator.Instance.tileSize) * MapGenerator.Instance.tileSize
            );

            currentPreview.transform.position = snappedPosition;
            currentPreview.SetActive(true);

            // (설치 가능 여부 체크 로직은 동일)
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