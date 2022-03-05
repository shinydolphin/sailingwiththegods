using UnityEngine;
using NaughtyAttributes;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class WorldTools : MonoBehaviour
{
	World _World;

	private void Awake() {
		_World = GetComponent<World>();
	}

#if UNITY_EDITOR

	[Button("Find Settlement By Name...")]
	void FindSettlementByName() {
		Shiny.Editor.TextPromptWindow.Show("Enter Settlement Name", settlementName => {
			UnityEditor.Selection.activeObject = GameObject.FindObjectsOfType<SettlementComponent>()
			.FirstOrDefault(s => s.thisSettlement?.name.ToLower() == settlementName.ToLower());
		});
	}

	[Button("Find Settlement By Id...")]
	void FindSettlementById() {
		Shiny.Editor.TextPromptWindow.Show("Enter Settlement ID", settlementId => {
			UnityEditor.Selection.activeObject = GameObject.FindObjectsOfType<SettlementComponent>()
			.FirstOrDefault(s => s.thisSettlement?.settlementID == int.Parse(settlementId));
		});
	}

	[Button("Save Water Current Zones")]
	void SaveWaterCurrentZones() {

		string waterRoseData = "";
		int rowCounter = 0;
		Transform waterZone;

		//Loop through all of the child objects of the current zone parent object
		//The parent stores them in a sequential list so every 40 objects represents a new line in the spread sheet csv file
		//The coordinate for the zones is 0,0 for the top left, ending with 39,39 on the bottom right
		for (int currentZone = 0; currentZone < _World.currentZoneParent.transform.childCount; currentZone++) {
			waterZone = _World.currentZoneParent.transform.GetChild(currentZone);
			waterRoseData += waterZone.GetChild(0).transform.localRotation.eulerAngles.y;
			waterRoseData += ",";
			waterRoseData += waterZone.GetChild(0).GetComponent<script_WaterWindCurrentVector>().currentMagnitude;
			rowCounter++;
			//If we've hit 40 objects, it's time to start a new row in the csv file
			if (rowCounter == 40) {
				rowCounter = 0;
				waterRoseData += "\n";
			}
			//only write a comma if we aren't at the last entry for the row
			else
				waterRoseData += ",";
		}
		//Debug.Log(waterRoseData);
		StreamWriter sw = new StreamWriter(@Application.persistentDataPath + "/" + "waterzones_january.txt");
		sw.Write(waterRoseData);
		sw.Close();
	}

	[Button("Save Settlement Positions")]
	void Tool_SaveCurrentSettlementPositionsToFile() {
		string writeToFile = "";
		var settlementParent = _World.settlement_masterList_parent;
		var sortedChildren = settlementParent.transform.GetChildren()
			.Select(c => c.GetComponent<SettlementComponent>())
			.OrderBy(c => c.thisSettlement.settlementID)
			.ToList();

		foreach (var child in sortedChildren) { 
			var ID = child.thisSettlement.settlementID.ToString();
			var unityX = child.transform.position.x.ToString();
			var unityY = child.transform.position.y.ToString();
			var unityZ = child.transform.position.z.ToString();
			var unityEulerY = child.transform.eulerAngles.y.ToString();
			string test = ((ID + "," + unityX + "," + unityY + "," + unityZ + "," + unityEulerY));

			//perform a quick check to make sure we aren't at the end of the file: if we are don't add a new line
			if (child != sortedChildren.Last())
				test += "\n";
			writeToFile += test;
		}

		//Write the string to file now
		StreamWriter sw = new StreamWriter("Assets/Resources/settlement_unity_position_offsets.txt");
		sw.Write(writeToFile);
		sw.Close();
	}

#endif
}
