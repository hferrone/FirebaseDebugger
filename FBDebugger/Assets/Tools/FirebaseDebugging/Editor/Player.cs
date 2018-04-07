using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

	public string email;
	public string username;
	public int score;
	public float exp;
	public bool isAdsActive;
	public List<string> equipment;

	public Player(string email, string username, int score, float exp, bool ads, List<string> equipment)
	{
		this.email = email;
		this.username = username;
		this.score = score;
		this.exp = exp;
		this.isAdsActive = ads;
		this.equipment = equipment;
	}
}
