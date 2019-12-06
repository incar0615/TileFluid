using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileEditor : MonoBehaviour {

    public WorldGenerator world;
    public TileStatus selectedStatus = TileStatus.EMPTY;
    public Text selectedStatusTxt;
    // Use this for initialization
    void Start () {
        selectedStatusTxt.text = "EMPTY";
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(((int)TileStatus.EMPTY + 1).ToString()) )
        {
            selectedStatus = TileStatus.EMPTY;
            selectedStatusTxt.text = "EMPTY";
        }
        else if (Input.GetKeyDown(((int)TileStatus.DIRT + 1).ToString()))
        {
            selectedStatus = TileStatus.DIRT;
            selectedStatusTxt.text = "DIRT";
        }
        else if (Input.GetKeyDown(((int)TileStatus.WATER + 1).ToString()))
        {
            selectedStatus = TileStatus.WATER;
            selectedStatusTxt.text = "WATER";
        }

        if (Input.GetMouseButton(0))
        {
            Ray2D ray = new Ray2D(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);

            foreach (var hit in hits)
            {
                if (!hit.collider.gameObject.CompareTag("Tile"))
                    continue;

                GameObject tileObj = hit.collider.gameObject;

                tileObj.GetComponent<TileBlock>().SetTileStatus(selectedStatus);
            }
        }
	}
}
