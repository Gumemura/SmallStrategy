using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChampsBehaviour: MonoBehaviour
{
	public int speed;
	public int hp;

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
}


