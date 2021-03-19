using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class ChampsBehaviour: MonoBehaviour
{
	public void turnOnSelection(bool active){
		Transform selectionCircle = transform.Find("Selected");
		selectionCircle.gameObject.SetActive(active);
	}

	public Vector3Int findPositionOnGrid(Grid gameGrid){
		return gameGrid.WorldToCell(this.transform.position);
	}

	public int heuristicDistante(Vector3Int start, Vector3Int end){
		return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
	}

	//Pathfinder method
	public List<Vector3Int> pathFinder(Grid grid, Tilemap tilemap, Vector3Int end){
		Vector3Int start = findPositionOnGrid(grid);
		Vector3Int current = start;

		List<Vector3Int> path = new List<Vector3Int>();
		List<Vector3Int> visited = new List<Vector3Int>();
		List<Vector3Int> neighbors = new List<Vector3Int>();

		path.Add(current);
		visited.Add(current);

		int heuristic = 100000;
		int tempHeuristic = 0;
		int index = 0;

		while(current != end){
			neighbors.Add(current + Vector3Int.up);
			neighbors.Add(current + Vector3Int.down);
			neighbors.Add(current + Vector3Int.left);
			neighbors.Add(current + Vector3Int.right);

			foreach(Vector3Int neighbor in neighbors){
				if(!visited.Contains(neighbor) && tilemap.GetTile(neighbor) != null){
					tempHeuristic = heuristicDistante(neighbor, end);
					if(tempHeuristic < heuristic){
						heuristic = tempHeuristic;
						tempHeuristic = 0;
						index = neighbors.IndexOf(neighbor);
					}
				}
			}

			current = neighbors[index];
			heuristic = 100000;

			neighbors.Clear();
			path.Add(current);
			visited.Add(current);
		}
		return path;
	}
}
