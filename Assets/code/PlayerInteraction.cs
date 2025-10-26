using UnityEngine;
using System.Collections.Generic; // List 사용 (선택 사항)

public class PlayerInteraction : MonoBehaviour
{
    [Header("Build Settings")]
    // [수정] 1개가 아닌 3개의 프리팹 배열로 변경
    public GameObject[] actualBlockPrefabs; // 1. 실제 설치할 기물 3개 (배열)
    public GameObject[] previewBlockPrefabs; // 2. 미리보기용 기물 3개 (배열)
    public float buildDistance = 5f;

    [Header("Links")]
    public Camera mainCamera;
    public LayerMask buildableLayer;

    // [수정] 미리보기 프리팹들을 담아둘 리스트
    private List<GameObject> currentPreviews = new List<GameObject>();
    private int currentBuildIndex = 0; // 현재 선택된 기물 인덱스 (0, 1, 2)

    private Vector3 snappedPosition;
    private bool canBuild = false;

    void Start()
    {
        // [수정] 3개의 미리보기 프리팹을 모두 생성하고 리스트에 추가
        foreach (GameObject prefab in previewBlockPrefabs)
        {
            if (prefab != null)
            {
                GameObject preview = Instantiate(prefab);
                preview.SetActive(false); // 일단 모두 끈다
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
        // --- [새 기능] 기물 선택 (1, 2, 3 키) ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectBuildObject(0); // 0번 기물
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectBuildObject(1); // 1번 기물
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectBuildObject(2); // 2번 기물
        }
        // --- [새 기능 끝] ---

        HandleBuildPreview();
        HandleBuildActions();
    }

    // --- [새 함수] 기물 선택 ---
    void SelectBuildObject(int index)
    {
        // 인덱스가 배열 범위를 벗어나지 않는지 확인
        if (index < 0 || index >= actualBlockPrefabs.Length || index >= currentPreviews.Count)
        {
            Debug.LogWarning("선택하려는 기물 인덱스가 잘못되었습니다: " + index);
            return;
        }

        currentBuildIndex = index;
        Debug.Log("기물 " + (index + 1) + " 선택됨.");

        // [수정] 모든 미리보기를 끈다 (선택된 것만 켜기 위함)
        HideAllPreviews();
    }

    // --- [새 함수] 모든 미리보기 끄기 ---
    void HideAllPreviews()
    {
        foreach (GameObject preview in currentPreviews)
        {
            preview.SetActive(false);
        }
    }

    void HandleBuildPreview()
    {
        if (mainCamera == null) return;

        // [수정] 현재 선택된 미리보기 오브젝트 가져오기
        GameObject currentPreview = currentPreviews[currentBuildIndex];

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, buildDistance, buildableLayer))
        {
            // (그리드 스냅 로직은 동일)
            Vector3 positionToPlace = hit.point + hit.normal * 0.5f;
            snappedPosition = new Vector3(
                Mathf.Round(positionToPlace.x / MapGenerator.Instance.tileSize) * MapGenerator.Instance.tileSize,
                0, // Y는 0으로 고정
                Mathf.Round(positionToPlace.z / MapGenerator.Instance.tileSize) * MapGenerator.Instance.tileSize
            );

            // [수정] 선택된 미리보기만 위치시키고 켠다
            currentPreview.transform.position = snappedPosition;
            currentPreview.SetActive(true);

            // (설치 가능 여부 체크 로직은 동일)
            MapGenerator.TileState state = MapGenerator.Instance.GetTileStateAtWorld(snappedPosition);
            bool isOccupied = Physics.CheckBox(
                snappedPosition + new Vector3(0, MapGenerator.Instance.tileSize / 2f, 0),
                Vector3.one * MapGenerator.Instance.tileSize * 0.45f,
                Quaternion.identity
            );

            if (state == MapGenerator.TileState.Buildable && !isOccupied)
            {
                canBuild = true;
                // (옵션) currentPreview.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                canBuild = false;
                // (옵션) currentPreview.GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            canBuild = false;
            // [수정] 레이저가 빗나가면 현재 미리보기만 끈다
            currentPreview.SetActive(false);
        }
    }

    void HandleBuildActions()
    {
        if (!canBuild) return;

        if (Input.GetMouseButtonDown(0))
        {
            // [수정] 현재 선택된(currentBuildIndex) '실제' 기물 프리팹을 설치
            Instantiate(actualBlockPrefabs[currentBuildIndex], snappedPosition, Quaternion.identity);
        }

        if (Input.GetMouseButtonDown(1))
        {
            // (제거 로직은 동일)
            if (Physics.Raycast(mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out RaycastHit hit, buildDistance))
            {
                if (hit.transform.gameObject.CompareTag("Block")) // (기물 3개 모두 "Block" 태그가 있어야 함)
                {
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }
}