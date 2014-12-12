using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ippo
{
	[RequireComponent(typeof(AudioSource))]
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class AlarmManager : MonoBehaviour
	{
		public Dictionary<FailureModule, int> loops;

		public void Start()
		{
			print("[DangIt] [AlarmManager] Starting...");
			print("[DangIt] [AlarmManager] Setting Volume...");
			this.audio.panLevel = 0f; //This disable the game scaling volume with distance from source
			this.audio.volume = 1f;

			print ("[DangIt] [AlarmManager] Creating Clip");
			this.audio.clip=GameDatabase.Instance.GetAudioClip("DangIt/Sounds/alarm"); //Load alarm sound

			print ("[DangIt] [AlarmManager] Creating Dictionary");
			this.loops=new Dictionary<FailureModule, int>(); //Reset counter, so on logic pass we play it
		}

		public void AddAlarm(FailureModule fm, int number)
		{
			if (number != 0) {
				print ("[DangIt] [AlarmManager] Adding '" + number.ToString () + "' alarms from '" + fm.ToString () + "'");
				loops.Add (fm, number); //subtract 1 because otherwise we play 1 extra
			} else {
				print ("[DangIt] [AlarmManager] No alarms added: Would have added 0 alarms");
			}
		}

		public void Update()
		{
			if (this.audio != null) {
				if (!this.audio.isPlaying){
					if (loops.Count > 0) {
						var element = loops.ElementAt (0);
						loops.Remove (element.Key);
						print ("[DangIt] [AlarmManager] Playing Clip");
						audio.Play ();
						if (element.Value != 0 && element.Value != 1) {
							if (element.Key.vessel == FlightGlobals.ActiveVessel) {
								loops.Add (element.Key, element.Value - 1); //Only re-add if still has alarms
							}
						}
					}
				}
			}
		}

		public void RemoveAllAlarmsForModule(FailureModule fm)
		{
			print ("[DangIt] [AlarmManager] Removing alarms...");
			if (this.loops.Keys.Contains (fm))
			{
				fm.AlarmsDoneCallback ();
				loops.Remove (fm);
			}
		}

		public bool HasAlarmsForModule(FailureModule fm)
		{
			if (this.loops.Keys.Contains (fm))
			{
				int i;
				loops.TryGetValue (fm, out i);
				if (i != 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}

