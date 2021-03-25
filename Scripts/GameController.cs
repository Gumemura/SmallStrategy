using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; 
using System;
using UnityEditor;


public class GameController : MonoBehaviour
{
	[Header("Debug")]
	public Transform debugBall;
	public Transform debugParent;
	public TextMeshProUGUI boundsCoordsDisplay;
	public TextMeshProUGUI vector3CoordsDisplay;
	public bool displayCoordinates;

	[Header("Cursor sprites")]
	public Sprite normalCursor;
	public Sprite xCursor;
	public Sprite attackCursor;
	[HideInInspector]public Transform cursorObject;
	public TextMeshProUGUI actionCostText;

	[Header("Tile and Grid")]
	public Grid gameGrid;
	public Tilemap floorTilemap;
	public float unitTileOffset;

	[Header("Champs")]
	public float movementVelocity;
	[HideInInspector]public bool unitIsMoving;

	[Header("UI")]
	public Transform walkableDots;
	public Transform walkableDotsParent;

	private string champsTag = "Champ";
	private SpriteRenderer cursorSpriteRenderer;
	private GameObject[] allHeroes;
	private bool somethingIsSelected;
	private RaycastHit2D hitBox;
	private Vector3 mousePosition;
	private Vector3 gridPosToWorld;
	private Vector3Int gridPos;
	private bool unitCanMove;
	private Vector3Int tempGridPosition;
	private Vector3Int selectecUnitPosition;
	private List<Vector3Int> selectedUnitWalkableArea;
	private LineRenderer selectedUnitLineRenderer;


	void Start(){
		//Reducing tilemap bounds to the place that contain tiles
		floorTilemap.CompressBounds();

		//Turning the default windows cursor off
		Cursor.visible = false;
		cursorObject = transform.Find("Cursor");
		cursorSpriteRenderer = cursorObject.GetComponent<SpriteRenderer>();

		//Filling the array with all controlable heros
		allHeroes = GameObject.FindGameObjectsWithTag(champsTag);

		somethingIsSelected = false;
		unitIsMoving = false;
	}

	//Upfating the cursor state
	void CursorState(){
		cursorObject.position = mousePosition;

		if(floorTilemap.GetTile(gridPos) == null && somethingIsSelected){
			cursorSpriteRenderer.sprite = xCursor;
			unitCanMove = false;
		}else{
			cursorSpriteRenderer.sprite = normalCursor;
			unitCanMove = true;
		}

	}

	// Update is called once per frame
	void Update(){
		mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition); //cursor position
		gridPos = gameGrid.WorldToCell(mousePosition);//coordinates of the cell that cursor in below (vector3int)
		gridPosToWorld = convertGidPosToWorldPos(gridPos);//converted coordinate of the frid position to world coords (vector3)

		CursorState();

		if(displayCoordinates){
			boundsCoordsDisplay.text = gridPos.x + " " + gridPos.y;
			vector3CoordsDisplay.text = gridPosToWorld.x + " " + gridPosToWorld.y;
		}

		if(Input.GetMouseButtonDown(0)){
			hitBox = Physics2D.Raycast(mousePosition, Vector2.zero);//The object that have been hit

			//Deselecting all heros
			foreach(GameObject hero in allHeroes){
				hero.transform.GetComponent<ChampsBehaviour>().turnOnSelection(false);
			}

			//removing previous walkable area dots
			foreach (Transform blueDot in walkableDotsParent) {
				GameObject.Destroy(blueDot.gameObject);
			}

			//turning off the steps line
			if(selectedUnitLineRenderer){
				selectedUnitLineRenderer.positionCount = 0;
			}
				  
			if (hitBox.collider != null) {
				somethingIsSelected = true;
				if(hitBox.transform.tag == champsTag){
					hitBox.transform.GetComponent<ChampsBehaviour>().turnOnSelection(true);

					selectecUnitPosition = hitBox.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
					selectedUnitWalkableArea = walkableArea(hitBox.transform);
					selectedUnitLineRenderer = hitBox.transform.GetComponent<LineRenderer>();
				}
			}else{
				somethingIsSelected = false;
			}
		}

		if(somethingIsSelected){
			//rendering path before user choses path
			if(tempGridPosition != gridPos && selectedUnitWalkableArea.Contains(gridPos) && unitIsMoving == false){
				tempGridPosition = gridPos;
				Vector3 convertedGridPosition;
				List<Vector3Int> path = pathFinder(floorTilemap, selectecUnitPosition, tempGridPosition, selectedUnitWalkableArea);
				Vector3[] convertedPath = new Vector3[path.Count];
				int index = 0;

				actionCostText.text = MovementCostCalculation(path).ToString();

				foreach(Vector3Int cell in path){
					convertedGridPosition = convertGidPosToWorldPos(cell);
					convertedPath[index++] = convertedGridPosition;
				}
				selectedUnitLineRenderer.positionCount = index;
				selectedUnitLineRenderer.SetPositions(convertedPath);
			}else if(!selectedUnitWalkableArea.Contains(gridPos) || unitIsMoving){
				//selectedUnitLineRenderer.positionCount = 0;
				actionCostText.text = "";
			}

			

			if(Input.GetMouseButtonDown(1)){
				if(hitBox.transform.tag == champsTag){
					//Movement of unit
					if(unitIsMoving == false && unitCanMove){
						List<Vector3Int> path = new List<Vector3Int>();//list with all cells the unit will move trought to reach the destination
						Vector3Int startPos = selectecUnitPosition;//the position fo the unit
						path = pathFinder(floorTilemap, startPos, gridPos, walkableArea(hitBox.transform));//grabing the list of the cells to go through

						foreach (Transform child in debugParent) {
							GameObject.Destroy(child.gameObject);
						}
						StartCoroutine(moveUnit(hitBox.transform, path)); //Moving
					}
				}
			}	
		}
	}

	//Receives a Vector3Int and returns the center of the cell 
	public Vector3 convertGidPosToWorldPos(Vector3Int gridPosition){
		return gameGrid.CellToWorld(gridPosition) + new Vector3(0, gameGrid.cellSize.y/2, 0);
	}

	//Returns all neighbors
	//Used by pathfinder
	private List<Vector3Int> getNeighbors(Vector3Int home){
		List<Vector3Int> neighbors = new List<Vector3Int>();

		if(floorTilemap.GetTile(home + Vector3Int.up) != null){neighbors.Add(home + Vector3Int.up);}
		if(floorTilemap.GetTile(home + Vector3Int.down) != null){neighbors.Add(home + Vector3Int.down);}
		if(floorTilemap.GetTile(home + Vector3Int.left) != null){neighbors.Add(home + Vector3Int.left);}
		if(floorTilemap.GetTile(home + Vector3Int.right) != null){neighbors.Add(home + Vector3Int.right);}

		if(floorTilemap.GetTile(home + Vector3Int.up + Vector3Int.left) != null && (floorTilemap.GetTile(home + Vector3Int.up) != null || floorTilemap.GetTile(home + Vector3Int.left) != null)){
			neighbors.Add(home + Vector3Int.up + Vector3Int.left);
		}
		if(floorTilemap.GetTile(home + Vector3Int.up + Vector3Int.right) != null && (floorTilemap.GetTile(home + Vector3Int.up) != null || floorTilemap.GetTile(home + Vector3Int.right) != null)){
			neighbors.Add(home + Vector3Int.up + Vector3Int.right);
		}
		if(floorTilemap.GetTile(home + Vector3Int.down + Vector3Int.left) != null && (floorTilemap.GetTile(home + Vector3Int.down) != null || floorTilemap.GetTile(home + Vector3Int.left) != null)){
			neighbors.Add(home + Vector3Int.down + Vector3Int.left);
		}
		if(floorTilemap.GetTile(home + Vector3Int.down + Vector3Int.right) != null && (floorTilemap.GetTile(home + Vector3Int.down) != null || floorTilemap.GetTile(home + Vector3Int.right) != null)){
			neighbors.Add(home + Vector3Int.down + Vector3Int.right);
		}

		return neighbors;
	}

	//Pathfinder using Breadth First Search
	public List<Vector3Int> pathFinder(Tilemap tilemap, Vector3Int start, Vector3Int end, List<Vector3Int> walkableArea){
		Queue<Vector3Int> frontier = new Queue<Vector3Int>();
		frontier.Enqueue(start);

		Dictionary<Vector3Int, Vector3Int> came_from = new Dictionary<Vector3Int, Vector3Int>();
		came_from.Add(start, default(Vector3Int));//dicitionary where will be stored a cell coords and from which cell it came from

		Vector3Int current;
		List<Vector3Int> neighbors; 

		while(frontier.Count != 0){
			current = frontier.Dequeue();
			neighbors = getNeighbors(current);//getting all 8 neighbors

			if(current == end){//if the current cell is the destination, break bcz we found the path we were looking for
				break;
			}

			foreach(Vector3Int neighbor in neighbors){//checking each neighbor
				//if that coord have a tile AND its in the walkable area, proceed
				if(tilemap.GetTile(neighbor) != null && walkableArea.Contains(neighbor)){
					//if we didn't check this cell, lets check it
					if(!came_from.ContainsKey(neighbor)){
						//if its not alredy on the queue, add it so this cell wil be checked later
						if(!frontier.Contains(neighbor)){
							frontier.Enqueue(neighbor);
						}
						came_from.Add(neighbor, current);//add the cell and where it came from
					}
				}
			}
		}

		current = end;
		List<Vector3Int> path = new List<Vector3Int>();

		//Now, to find the best path, we just check the dictionary from the end to the beginning and it will give us the path
		while(current != start){
			path.Add(current);
			try{
				current = came_from[current];
			}catch (Exception){//if a error is found (KeyNotFound) it means that the user ordened a movement outside the walkable area, so we return a empty list
				return new List<Vector3Int>() {};
			}
		}

		path.Add(start);//adding the starting point. its not necessary to move the unit as it stands below the starting poing (it will not have to move to tha starting point)
		//but its important when adjusting the walkable area so we can properly measure the cost fo movement
		path.Reverse();//reversing

		return path;
	}

	//Moving the unit
	IEnumerator moveUnit(Transform unit, List<Vector3Int> path){
		//destroying all walkable area dots
		foreach (Transform blueDot in walkableDotsParent) {
			GameObject.Destroy(blueDot.gameObject);
		}

		//turnign off the apths line
		unit.transform.GetComponent<LineRenderer>().positionCount = 0;

		foreach(Vector3Int breadCrumb in path){
			Vector3 convertedDestination = convertGidPosToWorldPos(breadCrumb);
			while(Vector3.Distance(unit.transform.position, convertedDestination) > unitTileOffset){ 
				unitIsMoving = true;
				unit.position = Vector3.MoveTowards(unit.position, convertedDestination, Time.deltaTime * movementVelocity);
				yield return null;
			}
		}
		unitIsMoving = false;

		//ipdating units position on grid
		selectecUnitPosition = hitBox.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);

		//calculating remaining speed
		//unit.transform.GetComponent<ChampsBehaviour>().remainingSpeed -= MovementCostCalculation(path);

		//rendering the new walkable area and calculating new one
		selectedUnitWalkableArea = walkableArea(unit.transform);
	}

	//Calculaing walkable area
	public List<Vector3Int> walkableArea(Transform unit){
		/* those ilustrations will help you to undernstand the pathfinder method

		imagine a unit with speed 3 (aka he can move up to 3 cells), which 'x' is his position on the grid below. 
			  0 1 2 3 4 5 6
			0 - - - - - - -
			1 - - - - - - - 
			2 - - - - - - - 
			3 - - - x - - - 
			4 - - - - - - - 
			5 - - - - - - - 
			6 - - - - - - - 

		as long as he have 3 speed, he can move to the cells represented by '0'
			  0 1 2 3 4 5 6
			0 - - - 0 - - -
			1 - - 0 0 0 - - 
			2 - 0 0 0 0 0 - 
			3 0 0 0 x 0 0 0 
			4 - 0 0 0 0 0 - 
			5 - - 0 0 0 - - 
			6 - - - 0 - - - 

		we chose the cell (0, 0) to starting going through the bidimensional array, below represented as the variable 'startingPoint': its just the unit position minus his speed (the 
		'y' axis is plus due to the grid orientation. check the unity inspection)

		the variable 'rows' is the dimension of the array

		the two 'ifs' below ('if(i + c > speed - 1 && i + c < (speed + 1) + (i * 2))' and 'if(i + c > (speed - 1) + 2 * (i - speed) && i + c <= speed * 3)') restrict the selection to the 
		cells og the diamong shape
		*/
		List<Vector3Int> walkable = new List<Vector3Int>();
		int speed = unit.GetComponent<ChampsBehaviour>().remainingSpeed;
		int rows = (2 * speed) + 1;

		Vector3Int unitPos = unit.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
		Vector3Int startingPoint = new Vector3Int(unitPos.x - speed, unitPos.y + speed, unitPos.z);

		for(int i = 0; i < rows; i++){
			for(int c = 0; c < rows; c++){
				if(floorTilemap.GetTile(startingPoint + new Vector3Int(i, -c, 0)) != null){
					if(i <= speed){
						if(i + c > speed - 1 && i + c < (speed + 1) + (i * 2)){
							walkable.Add(startingPoint + new Vector3Int(i, -c, 0));
						}
					}else{
						if(i + c > (speed - 1) + 2 * (i - speed) && i + c <= speed * 3){
							walkable.Add(startingPoint + new Vector3Int(i, -c, 0));
						}
					}
				}
			}
		}

		//correting the walkable area by removing the ones the unit cannot reach
		//basicaly this is whats its being doing: for each cell in the walkable area find its path to the unit position
		//if the distance is higher tha the unit's speed, removes it
		List<Vector3Int> allPaths = new List<Vector3Int>();
		List<Vector3Int> toRemove = new List<Vector3Int>();
		int stepsCounter = 0;

		foreach(Vector3Int cell in walkable){
			allPaths = pathFinder(floorTilemap, cell, unitPos, walkable);

			if(allPaths.Count > 0){
				for(int i = 0; i < allPaths.Count - 1; i++){
					if(allPaths[i].x != allPaths[i + 1].x && allPaths[i].y != allPaths[i + 1].y){
						stepsCounter += 2;
					}else{
						stepsCounter++;
					}
				}
				if(stepsCounter > speed){
					toRemove.Add(cell);
				}
				stepsCounter = 0;
			}else{
				toRemove.Add(cell);
			}
		}

		foreach(Vector3Int cellToRemove in toRemove){
			walkable.Remove(cellToRemove);
		}

		foreach(Vector3Int cell in walkable){
			Vector3 convetedWalkArea = convertGidPosToWorldPos(cell);
			Instantiate(walkableDots, convetedWalkArea, Quaternion.identity, walkableDotsParent);
		}

		return walkable;
	}

	public int MovementCostCalculation(List<Vector3Int> path){
		int pathCost = 0;
		for(int i = 0; i < path.Count - 1; i++){
			if(path[i].x != path[i + 1].x && path[i].y != path[i + 1].y){
				pathCost += 2;
			}else{
				pathCost++;
			}
		}
		return pathCost;
	}
}

