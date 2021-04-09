using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
	private int damageBot;
	private int damageTop;

	private int actionCost;
	private int criticalRate;

	public Vector2 GetDamage(){
		return new Vector2(damageBot, damageTop);
	}

	public int GetActionCost(){
		return actionCost;
	}

	public int GetCriticalRate(){
		return criticalRate;
	}
}
