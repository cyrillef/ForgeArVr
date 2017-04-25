using UnityEngine;

namespace HoloToolkit.Unity {
	/// <summary>
	/// The Interactible class flags a Game Object as being "Interactible".
	/// Determines what happens when an Interactible is being gazed at.
	/// </summary>
	public class Interactible : MonoBehaviour {
		[Tooltip ("Audio clip to play when interacting with this hologram.")]
		public AudioClip _TargetFeedbackSound ;
		private AudioSource _audioSource ;
		private Material [] _defaultMaterials ;

		void Start () {
			_defaultMaterials =GetComponent<Renderer> ().materials ;
			// Add a BoxCollider if the interactible does not contain one.
			Collider collider =GetComponentInChildren<Collider> () ;
			if ( collider == null )
				gameObject.AddComponent<BoxCollider> () ;
			EnableAudioHapticFeedback () ;
		}

		private void EnableAudioHapticFeedback () {
			// If this hologram has an audio clip, add an AudioSource with this clip.
			if ( _TargetFeedbackSound != null ) {
				_audioSource =GetComponent<AudioSource> () ;
				if ( _audioSource == null )
					_audioSource =gameObject.AddComponent<AudioSource> () ;
				_audioSource.clip =_TargetFeedbackSound ;
				_audioSource.playOnAwake =false ;
				_audioSource.spatialBlend =1 ;
				_audioSource.dopplerLevel =0 ;
			}
		}

		void GazeEntered () {
			for ( int i =0 ; i < _defaultMaterials.Length ; i++ )
				_defaultMaterials [i].SetFloat ("_Highlight", .25f) ;
		}

		void GazeExited () {
			for ( int i =0 ; i < _defaultMaterials.Length ; i++ )
				_defaultMaterials [i].SetFloat ("_Highlight", 0f) ;
		}

		void OnSelect () {
			for ( int i =0 ; i < _defaultMaterials.Length ; i++ )
				_defaultMaterials [i].SetFloat ("_Highlight", .5f) ;
			// Play the audioSource feedback when we gaze and select a hologram.
			if ( _audioSource != null && !_audioSource.isPlaying )
				_audioSource.Play () ;
			this.SendMessage ("PerformTagAlong") ;
		}

	}

}