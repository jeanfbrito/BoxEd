using UnityEngine;

/// <summary>
/// References that need assigning from within the Unity editor.
/// </summary>
public class References : MonoBehaviour
{
	public Mesh cubeMesh;
	public GameObject player;
	public GameObject playerCamera;
	public GUISkin guiSkin;
	public Material helperMaterial;

	public static References Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}
}