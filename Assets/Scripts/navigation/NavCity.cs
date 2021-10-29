using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nav
{
	public class NavCity
	{
		private Dictionary<string, Vector3> dictCity;
		private GameObject tmp;
		// Start is called before the first frame update
		public NavCity() {
			var tmp = GameObject.Find("Settlement Master List");
			do {
				if (tmp == null) {
					Debug.Log("Can't find the object");
				}
				else {
					dictCity = new Dictionary<string, Vector3>();
					foreach (Transform child in tmp.transform) {
						dictCity[child.name] = child.transform.position;
					}
					Debug.Log(dictCity.Count);
					break;
				}
			} while (tmp = null);
			
		}

		public Vector3 GetCityLocation(string cityName) {
			return dictCity[cityName];
		}
	}
}

