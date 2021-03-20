using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; 
using System;


public class GameController : MonoBehaviour
{
	[Header("Debug")]
	public Transform debugBall;
	public Transform debugParent;
	[SerializeField] TextMeshProUGUI boundsCoordsDisplay;
	[SerializeField] TextMeshProUGUI vector3CoordsDisplay;

	[Header("Cursor sprites")]
	public Sprite normalCursor;
	public Sprite xCursor;
	public Sprite attackCursor;
	
	[Header("Tile and Grid")]
	public Grid gameGrid;
	public Tilemap floorTilemap;

	[Header("Champs")]
	public float movementVelocity;

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
				spriteRenderer.sprite = xCursor;
			}else{
				if(Input.GetMouseButtonDown(1)){
					if(hitBox.transform.tag == champsTag){
						List<Vector3Int> path = new List<Vector3Int>();
						Vector3Int startPos = hitBox.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
						path = pathFinder(floorTilemap, startPos, gridPos);

						foreach (Transform child in debugParent) {
							GameObject.Destroy(child.gameObject);
						}
						foreach(Vector3Int cell in path){
							Vector3 convetedPath = gameGrid.CellToWorld(cell) + new Vector3(0, gameGrid.cellSize.y/2, 0);
							Instantiate(debugBall, convetedPath, Quaternion.identity, debugParent);
						}
					}
				}
			}		
		}
	}

	//Returns all neighbors
	private List<Vector3Int> getNeighbors(Vector3Int home){
		List<Vector3Int> neighbors = new List<Vector3Int>();
		neighbors.Add(home + Vector3Int.up);
		neighbors.Add(home + Vector3Int.down);
		neighbors.Add(home + Vector3Int.left);
		neighbors.Add(home + Vector3Int.right);

		neighbors.Add(home + Vector3Int.up + Vector3Int.left);
		neighbors.Add(home + Vector3Int.up + Vector3Int.right);
		neighbors.Add(home + Vector3Int.down + Vector3Int.left);
		neighbors.Add(home + Vector3Int.down + Vector3Int.right);
		return neighbors;
	}


	//Pathfinder using Breadth First Search
	public List<Vector3Int> pathFinder(Tilemap tilemap, Vector3Int start, Vector3Int end){
		Queue<Vector3Int> frontier = new Queue<Vector3Int>();
		frontier.Enqueue(start);

		Dictionary<Vector3Int, Vector3Int> came_from = new Dictionary<Vector3Int, Vector3Int>();
		came_from.Add(start, default(Vector3Int));

		Vector3Int current;
		List<Vector3Int> neighbors; 

		while(frontier.Count != 0){
			current = frontier.Dequeue();
			neighbors = getNeighbors(current);

			if(current == end){
				break;
			}

			foreach(Vector3Int neighbor in neighbors){
				if(tilemap.GetTile(neighbor) != null){
					if(!came_from.ContainsKey(neighbor)){
						frontier.Enqueue(neighbor);
						came_from.Add(neighbor, current);
					}
				}
			}
		}

		current = end;
		List<Vector3Int> path = new List<Vector3Int>();

		while(current != start){
			path.Add(current);
			current = came_from[current];
		}

		path.Reverse();

		return path;
	}

	public void moveChamp(Transform champ, Vector3Int path){

	}
}

