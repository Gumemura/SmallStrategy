using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IniciativePortrait: MonoBehaviour{
	public GameObject unitPortrait; //the portrait to be created

	private Vector2 startingPoint; //point at the middle of the screen
	private int quantityPortraits; // how many portraits will be renderized

	private float xStartingPoint; //X of starting position
	private float betweenPortraitsOffset; //a small space between portrait to avoid them to stick together
	private float spaceBetweenPortraits; // sum of portait length plus the offset
	private float portraitLength; //the lenght of portrait
	private List<Vector2> allPortraitsPosition; //Vector with all portraits positions

	void Start(){
		startingPoint = new Vector2(8, 15); //hardcode because yes
		xStartingPoint = startingPoint.x;
		betweenPortraitsOffset = .1f; 
		portraitLength = unitPortrait.transform.GetComponent<BoxCollider2D>().size.x;

		spaceBetweenPortraits = betweenPortraitsOffset + portraitLength;
		allPortraitsPosition = new List<Vector2>();
	}

	//setting the quantity of portraits
    public void SetQuantity(int quantity){
    	quantityPortraits = quantity;
    }

    //The first portrait, to the left
    private float FirstPortraitPosition(){
    	return xStartingPoint - ((quantityPortraits - 1) * (spaceBetweenPortraits/2));
    }

    //Calculatin the positions of all portraits
    private List<Vector2> CalculatinAllPositions(){
    	Vector2 portraitPosition = new Vector2(FirstPortraitPosition(), startingPoint.y);
    	for (int i = 0; i < quantityPortraits; i++){
    		allPortraitsPosition.Add(portraitPosition);
			portraitPosition += new Vector2(spaceBetweenPortraits, 0);
    	}
    	return allPortraitsPosition;
    }

    // public void a(){
    // 	CalculatinAllPositions();
    // 	foreach(Vector2 pos in allPortraitsPosition){
    // 		Instantiate(unitPortrait, pos, Quaternion.identity, this.transform);
    // 	}
    // }

   	public void PositioningPortraits(PriorityQueue<ChampsBehaviour> iniciativeOrder){
   		foreach(ChampsBehaviour unit in iniciativeOrder){
   			
   		}
   	}
}