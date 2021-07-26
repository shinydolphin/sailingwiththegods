using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogPair
{
	private string cityName;
	private string question;
	private string answer;

	public DialogPair(string city, string q, string a) 
	{
		cityName = city;
		question = q;
		answer = a;
	}
	
	public string CityName 
	{
		get { return cityName; }
	}

	public string Question
	{
		get { return question; }
	}

	public string Answer 
	{
		get { return answer; }
	}

}
