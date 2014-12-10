using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ippo
{
	[RequireComponent(typeof(AudioSource))]
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
			print ("[DangIt] [AlarmManager] Adding '" + number.ToString () + "' alarms from '"+fm.ToString()+"'");
			loops.Add (fm, number);
		}

		public void Update()
		{
			if (this.audio != null)
			{
				if (!FindObjectsOfType<FailureModule> ().Any (fm => fm.HasFailed))
				{
					this.audio.Stop (); //Make sure we arent playing if nothing has failed
				} else {
					if (loops.Count > 0)
					{
						var element = loops.ElementAt (0);
						if (element.Value == 0 || !element.Key.HasFailed || element.Key.vessel!=FlightGlobals.ActiveVessel) //If there are no loops remaining, it has been repaired, or it isn't on the vessel anymore:
						{
							element.Key.AlarmsDoneCallback ();
							loops.Remove (element.Key); //Stop playing it
							print ("[DangIt] [AlarmManager] Removing FM: Remove");
						}
						else if (!this.audio.isPlaying)
						{
							loops.Remove (element.Key);
							loops.Add (element.Key, element.Value - 1);
							print ("[DangIt] [AlarmManager] Playing Clip");
							audio.Play ();
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

