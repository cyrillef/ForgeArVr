using UnityEngine;
using HoloToolkit.Unity;

namespace Autodesk.Forge {

public class ForgeInteractions : Singleton<ForgeInteractions> {
	public GameObject _msgManager =null ;

	private int _interactibleLayerMask ;
	private GameObject _oldFocusedGameObject =null ;

	public GameObject FocusedGameObject { get; private set; }

	public void Start () {
		FocusedGameObject =null ;
		_interactibleLayerMask =LayerMask.NameToLayer (ForgeConstants.INTERACTIBLE) ;
		if ( _msgManager == null )
			 _msgManager =GameObject.Find (ForgeConstants.MGRPATH) ;
	}

	public void Update () {
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
				if (   FocusedGameObject.GetComponent<Interactible> () != null
					|| FocusedGameObject.layer == _interactibleLayerMask
				)
					//FocusedGameObject.SendMessage ("GazeEntered", SendMessageOptions.DontRequireReceiver) ;
					_msgManager.SendMessage ("GazeEntered", FocusedGameObject) ;
			}
		}
	}

	private void ResetFocusedInteractible () {
		if ( _oldFocusedGameObject != null ) {
			if ( _oldFocusedGameObject.GetComponent<Interactible> () != null || _oldFocusedGameObject.layer == _interactibleLayerMask )
				//_oldFocusedGameObject.SendMessage ("GazeExited", SendMessageOptions.DontRequireReceiver) ;
				_msgManager.SendMessage ("GazeExited", _oldFocusedGameObject) ;
		}
	}

}

}
