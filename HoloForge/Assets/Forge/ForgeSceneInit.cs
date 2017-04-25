using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using HoloToolkit.Unity;

namespace Autodesk.Forge {

public class ForgeSceneInit /*: EditorWindow*/ {

#if UNITY_EDITOR
	[MenuItem("Forge/Init")]
	protected static void InitScene () {
		InitCamera () ;
		GameObject root =InitRoot () ;
		GameObject tp =InitTooltip () ;
		GameObject menu =InitMenu () ;
		InitManagers (root, tp, menu) ;

		int interactibleLayer =LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE) ;
        if ( interactibleLayer == -1 )
            throw new UnassignedReferenceException ("Layer " + ForgeConstants.INTERACTIBLE + " must be assigned in Layer Manager.") ;
	}

	protected static Camera InitCamera () {
		//Scene scene =EditorSceneManager.GetActiveScene () ;
		if ( Camera.main != null && Camera.main.name != "Forge Camera" )
			GameObject.DestroyImmediate (Camera.main) ;
		if ( Camera.main == null ) {
			string path ="Assets/Toolkit/Utilities/Prefabs/Main Camera.prefab" ;
			Object obj =AssetDatabase.LoadAssetAtPath<GameObject> (path) as Object ;
			GameObject cam =GameObject.Instantiate (obj, Vector3.zero, Quaternion.identity) as GameObject ;
			cam.name ="Forge Camera" ;
		}
		Camera.main.nearClipPlane =0.1f ;
		Camera.main.farClipPlane =5000f ;
		return (Camera.main) ;
	}

#endif
				
	public static GameObject InitRoot () {
		GameObject root =GameObject.Find  (ForgeConstants.ROOTPATH) ;
		if ( root == null ) {
			root =new GameObject (ForgeConstants.ROOT, typeof (HoloToolkit.Unity.GazeManager)) ;
			root.layer =LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE) ;
		}
		return (root) ;
	}

#if UNITY_EDITOR
	protected static GameObject InitTooltip () {
		GameObject tp =GameObject.Find  (ForgeConstants.TOOLTIPPATH) ;
		if ( tp == null ) {
			tp =new GameObject (ForgeConstants.TOOLTIP, typeof (Billboard)) ;
			tp.layer =LayerMask.NameToLayer (ForgeConstants.MENUITEMS) ;

			Billboard billboard =tp.GetComponent<Billboard> () as Billboard ;
			billboard.PivotAxis =PivotAxis.Free ;

			Tooltip tooltipScript =tp.AddComponent<Tooltip> () ;
			
			GameObject canvasObj =new GameObject (ForgeConstants.TOOLTIP + " Canvas") ;
			canvasObj.transform.parent =tp.transform ;
			canvasObj.layer =LayerMask.NameToLayer (ForgeConstants.MENUITEMS) ;
			Canvas canvas =canvasObj.AddComponent<Canvas> () ;
			canvas.renderMode =RenderMode.WorldSpace ;
			canvas.additionalShaderChannels =
				  AdditionalCanvasShaderChannels.TexCoord1
				| AdditionalCanvasShaderChannels.Normal
				| AdditionalCanvasShaderChannels.Tangent
			;
			RectTransform rect =canvasObj.GetComponent<RectTransform> () ;
			rect.sizeDelta =new Vector2 (400f, 100f) ;
			rect.localScale =new Vector3 (0.0005f, 0.0005f, 0.0005f) ;
			/*CanvasScaler cs =*/canvasObj.AddComponent<CanvasScaler> () ;
			/*GraphicRaycaster gr =*/canvasObj.AddComponent<GraphicRaycaster> () ;

			GameObject tooltipTextObj =new GameObject (ForgeConstants.TOOLTIP + " Text") ;
			tooltipTextObj.transform.parent =canvasObj.transform ;
			tooltipTextObj.layer =LayerMask.NameToLayer (ForgeConstants.MENUITEMS) ;
			rect =tooltipTextObj.AddComponent<RectTransform> () ;
			rect.anchoredPosition3D =new Vector3 (10f, -6f, 0f) ;
			rect.sizeDelta =new Vector2 (128f, 30f) ;
			rect.localScale =new Vector3 (1f, 1f, 1f) ;
			tooltipTextObj.AddComponent<CanvasRenderer> () ;
			Text tooltipText =tooltipTextObj.AddComponent<Text> () ;
			tooltipText.text ="tooltip" ;
			Color color ;
			if ( ColorUtility.TryParseHtmlString ("#E71313FF", out color) )
				tooltipText.color =color ;

			GameObject tooltipImgObj =new GameObject (ForgeConstants.TOOLTIP + " BG") ;
			tooltipImgObj.transform.parent =canvasObj.transform ;
			tooltipImgObj.layer =LayerMask.NameToLayer (ForgeConstants.MENUITEMS) ;
			rect =tooltipImgObj.AddComponent<RectTransform> () ;
			rect.sizeDelta =new Vector2 (128f, 30f) ;
			rect.localScale =new Vector3 (1f, 1f, 1f) ;
			tooltipImgObj.AddComponent<CanvasRenderer> () ;
			Image tooltipImg =tooltipImgObj.AddComponent<Image> () ;
			if ( ColorUtility.TryParseHtmlString ("#6B1ECAFF", out color) )
				tooltipImg.color =color ;
			Sprite sprite =AssetDatabase.LoadAssetAtPath<Sprite> ("Assets/Resources/Tooltip/tooltip_bg_white.png") ;
			tooltipImg.sprite =sprite ;

			tooltipScript._userMessage =tooltipText ;
		}
		return (tp) ;
	}

	protected static GameObject InitMenu () {
		GameObject menu =GameObject.Find  (ForgeConstants.MENUPATH) ;
		if ( menu == null ) {
			menu =new GameObject (ForgeConstants.MENU, typeof (Billboard)) ;
			menu.layer =LayerMask.NameToLayer (ForgeConstants.MENUITEMS) ;

			Billboard billboard =menu.GetComponent<Billboard> () as Billboard ;
			billboard.PivotAxis =PivotAxis.Y ;

			GameObject textObj =new GameObject (ForgeConstants.MENU + " Text", typeof (MeshRenderer)) ;
			textObj.transform.parent =menu.transform ;
			textObj.transform.localScale =new Vector3 (.01f, .01f, .01f) ;
			textObj.transform.localPosition =new Vector3 (0f, .07f, 0f) ;

			TextMesh text =textObj.AddComponent<TextMesh> () ;
			text.text ="Hello World !" ;
			text.color =Color.white ;
			text.tabSize =4 ;
			text.richText =true ;
			text.alignment =TextAlignment.Left ;
		}
		return (menu) ;
	}

	protected static GameObject InitManagers (GameObject root, GameObject tp, GameObject menu) {
		GameObject mgr =GameObject.Find  (ForgeConstants.MGRPATH) ;
		if ( mgr == null ) {
			mgr =new GameObject (ForgeConstants.MGR, typeof (HoloToolkit.Unity.GazeManager)) ;
			mgr.layer =LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE) ;
		}
		ForgeInteractions interactions =null ;
		if ( mgr.GetComponent<ForgeInteractions> () == null ) {
			interactions =mgr.AddComponent<ForgeInteractions> () ;
			interactions._msgManager =mgr ;
		}
		if ( mgr.GetComponent<RotateObject> () == null )
			mgr.AddComponent<RotateObject> () ;
		if ( mgr.GetComponent<MoveObject> () == null )
			mgr.AddComponent<MoveObject> () ;
		if ( mgr.GetComponent<ScaleObject> () == null )
			mgr.AddComponent<ScaleObject> () ;

		ForgeSpeechManager speech =null ;
		if ( mgr.GetComponent<ForgeSpeechManager> () == null ) {
			speech =mgr.AddComponent<ForgeSpeechManager> () ;
			speech._root =root ;
			speech._tooltip =tp ;
			speech._holoMenu =menu ;
		}

		return (mgr) ;
	}

	[MenuItem("Forge/Init", true)]
	public static bool InitSceneValidation () {
		return (
			   Camera.main == null
			|| LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE) == -1
			|| GameObject.Find  (ForgeConstants.ROOTPATH) == null
			|| GameObject.Find  (ForgeConstants.MGRPATH) == null
		) ;
	}
#endif

}

#if UNITY_EDITOR
[InitializeOnLoad]
public class Tags {
 
	// STARTUP
	static Tags () {
		CheckLayers (new string [3] {
			ForgeConstants.INTERACTIBLE,
			ForgeConstants.ENVIRONEMENT,
			ForgeConstants.MENUITEMS
		}) ;
	}

	public static void CheckTags (string [] tagNames) {
		SerializedObject manager =new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]) ;
		SerializedProperty tagsProp =manager.FindProperty ("tags") ;
		List<string> DefaultTags =new List<string> () { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" } ;
		foreach ( string name in tagNames ) {
			if ( DefaultTags.Contains (name) )
					continue; 
			// check if tag is present
			bool found =false ;
			for ( int i =0 ; i < tagsProp.arraySize ; i++ ) {
				SerializedProperty t =tagsProp.GetArrayElementAtIndex (i) ;
				if ( t.stringValue.Equals (name) ) {
					found =true ;
					break ;
				}
			}
			// if not found, add it
			if ( !found ) {
				tagsProp.InsertArrayElementAtIndex (0) ;
				SerializedProperty n =tagsProp.GetArrayElementAtIndex (0) ;
				n.stringValue =name ;
			}
		}
		// save
		manager.ApplyModifiedProperties () ;
	}

	public static void CheckLayers (string [] layerNames) {
		SerializedObject manager =new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]) ;
		SerializedProperty layersProp =manager.FindProperty ("layers") ;
		foreach ( string name in layerNames ) {
			// check if layer is present
			bool found =false ;
			for ( int i =0 ; i <= 31 ; i++ ) {
				SerializedProperty sp =layersProp.GetArrayElementAtIndex (i) ;
				if ( sp != null && name.Equals (sp.stringValue) ) {
					found =true ;
					break ;
				}
			}
			// not found, add into 1st open slot
			if ( !found ) {
				SerializedProperty slot = null;
				for ( int i = 8 ; i <= 31 ; i++ ) {
					SerializedProperty sp =layersProp.GetArrayElementAtIndex (i) ;
					if ( sp != null && string.IsNullOrEmpty (sp.stringValue) ) {
						slot =sp ;
						break ;
					}
				}
				if ( slot != null )
					slot.stringValue =name ;
				else
					Debug.LogError ("Could not find an open Layer Slot for: " + name) ;
			}
		}
		// save
		manager.ApplyModifiedProperties () ;
	}

	public static void CheckSortLayers (string [] layerNames) {
		SerializedObject manager =new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/TagManager.asset") [0]) ;
		SerializedProperty sortLayersProp =manager.FindProperty ("m_SortingLayers") ;

		//for ( int i =0 ; i < sortLayersProp.arraySize; i++ ) {
		//	// used to figure out how all of this works and what properties values look like
		//	SerializedProperty entry =sortLayersProp.GetArrayElementAtIndex (i) ;
		//	SerializedProperty name =entry.FindPropertyRelative ("name") ;
		//	SerializedProperty unique =entry.FindPropertyRelative ("uniqueID") ;
		//	SerializedProperty locked =entry.FindPropertyRelative ("locked") ;
		//	Debug.Log(name.stringValue + " => " + unique.intValue + " => " + locked.boolValue) ;
		//}

		foreach ( string name in layerNames ) {
			// check if tag is present
			bool found =false ;
			for ( int i =0 ; i < sortLayersProp.arraySize ; i++ ) {
				SerializedProperty entry =sortLayersProp.GetArrayElementAtIndex (i) ;
				SerializedProperty t =entry.FindPropertyRelative ("name") ;
				if ( t.stringValue.Equals (name) ) {
					found =true ;
					break ;
				}
			}
			// if not found, add it
			if ( !found ) {
				manager.ApplyModifiedProperties () ;
				AddSortingLayer () ;
				manager.Update () ;
				int idx =sortLayersProp.arraySize - 1 ;
				SerializedProperty entry =sortLayersProp.GetArrayElementAtIndex (idx) ;
				SerializedProperty t =entry.FindPropertyRelative ("name") ;
				t.stringValue =name ;
			}
		}
		// save
		manager.ApplyModifiedProperties () ;
	}

	// you need 'using System.Reflection;' for these
	private static Assembly editorAsm ;
	private static MethodInfo AddSortingLayer_Method ;

	public static void AddSortingLayer () {
		if ( AddSortingLayer_Method == null ) {
			if ( editorAsm == null )
				editorAsm =Assembly.GetAssembly (typeof (Editor)) ;
			System.Type t =editorAsm.GetType ("UnityEditorInternal.InternalEditorUtility") ;
			AddSortingLayer_Method =t.GetMethod ("AddSortingLayer", (BindingFlags.Static | BindingFlags.NonPublic), null, new System.Type [0], null) ;
		}
		AddSortingLayer_Method.Invoke (null, null) ;
	}

}

#endif

}
