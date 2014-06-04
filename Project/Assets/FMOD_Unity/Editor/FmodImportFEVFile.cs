/*
 * +-------------------------------------------------------------------------------------+
 *  Project: FMOD_Unity
 *  Description: Handles the logic of the menu "FMOD/Import .FEV file..." that
 * 				 can be used instead of just dropping a .fev file into the project.
 *  Company: ISART Digital (www.isartdigital.com)
 *  Author: Galaad BROD (galaad.brod@gmail.com)
 * +-------------------------------------------------------------------------------------+
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class FmodImportFEVFile : EditorWindow
{
	[MenuItem("FMOD/Import/.FEV File...")]
	public static void ImportFEV() {
		string filePath = EditorUtility.OpenFilePanel("Find .FEV file", "", "fev");
		
		if (filePath != "") {
			string fileName = Path.GetFileName(filePath);
			string importDestination = EditorUtility.OpenFolderPanel("Import destination", "Assets", "");
			
			if (importDestination != "") {
				string projectRelativePathToFile = FileUtil.GetProjectRelativePath(importDestination + "/" + fileName);
				string projectRelativePathToAsset = FileUtil.GetProjectRelativePath(importDestination + "/" + fileName + @".asset");
				
				System.IO.File.Copy(filePath, importDestination + "/" + fileName, true);
				AssetDatabase.ImportAsset(projectRelativePathToFile, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				
				UnityEngine.Object ret = AssetDatabase.LoadAssetAtPath(projectRelativePathToAsset, typeof(FmodEventAsset));
				if (ret != null) {
					Selection.activeObject = ret;					
				}
			}
		}
	}
}
#endif
