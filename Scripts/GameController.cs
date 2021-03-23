﻿using System.Collections;
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
	public float unitTileOffset;

	[Header("Champs")]
	public float movementVelocity;
	[HideInInspector]public bool unitIsMoving;

	private string champsTag = "Champ";
	private SpriteRenderer spriteRenderer;
	private GameObject[] allHeroes;
	private bool somethingIsSelected;
	private RaycastHit2D hitBox;

	void Start(){
		floorTilemap.CompressBounds();

		//Turning the default windows cursor off
		Cursor.visible = false;
		spriteRenderer = transform.GetComponent<SpriteRenderer>();

		//Filling the array with all controlable heros
		allHeroes = GameObject.FindGameObjectsWithTag(champsTag);

		somethingIsSelected = false;
		unitIsMoving = false;
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
				foreach(Vector3Int walkArea in walkableArea(hitBox.transform)){
					Vector3 convetedWalkArea = convertGidPosToWorldPos(walkArea);
					Instantiate(debugBall, convetedWalkArea, Quaternion.identity, debugParent);
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
						//Movement
						if(unitIsMoving == false){
							List<Vector3Int> path = new List<Vector3Int>();
							Vector3Int startPos = hitBox.transform.GetComponent<ChampsBehaviour>().getPositionOnGrid(gameGrid);
							path = pathFinder(floorTilemap, startPos, gridPos, walkableArea(hitBox.transform));

							foreach (Transform child in debugParent) {
								GameObject.Destroy(child.gameObject);
							}
							StartCoroutine(moveUnit(hitBox.transform, path));

							foreach(Vector3Int cell in path){
								Vector3 convetedPath = gameGrid.CellToWorld(cell) + new Vector3(0, gameGrid.cellSize.y/2, 0);
								Instantiate(debugBall, convetedPath, Quaternion.identity, debugParent);
							}
						}
					}
				}
			}	
		}
	}

	public Vector3 convertGidPosToWorldPos(Vector3Int gridPos){
		return gameGrid.CellToWorld(gridPos) + new Vector3(0, gameGrid.cellSize.y/2, 0);
	}

	//Returns all neighbors
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
				if(tilemap.GetTile(neighbor) != null && walkableArea.Contains(neighbor)){
					if(!came_from.ContainsKey(neighbor)){
						if(!frontier.Contains(neighbor)){
							frontier.Enqueue(neighbor);
						}
						came_from.Add(neighbor, current);
					}
				}
			}
		}

		current = end;
		List<Vector3Int> path = new List<Vector3Int>();

		while(current != start){
			path.Add(current);
			try{
				current = came_from[current];
			}catch (Exception e){
				print("errou!");
				return new List<Vector3Int>() {};
			}
		}

		path.Add(start);
		path.Reverse();

		return path;
	}

	IEnumerator moveUnit(Transform unit, List<Vector3Int> path){
		foreach(Vector3Int breadCrumb in path){
			Vector3 convertedDestination = convertGidPosToWorldPos(breadCrumb);
			while(Vector3.Distance(unit.transform.position, convertedDestination) > unitTileOffset){ 
				unitIsMoving = true;
				unit.position = Vector3.MoveTowards(unit.position, convertedDestination, Time.deltaTime * movementVelocity);
				yield return null;
			}
			// while(Vector3.Distance(unit.transform.position, convertedDestination) > unitTileOffset){ 
			// 	unit.Translate(convertedDestination * Time.deltaTime);
			// 	yield return null;
			// }
		}
		unitIsMoving = false;
	}

	//Calculaing walkable area
	public List<Vector3Int> walkableArea(Transform unit){
		List<Vector3Int> walkable = new List<Vector3Int>();
		int speed = unit.GetComponent<ChampsBehaviour>().speed;
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

		return walkable;
	}
}

