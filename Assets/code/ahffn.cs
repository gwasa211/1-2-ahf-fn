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

               
                for (int h = 0; h < maxHeight; h++)
                {
                    if (h <= terrainHeight)
                    {
                       
                        if (h == terrainHeight)
                        {
                 
                            Place(grassPrefab, x, h, z);
                        }
                        else
                        {
                    
                            Place(dirtPrefab, x, h, z);
                        }
                    }
                    else if (h <= waterLevel)
                    {
                        Place(waterPrefab, x, h, z);
                    }
                  
                }
            }
        }
    }

   
    private void Place(GameObject prefabToPlace, int x, int h, int z)
    {
    
        var go = Instantiate(blockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
    
    }
 
    void Update()
    {

    }
}