using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using HoloToolkit.Unity;

namespace Autodesk.Forge {

#if UNITY_EDITOR
public class Hierarchy : EditorWindow {
	public string _name ="" ;
	public bool _flattern =false ;
	protected static string _resourcesPath ="Assets/Resources/" ;
	protected static string _bundlePath ="Assets/Bundles/" ;

	public string _resources { get { return (_resourcesPath + _name) ; } }
	public string _prefab { get { return (_resourcesPath + _name + ".prefab") ; } }
	public string _bundle { get { return (_bundlePath + _name + ".unity3d") ; } }

	//protected GameObject InitializePaths () {
	//	// Create some asset folders.
	//	//AssetDatabase.CreateFolder ("Assets/Meshes", "MyMeshes") ;
	//	//AssetDatabase.CreateFolder ("Assets/Prefabs", "MyPrefabs") ;

	//	//GameObject root =GameObject.Find ("/Root") ;
	//	//LMVExportToUnity[] script =root.GetComponents<LMVExportToUnity> () ;
	//	//if ( script != null && script.Length > 0 )
	//	//	_name =script [0]._project ;
	//	//LMVNodesToUnity[] script2 =root.GetComponents<LMVNodesToUnity> () ;
	//	//if ( script2 != null && script2.Length > 0 )
	//	//	_name =script2 [0]._project ;

	//	GameObject root =Selection.activeGameObject ;
	//	_name =root == null ? "test" : root.name ;
	//	return (root) ;
	//}

	protected void OnGUI () {
		_name =EditorGUILayout.TextField ("Prefab Name", _name) ;
		_flattern =EditorGUILayout.ToggleLeft ("Flattern Hierarchy", _flattern) ;
		EditorGUILayout.Space () ;
		/*Rect r =*/EditorGUILayout.BeginHorizontal () ;
		if ( GUILayout.Button ("Save Prefab") )
			OnClickSavePrefab () ;
		if ( GUILayout.Button ("Cancel") )
			CloseDialog () ;
		EditorGUILayout.EndHorizontal () ;
	}

	void OnClickSavePrefab () {
		_name =_name.Trim () ;
		if ( string.IsNullOrEmpty (_name) ) {
			EditorUtility.DisplayDialog ("Unable to save prefab", "Please specify a valid prefab name.", "Close") ;
			return ;
		}

		GameObject root =Selection.activeGameObject ;

		if ( !_flattern ) {
			Debug.Log ("Creating prefab: " + _prefab) ;
			/*GameObject prefab =*/PrefabUtility.CreatePrefab (
				_prefab,
				Selection.activeGameObject,
				ReplacePrefabOptions.ConnectToPrefab
			) ;
		} else {
			Debug.Log ("Creating flattern'ed prefab: " + _prefab) ;
			// In case we want to flattern the hierarchy
			//GameObject clone =new GameObject ("clone_xx", typeof (HoloToolkit.Unity.GazeManager)) ;
			//clone.layer =LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE) ;
			//Transform [] transforms =root.GetComponentsInChildren<Transform> () ;
			//foreach ( Transform tr in transforms ) {
			//	if ( tr.gameObject == root )
			//		continue ;
			//	tr.parent =clone.transform ;
			//}
			GameObject clone =Instantiate<GameObject> (root) ;
			Transform [] transforms =clone.GetComponentsInChildren<Transform> () ;
			foreach ( Transform tr in transforms ) {
				if ( tr.gameObject == clone )
					continue ;
				tr.parent =clone.transform ;
			}
			/*GameObject prefab =*/PrefabUtility.CreatePrefab (
				_prefab,
				clone,
				ReplacePrefabOptions.ConnectToPrefab
			) ;
			DestroyImmediate (clone) ;
		}
		CloseDialog () ;
	}

	protected void CloseDialog () {
		Close () ;
		GUIUtility.ExitGUI () ;
	}

	[MenuItem("Forge/Build Prefab")]
	public static void BuildPrefab () {
		Hierarchy window =GetWindow (typeof (Hierarchy)) as Hierarchy ;
		window._name =Selection.activeGameObject.name ;
		window._flattern =false ;
        window.Show () ;
	}

	[MenuItem("Forge/Build Prefab", true)]
	public static bool ObjectSelectedValidation () {
		return (
			   Selection.activeGameObject != null
			&& Selection.activeGameObject.GetComponent<GazeManager> () != null
			&& Selection.activeGameObject.layer == LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE)
			&& Selection.activeGameObject.transform.childCount > 0
		) ;
	}

}

#endif

}
