//David Herrod
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrGameTile : MonoBehaviour
{
	public bool isRosette = false;

	public GameObject highlight;
	private bool occupied = false;
	private UrPiece currentPiece = null;
	private UrGameController urGC;

	private void Start() {
		urGC = GameObject.FindWithTag("GameController").GetComponent<UrGameController>();
	}

	private void OnTriggerEnter(Collider other) 
	{
		UrPiece c = other.GetComponent<UrPiece>();
		if (c != null) {
			currentPiece = c;
		}
	}

	private void OnTriggerExit(Collider other) 
	{
		currentPiece = null;
	}

	public void ShowHighlight(bool toggle) 
	{
		if (highlight != null) {
			highlight.SetActive(toggle);
		}
	}

	public bool Occupied 
	{
		get {
			return occupied;
		}
		set {
			occupied = value;
		}
	}

	public void RemoveCurrentFromBoard() 
	{
		if (currentPiece != null) 
		{
			currentPiece.RemovePieceFromBoard();
			currentPiece = null;
		}
	}

	/// <summary>
	/// Returns true if the piece on the occupying square is not controlled by the player indicated by isPlayer
	/// </summary>
	/// <param name="isPlayer"></param>
	/// <returns></returns>
	public bool OppositeOccupyingPiece(bool isPlayer) 
	{
		if (currentPiece == null) 
		{
			return true;
		}
		return isPlayer ? currentPiece.CompareTag(urGC.enemyTag) : currentPiece.CompareTag(urGC.enemyTag);
	}
}
