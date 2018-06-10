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
			workData (recData, bytesRecieved);
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
	private void workData(byte[] recData, int recieved){
		if (next_read == 12) {
			float gesto = System.BitConverter.ToSingle (recData, 0);
			float izquierda = System.BitConverter.ToSingle (recData, 4);
			float derecha = System.BitConverter.ToSingle (recData, 8);
			if (izquierda == 1 && derecha == 1)
				next_read = 600;
			else if (izquierda == 1 || derecha == 1)
				next_read = 300;
			Debug.LogFormat ("Gesto: {0}", gesto);
			Debug.LogFormat ("Izquierda: {0}", izquierda);
			Debug.LogFormat ("Derecha: {0}", derecha);
		} else {
			next_read = 12;
		}
	}
	// Gestos
	public const int GESTO_VACIO = 0;
	public const int GESTO_LIMPIAR_DATOS = 1;
}
