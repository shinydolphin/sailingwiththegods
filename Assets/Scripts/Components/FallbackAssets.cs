using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FallbackAssets : MonoBehaviour
{
	// only used in editor, but needs to exist always for unity serialization, so suppressing the warning
#pragma warning disable 0414
	[SerializeField] FallbackLink[] _fallbacks = null;
#pragma warning restore

	[Serializable]
	class FallbackLink
	{
		public string ProprietaryGuid = null;
		public string FallbackGuid = null;
	}

#if UNITY_EDITOR
	void OnEnable() {
		if(!FallbackSwap.HasProprietaryAssets) {
			Swap();
		}
	}

	void Swap() {
		if(_fallbacks == null) return;	

		foreach(var entry in _fallbacks) {
			if(string.IsNullOrEmpty(entry.ProprietaryGuid)) continue;
			if(string.IsNullOrEmpty(entry.FallbackGuid)) continue;

			var fallbackPath = AssetDatabase.GUIDToAssetPath(entry.FallbackGuid);
			var fallbackFilename = new FileInfo(fallbackPath).Name;

			var finalPath = Path.Combine("Assets", "FallbackAssets", "_Generated", fallbackFilename);
			if(!Directory.Exists("Assets/FallbackAssets/_Generated"))
			{
				Directory.CreateDirectory("Assets/FallbackAssets/_Generated");
			}

			var finalMetaPath = finalPath + ".meta";
			File.Copy(fallbackPath, finalPath, true);
			File.Copy(fallbackPath + ".meta", finalMetaPath, true);

			var txt = File.ReadAllText(finalMetaPath);
			txt = txt.Replace(entry.FallbackGuid, entry.ProprietaryGuid);
			File.WriteAllText(finalMetaPath, txt);

			Debug.Log("Propreietary Asset " + entry.ProprietaryGuid + " was missing, so it was replaced with fallback " + fallbackFilename);
		}
	}
#endif

}

