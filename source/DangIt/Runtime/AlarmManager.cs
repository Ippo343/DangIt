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
						if (element.Value == 0 || !element.Key.HasFailed)
						{
							loops.Remove (element.Key);
							print ("[DangIt] [AlarmManager] Removing FM: All alarms run");
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
	}
}

