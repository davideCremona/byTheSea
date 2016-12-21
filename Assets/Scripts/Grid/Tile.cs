﻿using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Tile Position
	private Transform tr;

	// free if it has no constructions or traps over it
	private bool free=true;

	private bool collided_with_dummy=false;

	private BuildableEnum instance_to_build = BuildableEnum.NoBuilding;



	[Header("Water Resource to be spawn over the tile")]
	public GameObject m_my_tile_water;

	//DUMMY TOWERS PREVIEW: ACTUALLY THEY ARE SIMPLY A SPRITE AND WON'T SHOOT TO THE INCOMING ENEMIES
	[Header("Archer Castle Preview")]
	public GameObject m_preview_archer_castle_prefab;

	[Header("Cannon Castle Preview")]
	public GameObject m_preview_cannon_castle_prefab;

	//REAL TOWERS THAT HAVE TO BE BUILT
	[Header("Archer Castle")]
	public GameObject m_archer_castle_prefab;

	[Header("Cannon Castle")]
	public GameObject m_cannon_castle_prefab;

	[Header("Sand Hole Dummy")]
	public GameObject m_sand_hole_preview_prefab;

	[Header("Sand Hole")]
	public GameObject m_sand_trap_prefab;

	//Insance being displayed in preview:
	private GameObject instance_in_preview;

	// boolean to express wheter a tile is shadow (and so you cna build on top of it) or not
	private bool is_shadow_tile= true; 


	//TILE IDENTIFIER USED FOR CHANGING SCENES
	//TODO: the tiles need to be persistent between different scenes

	public int tile_id;


	//TILE READY: HAS ITS IDENTIFIER ASSIGNED FOR SURE
	//TODO: the tiles need to be persistent between different scenes
	private bool tile_ready= false;
	//TODO: the tiles need to be persistent between different scenes
	public void SetID(int id){
		tile_id = id;
	}




	void Awake(){
		//You get the transform of this component in the awake to avoid errors
		tr = GetComponent<Transform> (); 
	}



	// Use this for initialization
	void Start ()
	{
		//POOLS THINGS!!
		//WATER
		ObjectPoolingManager.Instance.CreatePool (m_my_tile_water, 50, 50);

		// ArcherCastle preview
		ObjectPoolingManager.Instance.CreatePool (m_preview_archer_castle_prefab, 5, 5);

		// Cannon Castle preview
		ObjectPoolingManager.Instance.CreatePool (m_preview_cannon_castle_prefab, 5, 5);

		// ArcherCastle
		ObjectPoolingManager.Instance.CreatePool (m_archer_castle_prefab, 50, 50);

		// Cannon Castle
		ObjectPoolingManager.Instance.CreatePool (m_cannon_castle_prefab, 50, 50);

		// Sand Trap
		ObjectPoolingManager.Instance.CreatePool (m_sand_trap_prefab, 50, 50);

		// Sand Trap preview
		ObjectPoolingManager.Instance.CreatePool (m_sand_hole_preview_prefab,20,20);
	

		// Castle Spawn Events. they update the castle_to_build enum variable

		EventManager.StartListening ("MouseReleased",BuildCastle);

		//EventManager.StartListening ("PassToPlatformScene",Save);
		//TODO: the tiles need to be persistent between different scenes
		EventManager.StartListening ("FinishedTileIDAssignement", setTileReady);



	}


	//TODO: the tiles need to be persistent between different scenes
	IEnumerator WaitTileToBeReady(){
		yield return new WaitForSeconds (0.5f);
		while (!tile_ready) {
			yield return new WaitForSeconds (0.5f);
		}
		//Load ();

	}

	//TODO: make the castles don't destroy on load
	//TODO: the tiles need to be persistent between different scenes
	public void setTileReady(){
		tile_ready = true;
	}

	public void setLightTile(){
		is_shadow_tile = false;
	}

	public void isShadowTile(MessageClass args){
		args.isfree = is_shadow_tile;
		return;
	}
		


	//Build up the castle in the tile //TODO castle_built
	public void BuildCastle(){
		if (collided_with_dummy == true) {
			//Debug.Log ("BuildCastle has been called");
			EventManager.TriggerEvent ("SettedWithSuccess");
			if (instance_to_build == BuildableEnum.ArcherTower) {
				GameObject go = MaterializeGameObject(m_archer_castle_prefab.name);
				go.SendMessage ("SetParentTile", this.gameObject);
			}
			if (instance_to_build == BuildableEnum.CannonTower) {
				GameObject go = MaterializeGameObject (m_cannon_castle_prefab.name);
				go.SendMessage ("SetParentTile", this.gameObject);
			}
			if (instance_to_build == BuildableEnum.SandHole) {
				GameObject go = MaterializeGameObject (m_sand_trap_prefab.name);
				go.SendMessage ("SetParentTile", this.gameObject);
			}
			free = false;
		}
	}
		

	// This methodsets water over the selected tile.
	public void SetWater(){
	
		free = false;
		//Instantiate the water as its own child
		GameObject go = ObjectPoolingManager.Instance.GetObject(m_my_tile_water.name);
		go.transform.position = new Vector3(tr.position.x, tr.position.y, 99); ///!!!TODO modify it,z z=99 is bad in the z=0 scene
		go.transform.rotation = Quaternion.identity;
		//Pass to go the parent reference by calling him with some kind of message
		go.SendMessage("setDaddy", this.gameObject);

	
	
	}

	//Method called by the Grid to check wheter the tile is free or not
	public void IsFree(MessageClass args){
		args.isfree = free;
		return;
	}

	//Method called by the Grid to check wheter the tile is displaying in preview something or not

//	public void IsDisplayingInPreview(MessageClass args){
//		args.isfree = displaying_in_prevew;
//	}
//		


	public void SetFree(){
		free=true;
	}

	private GameObject MaterializeGameObject(string object_name){
		GameObject go = ObjectPoolingManager.Instance.GetObject (object_name);
		go.transform.position = tr.transform.position;
		go.transform.rotation = Quaternion.identity;
		return go;
	
	}






	// Collider Part: You don't want to make the player able to build up things if an enemy is nearby
	// Whenever you have a collision between a tile and an enemy, the free variable is setted to false, making the player unable to 
	// build over an occupied tile

	void OnTriggerEnter2D(Collider2D other){
		//Debug.Log ("Hit");
		// If an enemy is over a tile it is no more free, such that the player cannot build things over the head of enemies
		if (other.gameObject.tag == "Enemy") {
			//Debug.Log ("Hit, and it's an enemy");
			free = false;
		} else {
			collided_with_dummy = true;
			if (free == true) {
				if (is_shadow_tile == true) {
					if (other.gameObject.tag == "ArcherCastleDummy") {
						instance_to_build = BuildableEnum.ArcherTower;
						GameObject go = MaterializeGameObject (m_preview_archer_castle_prefab.name);
						instance_in_preview = go;
					}
					if (other.gameObject.tag == "CannonCastleDummy") {
						instance_to_build = BuildableEnum.CannonTower;
						GameObject go = MaterializeGameObject (m_preview_cannon_castle_prefab.name);
						instance_in_preview = go;
					}
				} //If it's a light tile, you want to show the trap preview.
			else if (is_shadow_tile == false) {
					if (other.gameObject.tag == "SandHoleDummy") {
						instance_to_build = BuildableEnum.SandHole;
						GameObject go = MaterializeGameObject (m_sand_hole_preview_prefab.name);
						instance_in_preview = go;
					}

				}

			}
		}

	}

	void OnTriggerExit2D(Collider2D other){
		//DestroyThePreview.

		if(other.gameObject.tag == "ArcherCastleDummy" || other.gameObject.tag == "CannonCastleDummy"){
			instance_in_preview.SetActive (false);
			instance_to_build = BuildableEnum.NoBuilding;
			collided_with_dummy = false;
		}
		//Debug.Log ("Exited");
		if (other.gameObject.tag == "Enemy") {
			free = true;
		}
	}
}




//TODO: the tiles need to be persistent between different scenes
//	void Save(){
//		//Debug.Log ("Tile Saved");
//		SavedInfo.instance.SaveTile (tile_id, tile_building);
//
//	}

//	private void BuildLoadedCastle(BuildableEnum thing_to_build){
//		//TODO check: it has to initialize everything in the right way.
//		//You don't need to check wheter it's a light or dark tile because it's from an already existed, controlled scene
//		if (thing_to_build != BuildableEnum.NoBuilding) {
//			free = false;
//			if (thing_to_build == BuildableEnum.ArcherTower) {
//				GameObject go = ObjectPoolingManager.Instance.GetObject (m_archer_castle_prefab.name);
//				go.transform.position = tr.transform.position;
//				go.transform.rotation = Quaternion.identity;
//
//			}
//			if (thing_to_build == BuildableEnum.CannonTower) {
//				GameObject go = ObjectPoolingManager.Instance.GetObject (m_cannon_castle_prefab.name);
//				go.transform.position = tr.transform.position;
//				go.transform.rotation = Quaternion.identity;
//
//			}
//
//		}
//
//
//	}


//TODO: the tiles need to be persistent between different scenes
//	private void Load(){
//		if (!SavedInfo.instance.isFirstScene ()) {
//			if (!tile_ready) {
//				StartCoroutine (WaitTileToBeReady ());
//			} else {
//				BuildableEnum building = SavedInfo.instance.LoadTileInformation (tile_id);
//				BuildLoadedCastle (building);
//
//			}
//
//
//		}
//	}
