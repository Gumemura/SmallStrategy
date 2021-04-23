using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class ChampsBehaviour: MonoBehaviour
{
	public int speed;
	public int hp;
	public int maxIniciative;
	[HideInInspector]public int iniciative;
	public Sprite unitPortrait;

	public Item equipedItem;
	public Item[] invetory;

	[HideInInspector]public int remainingSpeed;
	[HideInInspector]public int remainingHP;
	public bool isAttackMelee;

	private TextMeshProUGUI unitText;
	private float textSizeVariation = .01f;

	void Start(){
		remainingSpeed = speed;
		unitText = transform.Find("UnitCanvas").Find("UnitText").GetComponent<TextMeshProUGUI>();
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

	//display a text for a briefly moment
	public IEnumerator Displaytext(string text, int secondsTime, Color textColor){
		unitText.text = text;
		unitText.color = textColor;
		yield return new WaitForSeconds(secondsTime);
		unitText.text = "";
	}

	//makes the text go bigger than return to normal. A easy way to gain focus on the text
	private IEnumerator PopUpText(float maxFontSize){
		float i = 0;
		int inverse = 1;
		while (i < maxFontSize * 2){
			if(i >= maxFontSize){
				inverse = -1;
			}
			unitText.fontSize += textSizeVariation * inverse;
			yield return new WaitForSeconds(textSizeVariation);
			i += textSizeVariation;
		}
		unitText.fontSize = .4f;
	}

	//makes a effect of "dice rolling" with the text
	public IEnumerator RollingIniciative(){
		int i = 0;
		while(i < 300){
			unitText.text = (Random.Range(1, maxIniciative)).ToString();
			//y = 10 ^ x - 30
			yield return new WaitForSeconds(Mathf.Pow(100, i*.1f - 30));
			i++;
		}
		StartCoroutine(Displaytext(iniciative.ToString(), 3, Color.white)); //displaying the inciative
		StartCoroutine(PopUpText(textSizeVariation * 50)); //making it go big
	}
}


