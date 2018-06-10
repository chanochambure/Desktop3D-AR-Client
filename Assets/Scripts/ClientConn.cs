using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientConn {
	// Singleton
	private static ClientConn instance = null;
	public static ClientConn Instance
	{
		get
		{
			if (instance == null)
				instance = new ClientConn();
			return instance;
		}
	}
	// Constructor
	public ClientConn(){
	}
	// Network
	private Socket socket;
	private byte[] recieveBuffer = new byte[600];
	public int status_connection = 0;
	private int next_read = 12;
	public String last_ip = "127.0.0.1";
	public int last_port = 8888;
	public bool do_reconection = false;
	public void beginConnection(String ip, int port){
		last_ip = ip;
		last_port = port;
		restartConnection ();
	}
	public void restartConnection(){
		try {
			status_connection = 1;
			socket = new Socket (
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp
			);
			socket.BeginConnect (
				new IPEndPoint (IPAddress.Parse(last_ip), last_port),
				new AsyncCallback (startConnection),
				null
			);
		} catch (SocketException ex) {
			Debug.Log (ex.Message);
			socket = null;
			status_connection = 0;
		}
	}
	private void startConnection(IAsyncResult AR){
		try{
			socket.EndReceive(AR);
			if(socket.Connected){
				socket.BeginReceive (
					recieveBuffer,
					0,
					next_read,
					SocketFlags.None,
					new AsyncCallback (recievePacket),
					null
				);
				status_connection = 2;
			} else {
				status_connection = 0;
			}
		} catch(Exception){
			status_connection = 0;
		}
	}
	private void recievePacket(IAsyncResult AR){
		int bytesRecieved = socket.EndReceive(AR);
		Debug.LogFormat ("Recieved: {0}", bytesRecieved);
		if (bytesRecieved <= 0) {
			socket.Shutdown (SocketShutdown.Both);
			socket.Disconnect (true);
			socket.Close ();
			status_connection = 0;
			do_reconection = true;
		} else {
			byte[] recData = new byte[bytesRecieved];
			Buffer.BlockCopy (recieveBuffer, 0, recData, 0, bytesRecieved);
			getData (recData, bytesRecieved);
			socket.BeginReceive (
				recieveBuffer,
				0,
				next_read,
				SocketFlags.None,
				new AsyncCallback (recievePacket),
				null
			);
		}
	}
	// Recepción de datos
	public bool read_data = true;
	public float temp_gesto = 0;
	public float temp_izquierda = 0;
	public float temp_derecha = 0;
	public int gesture = 0;
	public bool[] hand_status = new bool[2];
	public Vector3[,] hand_pos = new Vector3[2, 5];
	public Vector3[,,] hand_dir = new Vector3[2, 5, 4];
	private void getData(byte[] recData, int recieved){
		if (next_read == 12) {
			temp_gesto = System.BitConverter.ToSingle (recData, 0);
			temp_izquierda = System.BitConverter.ToSingle (recData, 4);
			temp_derecha = System.BitConverter.ToSingle (recData, 8);
			if (temp_izquierda == 1 && temp_derecha == 1)
				next_read = 600;
			else if (temp_izquierda == 1 || temp_derecha == 1)
				next_read = 300;
			if (next_read == 12) {
				writeChanges (null);
			}
			//Debug.LogFormat ("Gesto: {0}", gesto);
			//Debug.LogFormat ("Izquierda: {0}", izquierda);
			//Debug.LogFormat ("Derecha: {0}", derecha);
		} else {
			writeChanges (recData);
			next_read = 12;
		}
	}
	private void clearData(){
		gesture = GESTO_VACIO;
		for (int hand = 0; hand<NUMBER_HAND; ++hand)
		{
			hand_status[hand] = false;
			for (int finger = 0; finger<NUMBER_FINGER; ++finger)
			{
				hand_pos [hand, finger].x = 0;
				hand_pos [hand, finger].y = 0;
				hand_pos [hand, finger].z = -9999;
				for (int bone = 0; bone < NUMBER_BONES; ++bone)
				{
					hand_dir[hand, finger, bone].x = 0;
					hand_dir[hand, finger, bone].y = 0;
					hand_dir[hand, finger, bone].z = 0;
				}
			}
		}
	}
	private void writeChanges(byte[] recData){
		read_data = false;
		clearData ();
		gesture = (int)temp_gesto;
		hand_status [0] = (temp_izquierda == 1);
		hand_status [1] = (temp_derecha == 1);
		if (recData != null) {
			int byte_counter = 0;
			for (int hand = 0; hand<NUMBER_HAND; ++hand)
			{
				if (hand_status[hand])
				{
					for (int finger = 0; finger < NUMBER_FINGER; ++finger)
					{
						hand_pos [hand, finger].x = System.BitConverter.ToSingle (recData, byte_counter + 0);
						hand_pos [hand, finger].y = System.BitConverter.ToSingle (recData, byte_counter + 4);
						hand_pos [hand, finger].z = System.BitConverter.ToSingle (recData, byte_counter + 8);
						byte_counter += 12;
						for (int bone = 0; bone < NUMBER_BONES; ++bone)
						{
							hand_dir [hand, finger, bone].x = System.BitConverter.ToSingle (recData, byte_counter + 0);
							hand_dir [hand, finger, bone].y = System.BitConverter.ToSingle (recData, byte_counter + 4);
							hand_dir [hand, finger, bone].z = System.BitConverter.ToSingle (recData, byte_counter + 8);
							byte_counter += 12;
						}
					}
				}
			}
		}
		read_data = true;
	}
	// Gestos
	public const int GESTO_VACIO = 0;
	public const int GESTO_LIMPIAR_DATOS = 1;
	// Constantes
	public const int NUMBER_HAND = 2;
	public const int NUMBER_FINGER = 5;
	public const int NUMBER_BONES = 4;
}
