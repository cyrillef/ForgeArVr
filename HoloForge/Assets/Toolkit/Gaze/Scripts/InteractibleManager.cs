using HoloToolkit;
using UnityEngine;

namespace HoloToolkit.Unity {
	/// <summary>
	/// InteractibleManager keeps tracks of which GameObject
	/// is currently in focus.
	/// </summary>
	public class InteractibleManager : Singleton<InteractibleManager> {
		private GameObject _oldFocusedGameObject =null ;
		private int _interactibleLayerMask ;

		public GameObject FocusedGameObject { get; private set; }

		void Start () {
			FocusedGameObject =null ;
			_interactibleLayerMask =LayerMask.NameToLayer ("Interactible") ;
		}

		void Update () {
			_oldFocusedGameObject =FocusedGameObject ;
			if ( GazeManager.Instance.Hit ) {
				RaycastHit hitInfo =GazeManager.Instance.HitInfo ;
				if ( hitInfo.collider != null )
					FocusedGameObject =hitInfo.collider.gameObject ;
				else
					FocusedGameObject =null ;
			} else {
				FocusedGameObject =null ;
			}

			if ( FocusedGameObject != _oldFocusedGameObject ) {
				ResetFocusedInteractible () ;
				if ( FocusedGameObject != null ) {
					if ( FocusedGameObject.GetComponent<Interactible> () != null || FocusedGameObject.layer == _interactibleLayerMask )
						FocusedGameObject.SendMessage ("GazeEntered", SendMessageOptions.DontRequireReceiver) ;
				}
			}
		}

		private void ResetFocusedInteractible () {
			if ( _oldFocusedGameObject != null ) {
				if ( _oldFocusedGameObject.GetComponent<Interactible> () != null || _oldFocusedGameObject.layer == _interactibleLayerMask )
					_oldFocusedGameObject.SendMessage ("GazeExited", SendMessageOptions.DontRequireReceiver) ;
			}
		}

	}

}