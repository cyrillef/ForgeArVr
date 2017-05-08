using System.Collections;
using System.Runtime.InteropServices;
using System.IO;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using SimpleJSON;

namespace Autodesk.Forge {

#if UNITY_EDITOR

public struct BubbleStats {
	public int total { get; set; } //=0 ;
	public int nodesProcessed { get; set; } //=0 ;
	public int transforms { get; set; } //=0 ;
	public int meshes { get; set; } //=0 ;
	public int materials { get; set; } //=0 ;

	//public float percent { get { return ((float)_nodesProcessed / (float)_total) ; } }
	public float percent { get { return ((float)meshes / (float)total) ; } }
	public string msg { get {
		return (
			  transforms.ToString () + " transforms, "
			+ meshes.ToString () + " mesh, "
			+ materials.ToString () + " materials / "
			+ nodesProcessed.ToString () + " processed."
		) ;
	} }

	public void Reset (int totalMesh =0) {
		total =totalMesh ;
		nodesProcessed =0 ;
		transforms =0 ;
		meshes =0 ;
		materials =0 ;
	}

}

public partial class ForgeImport {

	#region LMVTK Imports
	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public static extern int loadSvf (string path, string dbpath) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public static extern void unloadSvf (string path) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
	public static extern void setCurrentSvf (string path) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int getCurrentSvfLength () ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void getCurrentSvf (System.Text.StringBuilder json, int len) ;

	[DllImport ("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int instanceTreeLength () ;

	[DllImport ("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void instanceTreeJson (System.Text.StringBuilder json, int len) ;

	[DllImport ("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int propertiesLength () ;

	[DllImport ("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void propertiesJson (System.Text.StringBuilder json, int len) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int meshesCount () ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int vertexCount (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr vertex (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int triCount (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr tri (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int uvCount (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern System.IntPtr uv (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int simpleMaterialLength (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void simpleMaterialJson (int id, System.Text.StringBuilder json, int len) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl)]
	public static extern int proteinMaterialLength (int id) ;

	[DllImport("UnityLMVTK", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void proteinMaterialJson (int id, System.Text.StringBuilder json, int len) ;

	#endregion
	
	public delegate bool ProcessedNodesDelegate (BubbleStats stats) ;
	public static event ProcessedNodesDelegate ProcessedNodes ;

	protected static string _project ="" ;
	public static string _resources { get { return (ForgeConstants._resourcesPath + _project) ; } }
	public static string _bundle { get { return (ForgeConstants._bundlePath + _project) ; } }
	protected static string _svfDir { get; set; }

	public static BubbleStats _stats =new BubbleStats () ;

	// Use this for initialization
	public static BubbleStats CreateAssets (string name, bool autoScale, string svf, string db) {
		_stats.Reset () ;

		_project =name.Trim () ;
		_svfDir =Path.GetDirectoryName (svf) ;
		FileUtil.DeleteFileOrDirectory (_resources) ;
		System.IO.Directory.CreateDirectory (_resources) ;

		// Check we got a Root object ready
		GameObject root =ForgeSceneInit.InitRoot () ;
		root.name =_project ;
		_stats.Reset (loadSvf (svf, db)) ;
		if ( _stats.total == 0 ) {
			GameObject temp =GameObject.CreatePrimitive (PrimitiveType.Cube) ;
			temp.transform.parent =root.transform ;
			return (_stats) ;
		}

		// Save Properties DB
		int len =propertiesLength () ;
		System.Text.StringBuilder sb =new System.Text.StringBuilder (len) ;
		propertiesJson (sb, len) ;
		string jsonSt =sb.ToString () ;
		File.WriteAllText (_bundle + "-properties.json", jsonSt) ;
		
		// Save Hierarchy
		len =instanceTreeLength () ;
		sb =new System.Text.StringBuilder (len) ;
		instanceTreeJson (sb, len) ;
		jsonSt =sb.ToString () ;
		File.WriteAllText (_bundle + ".json", jsonSt) ;

		// Import geometry & material
		JSONNode hierarchy =JSON.Parse (jsonSt) ;
		foreach ( JSONNode child in hierarchy ["childs"].AsArray )
			IteratorNodes (child, root) ;

		AutoScale (root, autoScale) ;

		// Create Menu item
		ForgeSceneInit.CreateMenuItem (_project, svf + ".png", _resources + "/") ;

		return (_stats) ;
	}

	public static GameObject AutoScale (GameObject root, bool autoScale) {
		// Auto Scale node
		GameObject autoScaleNode =new GameObject ("Auto Scale") ;
		for ( int i =0 ; i < root.transform.childCount && autoScale ; i++ )
			root.transform.GetChild (i).transform.parent =autoScaleNode.transform ;
		autoScaleNode.transform.parent =root.transform ;

		Bounds bounds =GameObjectBounds (root) ;
		float factor =1.0f / bounds.size.magnitude ;
		autoScaleNode.transform.localScale =new Vector3 (factor, factor, factor) ;
		
		bounds =GameObjectBounds (root) ;
		Vector3 tr =new Vector3 (
			-bounds.center.x - bounds.extents.x / 2,
			-bounds.center.y - bounds.extents.y / 2,
			-bounds.center.z - bounds.extents.z / 2 + 1.0f
		) ;
		autoScaleNode.transform.localPosition =tr ;

		return (autoScaleNode) ;
	}

	#region Transform / Mesh / Material generators
	protected static void IteratorNodes (JSONNode node, GameObject go) {
		_stats.nodesProcessed++ ;
		if ( ProcessedNodes != null )
			ProcessedNodes (_stats) ;		

		GameObject obj =null ;
		switch ( node ["type"] ) {
			case "Transform":
				string nodeName =buildName ("transform", 0, node ["id"].AsInt, node ["pathid"]) ;
				obj =new GameObject (nodeName) ;
				setTransform (node, obj) ;
				obj.transform.parent =go.transform ;
				_stats.transforms++ ;
				break ;
			case "Mesh":
				for ( int i =0 ; i < node ["fragmentEntryCount"].AsInt ; i++ ) {
					int index =node ["fragments"] [i].AsInt ;
					obj =CreateMeshObject (index, node ["id"].AsInt, node ["pathid"]) ;
					setTransform (node, obj) ;
					obj.transform.parent =go.transform ;
				}
				_stats.meshes++ ;
				break ;
			default:
				break ;
		}
		if ( obj != null && node ["childs"] != null ) {
			try {
				foreach ( JSONNode child in node ["childs"].AsArray )
					IteratorNodes (child, obj) ;
			} catch ( System.Exception ) {
			}
		}
	}

	public static void setTransform (JSONNode node, GameObject obj) {
		//Matrix4x4 mat =Matrix4x4.identity ;
		if ( node ["mtype"] != null ) {
			switch ( node ["mtype"] ) {
				case "Identity":
					break ;
				case "Translation":
					obj.transform.Translate (new Vector3 (node ["tr"] ["x"].AsFloat, node ["tr"] ["y"].AsFloat, node ["tr"] ["z"].AsFloat)) ;
					break ;
				case "RotationTranslation":
					obj.transform.Translate (new Vector3 (node ["tr"] ["x"].AsFloat, node ["tr"] ["y"].AsFloat, node ["tr"] ["z"].AsFloat)) ;
					obj.transform.rotation =new Quaternion (
						node ["rt"] ["a"].AsFloat, node ["rt"] ["b"].AsFloat, node ["rt"] ["c"].AsFloat,
						node ["rt"] ["d"].AsFloat
					) ;
					break ;
				case "UniformScaleRotationTranslation":
					obj.transform.Translate (new Vector3 (node ["tr"] ["x"].AsFloat, node ["tr"] ["y"].AsFloat, node ["tr"] ["z"].AsFloat)) ;
					obj.transform.localScale =new Vector3 (node ["scale"].AsFloat, node ["scale"].AsFloat, node ["scale"].AsFloat) ;
					obj.transform.rotation =new Quaternion (
						node ["rt"] ["a"].AsFloat, node ["rt"] ["b"].AsFloat, node ["rt"] ["c"].AsFloat,
						node ["rt"] ["d"].AsFloat
					) ;
					break ;
				case "AffineMatrix":
					Matrix4x4 mat =new Matrix4x4 () ;
					mat.m00 =node ["mt"] ["m00"].AsFloat ;
					mat.m01 =node ["mt"] ["m01"].AsFloat ;
					mat.m02 =node ["mt"] ["m02"].AsFloat ;
					mat.m03 =node ["mt"] ["m03"].AsFloat ;
					mat.m10 =node ["mt"] ["m10"].AsFloat ;
					mat.m11 =node ["mt"] ["m10"].AsFloat ;
					mat.m12 =node ["mt"] ["m12"].AsFloat ;
					mat.m13 =node ["mt"] ["m13"].AsFloat ;
					mat.m20 =node ["mt"] ["m20"].AsFloat ;
					mat.m21 =node ["mt"] ["m21"].AsFloat ;
					mat.m22 =node ["mt"] ["m22"].AsFloat ;
					mat.m23 =node ["mt"] ["m23"].AsFloat ;
					mat.m30 =node ["mt"] ["m30"].AsFloat ;
					mat.m31 =node ["mt"] ["m31"].AsFloat ;
					mat.m32 =node ["mt"] ["m32"].AsFloat ;
					mat.m33 =node ["mt"] ["m33"].AsFloat ;
					obj.transform.localScale =ScaleFromMatrix (mat) ;
					obj.transform.rotation =RotationFromMatrix (mat) ;
					obj.transform.position =TranslationFromMatrix (mat) ;
					break ;
			}
		}
	}

	protected static Vector3 TranslationFromMatrix (Matrix4x4 matrix) {
		Vector3 translate ;
		translate.x =matrix.m03 ;
		translate.y =matrix.m13 ;
		translate.z =matrix.m23 ;
		return (translate);
	}

	protected static Quaternion RotationFromMatrix (Matrix4x4 matrix) {
		Vector3 forward ;
		forward.x =matrix.m02 ;
		forward.y =matrix.m12 ;
		forward.z =matrix.m22 ;
 		Vector3 upwards ;
		upwards.x =matrix.m01 ;
		upwards.y =matrix.m11 ;
		upwards.z =matrix.m21 ;
 		return (Quaternion.LookRotation (forward, upwards)) ;
	}

	protected static Vector3 ScaleFromMatrix (Matrix4x4 matrix) {
		Vector3 scale =new Vector3 (
			matrix.GetColumn (0).magnitude,
			matrix.GetColumn (1).magnitude,
			matrix.GetColumn (2).magnitude
		) ;
		if ( Vector3.Cross (matrix.GetColumn (0), matrix.GetColumn (1)).normalized != (Vector3)matrix.GetColumn (2).normalized )
			scale.x *=-1 ;
		return (scale) ;
	}

	protected static GameObject CreateMeshObject (int id, int dbid, string pathid) {
		string nodeName =buildName ("mesh", id, dbid, pathid) ;
	
		int nbCoords =vertexCount (id) ;
		float[] coords =GetLMVFloatArray (vertex (id), nbCoords) ;
		Vector3[] vertices =new Vector3 [nbCoords / 3] ;
		for ( int i =0, ii =0 ; i < nbCoords ; i +=3, ii++ )
			vertices [ii] =new Vector3 (coords [i], coords [i + 1], coords [i + 2]) ;
		int nbTriangle =triCount (id) ;
		int[] tris =GetLMVIntArray (tri (id), nbTriangle) ;
		int nbUVs =uvCount (id) ;
		float[] uv_a =GetLMVFloatArray (uv (id), nbUVs) ;
		Vector2[] uvs =nbUVs != 0 ? new Vector2 [nbUVs / 2] : null ;
		for ( int i =0, ii =0 ; i < nbUVs ; i +=2, ii++ )
			uvs [ii] =new Vector2 (uv_a [i], uv_a [i + 1]) ;

		System.Text.StringBuilder sb =new System.Text.StringBuilder (simpleMaterialLength (id)) ;
		simpleMaterialJson (id, sb, sb.Capacity) ;
		string simpleMat =sb.ToString () ;
		sb =new System.Text.StringBuilder (proteinMaterialLength (id)) ;
		proteinMaterialJson (id, sb, sb.Capacity) ;
		string proteinMat =sb.ToString () ;

		Mesh mesh =new Mesh () ;
		mesh.vertices =vertices ;
		mesh.triangles =tris ;
		if ( nbUVs != 0 )
			mesh.uv =uvs ;
		mesh.RecalculateNormals () ;
		mesh.RecalculateBounds () ;

		//print (id + " - vertex: " + vertices.Length + " tri: " + tris.Length + " uvs: " + uvs.Length) ;

		GameObject obj =new GameObject (nodeName) ;
		MeshFilter filter =obj.AddComponent<MeshFilter> () ;
		MeshCollider collider =obj.AddComponent<MeshCollider> () ;
		MeshRenderer renderer =obj.AddComponent<MeshRenderer> () ;
		Material mat =CreateMaterial (id, simpleMat, proteinMat) ;

		AssetDatabase.CreateAsset (mesh, _resources + "/" + nodeName + ".asset") ;
		AssetDatabase.SaveAssets () ;
		AssetDatabase.Refresh () ;
		mesh =AssetDatabase.LoadAssetAtPath<Mesh> (_resources + "/" + nodeName + ".asset") ;

		filter.sharedMesh =mesh ;
		collider.sharedMesh =mesh ;
		renderer.sharedMaterial =mat ;
		return (obj) ;
	}

	#endregion

	#region Utilities
	//public static T[] GetLMVArray<T> (System.IntPtr ptr, int length) {
	//	if ( length == 0 || ptr == null )
	//		return (null) ;
	//	T[] arr =new T [length] ;
	//	Marshal.Copy (ptr, arr, 0, length) ;
	//	return (arr) ;
	//}

	public static int[] GetLMVIntArray (System.IntPtr ptr, int length) {
		if ( length == 0 || ptr == System.IntPtr.Zero )
			return (null) ;
		int[] arr =new int [length] ;
		Marshal.Copy (ptr, arr, 0, length) ;
		return (arr) ;
	}

	public static float[] GetLMVFloatArray (System.IntPtr ptr, int length) {
		if ( length == 0 || ptr == System.IntPtr.Zero )
			return (null) ;
		float[] arr =new float [length] ;
		Marshal.Copy (ptr, arr, 0, length) ;
		return (arr) ;
	}

	#endregion

	public enum BlendMode {
        Opaque =0,
        Cutout =1,
		Fade =2,
        Transparent =3
    } ;

	#region Simple Material
	protected static Material CreateMaterial (int id, string jsonSt, string proteinSt) {
		_stats.materials++ ;

		//File.WriteAllText ("Assets/Resources/" + _project + "/simple-" + id + ".json", jsonSt) ;
		//if ( proteinSt != "" )
		//	File.WriteAllText ("Assets/Resources/" + _project + "/protein-" + id + ".json", proteinSt) ;
		LMVMaterial lmvMat =new LMVMaterial (jsonSt, proteinSt) ;

		//Material mat =new Material (Shader.Find ("HoloToolkit/StandardFast")) ;
		Material mat =new Material (
			lmvMat.isMetal == true ?
				  Shader.Find ("Standard")
				: Shader.Find ("Standard (Specular setup)")
		) ;
		AssetDatabase.CreateAsset (mat, _resources + "/material" + id + ".mat") ;
		mat =AssetDatabase.LoadAssetAtPath<Material> (_resources + "/material" + id + ".mat") ;
		try {
			if ( lmvMat.specular_tex != null ) {
				mat.EnableKeyword ("_SPECULARHIGHLIGHTS_OFF") ;
				mat.SetFloat ("_SpecularHighlights", 0f) ;
			}
			//mat.DisableKeyword ("_SPECULARHIGHLIGHTS_OFF") ;
			//mat.SetFloat ("_SpecularHighlights", 1f) ;
			mat.EnableKeyword ("_GLOSSYREFLECTIONS_OFF") ;
			mat.SetFloat ("_GlossyReflections", 0f) ;

			var ambiant =lmvMat.ambient ;
			if ( ambiant != Color.clear )
				mat.SetColor ("_Color", ambiant) ;

			var diffuse =lmvMat.diffuse ;
			if ( diffuse != Color.clear )
				mat.SetColor ("_Color", diffuse) ;

			var emissive =lmvMat.emissive ;
			if ( emissive != Color.clear )
				mat.SetColor ("_EmissionColor", emissive) ;

			var specular =lmvMat.specular ;
			if ( specular != Color.clear )
				mat.SetColor ("_SpecColor", specular) ;


			var transparent =lmvMat.transparent ;
			if ( transparent ) {
				mat.SetFloat ("_Mode", (float)BlendMode.Transparent) ;
				mat.EnableKeyword ("_ALPHABLEND_ON") ;
				Color color =mat.GetColor ("_Color") ;
				color.a =lmvMat.transparency ;
				mat.SetColor ("_Color", color) ;
			}

			if ( lmvMat.diffuse_tex != null ) {
				CreateMaterialTex (id, lmvMat.diffuse_tex, "_MainTex", ref mat) ;
				mat.mainTextureScale =new Vector2 (lmvMat.diffuse_tex.u, lmvMat.diffuse_tex.v) ;
			}
			if ( lmvMat.specular_tex != null ) {
				CreateMaterialTex (id, lmvMat.specular_tex, "_SpecGlossMap", ref mat) ;
				//mat.SetFloat ("_Glossiness", lmvMat.specular_tex.u) ;
				mat.SetFloat ("_GlossMapScale", lmvMat.specular_tex.u) ;
			}
			if ( lmvMat.bump_tex != null ) {
				CreateMaterialTex (id, lmvMat.bump_tex, "_BumpMap", ref mat) ;
				mat.SetFloat ("_BumpScale", lmvMat.bump_tex.u) ;
			}
		} catch ( System.Exception e ) {
			Debug.Log ("exception " + e.Message) ;
			mat =GetDefaultMaterial () ;
		}
		return (mat) ;
	}

	protected static void CreateMaterialTex (int id, LMVTexture lmvtex, string shaderKey, ref Material mat) {
		try {
			byte [] bytes =File.ReadAllBytes (_svfDir + @"\" + lmvtex.tex.Replace ('/', '\\')) ;
			string filename =Path.GetFileName (lmvtex.tex) ;
			string assetref =_resources + "/" + filename ;
#if UNITY_EDITOR
			//FileStream file =File.Create (assetref) ;
			//file.Write (bytes, 0, bytes.Length) ;
			//file.Close () ;
			File.WriteAllBytes (assetref, bytes) ;
			AssetDatabase.Refresh () ;
#endif
			Texture2D tex =AssetDatabase.LoadAssetAtPath<Texture2D> (assetref) ;
			//tex.
			mat.SetTexture (shaderKey, tex) ;
			//mat.mainTexture =tex ;
			//mat.SetColor ("_Color", Color.black) ;
			//mat.SetTexture ("_MainTex", tex) ;
			//mat.SetTexture ("_SpecGlossMap", tex) ;
		} catch ( System.Exception /*e*/ ) {
		}
	}

	#endregion

	#region Default Material
	private static Material _defaultMaterial =null ;
	public static Material GetDefaultMaterial () {
		if ( _defaultMaterial == null ) {
			GameObject temp =GameObject.CreatePrimitive (PrimitiveType.Cube) ;
			MeshRenderer renderer =temp.GetComponent<MeshRenderer> () ;
			_defaultMaterial =renderer.material ;
			GameObject.DestroyObject (temp) ;
		}
		return (_defaultMaterial) ;
	}
	#endregion

}

#endif

public partial class ForgeImport {

	#region Utilities

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

	#endregion

}

}
