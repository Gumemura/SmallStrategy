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
	public bool reduceSpeed;

	[Header("Cursor sprites")]
	public Sprite normalCursor;
	public Sprite xCursor;
	public Sprite meleeAttackCursor;
	public Sprite rangedAttackCursor;

	[HideInInspector]public Transform cursorObject;
	public TextMeshProUGUI actionCostText;

	[Header("Tile and Grid")]
	public Grid gameGrid;
	public Tilemap floorTilemap;
	public Tilemap blocksTilemap;
	public float unitTileOffset;

	[Header("Champs")]
	public float movementVelocity;
	public int movementCost;
	[HideInInspector]public bool unitIsMoving;

	[Header("UI")]
	public Transform walkableDots;
	public Transform walkableDotsParent;

	private string champsTag = "Champ";
	private string enemyTag = "Enemy";
	private SpriteRenderer cursorSpriteRenderer;
	private GameObject[] allHeroes;
	private GameObject[] allEnemies;
	private bool somethingIsSelected;
	private RaycastHit2D hitBox;
	private RaycastHit2D hitBoxWithUnitSelected;
	private Vector3 mousePosition;
	private Vector3 gridPosToWorld;
	private Vector3Int mousePositionConvertedToGrid;
	private bool unitCanMove;
	private Vector3Int tempGridPosition;
	private Vector3Int selectecUnitPosition;
	private List<Vector3Int> selectedUnitWalkableArea;
	private LineRenderer lineRenderer;
	private List<Vector3Int> pathToMove = new List<Vector3Int>();

	void Start(){
		//Reducing tilemap bounds to the place that contain tiles
		floorTilemap.CompressBounds();

		lineRenderer = transform.GetComponent<LineRenderer>();

		//Turning the default windows cursor off
		Cursor.visible = false;
		cursorObject = transform.Find("Cursor");
		cursorSpriteRenderer = cursorObject.GetComponent<SpriteRenderer>();

		//Filling the array with all controlable heros
		allHeroes = GameObject.FindGameObjectsWithTag(champsTag);
		allEnemies = GameObject.FindGameObjectsWithTag(enemyTag);

		somethingIsSelected = false;
		unitIsMoving = false;
	}

	//Upfating the cursor state
	void CursorState(){
		cursorObject.position = mousePosition;

		cursorSpriteRenderer.sprite = normalCursor;
		unitCanMove = true;

		if(somethingIsSelected){
			if(hitBox.transform.tag == champsTag){
				if(ValidClickedPosition(mousePositionConvertedToGrid) == false){
					cursorSpriteRenderer.sprite = xCursor;
					unitCanMove = false;
				}else if(hitBoxWithUnitSelected && hitBoxWithUnitSelected.transform.tag == enemyTag){
					if(hitBox.transform.GetComponent<ChampsBehaviour>().isAttackMelee){
						cursorSpriteRenderer.sprite = meleeAttackCursor;
					}else{
						cursorSpriteRenderer.sprite = rangedAttackCursor;
					}
				}
			}
		}
	}

	bool ValidClickedPosition(Vector3Int cell){
		if(floorTilemap.GetTile(cell) == null){
			return false;
		}

		foreach(GameObject hero in allHeroes){
			if(cell == hero.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid)){
				return false;
			}
		}

		return true;
	}

	// Update is called once per frame
	void Update(){
		mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition); //cursor position
		mousePositionConvertedToGrid = gameGrid.WorldToCell(mousePosition);//coordinates of the cell that cursor in below (vector3int)
		gridPosToWorld = convertGidPosToWorldPos(mousePositionConvertedToGrid);//converted coordinate of the frid position to world coords (vector3)

		CursorState();

		if(displayCoordinates){
			boundsCoordsDisplay.text = mousePositionConvertedToGrid.x + " " + mousePositionConvertedToGrid.y;
			vector3CoordsDisplay.text = gridPosToWorld.x + " " + gridPosToWorld.y;
		}

		if(Input.GetMouseButtonDown(0)){
			hitBox = Physics2D.Raycast(mousePosition, Vector2.zero);//The object that have been hit
			actionCostText.text = "";

			//Deselecting all heros
			foreach(GameObject hero in allHeroes){
				hero.transform.GetComponent<ChampsBehaviour>().turnOnSelection(false);
			}

			//removing previous walkable area dots
			foreach (Transform blueDot in walkableDotsParent) {
				GameObject.Destroy(blueDot.gameObject);
			}

			//turning off the steps line
			lineRenderer.positionCount = 0;
			
			if (hitBox.collider != null) {
				if(unitIsMoving == false ){
					somethingIsSelected = true;
					if(hitBox.transform.tag == champsTag){
						hitBox.transform.GetComponent<ChampsBehaviour>().turnOnSelection(true);

						selectecUnitPosition = hitBox.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
						selectedUnitWalkableArea = walkableArea(hitBox.transform);
					}
				}
			}else{
				somethingIsSelected = false;
			}
		}

		if(somethingIsSelected){
			if(hitBox.transform.tag == champsTag){
				hitBoxWithUnitSelected = Physics2D.Raycast(mousePosition, Vector2.zero);

				//rendering path before user choses path and calculating path
				if((tempGridPosition != mousePositionConvertedToGrid || hitBoxWithUnitSelected ) && !unitIsMoving){
					tempGridPosition = mousePositionConvertedToGrid;
					if(selectedUnitWalkableArea.Contains(tempGridPosition) && unitIsMoving == false){
						if(hitBoxWithUnitSelected && hitBoxWithUnitSelected.transform.tag == enemyTag){
							tempGridPosition = hitBoxWithUnitSelected.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
						}
						pathToMove = pathFinder(floorTilemap, selectecUnitPosition, tempGridPosition, selectedUnitWalkableArea);

						if(hitBoxWithUnitSelected && hitBoxWithUnitSelected.transform.tag == enemyTag){
							pathToMove.RemoveAt(pathToMove.Count - 1);
						}

						int index = 0;
						Vector3 convertedGridPosition;
						Vector3[] convertedPath = new Vector3[pathToMove.Count];

						actionCostText.text = MovementCostCalculation(pathToMove).ToString();
						//NormalizingPath(pathToMove, hitBox.transform);

						foreach(Vector3Int cell in pathToMove){
							convertedGridPosition = convertGidPosToWorldPos(cell);
							convertedPath[index++] = convertedGridPosition;
						}
						lineRenderer.positionCount = index;
						lineRenderer.SetPositions(convertedPath);
					}
				}else if(!selectedUnitWalkableArea.Contains(mousePositionConvertedToGrid) || unitIsMoving){
					lineRenderer.positionCount = 0;
					actionCostText.text = "";
				}
			}

			if(Input.GetMouseButtonDown(1)){
				if(hitBox.transform.tag == champsTag){
					//Movement of unit
					if(unitIsMoving == false && unitCanMove && selectedUnitWalkableArea.Contains(mousePositionConvertedToGrid)){
						StartCoroutine(moveUnit(hitBox.transform, pathToMove)); //Moving
					}
				}
			}	
		}
	}

	//Receives a Vector3Int and returns the center of the cell 
	public Vector3 convertGidPosToWorldPos(Vector3Int gridPosition){
		return gameGrid.CellToWorld(gridPosition) + new Vector3(0, gameGrid.cellSize.y/2, 0);
	}

	//removing edges corner to simulate diagonal movement
	public void NormalizingPath(List<Vector3Int> path, Transform unit){
		Vector3Int destinationCell;
		Vector3Int analisedCell;

		Vector3Int origin = path[0];

		int speedCounter = 0;

		int x;
		int y;

		for(int i = path.Count - 1; i > 0; i--){
			if(i < path.Count){
				destinationCell = path[i];

				x = (int)Mathf.Sign(origin.x - destinationCell.x);
				y = (int)Mathf.Sign(Mathf.Abs(destinationCell.y) - Mathf.Abs(origin.y));

				if(floorTilemap.GetTile(destinationCell + new Vector3Int(x, 0, 0)) != null && floorTilemap.GetTile(destinationCell + new Vector3Int(0, y, 0)) != null){
					analisedCell = destinationCell + new Vector3Int(x, y, 0);

					while(!path.Contains(analisedCell)){
						if(floorTilemap.GetTile(analisedCell) == null || (floorTilemap.GetTile(analisedCell + new Vector3Int(x, 0, 0)) == null && floorTilemap.GetTile(analisedCell + new Vector3Int(0, y, 0)) == null) ){
							break;
						}
						analisedCell += new Vector3Int(x, y, 0);
						speedCounter += movementCost;
						if(speedCounter > unit.GetComponent<ChampsBehaviour>().remainingSpeed){
							break;
						}
					}
					if(path.Contains(analisedCell)){
						path.RemoveRange(path.IndexOf(analisedCell) + 1, path.IndexOf(destinationCell) - path.IndexOf(analisedCell) - 1);
					}
					speedCounter = 0; 
				}
			}
		}
		for(int i = 0; i < path.Count - 2; i++){
			if(Mathf.Abs(Mathf.Abs(path[i].x) - Mathf.Abs(path[i + 2].x)) == 1 && Mathf.Abs(Mathf.Abs(path[i].y) - Mathf.Abs(path[i + 2].y)) == 1){
				path.Remove(path[i + 1]);
			}
		}
	}

	//Returns all neighbors
	//Used by pathfinder
	private List<Vector3Int> getNeighbors(Vector3Int home){
		List<Vector3Int> neighbors = new List<Vector3Int>();

		if(floorTilemap.GetTile(home + Vector3Int.up) != null){neighbors.Add(home + Vector3Int.up);}
		if(floorTilemap.GetTile(home + Vector3Int.right) != null){neighbors.Add(home + Vector3Int.right);}
		if(floorTilemap.GetTile(home + Vector3Int.left) != null){neighbors.Add(home + Vector3Int.left);}
		if(floorTilemap.GetTile(home + Vector3Int.down) != null){neighbors.Add(home + Vector3Int.down);}
		if(floorTilemap.GetTile(home + Vector3Int.up + Vector3Int.left) != null && (floorTilemap.GetTile(home + Vector3Int.up) != null || floorTilemap.GetTile(home + Vector3Int.left) != null)){
			neighbors.Add(home + Vector3Int.up + Vector3Int.left);
		}
		if(floorTilemap.GetTile(home + Vector3Int.down + Vector3Int.left) != null && (floorTilemap.GetTile(home + Vector3Int.down) != null || floorTilemap.GetTile(home + Vector3Int.left) != null)){
			neighbors.Add(home + Vector3Int.down + Vector3Int.left);
		}
		if(floorTilemap.GetTile(home + Vector3Int.down + Vector3Int.right) != null && (floorTilemap.GetTile(home + Vector3Int.down) != null || floorTilemap.GetTile(home + Vector3Int.right) != null)){
			neighbors.Add(home + Vector3Int.down + Vector3Int.right);
		}
		if(floorTilemap.GetTile(home + Vector3Int.up + Vector3Int.right) != null && (floorTilemap.GetTile(home + Vector3Int.up) != null || floorTilemap.GetTile(home + Vector3Int.right) != null)){
			neighbors.Add(home + Vector3Int.up + Vector3Int.right);
		}

		return neighbors;
	}

	private int HeuristicDistance(Vector3Int a, Vector3Int b){
		//return Mathf.Abs(a.x - b.x) + Mathf.Abs(Mathf.Abs(a.y) - Mathf.Abs(b.y));//the y axis is positivie bcz in tilemap y is decreasing
		int dx = Mathf.Abs(a.x - b.x);
		int dy = Mathf.Abs(Mathf.Abs(a.y) - Mathf.Abs(b.y));

		return movementCost * (dx + dy) + ((1 + movementCost) - 2 * movementCost) * Mathf.Min(dx, dy);
	}

	//Pathfinder using Breadth First Search
	public List<Vector3Int> pathFinder(Tilemap tilemap, Vector3Int start, Vector3Int end, List<Vector3Int> walkableArea){
		PriorityQueue frontier = new PriorityQueue();
		frontier.Add(start, 0);

		Dictionary<Vector3Int, Vector3Int> came_from = new Dictionary<Vector3Int, Vector3Int>();
		came_from.Add(start, default(Vector3Int));//dicitionary where will be stored a cell coords and from which cell it came from

		Dictionary<Vector3Int, int> cost_so_far = new Dictionary<Vector3Int, int>();
		cost_so_far.Add(start, 0);

		Vector3Int current;
		List<Vector3Int> neighbors; 
		int new_cost, priority, diagonalCost;

		while(!frontier.IsEmpty()){
			current = frontier.Pop();
			neighbors = getNeighbors(current);//getting all 8 neighbors

			if(current == end){//if the current cell is the destination, break bcz we found the path we were looking for
				break;
			}

			foreach(Vector3Int neighbor in neighbors){//checking each neighbor
				if(tilemap.GetTile(neighbor) != null && walkableArea.Contains(neighbor)){
					if(current.x != neighbor.x && current.y != neighbor.y){
						diagonalCost = 1;
					}else{
						diagonalCost = 0;
					}
					new_cost = cost_so_far[current] + movementCost + diagonalCost;
					if(!cost_so_far.ContainsKey(neighbor) || new_cost < cost_so_far[neighbor]){

						if(cost_so_far.ContainsKey(neighbor)){
							if(new_cost < cost_so_far[neighbor]){
								cost_so_far[neighbor] = new_cost;
							}
						}else{
							cost_so_far.Add(neighbor, new_cost);
						}

						if(!frontier.Contains(neighbor)){
		        			priority = new_cost + HeuristicDistance(end, neighbor);
		         			frontier.Add(neighbor, priority);
						}
						if(!came_from.ContainsKey(neighbor)){
	         				came_from.Add(neighbor, current);
						}
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

	public void FlipUnit(Transform unit, Vector3 target){
		if(unit.GetComponent<SpriteRenderer>() && target.x != unit.transform.position.x){
			unit.GetComponent<SpriteRenderer>().flipX = (target.x < unit.transform.position.x);
		}
	}

	//Moving the unit
	IEnumerator moveUnit(Transform unit, List<Vector3Int> path){
		//destroying all walkable area dots
		foreach (Transform blueDot in walkableDotsParent) {
			GameObject.Destroy(blueDot.gameObject);
		}

		unitIsMoving = true;
		unit.transform.GetComponent<Animator>().SetBool("isMoving", true);
		foreach(Vector3Int breadCrumb in path){
			Vector3 convertedDestination = convertGidPosToWorldPos(breadCrumb);

			FlipUnit(unit, convertedDestination);

			while(Vector3.Distance(unit.transform.position, convertedDestination) > unitTileOffset){ 
				unit.position = Vector3.MoveTowards(unit.position, convertedDestination, Time.deltaTime * movementVelocity);
				yield return null;
			}
		}
		unitIsMoving = false;
		unit.transform.GetComponent<Animator>().SetBool("isMoving", false);

		//Updating units position on grid
		selectecUnitPosition = unit.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
		tempGridPosition = selectecUnitPosition;

		//calculating remaining speed
		if(reduceSpeed){
			unit.transform.GetComponent<ChampsBehaviour>().remainingSpeed -= MovementCostCalculation(path);
		}

		//rendering the new walkable area and calculating new one
		if(somethingIsSelected){
			selectedUnitWalkableArea = walkableArea(unit.transform);
		}
	}

	//Calculaing walkable area
	public List<Vector3Int> walkableArea(Transform unit){
		/* those ilustrations will help you to undernstand how to find the walkable area

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

		the two 'ifs' below ('if(i + c > speed - 1 && i + c < (speed + 1) + (i * 2))' and 'if(i + c > (speed - 1) + 2 * (i - speed) && i + c <= speed * 3)') restrict the 
		selection to the cells og the diamong shape
		*/
		List<Vector3Int> walkable = new List<Vector3Int>();
		int speed = unit.GetComponent<ChampsBehaviour>().remainingSpeed;
		int rows = (2 * speed) + 1;

		Vector3Int unitPos = unit.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
		Vector3Int startingPoint = new Vector3Int(unitPos.x - speed, unitPos.y + speed, unitPos.z);

		for(int i = 0; i < rows; i++){
			for(int c = 0; c < rows; c++){
				if(floorTilemap.GetTile(startingPoint + new Vector3Int(i, -c, 0)) != null){
					walkable.Add(startingPoint + new Vector3Int(i, -c, 0));
				}
			}
		}

		//correting the walkable area by removing the ones the unit cannot reach
		//basicaly this is whats its being doing: for each cell in the walkable area find its path to the unit position
		//if the distance is higher tha the unit's speed, removes it
		List<Vector3Int> allPaths = new List<Vector3Int>();
		List<Vector3Int> toRemove = new List<Vector3Int>();

		for (int i = 0; i < walkable.Count; i++){
			allPaths = pathFinder(floorTilemap, walkable[i], unitPos, walkable);
			if(MovementCostCalculation(allPaths) > speed || (MovementCostCalculation(allPaths) == 0 && walkable[i] != unitPos)){
				toRemove.Add(walkable[i]);
			}
		}

		foreach(Vector3Int cellToRemove in toRemove){
			walkable.Remove(cellToRemove);
		}

		foreach(Vector3Int cell in walkable){
			Vector3 convertedWalkArea = convertGidPosToWorldPos(cell);
			Instantiate(walkableDots, convertedWalkArea, Quaternion.identity, walkableDotsParent);
		}

		return walkable;
	}

	public int MovementCostCalculation(List<Vector3Int> path){
		int pathCost = 0;
		for(int i = 0; i < path.Count - 1; i++){
			if(path[i].x != path[i + 1].x && path[i].y != path[i + 1].y){
				pathCost += 2;
			}else{
				pathCost += 1;
			}
		}
		return pathCost;
	}
}

//used by A* pathfinder
public class PriorityQueue{
	public List<Vector3Int> cells = new List<Vector3Int>();
	List<int> cellPriority = new List<int>();

	private int FindInsertIndex(int priority){
		foreach(int p in cellPriority){
			if(priority < p){
				return cellPriority.IndexOf(p);
			}
		}
		return cellPriority.Count;
	}

	public void Add(Vector3Int cell, int priority){
		int index = FindInsertIndex(priority);
		cells.Insert(index, cell);
		cellPriority.Insert(index, priority);
	}

	public Vector3Int Pop(){
		Vector3Int cell = cells[0];
		cells.RemoveAt(0);
		cellPriority.RemoveAt(0);
		return cell;
	}

	public void Print(){
		int index = 0;
		foreach(int p in cellPriority){
			Console.WriteLine(p + ", " + cells[index++]);
		}
	}

	public bool IsEmpty(){
		return (cells.Count == 0);
	}

	public bool Contains(Vector3Int element){
		return cells.Contains(element);
	}
}

