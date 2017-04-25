using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;
 
namespace Autodesk.Forge {

public class ScaleObject : Singleton<ScaleObject> {
	// Usage example
	// Scale object by a factor of 2 over 0.5 seconds
	// yield return StartCoroutine (ScaleObject.Instance.Scale (gameObject.transform, 2.0f, 0.5f, ScaleObject.ScaleType.Time)) ;

	public enum ScaleType { Time, Speed }

	void Awake () {
	}

	public IEnumerator ScaleTo (Transform thisTransform, float endScale, float value, ScaleType moveType) {
		yield return Scale (thisTransform, thisTransform.localScale, new Vector3 (endScale, endScale, endScale), value, moveType) ;
	}

	public IEnumerator Scale (Transform thisTransform, float byThatMuch, float value, ScaleType moveType) {
		yield return Scale (thisTransform, thisTransform.localScale, thisTransform.localScale * byThatMuch, value, moveType) ;
	}

	public IEnumerator Scale (Transform thisTransform, Vector3 startScale, Vector3 endScale, float value, ScaleType moveType) {
		Vector3 pos =thisTransform.position ;
		float rate =(moveType == ScaleType.Time) ? 1.0f / value : 1.0f / Vector3.Distance (startScale, endScale) * value ;
		float t =0.0f ;
		while ( t < 1.0 ) {
			t +=Time.deltaTime * rate ;
			thisTransform.localScale =Vector3.Lerp (startScale, endScale, Mathf.SmoothStep (0.0f, 1.0f, t)) ;
			thisTransform.position =pos ;
			yield return null ;
		}
	}

	public IEnumerator Rotation (Transform thisTransform, Vector3 degrees, float time) {
		Quaternion startRotation =thisTransform.rotation ;
		Quaternion endRotation =thisTransform.rotation * Quaternion.Euler (degrees) ;
		float rate =1.0f / time ;
		float t =0.0f;
		while ( t < 1.0f ) {
			t +=Time.deltaTime * rate ;
			thisTransform.rotation =Quaternion.Slerp (startRotation, endRotation, t) ;
			yield return null ;
		}
	}

}

}
