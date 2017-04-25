using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;

namespace Autodesk.Forge {

public class MoveObject : Singleton<MoveObject> {
	// Usage example
	// Move object up one unit over 0.5 seconds
	// yield return StartCoroutine (MoveObject.Instance.Translation (gameObject.transform, Vector3.up, 0.5f, MoveObject.MoveType.Time)) ;

	public enum MoveType { Time, Speed }

	void Awake () {
	}

	public IEnumerator TranslateTo (Transform thisTransform, Vector3 endPos, float value, MoveType moveType) {
		yield return Translation (thisTransform, thisTransform.position, endPos, value, moveType) ;
	}

	public IEnumerator Translation (Transform thisTransform, Vector3 endPos, float value, MoveType moveType) {
		yield return Translation (thisTransform, thisTransform.position, thisTransform.position + endPos, value, moveType) ;
	}

	public IEnumerator Translation (Transform thisTransform, Vector3 startPos, Vector3 endPos, float value, MoveType moveType) {
		float rate =(moveType == MoveType.Time) ? 1.0f / value : 1.0f / Vector3.Distance (startPos, endPos) * value ;
		float t =0.0f ;
		while ( t < 1.0 ) {
			t +=Time.deltaTime * rate ;
			thisTransform.position =Vector3.Lerp (startPos, endPos, Mathf.SmoothStep (0.0f, 1.0f, t)) ;
			yield return null ;
		}
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
