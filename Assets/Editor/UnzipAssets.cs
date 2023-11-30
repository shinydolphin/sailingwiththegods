using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

public class UnzipAssets : AssetPostprocessor
{
	static readonly (string, string, string)[] _knownZips = new[] {
		("Assets/_Scenes/Additive/World/NavMesh.zip", "Assets/_Scenes/Additive/World/NavMesh.asset", "b0f7be0c2511a2b43967d4790f69b935")
	};

	[MenuItem("SWTG/Unzip Assets")]
	static void Execute() {
		UnzipAll(_knownZips);
	}

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
		var toProcess = _knownZips
			.Where(zip => importedAssets.Any(i => zip.Item1.Contains(i)));
		
		if (!toProcess.Any()) return;

		// ask first in case you just rebaked the navmesh and don't want to overwrite your changes, but always do it from cloud builds (batch mode)
		var strList = string.Join("\n", toProcess.Select(tuple => tuple.Item1));
		var shouldUnzip = Application.isBatchMode ||
			EditorUtility.DisplayDialog("Unzip Assets", "Zipped assets have been updated, extract them?\n\n" + strList, "Yes (recommended)", "No");

		if(shouldUnzip) {
			UnzipAll(toProcess);
		}
	}

	static void UnzipAll(IEnumerable<(string, string, string)> zips) {
		foreach (var (zip, dest, guid) in zips) {
			EditorUtility.DisplayProgressBar("Unzip Assets", zip, 0.5f);
			UnzipFile(zip, dest);
			AssetDatabase.ImportAsset(dest);

			// ensure the expected guid is used to preserve project references since these files are ignored from git
			var importGuid = AssetDatabase.AssetPathToGUID(dest);
			ReplaceInFile(dest + ".meta", importGuid, guid);

			Debug.Log(zip + " unzipped to " + dest);
			EditorUtility.ClearProgressBar();
		}
	}

	// had to use a library for unzipping because Unity' doesn't support .net 4.5's zip file library out of the box. GzipStream is for .gz only. has a different format.
	// only supporting zips which have a single file in them for now. if we start putting multiple assets into zips, we'll have to loop over the contents
	static void UnzipFile(string zipPath, string destPath) {
		using (var zipped = File.Open(zipPath, FileMode.Open, FileAccess.Read))
		using (var unzipped = new ZipFile(zipped))
		using (var output = File.Open(destPath, FileMode.Append, FileAccess.Write)) {
			var zipStream = unzipped.GetInputStream(0);
			zipStream.CopyTo(output);
		}
	}

	static void ReplaceInFile(string filename, string orig, string replaceWith) {
		string text = File.ReadAllText(filename);
		text = text.Replace(orig, replaceWith);
		File.WriteAllText(filename, text);
	}
}
