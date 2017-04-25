using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

namespace Autodesk.Forge {

public class RotateObject : Singleton<RotateObject> {
	// Usage example
	// Rotate object by 180 degress around Y in 5 seconds
	// yield return StartCoroutine (RotateObject.Instance.Rotation (gameObject.transform, new Vector3 (0f, 180f, 0f), 5f)) ;

	public enum RotateAxis { X, Y, Z, Free }

	void Awake () {
	}

	public IEnumerator Rotation (Transform thisTransform, Vector3 degrees, float time) {
		Quaternion startRotation =thisTransform.rotation ;
		Quaternion endRotation =thisTransform.rotation * Quaternion.Euler (degrees) ;
		float rate =1.0f / time ;
		float t =0.0f ;
		while ( t < 1.0f ) {
			t +=Time.deltaTime * rate ;
			thisTransform.rotation =Quaternion.Slerp (startRotation, endRotation, t) ;
			yield return null ;
		}
	}

}

}