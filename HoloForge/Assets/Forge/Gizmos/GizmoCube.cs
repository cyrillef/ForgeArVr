using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Autodesk.Forge {

public class GizmoCube : MonoBehaviour {
	public Color _color =Color.green ;
	protected Bounds _bound ;

	public void Start () {
	}
	
	public void Update () {
	}

	protected void OnDrawGizmosSelected () {
		//if ( GetComponent<MeshFilter> () == null )
		//	return ;
		//Bounds b =GetComponent<MeshFilter> ().sharedMesh.bounds ;
		//Gizmos.color =_color ;
		//Gizmos.DrawWireCube (transform.position + b.center, 2 * b.extents) ;

		Bounds b =ForgeImport.GameObjectBounds (this.gameObject) ;
		Gizmos.color =_color ;
		Gizmos.DrawWireCube (b.center, 2 * b.extents) ;
	}

#if UNITY_EDITOR
	[MenuItem("Forge/Calc local Renderer BoundingBox")]
	public static void CalculateRendererBounds () {
		GameObject obj =Selection.activeGameObject ;
		Quaternion currentRotation =obj.transform.rotation ;
		obj.transform.rotation =Quaternion.Euler (0f, 0f, 0f) ;
		Bounds bounds =new Bounds (obj.transform.position, Vector3.zero) ;
		foreach ( Renderer renderer in obj.GetComponentsInChildren<Renderer> () )
			bounds.Encapsulate (renderer.bounds) ;
		Vector3 localCenter =bounds.center - obj.transform.position ;
		bounds.center =localCenter ;
		Debug.Log ("The local center of this model is " + localCenter) ;
		Debug.Log ("The local bounds of this model is " + bounds) ;
		obj.transform.rotation =currentRotation ;
	}

	[MenuItem("Forge/Calc local Renderer BoundingBox", true)]
	public static bool CalculateRendererBoundsValidation () {
		return (Selection.activeGameObject != null) ;
	}

	[MenuItem("Forge/Calc Collider BoundingBox")]
	public static void CalculateColliderBounds () {
		GameObject obj =Selection.activeGameObject ;
		Bounds bounds =new Bounds (obj.transform.position, Vector3.one) ;
		Renderer[] renderers =obj.GetComponentsInChildren<Renderer> () ;
		foreach ( Renderer renderer in renderers )
			bounds.Encapsulate (renderer.bounds) ;
		Debug.Log (bounds) ;
		bounds =new Bounds (obj.transform.position, Vector3.one) ;
		Collider[] colliders =obj.GetComponentsInChildren<Collider> () ;
		foreach ( Collider collider in colliders )
			bounds.Encapsulate (collider.bounds) ;
		Debug.Log ("The local bounds of this model is " + bounds) ;
	}
	
	[MenuItem("Forge/Calc Collider BoundingBox", true)]
	public static bool CalculateColliderBoundsValidation () {
		return (Selection.activeGameObject != null) ;
	}

	[MenuItem("Forge/Add Gizmo BoundingBox")]
    public static void GizmoCubeMenu () {
		Selection.activeGameObject.AddComponent<GizmoCube> () ;
    }

	[MenuItem("Forge/Add Gizmo BoundingBox", true)]
	private static bool GizmoCubeMenuValidation () {
		GameObject obj =Selection.activeGameObject ;
		return (obj != null && obj.GetComponent<GizmoCube> () == null) ;
	}

#endif

}

}