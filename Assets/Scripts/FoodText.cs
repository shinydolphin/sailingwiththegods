using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodText
{
	public enum Type {
		Food,
		Wine
	}

	private string item, quote;
	private Type textType;

	public FoodText() 
	{
		item = "default food";
		quote = "default food quote";
		textType = Type.Food;
	}

	public FoodText(string foodItem, string foodQuote, Type t) 
	{
		item = foodItem;
		quote = foodQuote;
		textType = t;
	}
	
	public Type TextType 
	{
		get { return textType; }
		set { textType = value; }
	}

	public string Item 
	{
		get { return item; }
		set { item = value; }
	}

	public string Quote 
	{
		get { return quote; }
		set { quote = value; }
	}
}
