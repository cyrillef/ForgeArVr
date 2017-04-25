using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Autodesk.Forge {

public class Tooltip : MonoBehaviour {
	public Text _userMessage ;

	private Color _startingColor ;
	private Color _commandColor =new Color (0.33f, 0.14f, 0.93f, 1.0f) ;

	void Start () {
		_startingColor =_userMessage.color ;
	}

	public void VoiceCommandHeard () {
		_userMessage.color =_commandColor ;
	}

	public void ResetTooltip () {
		_userMessage.color =_startingColor ;
	}

}

}
