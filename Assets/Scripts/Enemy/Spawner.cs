﻿using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	[Header("Syncronized Timer")]
	public GameObject timer;
	private Animator animatorTimer;

	
	[Header("Waves Scriptable Objects")]
	public Wave[] m_waves;

	// Remark: since the spawner reads directly form the Scriptableobject what kind of enemy to spawn, it doesn't require any prefab

	private int current_level=0;

	//Positions at which the Enemies will be spawned. 
	[Header("Spawn Positions")]
	public GameObject m_first_lane;
	public GameObject m_second_lane;
	public GameObject m_third_lane;
	public GameObject m_fourth_lane;
	public GameObject m_fifth_lane;


	[Header("Grid Reference")]
	public GameObject m_grid;

	[Header("Timer Reference")]
	public GameObject m_timer;

	[Header("HermitCrabs Prefab")]
	public GameObject m_hermit_crab;

	[Header("Octopus Prefab")]
	public GameObject m_octopus;

	[Header("Crab Prefab")]
	public GameObject m_crab;

	[Header("Waiting Time")]
	[Range(0f, 20f)]
	public int m_waiting_time;
	// Use this for initialization
	void Start () {
		animatorTimer = timer.GetComponent<Animator> ();
		//HERMITCRABS
		ObjectPoolingManager.Instance.CreatePool (m_hermit_crab,70,70);
		//OCTOPUS
		ObjectPoolingManager.Instance.CreatePool (m_octopus,70,70);
		//CRAB
		ObjectPoolingManager.Instance.CreatePool (m_crab,70,70);

		//Starts listening to the NextWave event: at that time it will spawn the enemies for the next wave
		EventManager.StartListening ("NewWave",Spawn);
		// For the very first time, it triggers itself

		Load();

		StartCoroutine (WaitForTheSpawning());

		EventManager.StartListening ("PassToPlatformScene", Save);
	
	}

	IEnumerator WaitForTheSpawning(){
		animatorTimer.speed = Timer.timeAnimationBase / m_waiting_time;
		animatorTimer.SetTrigger ("Start");
		yield return new WaitForSeconds (m_waiting_time);
		//Indirect call to the Spawn() method and other things in the game
		EventManager.TriggerEvent ("NewWave");
	}




	private void Load(){
		//Debug.Log ("Spawer tries to load");
		if (!SavedInfo.instance.isFirstScene ()) {
			current_level = SavedInfo.instance.LoadCurrentLevel ();
			Debug.Log ("Level Loaded:"+ current_level);
		}
	}

	private void Save(){
		SavedInfo.instance.SaveLevel (current_level);
		Debug.Log ("Level Saved");
	}











void Spawn(){
		if (current_level < m_waves.Length) {


			//At the beginning of each new wave a certain number of water are randomically dropped over the grid
			m_grid.SendMessage ("spawnRandomWater", m_waves [current_level].n_water_drops);

			//Call the timer to keep track of the time for the next wave 
			m_timer.SendMessage ("StartTiming", m_waves [current_level].wave_time);


			//Takes the subwaves from the current wave
			Subwave[] subwav = m_waves [current_level].m_subwaves;
			float wait_time = 0f;
	
			for (int i = 0; i < subwav.Length; i++) {
				wait_time = wait_time + subwav [i].m_spawn_time;
				if(i== (subwav.Length -1))
					StartCoroutine (SpawnAtSubwave (wait_time, subwav [i].m_enemies, true));
				else
					StartCoroutine (SpawnAtSubwave (wait_time, subwav [i].m_enemies, false));
			}

			//Current level gets incremented at the end of the function in order to align the level 1 to the array element at position 0
			current_level++;
		} else {
			//Else repeat in a forever loop the very last 3 Levels!
//			current_level = current_level - 2;
//			Spawn ();
			EventManager.StopListening("NewWave",Spawn);
		}
	}


	//Couroutine that, after a given amount of waiting time, spawns the enemies
	IEnumerator SpawnAtSubwave(float waiting_time, EnemySpawn[] enemies, bool last ){


		Animator animator = GetComponent<Animator> () as Animator;
		animator.speed = 1.817f/ waiting_time;
		animator.SetTrigger ("Wave");

		yield return new WaitForSeconds (waiting_time);

		if (last) {
			timer.GetComponent<Timer> ().forceAnim ();
		}



		for (int i = 0; i < enemies.Length; i++) {
			string enemy_name = enemies[i].m_type.ToString ();
			GameObject go = ObjectPoolingManager.Instance.GetObject (enemy_name);
			//The enemy is spawns at the specified position, usinge the three (but possibily more) children of Spawner GameObject:
			if(enemies[i].m_spawn_position.Equals(TilePosition.FirstLane))
				go.transform.position = m_first_lane.transform.position;
			else
				if(enemies[i].m_spawn_position.Equals(TilePosition.SecondLane))
					go.transform.position = m_second_lane.transform.position;
				else
					if(enemies[i].m_spawn_position.Equals(TilePosition.ThirdLane))
						go.transform.position= m_third_lane.transform.position;
					else
						if(enemies[i].m_spawn_position.Equals(TilePosition.FourthLane))
							go.transform.position= m_fourth_lane.transform.position;
						else
							if(enemies[i].m_spawn_position.Equals(TilePosition.FifthLane))
								go.transform.position= m_fifth_lane.transform.position;
			//No matter what, the rotation is always the Quaternion Identity
			go.transform.rotation = Quaternion.identity;
		}
			



	}



	// Update is called once per frame
	void Update () {
	
	}
}
