using System.Collections;
using System.IO;
using System.Threading;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autodesk.Forge {

#if UNITY_EDITOR
class ImportBubbleWindow : EditorWindow {

	public string _name ="" ;
	public bool _autoScale =true ;
	protected static string _resourcesPath ="Assets/Resources/" ;
	protected static string _bundlePath ="Assets/Bundles/" ;

	protected string _folder =@"y:\Projects\Hololens\Samples\228114-gatehouse_pubnwd\" ;
	protected string[] _svf =new string[] { @"0\0.svf" } ;
	protected int _svfIndex =0 ;
	protected string _db =@"0\properties.db" ;

	//private Thread _svfThread ;
	//private volatile bool workInProgress =false ;
	//protected int _nodes =0 ;
	//protected int _nodesProcessed =0 ;

	[MenuItem ("Forge/Import Bubble")]
	public static void Init () {
		ImportBubbleWindow window =EditorWindow.GetWindowWithRect (
			typeof (ImportBubbleWindow),
			new Rect (0, 0, 640, 160)
		) as ImportBubbleWindow ;
		window.Show () ;
	}

	protected void OnGUI () {
		EditorGUILayout.Space () ;
		/*Rect r =*/EditorGUILayout.BeginHorizontal () ;
		_folder =EditorGUILayout.TextField ("Project", _folder) ;
		if ( GUILayout.Button ("...", GUILayout.Width (30)) )
			SelectNewProject () ;
		EditorGUILayout.EndHorizontal () ;

		EditorGUILayout.Space () ;
		_svfIndex =EditorGUILayout.Popup ("Bubble(s)", _svfIndex, _svf) ; 
		/*Rect r =*/EditorGUILayout.BeginHorizontal () ;
		_db =EditorGUILayout.TextField ("Property DB", _db) ;
		if ( GUILayout.Button ("...", GUILayout.Width (30)) )
			SelectNewDB () ;
		EditorGUILayout.EndHorizontal () ;

		EditorGUILayout.Space () ;
		_name =EditorGUILayout.TextField ("Project Name", _name) ;

		EditorGUILayout.Space () ;
		_autoScale =EditorGUILayout.ToggleLeft ("Auto Scale", _autoScale) ;

		EditorGUILayout.Space () ;
		/*Rect r =*/EditorGUILayout.BeginHorizontal () ;
		if ( GUILayout.Button ("Import") )
			ImportBubble () ;
		if ( GUILayout.Button ("Cancel") )
			CloseDialog () ;
		EditorGUILayout.EndHorizontal () ;

		//EditorGUI.ProgressBar (new Rect (4, 128, 630, 25), _nodes > 0 ? (float)_nodesProcessed / (float)_nodes : 0f, _nodesProcessed.ToString () + " / " + _nodes.ToString ()) ;
	}

	protected void ImportBubble () {
		_name =_name.Trim () ;
		if ( string.IsNullOrEmpty (_name) ) {
			EditorUtility.DisplayDialog ("Unable to import", "Please specify a valid project name.", "Close") ;
			return ;
		}
		ForgeImport.ProcessedNodes +=new ForgeImport.ProcessedNodesDelegate (ProcessedNodes) ;
		ForgeImport.CreateAssets (_name, _autoScale, _folder + _svf [_svfIndex], _folder + _db) ;

		EditorUtility.ClearProgressBar () ;
		this.Repaint () ;
		CloseDialog () ;
	}

	//protected void ImportBubble () {
	//	_name =_name.Trim () ;
	//	if ( string.IsNullOrEmpty (_name) ) {
	//		EditorUtility.DisplayDialog ("Unable to import", "Please specify a valid project name.", "Close") ;
	//		return ;
	//	}
	//	_svfThread =new Thread (ImportBubbleThread) { Name ="ImportBubbleThread" } ;
	//	_svfThread.Start () ;
	//}

	//private void ImportBubbleThread () {
	//	ForgeImport.ProcessedNodes +=new ForgeImport.ProcessedNodesDelegate (ProcessedNodes) ;
	//	ForgeImport.CreateAssets (_name, _folder + _svf [_svfIndex], _folder + _db) ;
	//}

	//protected void ImportProgress () {
	//	if ( _nodesProcessed < _nodes )
	//		EditorUtility.DisplayProgressBar ("Processing Bubble", " seconds", (float)_nodesProcessed / (float)_nodes) ;
	//	else
	//		EditorUtility.ClearProgressBar () ;
	//}

	public bool ProcessedNodes (BubbleStats stats) {
		//_nodesProcessed =processed ;
		//if ( _nodes != total )
		//	_nodes =total ;
		////this.Repaint () ;

		//EditorUtility.DisplayProgressBar (
		//	"Importing Bubble",
		//	processed.ToString () + " / " + total.ToString (),
		//	(float)processed / (float)total
		//) ;

		EditorUtility.DisplayProgressBar (
			"Importing Bubble",
			stats.msg,
			stats.percent
		) ;

		return (true) ;
	}

	protected void SelectNewProject () {
		string path =EditorUtility.OpenFolderPanel ("Load svf", _folder, "").Trim () ;
		if ( string.IsNullOrEmpty (path) )
			return ;
		_folder =path ;
		string[] files =System.IO.Directory.GetFiles (_folder, "*.svf", SearchOption.AllDirectories) ;
		for ( int i =0 ; i < files.Length ; i++ )
			files [i] =files [i].Trim ().Substring (_folder.Length) ;
		_svf =files ;
		files =System.IO.Directory.GetFiles (_folder, "*.db", SearchOption.AllDirectories) ;
		if ( !string.IsNullOrEmpty (files [0].Trim ()) )
			_db =files [0].Trim ().Substring (_folder.Length) ;
		this.Repaint () ;
	}

	protected void SelectNewDB () {
		string path =EditorUtility.OpenFilePanel ("Select DB", _folder, "db") ;
		if ( !string.IsNullOrEmpty (path.Trim ()) ) {
			if ( path.Trim ().Contains (_folder) )
				_db =path.Trim ().Substring (_folder.Length + 1) ;
			else
				_db =path.Trim () ;
			this.Repaint () ;
		}
	}

	protected void CloseDialog () {
		Close () ;
		GUIUtility.ExitGUI () ;
	}

	protected void OnInspectorUpdate () {
		this.Repaint () ;
    }

	[MenuItem ("Forge/Auto Scale")]
	public static void AutoScale () {
		ForgeImport.AutoScale (Selection.activeGameObject, true) ;
	}

	[MenuItem("Forge/Auto Scale", true)]
	private static bool AutoScaleValidation () {
		GameObject obj =Selection.activeGameObject ;
		return (obj != null) ;
	}


}

#endif

}
