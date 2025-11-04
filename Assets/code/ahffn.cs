using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ahffn : MonoBehaviour
{

    public GameObject grassPrefab;
    public GameObject dirtPrefab;
    public GameObject waterPrefab;

    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
    [SerializeField] float noiseScale = 20f;

    public int waterLevel = 5;

    void Start()
    {
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < width; x++)
        {
      
            for (int z = 0; z < depth; z++)
            {
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;

                float noise = Mathf.PerlinNoise(nx, nz);

               
                int terrainHeight = Mathf.FloorToInt(noise * maxHeight);

               
                for (int y = 0; y < maxHeight; y++)
                {
                    if (y <= terrainHeight)
                    {
                       
                        if (y == terrainHeight)
                        {
                 
                            Place(grassPrefab, x, y, z);
                        }
                        else
                        {
                    
                            Place(dirtPrefab, x, y, z);
                        }
                    }
                    else if (y <= waterLevel)
                    {
                        Place(waterPrefab, x, y, z);
                    }
                  
                }
            }
        }
    }

   
    private void Place(GameObject prefabToPlace, int x, int y, int z)
    {
        var go = Instantiate(prefabToPlace, new Vector3(x, y, z), Quaternion.identity, transform);
        go.name = $"B_{x}_{y}_{z}";
    }

    void Update()
    {

    }
}