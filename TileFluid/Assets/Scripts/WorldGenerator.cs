using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileStatus
{
    EMPTY = 0,
    DIRT = 1,
    WATER = 2,
    STATUS_MAX = 3,
}

public class WorldGenerator : MonoBehaviour {

    public int width = 50;
    public int height = 50;

    private static WorldGenerator instance = null;
    public static WorldGenerator Instance
    {
        get
        {
            if(!instance)
            {
                instance = FindObjectOfType<WorldGenerator>();
                if(!instance) instance = new GameObject("WorldGenerator").AddComponent<WorldGenerator>();
            }
            return instance;
        }
    }

    private Vector2Int worldSize;
    public Vector2Int WorldSize
    {
        get { return worldSize; }
    }

    private Vector2Int startPos;
    public Vector2Int StartPos
    {
        get { return startPos; }
    }

    private Vector2Int endPos;
    public Vector2Int EndPos
    {
        get { return endPos; }
    }

    private void Awake()
    {
        // chunk 만들고 청크로 총크기 받아와서 생성 및 worldSize세팅
        worldSize = new Vector2Int(width, height);
        startPos = new Vector2Int(0, 0);
        endPos = new Vector2Int(width, height);

        ResetCameraPos();
    }

    public void ResetCameraPos()
    {
        Camera.main.transform.SetPositionAndRotation(new Vector3(worldSize.x * 0.5f, worldSize.y * 0.5f - 0.5f, Camera.main.transform.position.z), Quaternion.identity);
        Camera.main.orthographicSize = worldSize.x > worldSize.y ? worldSize.x * 0.5f : worldSize.y * 0.5f;
    }
    public void ChunksStateChanged()
    {
        // 활성화 되어있는 chunk들을 가져와서 startPos, endPos 세팅
    }
}
