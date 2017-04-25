using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Windows.Speech;

using HoloToolkit.Unity;
using UnityEngine.UI;

using SimpleJSON;
using Autodesk.Forge;

public class SpeechManager : Singleton<SpeechManager> {
	private KeywordRecognizer _keywordRecognizer =null ;
	private readonly Dictionary<string, System.Action> _keywords =new Dictionary<string, System.Action> () ;
	public GameObject _holoMenu =null ;
	public GameObject _tooltip =null ;
	public GameObject _root =null ;
	protected Material _silhouetteMaterial =null ;
	public AudioClip _objectFocusChanged =null ;
	public AudioClip _operationInProgress =null ;
	protected Bounds _bounds ;
	protected Autodesk.Forge.Explode _explode =null ;

	void Start () {
		_keywords.Add ("Menu", ToggleMenuVisibility) ;
		_keywords.Add ("Load", LoadScene) ;
		// isolate
		_keywords.Add ("Isolate", Isolate) ;
		_keywords.Add ("Show this", Isolate) ;
		_keywords.Add ("Show All", ShowAll) ;
		// select
		_keywords.Add ("Select", Select) ;
		_keywords.Add ("Unselect", Unselect) ;
		_keywords.Add ("Parent", SelectParent) ;
		_keywords.Add ("Root", SelectRoot) ;
		// zoom
		_keywords.Add ("Zoom In", ZoomIn) ;
		_keywords.Add ("Bigger", ZoomIn) ;
		_keywords.Add ("Away", ZoomOut) ;
		_keywords.Add ("Zoom Out", ZoomOut) ;
		_keywords.Add ("Smaller", ZoomOut) ;
		_keywords.Add ("Scale Ten", ScaleTen) ;
		_keywords.Add ("Scale Twenty", ScaleTwenty) ;
		_keywords.Add ("Scale Thirty", ScaleThirty) ;
		_keywords.Add ("Scale Fourty", ScaleFourthy) ;
		// animate
		_keywords.Add ("Animate", Animate) ;
		_keywords.Add ("Rotate", Animate) ;
		_keywords.Add ("Stop", Stop) ;
		_keywords.Add ("Move", Move) ;
		_keywords.Add ("Place", Positionned) ;
		_keywords.Add ("Put", Positionned) ;
		_keywords.Add ("Floor", Floor) ;
		_keywords.Add ("Explode", RunExplode) ;
		_keywords.Add ("Combine", Combine) ;
		_keywords.Add ("Reset", Reset) ;
			_keywords.Add ("Properties", Properties) ;

		_keywordRecognizer =new KeywordRecognizer (_keywords.Keys.ToArray ()) ;
		_keywordRecognizer.OnPhraseRecognized +=KeywordRecognizer_OnPhraseRecognized ;
		_keywordRecognizer.Start () ;

		if ( _root == null )
			_root =GameObject.Find ("/Root") ;
		if ( _tooltip == null )
			_tooltip =GameObject.Find ("/Tooltip") ;
		ToggleMenuVisibility () ; // Hide Menu when we start

		_silhouetteMaterial =Resources.Load<Material> ("ElementSelection") ;

		_explode =new Explode (_root) ;
	}
	
	void Update () {
		if ( _rotationActivated )
			_root.transform.RotateAround (_bounds.center, Vector3.up, _RotationSpeed / 100f) ;
//		if ( _explodeActivated && _explode._scale != _explodeTarget )
//			_explode.explode (_explode._scale + _explodeSpeed) ;
	}

	void OnDestroy () {
		if ( _keywordRecognizer != null ) {
			_keywordRecognizer.Stop () ;
			_keywordRecognizer.OnPhraseRecognized -=KeywordRecognizer_OnPhraseRecognized ;
			_keywordRecognizer.Dispose () ;
		}
	}

	protected void KeywordRecognizer_OnPhraseRecognized (PhraseRecognizedEventArgs args) {
		SetMenuText (args.text) ;
		System.Action keywordAction ;
		if ( _keywords.TryGetValue (args.text, out keywordAction) )
			keywordAction.Invoke () ;
	}

	protected void SetMenuText (string text) {
		GameObject obj =_holoMenu.transform.FindChild ("MenuText").gameObject ;
		TextMesh txt =obj.GetComponent<TextMesh> () ;
		txt.text =text ;
	}

	public void ToggleMenuVisibility () {
		if ( _holoMenu == null )
			 _holoMenu =GameObject.Find ("/HoloMenu") ;
		//var rend =_holoMenu.GetComponent<Renderer> () ;
		//if ( rend != null )
		//	rend.enabled =_menuVisible ;
		//_holoMenu.SetActive (!_holoMenu.activeSelf) ;
		_holoMenu.SetActive (!_holoMenu.activeSelf) ;
		//_holoMenu.transform.localScale =Vector3.one * 0.3f ;
		_holoMenu.transform.position =Camera.main.transform.position + Camera.main.transform.forward ;

		//_root.SetActive (!_root.activeSelf) ;
	}

	public void LoadScene () {
		GameObject menuItem =ForgeInteractions.Instance ? ForgeInteractions.Instance.FocusedGameObject : null ;
		if ( menuItem == null )
			return ;
		string sceneName =menuItem.name ;
		UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single) ;
	}

	protected GameObject _isolatedComp =null ;
	//protected Transform _isolatedCompParent =null ;
	//public void Isolate () {
	//	if ( _isolatedComp != null )
	//		ShowAll () ;
	//	_isolatedComp =ForgeInteractions.Instance ? ForgeInteractions.Instance.FocusedGameObject : null ;
	//	if ( _isolatedComp == null )
	//		_isolatedComp =GameObject.Find ("/CraneHook3/transform-2-0-0/transform-3-0_0-0/transform-11-0_0_1-0/transform-14-0_0_1_1-0") ;
	//	_root.SetActive (false);
	//	_isolatedCompParent = _isolatedComp.transform.parent;
	//	_isolatedComp.SetActive (true);
	//	_isolatedComp.transform.parent = GameObject.Find ("Managers").transform;
	//}
	//
	//public void ShowAll () {
	//	if ( _isolatedComp == null )
	//		return ;
	//	_isolatedComp.transform.parent =_isolatedCompParent ;
	//	_isolatedComp =null ;
	//	_root.SetActive (true) ;
	//}

	public void Isolate () {
		if ( _isolatedComp != null )
			ShowAll () ;
		_isolatedComp =ForgeInteractions.Instance ? ForgeInteractions.Instance.FocusedGameObject : null ;
		foreach ( Renderer rd in _root.GetComponentsInChildren<Renderer> (true) )
			rd.enabled =false ;
		foreach ( Renderer rd in _isolatedComp.GetComponentsInChildren<Renderer> (true) )
			rd.enabled =true ;
	}

	public void ShowAll () {
		if ( _isolatedComp == null )
			return ;
		Renderer[] rds=_root.GetComponentsInChildren<Renderer> (true) ;
		foreach ( Renderer rd in rds )
			rd.enabled =true ;
		_isolatedComp =null ;
	}

	public void Properties () {
	}

	protected GameObject _selectedObject =null ;
	protected void SelectMain () {
		//RaycastHit hit =GazeManager.Instance.HitInfo ;
		//if ( hit.collider != null ) // Got a mesh, move to the parent transform to get the entire object vs fragment
		//	_selectedObject =hit.collider.gameObject.transform.parent.gameObject ;
		_selectedObject =ForgeInteractions.Instance ? ForgeInteractions.Instance.FocusedGameObject : null ;
		if ( _selectedObject == null )
			return ;
		_selectedObject =_selectedObject.transform.parent.gameObject ;
	}

	public void Select () {
		SelectMain () ;
		HighlightSelected () ;
	}

	public void Unselect () {
		ClearSelection (true) ;
	}

	public void SelectParent () {
		if ( _selectedObject == null )
			SelectMain () ;
		if ( _selectedObject != _root )
			_selectedObject =_selectedObject.transform.parent.gameObject ;
		HighlightSelected () ;
	}

	public void SelectRoot () {
		_selectedObject =_root ;
		HighlightSelected () ;
	}

	public void HighlightSelected () {
		HighlightSelection (_selectedObject) ;
	}

	public void HighlightSelection (GameObject selectedObject =null) {
		ClearSelection (false) ;
		if ( selectedObject == null )
			return ;
		MeshRenderer[] rds =selectedObject.GetComponentsInChildren<MeshRenderer> () ;
		foreach ( MeshRenderer rd in rds )
			rd.materials =new Material [2] { rd.material, _silhouetteMaterial } ;
			//rd.material =_silhouetteMaterial ;
	}

	public void ClearSelection (bool clearRef =true) {
		MeshRenderer[] rds =_root.GetComponentsInChildren<MeshRenderer> () ;
		foreach ( MeshRenderer rd in rds ) {
			if ( rd.materials.Length > 1 )
				rd.materials =new Material [1] { rd.materials [0] } ;
		}
		if ( clearRef )
			_selectedObject =null ;
	}

	public void ZoomIn () {
		StartCoroutine (/*ScaleObject.Instance.*/Scale (_root.transform, 2.0f, 1f, ScaleObject.ScaleType.Time)) ;
	}

	public void ScaleTen () {
		StartCoroutine (/*ScaleObject.Instance.*/Scale (_root.transform, 10.0f, 1f, ScaleObject.ScaleType.Time)) ;
	}

	public void ScaleTwenty () {
		StartCoroutine (/*ScaleObject.Instance.*/Scale (_root.transform, 20.0f, 1f, ScaleObject.ScaleType.Time)) ;
	}

	public void ScaleThirty () {
		StartCoroutine (/*ScaleObject.Instance.*/Scale (_root.transform, 30.0f, 1f, ScaleObject.ScaleType.Time)) ;
	}

	public void ScaleFourthy () {
		StartCoroutine (/*ScaleObject.Instance.*/Scale (_root.transform, 40.0f, 1f, ScaleObject.ScaleType.Time)) ;
	}

	public void ZoomOut () {
		StartCoroutine (/*ScaleObject.Instance.*/Scale (_root.transform, .5f, 1f, ScaleObject.ScaleType.Time)) ;
	}

	public IEnumerator Scale (Transform thisTransform, float byThatMuch, float value, ScaleObject.ScaleType moveType) {
		_bounds =GameObjectBounds (thisTransform.gameObject) ;
		yield return Scale (thisTransform, thisTransform.localScale, thisTransform.localScale * byThatMuch, value, moveType) ;

	}

	public IEnumerator Scale (Transform thisTransform, Vector3 startScale, Vector3 endScale, float value, ScaleObject.ScaleType moveType) {
		float rate =(moveType == ScaleObject.ScaleType.Time) ? 1.0f / value : 1.0f / Vector3.Distance (startScale, endScale) * value ;
		float t =0.0f ;
		while ( t < 1.0 ) {
			t +=Time.deltaTime * rate ;
			thisTransform.localScale =Vector3.Lerp (startScale, endScale, Mathf.SmoothStep (0.0f, 1.0f, t)) ;
			Bounds bounds =GameObjectBounds (thisTransform.gameObject) ;
			thisTransform.position +=(_bounds.center - bounds.center) ;
			yield return null ;
		}
	}

	//protected Coroutine _rotationCoR =null ;
	protected bool _rotationActivated =false ;
	protected static float _RotationSpeed =80f ;
	public void Animate () {
		_bounds =GameObjectBounds (_root) ;
		_rotationActivated =true ;
		//if ( _rotationCoR != null )
		//	return ;
		//_rotationCoR =StartCoroutine (RotateObject.Instance.Rotation (_root.transform, new Vector3 (0f, 180f, 0f), 5f)) ;
		////_rotationCoR =StartCoroutine (RotateObject.Instance.Rotation (_root.transform, new Vector3 (0f, 0f, 0f), 5f)) ;
	}

	public void Stop () {
		_rotationActivated =false ;
		//if ( _rotationCoR != null )
		//	StopCoroutine (_rotationCoR) ;
		//_rotationCoR =null ;
	}

	public void Move () {
		Vector3 position =Camera.main.transform.position + Camera.main.transform.forward ;
		_root.transform.localPosition =position ;
	}

	public void Positionned () {
		Vector3 position =Camera.main.transform.position + Camera.main.transform.forward ;
		_selectedObject.transform.parent =null ;
		_selectedObject.transform.localPosition =position ;
	}

	public void Floor () {
		Bounds bounds =GameObjectBounds (_root) ;
		Vector3 bottom =bounds.center - new Vector3 (0f, bounds.size.y / 2f, 0f) ;
		float offset =-1.5f - bottom.y ;
		_root.transform.localPosition +=new Vector3 (0f, offset, 0f) ;

		//_root.transform.localPosition =new Vector3 (0f, -1.5f, 0f)
		//	+ new Vector3 (0f, bounds.extents.y, 0f) ;
		//_root.transform.localPosition +=new Vector3 (
		//	0f, 
		//	_root.transform.localPosition.y - 1.5f + bounds.extents.y,
		//	0f
		//) ;
	}

	protected bool _explodeActivated =false ;
	protected static float _explodeSpeed =80f ;
	protected static float _explodeTarget =1f ;
	public void RunExplode () {
		//if ( _explodeActivated )
		//	return ;
		_explodeTarget =1f ;
		_explodeActivated =true ;
		_explode.explode (0.75f) ;
		
		//StartCoroutine (AnimationNumber.Instance.Animate (_explode, _explode._scale, 1f, 5f)) ;
	}

	public void Combine () {
		_explode.explode (0f) ;
		//_explodeTarget =0f ;
		_explodeActivated =false ;
		//StartCoroutine (AnimationNumber.Instance.Animate (_explode, _explode._scale, 0f, 5f)) ;
	}

	public void Reset () {
		/* zoom */		StartCoroutine (ScaleObject.Instance.ScaleTo (_root.transform, 1f, 1f, ScaleObject.ScaleType.Time)) ;
		/* isolate */	ShowAll () ;
		/* select */	ClearSelection () ;
		/* animate */	_root.transform.position =Vector3.zero ;
						_root.transform.rotation =Quaternion.identity ;
		/*explode*/		Combine () ;
	}

	public static Vector3 calculateTooltipPosition (Camera camera, RaycastHit info) {
		float distance =Mathf.Min (.8f, .9f * info.distance) ;
		Vector3 position =Camera.main.transform.position + distance * Camera.main.transform.forward ;
		Vector3 x =camera.transform.right ;
		position +=x * .05f ;
		return (position) ;
	}

	public void GazeEntered (GameObject value) {
		_tooltip.transform.position =SpeechManager.calculateTooltipPosition (
			Camera.main,
			GazeManager.Instance.HitInfo
		) ;
		//_tooltip.transform.position =Camera.main.transform.position + .8f * Camera.main.transform.forward ;
		_tooltip.SetActive (true) ;
		_tooltip.GetComponent<Tooltip> ()._userMessage.text =value.name ;

		int index =0, dbid =0 ;
		string pathid ="" ;
		decodeName (value.name, ref index, ref dbid, ref pathid) ;
		GameObject root =value.GetComponent<Collider> ().transform.root.gameObject ; // should be _root for now
		TextAsset text =Resources.Load<TextAsset> (root.name + "-properties") ;
		if ( text == null || text.text == "" )
			return ;
		JSONNode json =JSON.Parse (text.text) ;
		JSONArray arr =json ["data"] ["collection"].AsArray ;
		foreach ( JSONNode obj in arr ) {
			if ( obj ["objectid"].AsInt == dbid ) {
				_tooltip.SetActive (true) ;
				_tooltip.GetComponent<Tooltip> ()._userMessage.text =obj ["name"] ;
				//_tooltip.GetComponent<Tooltip> ().VoiceCommandHeard () ;
				break ;
			}
		}
    }

    public void GazeExited (GameObject value) {
		_tooltip.SetActive (false) ;
    }

	public static string buildName (string tp, int index, int dbid, string pathid) {
		return (tp + "-" + dbid.ToString () + "-" + pathid.Replace (":", "_") + "-" + index.ToString ()) ;
	}

	public static string decodeName (string name, ref int index, ref int dbid, ref string pathid) {
		string[] arr =name.Split (new char [1] { '-' }) ;
		dbid =System.Convert.ToInt32 (arr [1]) ;
		pathid =arr [2].Replace ("_", ":") ;
		index =System.Convert.ToInt32 (arr [3]) ;
		return (arr [0]) ;
	}

	public static Bounds GameObjectBounds (GameObject obj) {
		Renderer[] renderers =obj.GetComponentsInChildren<Renderer> () ;
		Bounds bounds =new Bounds (renderers [0].bounds.center, renderers [0].bounds.size) ;
		foreach ( Renderer renderer in renderers )
			bounds.Encapsulate (renderer.bounds) ;
		return (bounds) ;
	}

}