using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IniciativePortrait: MonoBehaviour{
	public GameObject unitPortrait; //the portrait to be created
	public float portraitMovementVelocity; //the velocity of portrait when its moving (OBVIOUS thats when its moving)

	private Vector2 startingPoint; //point at the middle of the screen//hardcode because yes
	private int quantityPortraits; // how many portraits will be renderized

	private float xStartingPoint; //X of starting position
	private float yEndingPoint; //y of the position where the portrait will stop
	private float betweenPortraitsOffset; //a small space between portrait to avoid them to stick together
	private float portraitLength; //the lenght of portrait
	private float spaceBetweenPortraits; // sum of portait length plus the offset
	private Dictionary<Transform, Vector2> allPortraits = new Dictionary<Transform, Vector2>();

	void Awake(){//using awake so it will be execute before gamecontroller start
		startingPoint = new Vector2(8, 15);
		xStartingPoint = startingPoint.x;
		betweenPortraitsOffset = .1f;
		portraitLength = unitPortrait.transform.GetComponent<BoxCollider2D>().size.x;

		spaceBetweenPortraits = betweenPortraitsOffset + portraitLength;
	}

	//setting the quantity of portraits and instatiating all portraits behind the scenes
    public void SetQuantity(PriorityQueue<ChampsBehaviour> iniciativeOrder){
    	quantityPortraits = iniciativeOrder.Size();
    	List<Vector2> allPortraitsPosition = CalculatinAllPositions();
    	GameObject portrait;
    	SpriteRenderer unitInThePortrait;

    	for(int i = 0; i < quantityPortraits; i++){
    		Vector2 position = allPortraitsPosition[i]; //stores the portrati positino
    		portrait = Instantiate(unitPortrait, position + new Vector2(0, 2), Quaternion.identity, this.transform.Find("Portraits")); //instantiating the portrati
    		unitInThePortrait = portrait.transform.Find("Unit").GetComponent<SpriteRenderer>();//getting the sprite renderer of child element
    		unitInThePortrait.sprite = iniciativeOrder.SeeElement(i).unitPortrait; //setting the unit sprite inside the protrait

    		allPortraits.Add(portrait.transform, position);
    	}
    }

    //The first portrait, to the left
    private float FirstPortraitPosition(){
    	return xStartingPoint - ((quantityPortraits - 1) * (spaceBetweenPortraits/2));
    }

    //Calculatin the positions of all portraits
    private List<Vector2> CalculatinAllPositions(){
    	List<Vector2> allPortraitsPosition = new List<Vector2>();
    	Vector2 portraitPosition = new Vector2(FirstPortraitPosition(), startingPoint.y);
    	for (int i = 0; i < quantityPortraits; i++){
    		allPortraitsPosition.Add(portraitPosition);
			portraitPosition += new Vector2(spaceBetweenPortraits, 0);
    	}
    	return allPortraitsPosition;
    }

    private IEnumerator MovePortraits(Transform portrait, Vector2 destination){
    	while(Vector2.Distance((Vector2)portrait.position, destination) > 0){
    		portrait.position = Vector2.MoveTowards((Vector2)portrait.position, destination, Time.deltaTime * portraitMovementVelocity);
	    	yield return null;
    	}
    }

    public IEnumerator LowerPortraits(){
    	foreach(KeyValuePair<Transform, Vector2> portrait in allPortraits){
    		StartCoroutine(MovePortraits(portrait.Key, portrait.Value));
    		yield return new WaitForSeconds(.5f);
		}
    }
}