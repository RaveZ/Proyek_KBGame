#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;

[ExecuteAlways]
public class LevelConstructor : MonoBehaviour
{
    public int numRows = 3;                 // Number of rows of levels
    public int numColumns = 3;              // Number of columns of levels
    public Vector3 levelOffset = new Vector3(10f, 0f, 10f); // Offset between levels

    public GameObject levelPrefab;          // Prefab for the level

    public GameObject[,] instantiatedLevels; // 2D array to hold instantiated level objects

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            UpdateOffset();
        }
#endif
    }

    [ContextMenu("Destroy Level")]
    void DestroyLevels()
    {
        try
        {
            if (instantiatedLevels != null)
            {
                for (int row = 0; row < numRows; row++)
                {
                    for (int col = 0; col < numColumns; col++)
                    {
                        if (instantiatedLevels[row, col] != null)
                        {
                            DestroyImmediate(instantiatedLevels[row, col]);
                        }
                    }
                }
            }
        }catch(Exception e)
        {
            Debug.LogException(e);
        }
        
    }


    [ContextMenu("Construct Level")]
    void ConstructLevels()
    {
        DestroyLevels();
        instantiatedLevels = new GameObject[numRows, numColumns];

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                Vector3 position = new Vector3(col * levelOffset.x, 0f, row * levelOffset.z);
                instantiatedLevels[row, col] = Instantiate(levelPrefab, position, Quaternion.identity);
            }
        }
    }

    private void UpdateOffset()
    {
        if (instantiatedLevels != null)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numColumns; col++)
                {
                    if (instantiatedLevels[row, col] != null)
                    {
                        Vector3 positionOffset = new Vector3(col * levelOffset.x, 0f, row * levelOffset.z);
                        instantiatedLevels[row, col].gameObject.transform.position = positionOffset;
                    }
                }
            }
        }
    }
}
