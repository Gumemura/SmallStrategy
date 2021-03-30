using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
	public Grid gamegrid;

	public TileBase blockTile;
	public TileBase greenTile;

	//public GameObject forest;

	private Tilemap floorTilemap;
	private Tilemap gridTilemap;
	private Tilemap blockTilemap;

	private Vector3Int floorOrigin;
	private Vector3Int checkingCell;


    // Start is called before the first frame update
    void Start()
    {
    	floorTilemap = gamegrid.transform.Find("Floor").GetComponent<Tilemap>();
    	gridTilemap = gamegrid.transform.Find("Grid").GetComponent<Tilemap>();
    	blockTilemap = gamegrid.transform.Find("Blocks").GetComponent<Tilemap>();

    	floorOrigin = floorTilemap.origin;

        floorTilemap.CompressBounds();

        for(int i = 0; i < floorTilemap.size.x; i++){
        	for(int c = 0; c > -floorTilemap.size.y; c--){
        		checkingCell = new Vector3Int(i, c, floorOrigin.z);
        		if(floorTilemap.GetTile(checkingCell) == null){
        			blockTilemap.SetTile(checkingCell, blockTile);
        			//Instantiate(forest, gamegrid.CellToWorld(checkingCell) + new Vector3(0, gamegrid.cellSize.y/2, 0), Quaternion.identity, this.transform);
        		}
        	}
        }
    }
}
