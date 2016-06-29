using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManagerNonLAN : NetworkBehaviour {

	string registeredGameName = "ROC-HCI-Lab_Server";
	bool isRefreshing = false;
	float refreshRequestLength = 3.0f;
	HostData[] hostData;

	private void StartServer()
	{
		Network.InitializeServer (16, 25002, false);
		MasterServer.RegisterHost (registeredGameName, "ROCConf Testing", "Test Server for ROCConf");
	}

	void OnServerInitialized()
	{
		Debug.Log ("Server has been initialized");
		SpawnPlayer ();
	}

	void OnMasterServerEvent(MasterServerEvent masterServerEvent)
	{
		if (masterServerEvent == MasterServerEvent.RegistrationSucceeded) 
		{
			Debug.Log ("Registration Succesful!");
		}
	}

	public IEnumerator RefreshHostList(){
		Debug.Log ("Refreshing...");
		MasterServer.RequestHostList (registeredGameName);
		float timeStarted = Time.time;
		float timeEnd = Time.time + refreshRequestLength;

		while (Time.time < timeEnd) 
		{
			hostData = MasterServer.PollHostList ();
			yield return new WaitForEndOfFrame ();
		}

		if (hostData == null || hostData.Length == 0) {
			Debug.Log ("No active servers have been found.");
		} else
			Debug.Log (hostData.Length + " have been found.");
	}

	/////////////
	/// Spawn and Player Server Communication
	/// /////////

	private void SpawnPlayer()
	{
		Debug.Log ("Spawning Player...");
		GameObject player = (GameObject)Network.Instantiate (Resources.Load ("Player"), new Vector3 (0f, 2.5f, 0), Quaternion.identity, 0);
		NetworkServer.SpawnWithClientAuthority (player, connectionToClient);
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log ("Player disconnected from: " + player.ipAddress + ";" + player.port);
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
	}

	void OnApplicationQuit()
	{
		if (Network.isServer) 
		{
			Network.Disconnect (200);
			MasterServer.UnregisterHost ();
		}
		if (Network.isClient) 
		{
			Network.Disconnect (200);
		}
	}

	/// <summary>
	/// GUI Settup
	/// </summary>

	public void OnGUI()
	{
		if (Network.isServer) {
			GUILayout.Label ("Running as a server.");
		} else if (Network.isClient) {
			GUILayout.Label ("Running as a client");
		}

		if (Network.isClient) {
			if (GUI.Button (new Rect (25f, 25f, 150f, 30f), "Spawn"))
			{
				SpawnPlayer ();
			}
		}

		if (!Network.isClient && !Network.isServer) {

			if (GUI.Button (new Rect (25f, 25f, 150f, 30f), "Start New Server")) {
				StartServer ();

			}

			if (GUI.Button (new Rect (25f, 65f, 150f, 30f), "Refresh Server List")) {
				StartCoroutine ("RefreshHostList");
			}

			if (hostData != null) {
				for (int i = 0; i < hostData.Length; i++) {
					if (GUI.Button (new Rect (Screen.width / 2, 65 + (30f * i), 300f, 30f), hostData [i].gameName)) {
						Network.Connect (hostData [i]);
					}
				}
			}
		}
	}
}