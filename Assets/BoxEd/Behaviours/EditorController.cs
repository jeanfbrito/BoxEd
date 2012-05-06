using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoxEd;
using UnityEngine;

public class EditorController : MonoBehaviour
{
	#region Statics
	private static PlayerControl _player;
	private static PlayerCamera _playerCam;
	private static EditorController _instance;
	private static GameObject _plane;
	public static EditorState State { get; private set; }

	private static bool _isIngame;
	public static bool IsIngame
	{
		get
		{
			return _isIngame;
		}
		set
		{
			_isIngame = value;
			ShowHelpers = !value;

			if(value)
			{
				var spawnpoints = LevelManager.Find<Spawnpoint>();
				var spawnLocation = spawnpoints.Count() > 0 ? spawnpoints.First().transform.position : Vector3.zero;

				_player = (Network.Instantiate(References.Instance.player, spawnLocation, Quaternion.identity, 0) as GameObject).GetComponent<PlayerControl>();
				_playerCam = (Network.Instantiate(References.Instance.playerCamera, spawnLocation, Quaternion.identity, 0) as GameObject).GetComponent<PlayerCamera>();
			}
			else if(_player != null)
			{
				Network.Destroy(_player.gameObject);
				Network.Destroy(_playerCam.gameObject);
			}

			_instance.camera.enabled = !value;
			_plane.collider.isTrigger = value;

			foreach(var entity in LevelManager.Find<Entity>())
			{
				if(_isIngame)
					entity.OnEnterGame();
				else
					entity.OnLeaveGame();
			}
		}
	}

	private static bool _showHelpers;

	/// <summary>
	/// Enables and disables the showing of helpers for entities.
	/// </summary>
	public static bool ShowHelpers
	{
		get
		{
			return _showHelpers;
		}
		set
		{
			_showHelpers = value;
			foreach(var ent in LevelManager.Find<Entity>())
			{
				if(_showHelpers)
					ent.OnEnableHelpers();
				else
					ent.OnDisableHelpers();
			
				EntityHelper helper;
				if((helper = ent as EntityHelper) != null)
					helper.StartCoroutine(helper.Fade(!value));
			}

			_instance.StartCoroutine(FadeGrid(!value));
		}
	}
	#endregion

	private void Awake()
	{
		Network.InitializeServer(0, 1337, false);
		LevelManager.LevelName = "Untitled";
		LevelManager.AuthorName = "Unknown Author";
		_entitiesByCategory = Entity.EntitiesByCategory;
		_instance = this;
		_plane = GameObject.FindGameObjectWithTag("Plane");
		SnapToGrid = true;
		ShowHelpers = true;
		_planeLayer = _plane.layer;
	}

	#region Private Members

	private EditorState _previousEditorState;
	private Entity _selectedEntity;
	private Vector3 _mousePos;
	private Vector3 _mousePosPrev;
	private Vector3 _movementDelta;
	private bool _spawnedThisFrame;
	private Vector3 _mouseScreenPos;
	private Vector3 _movementRequest;

	private bool _movingEntity;
	private int _planeLayer;

	private Entity _entity;
	private bool _entitySelected;

	private bool _posSelected;
	private Vector3 _startingPos;

	private string _entityName;
	private bool _entitySpawned;

	private EntityCategory _selectedCategory = EntityCategory.Geometry;

	private IEnumerable<EditorState> _states = Enum.GetValues(typeof(EditorState)).Cast<EditorState>();
	private Dictionary<EntityCategory, Dictionary<Type, string>> _entitiesByCategory;
	#endregion

	private static bool _snapToGrid;
	public static bool SnapToGrid
	{
		get
		{
			return _snapToGrid;
		}
		set
		{
			_snapToGrid = value;
			//_instance.StartCoroutine(FadeGrid(!value));
		}
	}

	public static IEnumerator FadeGrid(bool fadeout)
	{
		if(fadeout)
		{
			while(_plane.renderer.material.color.a > 0)
			{
				var colour = _plane.renderer.material.color;
				colour.a -= Time.deltaTime / 10;
				_plane.renderer.material.color = colour;
				yield return null;
			}
		}
		else
		{
			while(_plane.renderer.material.color.a < 1)
			{
				var colour = _plane.renderer.material.color;
				colour.a += Time.deltaTime / 10;
				_plane.renderer.material.color = colour;
				yield return null;
			}
		}
	}

	private void Update()
	{
		#region This is old and horrible and fugly BUT it works.

		if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftApple)) && Input.GetKeyDown(KeyCode.Z))
			ActionManager.Undo();

		if(Input.GetMouseButtonUp(0))
		{
			_entitySpawned = false;
			_movingEntity = false;
			_posSelected = false;

			if(_entity != null && _entitySelected)
			{
				Editor.Log("Adding a new move action to the stack");
				ActionManager.ActionStack.Push(new MoveAction { Entity = _entity, Movement = _entity.transform.position - _startingPos });
			}
		}

		if(IsIngame)
		{
			if(Input.GetKeyDown(KeyCode.Escape))
				IsIngame = false;
		}
		else if(Input.GetMouseButton(2) && _mouseScreenPos != Vector3.zero)
		{
			Screen.showCursor = false;
			_movementRequest = (_mouseScreenPos - Input.mousePosition) / 4;
		}
		else
		{
			Screen.showCursor = true;
			_movementRequest = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetAxis("ScrollWheel") * 1.5f);
		}

		_mouseScreenPos = Input.mousePosition;
		_movementDelta = new Vector3();

		transform.position = Vector3.Lerp(transform.position, transform.position + _movementRequest, 0.5f);

		RaycastHit mouseHitInfo;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out mouseHitInfo, 100, ~_planeLayer))
		{
			_mousePos = new Vector3(
				_snapToGrid ? (float)Math.Round(mouseHitInfo.point.x * 2, MidpointRounding.AwayFromZero) / 2 : mouseHitInfo.point.x,
				_snapToGrid ? (float)Math.Round(mouseHitInfo.point.y * 2, MidpointRounding.AwayFromZero) / 2 : mouseHitInfo.point.y,
				0);

			_movementDelta = _mousePos - _mousePosPrev;
			_mousePosPrev = _mousePos;
		}

		RaycastHit entityHitInfo;
		Entity rayEntity;

		if(!Input.GetMouseButton(0))
			_entitySelected = false;

		_movingEntity = Input.GetMouseButton(0) && _entitySelected;

		var screenMousePos = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Input.mousePosition.z);

		var rayResult = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out entityHitInfo, 100, ~_planeLayer) && !_movingEntity
			&& !EntityRect.Contains(screenMousePos) && !PropertyRect.Contains(screenMousePos);

		if(rayResult)
		{
			if(entityHitInfo.collider.TryGetComponent(out rayEntity) && Input.GetMouseButton(0))
			{
				_entitySelected = true;
				_entity = Entity.SelectedEntity = rayEntity;

				if(!_posSelected)
				{
					_posSelected = true;
					_startingPos = _entity.transform.position;
				}
			}
		}
		#endregion

		var hasEntity = _entity != null;

		//Should probably add some more editor states one day...
		switch(State)
		{
			case EditorState.Edit:
				{
					if(hasEntity)
					{
						if((Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.C))
							_entity = Entity.Create<Entity>(_mousePos, _entity.transform.rotation.eulerAngles, _entity.transform.localScale, _entity.GetType());
						else if(Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
								Entity.Destroy(_entity.gameObject);

						if(_movingEntity)
							_entity.transform.position = _entity.transform.position + _movementDelta;

						if(Input.GetMouseButtonUp(0))
						{
							_entitySpawned = false;
							_movingEntity = false;
						}
					}
				}
				break;
		}

		if(State != _previousEditorState)
		{
			_entitySpawned = true;
			Entity.SelectedEntity = null;
		}

		_previousEditorState = State;
	}

	public Rect EntityRect { get { return new Rect(10, Screen.height - 160, Screen.width - 20, 150); } }
	public Rect PropertyRect { get { return new Rect(Screen.width - 200, 5, 190, Screen.height - 170); } }

	private void OnGUI()
	{
		if(MenuManager.IsMenuOpen)
			return;

		GUI.skin = References.Instance.guiSkin;

		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		GUILayout.Space(30);

		SnapToGrid = GUILayout.Toggle(SnapToGrid, Localisation.BoxEd["Snap to Grid"]);
		ShowHelpers = GUILayout.Toggle(ShowHelpers, Localisation.BoxEd["Show Helpers"]);

		if(IsIngame && GUILayout.Button(Localisation.BoxEd["Restart Objects"]))
		{
			foreach(var entity in LevelManager.Find<Entity>())
			{
				entity.OnLeaveGame();
				entity.OnEnterGame();
			}
		}

		GUILayout.Window(0, EntityRect, DrawCreationWindow, Localisation.BoxEd["Entities"]);
		GUILayout.Window(1, PropertyRect, DrawEditWindow, Localisation.BoxEd["Properties"]);

		if(_states != null && _states.Count() > 1)
		{
			foreach(var state in _states)
			{
				GUILayout.BeginHorizontal();
				{
					if(State == state)
						GUILayout.Space(20);

					if(GUILayout.Button(state.ToString(), GUILayout.Width(50), GUILayout.Height(50)))
						State = state;
				}
				GUILayout.EndHorizontal();
			}
		}
	}

	private void DrawEditWindow(int id)
	{
		var entity = _entity;

		if(entity == null)
		{
			GUILayout.Label(Localisation.BoxEd["No entity selected."]);
			return;
		}

		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(entity.ClassName);

			if(GUILayout.Button(Localisation.BoxEd["Delete"]))
			{
				Destroy(entity.gameObject);
				return;
			}
		}
		GUILayout.EndHorizontal();

		foreach(var propertyPair in entity.Properties)
		{
			var property = propertyPair.Key;
			var type = property.PropertyType;
			var attr = propertyPair.Value;
			var obj = property.GetValue(entity, null);

			GUILayout.Label(property.Name + ": " + property.GetValue(entity, null));

			if(type == typeof(string))
				property.SetValue(entity, GUILayout.TextField((string)obj ?? ""), null);
			else if(type == typeof(float))
				property.SetValue(entity, GUILayout.HorizontalSlider((float)obj, attr.Min, attr.Max), null);
			else if(type == typeof(int))
				property.SetValue(entity, (int)GUILayout.HorizontalSlider((int)obj, attr.Min, attr.Max), null);
		}
	}

	private void DrawCreationWindow(int id)
	{
		//GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

		GUILayout.BeginHorizontal();
		{
			foreach(var categoryEntry in _entitiesByCategory)
			{
				if(GUILayout.Button(categoryEntry.Key.ToString(), GUILayout.Width(150)))
					_selectedCategory = categoryEntry.Key;
			}
		}
		GUILayout.EndHorizontal();

		if(_selectedCategory != EntityCategory.None)
		{
			GUILayout.BeginHorizontal();
			{
				foreach(var entityType in _entitiesByCategory[_selectedCategory])
				{
					GUILayout.BeginVertical(GUILayout.Width(100));
					if(GUILayout.RepeatButton("", GUILayout.Width(50), GUILayout.Height(50)) && !_entitySpawned)
					{
						_entity = Entity.SelectedEntity = Entity.Create<Entity>(_mousePos, type: entityType.Key);

						ShowHelpers = true;
						_entitySpawned = true;
						_entitySelected = true;
					}
					GUILayout.Label(entityType.Value);

					GUILayout.EndVertical();
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}

public enum EditorState
{
	Edit
}