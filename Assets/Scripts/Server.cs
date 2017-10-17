using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
	GUIStyle textStyle;
	public Vector3 gyroValue;
	public Vector3 accValue;
	public bool debug;

	// Use this for initialization
	void Start ()
	{
		Network.InitializeServer (1, 25002, !Network.HavePublicAddress ());
		MasterServer.RegisterHost ("GYROCONNECT", "Flow Of Spirit");
	}

	void OnGUI ()
	{
		if (debug) {
			textStyle = new GUIStyle ();
			textStyle.fontSize = 10;
			textStyle.normal.textColor = Color.white;

			GUI.Label (new Rect (0, 10, 100, 25), "Connects: " + Network.connections.Length, textStyle);
		}
	}

	[RPC]
	private void MobileSensing (Vector3 gyro, Vector3 acc)
	{
		gyroValue = gyro;
		accValue = acc;
	}

}
