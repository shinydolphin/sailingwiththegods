//David Herrod
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrGameTile : MonoBehaviour
{
	//public Transform nextTile;
	//public Transform prevTile;

	//public Transform nextTileAL;
	//public Transform prevTileAL;

	public bool isRosette = false;
	//public int timesLandedOn = 0;

	public GameObject highlight;
	private bool occupied = false;
	private UrCounter occupyingCounter = null;

	private void Awake() 
	{
		//isAvailable = transform.GetChild(0).gameObject;
	}

	//public void ShowAvailablePositions(int drv) {
	//	List<GameTile> aTiles = new List<GameTile>();
	//	aTiles.Add(nextTile.GetComponent<GameTile>());
	//	for (int i = 0; i< drv; i++) {
	//		aTiles[i].available.SetActive(true);
	//	}

	//}

	private void OnTriggerEnter(Collider other) 
	{
		UrCounter c = other.GetComponent<UrCounter>();
		if (c != null) {
			occupyingCounter = c;
		}
	}

	private void OnTriggerExit(Collider other) 
	{
		occupyingCounter = null;
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
}
