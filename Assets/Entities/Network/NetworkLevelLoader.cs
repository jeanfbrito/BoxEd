using UnityEngine;
using System.Collections;

/// <summary>
/// THIS FILE IS A REFERENCE FROM AN OLD BUILD AND THE UNITY DOCS
/// IT'S STILL HERE BECAUSE I NEED TO STEAL CODE LATER
/// </summary>
[RequireComponent(typeof(NetworkView))]
public class NetworkLevelLoader : MonoBehaviour
{
	public string[] networkLevels;
	public string disconnectedLevel;
	private int lastLevelPrefix = 0;
	//private Rect lobbyWindow = new Rect(20, 20, 200, 50);

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		networkView.group = 1;
		//Network.minimumAllocatableViewIDs = 2000;
		//LevelListings.LoadStockLevels();
	
		Application.LoadLevel(disconnectedLevel);
		
		//StartCoroutine(StartGame());
		
		//MasterServer.ipAddress = "78.46.172.55";
		//MasterServer.port = 23466;
		//Network.natFacilitatorIP = "78.46.172.55";
		//Network.natFacilitatorPort = 50000;
	}
	
	IEnumerator StartGame()
	{
		yield return new WaitForSeconds(5);
		Application.LoadLevel(disconnectedLevel);
	}

	void DrawLobbyWindow(int windowID)
	{
		if(Network.peerType == NetworkPeerType.Server)
			GUILayout.Label("Clients:");
		else
			GUILayout.Label("Connected to:");

		foreach(NetworkPlayer player in Network.connections)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(player.ipAddress);
			GUILayout.EndHorizontal();
		}

		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	[RPC]
	IEnumerator LoadLevel(string level, int levelPrefix)
	{
		lastLevelPrefix = levelPrefix;
		Network.SetSendingEnabled(0, false);
		Network.isMessageQueueRunning = false;
		Network.SetLevelPrefix(levelPrefix);
		Application.LoadLevel(level);
		yield return null;
		yield return null;

		Network.isMessageQueueRunning = true;
		Network.SetSendingEnabled(0, true);

		foreach(GameObject obj in FindObjectsOfType(typeof(GameObject)))
		{
			obj.SendMessage("OnNetworkLevelLoaded", SendMessageOptions.DontRequireReceiver);
		}
	}

	[RPC]
	public void LoadLevelByName(string level)
	{
		networkView.RPC("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	void OnDisconnectedFromServer()
	{
		Application.LoadLevel(disconnectedLevel);
	}

}