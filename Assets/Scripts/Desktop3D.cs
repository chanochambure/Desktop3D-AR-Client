using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Gvr.Internal;

public class Desktop3D : MonoBehaviour {
	private int reconnection_test = 1;
	private int max_reconnection_test = 5;
	private TextMesh mainReconnection;
	private List<LineRenderer> lines= new List<LineRenderer>();
	// Use this for initialization
	void Start () {
		VRSettings.enabled = true;
		mainReconnection = (TextMesh)GameObject.Find ("Reconnection").GetComponent<TextMesh> ();
		for (int i = 0; i < ClientConn.NUMBER_HAND; i++) {
			for (int j = 0; j < ClientConn.NUMBER_FINGER; j++) {
				for (int k = 0; k < (ClientConn.NUMBER_BONES - 1); k++) {
					GameObject new_line = new GameObject ();
					LineRenderer line_renderer = new_line.AddComponent<LineRenderer> ();
					line_renderer.SetWidth (3.0f, 3.0f);
					lines.Add (line_renderer);
				}
			}
		}
		//Debug.Log(Gvr.Internal.EmulatorConfig.Instance);
	}
	// Update is called once per frame
	void Update () {
		if(ClientConn.Instance.read_data) {
			int line_number = 0;
			for (int hand = 0; hand < ClientConn.NUMBER_HAND; ++hand) {
				for (int finger = 0; finger < ClientConn.NUMBER_FINGER; ++finger) {
					Vector3 start_position = new Vector3 (
						                         ClientConn.Instance.hand_pos [hand, finger].x,
						                         ClientConn.Instance.hand_pos [hand, finger].y,
						                         ClientConn.Instance.hand_pos [hand, finger].z
					                         );
					GameObject old_obj = null;
					for (int bone = 0; bone < ClientConn.NUMBER_BONES; ++bone) {
						string name = "Sphere_" + (hand + 1).ToString () + "_" + (finger + 1).ToString () + "_" + (bone + 1).ToString ();
						GameObject obj = GameObject.Find (name);
						obj.transform.localPosition = start_position = start_position + ClientConn.Instance.hand_dir [hand, finger, bone];
						if (old_obj != null) {
							lines [line_number].SetPosition (0, old_obj.transform.position);
							lines [line_number].SetPosition (1, obj.transform.position);
							++line_number;
						}
						old_obj = obj;
					}
				}
			}
		}
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
			SceneManager.LoadScene("menu_scene");
		}
	}
}
