using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Gvr.Internal;

public class Desktop3D : MonoBehaviour {
	private int reconnection_test = 1;
	private int max_reconnection_test = 10;
	Text mainReconnection;
	// Use this for initialization
	void Start () {
		VRSettings.enabled = true;
		mainReconnection = (Text)GameObject.Find ("Reconnection").GetComponent<Text> ();
		//Debug.Log(Gvr.Internal.EmulatorConfig.Instance);
	}
	// Update is called once per frame
	void Update () {
		if (ClientConn.Instance.do_reconection) {
			if (ClientConn.Instance.status_connection == 2) {
				reconnection_test = 1;
				ClientConn.Instance.do_reconection = false;
				mainReconnection.text = "";
			}
			else if (reconnection_test <= max_reconnection_test && ClientConn.Instance.status_connection == 0) {
				mainReconnection.text = string.Format ("Reconectando: ({0}/{1})", reconnection_test, max_reconnection_test);
				ClientConn.Instance.restartConnection();
				++reconnection_test;
			}
		}
		if (reconnection_test > max_reconnection_test) {/*
			DestroyImmediate (GameObject.Find ("GvrControllerMain"));
			DestroyImmediate (GameObject.Find ("GvrEventSystem"));
			DestroyImmediate (GameObject.Find ("GvrControllerPointer"));
			DestroyImmediate (GameObject.Find ("PhoneRemoteConfig"));
			DestroyImmediate (GameObject.Find ("PhoneRemote"));*/
			SceneManager.LoadScene(0);
		}
	}
}
