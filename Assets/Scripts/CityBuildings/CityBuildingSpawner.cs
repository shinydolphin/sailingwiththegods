using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityBuildingSpawner : MonoBehaviour
{
	[SerializeField] int NumHouses = 30;
	[SerializeField] float CityRadius = 1f;
	[SerializeField] float BuildingRadius = 0.05f;

	[SerializeField] GameObject[] HousePrefabs = null;

	// fallback to slower FindObjectByType for editor usage
	World World => Globals.World 
		?? GameObject.FindObjectOfType<World>();

    // Start is called before the first frame update
    void Start()
    {
		Regenerate();
	}

	void SafeDestroy(GameObject obj) {
		if(Application.isPlaying) {
			Destroy(obj);
		}
		else {
			DestroyImmediate(obj);
		}
	}

	[Button]
	void Regenerate() {
		Clear();

		for(var i = 0; i < NumHouses; i++) {
			GameObject.Instantiate(
				HousePrefabs.RandomElement(), 
				GetBestRandomPosition(), 
				Quaternion.Euler(0, Random.Range(0, 360), 0),
				transform
			);
		}

		foreach(var building in transform.GetChildren().ToList()) {
			// kill buildings that are in the water. we don't try to replace them since it'd probably make things too dense
			if (World.IsBelowWaterLevel(building.position)) {
				Debug.LogWarning("Spawned on water");
				SafeDestroy(building.gameObject);
			}
		}
	}

	Vector3 GetRandomPositionOption() {
		var pos = Utils.RandomPositionIn(new Bounds(transform.position, Vector3.one * CityRadius));
		return World.GetPosOnLand(pos);
	}

	Vector3 GetBestRandomPosition() {
		const int maxTries = 100;
		var buildingMask = LayerMask.GetMask("cityBuilding");

		Vector3 pos = GetRandomPositionOption();
		for(var i = 0; i < maxTries; i++) {
			if(!Physics.OverlapSphere(pos, BuildingRadius, buildingMask).Any()) {
				return pos;
			}

			pos = GetRandomPositionOption();
		}

		// use the last pick if we couldn't find a non-overlapping option
		return pos;
	}

	[Button]
	void Clear() {
		foreach(var child in transform.GetChildren().ToList()) {
			SafeDestroy(child.gameObject);
		}
	}
}
