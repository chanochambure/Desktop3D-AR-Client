using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class start_menu : MonoBehaviour {
	Button buttonConnection;
	InputField mainIP;
	InputField mainPuerto;
	Text mainError;
	// Use this for initialization
	void Start () {
		VRSettings.enabled = false;
		try{
			buttonConnection = (Button)GameObject.Find ("Connect").GetComponent<Button> ();
			mainIP = (InputField)GameObject.Find ("IP").GetComponent<InputField> ();
			mainPuerto = (InputField)GameObject.Find ("Port").GetComponent<InputField> ();
			mainError = (Text)GameObject.Find ("Error").GetComponent<Text> ();
			mainIP.text = ClientConn.Instance.last_ip;
			mainPuerto.text = ClientConn.Instance.last_port.ToString();
			if (ClientConn.Instance.do_reconection) {
				mainError.text = "Se perdio la Conexión";
				ClientConn.Instance.do_reconection = false;
			}
			buttonConnection.interactable = true;
		} catch(Exception){
		}
	}
	private bool waiting_connection = false;
	// Update is called once per frame
	void Update () {
		if (ClientConn.Instance.status_connection == 2) {
			waiting_connection = false;
			SceneManager.LoadScene("Desktop3D");
		}
		if (waiting_connection && ClientConn.Instance.status_connection == 0) {
			mainError.text = "Fallo la Conexión";
			waiting_connection = false;
			buttonConnection.interactable = true;
		}
	}

	public void LoadByIndex(int unused_data){
		try
		{
			if(mainIP.text.Length == 0 || mainPuerto.text.Length == 0){
				mainError.text = "Faltan Datos";
			} else {
				ClientConn.Instance.beginConnection(mainIP.text,Int32.Parse(mainPuerto.text));
				buttonConnection.interactable = false;
				mainError.text = "Esperando Conexión";
				waiting_connection = true;
			}
		} catch (FormatException) {
			mainError.text = "IP puerto no válidos.";
		} catch (Exception) {
			mainError.text = "Error Desconocido";
		}
	}
}
