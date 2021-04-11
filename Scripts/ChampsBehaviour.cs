using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChampsBehaviour: MonoBehaviour
{
	public int speed;
	public int hp;
	public int maxIniciative;
	public int iniciative;

	public Item equipedItem;
	public Item[] invetory;

	[HideInInspector]public int remainingSpeed;
	[HideInInspector]public int remainingHP;
	public bool isAttackMelee;

	void Start(){
		remainingSpeed = speed;
	}

	public void turnOnSelection(bool active){
		Transform selectionCircle = transform.Find("Selected");
		selectionCircle.gameObject.SetActive(active);
	}

	public Vector3Int getPositionOnGrid(Grid gameGrid){
		return gameGrid.WorldToCell(this.transform.position);
	}

	public void EquipItem(Item item){
		equipedItem = item;
	}

	public int SetIniciative(){
		iniciative = Random.Range(1, maxIniciative);
		return iniciative;
	}
}


