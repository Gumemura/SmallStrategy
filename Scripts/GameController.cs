using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; 

public class GameController : MonoBehaviour
{
	[Header("Debug")]
	public Transform debugBall;
	[SerializeField] TextMeshProUGUI boundsCoordsDisplay;
	[SerializeField] TextMeshProUGUI vector3CoordsDisplay;


	[Header("Cursor sprites")]
	public Sprite normalCursor;
	public Sprite xCursor;
	public Sprite attackCursor;
	
	[Header("Tile and Grid")]
	public Grid gameGrid;
	public Tilemap floorTilemap;

	private string champsTag = "Champ";
	private SpriteRenderer spriteRenderer;
	private GameObject[] allHeroes;
	private bool somethingIsSelected;
	private RaycastHit2D hitBox;

	[HideInInspector]
	public int[,] tileArray;

	void Start(){
		floorTilemap.CompressBounds();

		Vector3 center = gameGrid.CellToWorld(floorTilemap.origin);

		tileArray = new int[floorTilemap.size.x, floorTilemap.size.y];

		// foreach(var cells in floorTilemap.cellBounds.allPositionsWithin){
		// 	center = gameGrid.CellToWorld(new Vector3Int(cells.x, cells.y, cells.z)) + new Vector3(0, gameGrid.cellSize.y/2, 0);
		// 	print(cells.x + " " + cells.y + " | " + center);
		// 	Instantiate(debugBall, center, Quaternion.identity);
		// }


		//Turning the default windows cursor off
		Cursor.visible = false;
		spriteRenderer = transform.GetComponent<SpriteRenderer>();

		//Filling the array with all controlable heros
		allHeroes = GameObject.FindGameObjectsWithTag(champsTag);

		somethingIsSelected = false;
	}

	// Update is called once per frame
	void Update(){
		spriteRenderer.sprite = normalCursor;

		Vector3 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
		transform.position = mousePos;

		Vector3Int gridPos = gameGrid.WorldToCell(mousePos);
		Vector3 gridPosToWorld = gameGrid.CellToWorld(gridPos) + new Vector3(0, gameGrid.cellSize.y/2, 0);

		boundsCoordsDisplay.text = gridPos.x + " " + gridPos.y;
		vector3CoordsDisplay.text = gridPosToWorld.x + " " + gridPosToWorld.y;

		if(Input.GetMouseButtonDown(0)){
			hitBox = Physics2D.Raycast(mousePos, Vector2.zero);

			//Deselecting all heros
			foreach(GameObject hero in allHeroes){
				hero.transform.GetComponent<ChampsBehaviour>().turnOnSelection(false);
			}
				  
			if (hitBox.collider != null) {
				somethingIsSelected = true;
				if(hitBox.transform.tag == champsTag){
					hitBox.transform.GetComponent<ChampsBehaviour>().turnOnSelection(true);
				}
			}else{
				somethingIsSelected = false;
			}
		}

		if(somethingIsSelected){

			if(floorTilemap.GetTile(gridPos) == null){
				spriteRenderer. sprite = xCursor;
			}else{
				if(Input.GetMouseButtonDown(1)){
					List<Vector3Int> path = new List<Vector3Int>();
					path = hitBox.transform.GetComponent<ChampsBehaviour>().pathFinder(gameGrid, floorTilemap, gridPos);

					foreach(Vector3Int cell in path){
						Vector3 convetedPath = gameGrid.CellToWorld(cell) + new Vector3(0, gameGrid.cellSize.y/2, 0);
						Instantiate(debugBall, convetedPath, Quaternion.identity);
					}
				}
			}		
		}
	}
}
