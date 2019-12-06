using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileBlock : MonoBehaviour {

    public TileStatus status;

    public float viscosity = 0.0f;
    
    public float curAmount = 0.0f;

    public TileBlock topBlock;
    public TileBlock bottomBlock;
    public TileBlock leftBlock;
    public TileBlock rightBlock;

    public Sprite waterSpr;
    public Sprite emptySpr;
    public Sprite dirtSpr;

    void Start()
    {
        //status = TileStatus.DIRT;
    }

    public void ChangeSprite()
    {
        switch(status)
        {
            case TileStatus.EMPTY:
                curAmount = 0.0f;
                GetComponent<SpriteRenderer>().sprite = emptySpr;
                GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
                break;

            case TileStatus.WATER:
                GetComponent<SpriteRenderer>().sprite = emptySpr;
                GetComponent<SpriteRenderer>().color = new Color(0, 0, 0.2f + curAmount * 0.8f);
                break;

            case TileStatus.DIRT:
                curAmount = 0.0f;
                GetComponent<SpriteRenderer>().sprite = dirtSpr;
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
                break;
        }
    }

    public void SetTileStatus(TileStatus statusParam)
    {
        status = statusParam;
        if (status == TileStatus.WATER) curAmount = Random.Range(0.5f, 1.0f);

        ChangeSprite();
    }

    public void SetTileStatusByRandom()
    {
        status = (TileStatus)Random.Range(0, (int)TileStatus.STATUS_MAX);
        if (status == TileStatus.WATER) curAmount = Random.Range(0.5f, 1.0f);
        ChangeSprite();
    }

    public void SetTileStatusByNoise(float noise)
    {
        if (noise < 0.20f) SetTileStatus(TileStatus.EMPTY);
        else if (noise < 0.35f) SetTileStatus(TileStatus.WATER);
        else  SetTileStatus(TileStatus.DIRT);
    }

    public void SetEmpty()
    {
        status = TileStatus.EMPTY;
        ChangeSprite();
    }

    public void SetWater()
    {
        status = TileStatus.WATER;
        ChangeSprite();
    }

    public void SetDirt()
    {
        status = TileStatus.DIRT;
        ChangeSprite();
    }
    /*
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/TileBlock")]
    public static void CreateTileBlock()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save TileBlock", "New TileBlock", "asset", "Save TileBlock", "Assets");

        if (path == "") return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TileBlock>(), path);
    }
#endif
    //*/
}
