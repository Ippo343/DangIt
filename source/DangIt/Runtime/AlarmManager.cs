using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace DangIt
{
	[RequireComponent(typeof(AudioSource))]
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class AlarmManager : MonoBehaviour
	{
		public Dictionary<FailureModule, int> loops;

        AudioSource audio = new AudioSource();

		public void Start()
		{
			print("[CDangIt] [AlarmManager] Starting...");
			print("[CDangIt] [AlarmManager] Setting Volume...");
			this.audio.spatialBlend = 0f; //This disable the game scaling volume with distance from source
			this.audio.volume = 1f;

			print ("[CDangIt] [AlarmManager] Creating Clip");
			this.audio.clip=GameDatabase.Instance.GetAudioClip("CDangIt/Sounds/alarm"); //Load alarm sound

			print ("[CDangIt] [AlarmManager] Creating Dictionary");
			this.loops=new Dictionary<FailureModule, int>(); //Reset counter, so on logic pass we play it
		}

		public void UpdateSettings(){
			float scaledVolume = CDangIt.Instance.CurrentSettings.AlarmVolume / 100f;
			print ("[CDangIt] [AlarmManager] Rescaling Volume (at UpdateSettings queue)..., now at " + scaledVolume);
			this.audio.volume = scaledVolume;
		}

		public void AddAlarm(FailureModule fm, int number)
		{
			this.audio.volume = CDangIt.Instance.CurrentSettings.GetMappedVolume(); //This seems like an OK place for this, because if I put it in the constructor...
			                                                                       // ...you would have to reboot to change it, but I don't want to add lag by adding it to each frame in Update()
			if (number != 0) {
				print ("[CDangIt] [AlarmManager] Adding '" + number.ToString () + "' alarms from '" + fm.ToString () + "'");
				loops.Add (fm, number);
			} else {
				print ("[CDangIt] [AlarmManager] No alarms added: Would have added 0 alarms");
			}
		}

		public void Update()
		{
			if (this.audio != null) {
				if (!this.audio.isPlaying){
					if (loops.Count > 0) {
						var element = loops.ElementAt (0);
						loops.Remove (element.Key);
						print ("[CDangIt] [AlarmManager] Playing Clip");
						audio.Play ();
						if (element.Value != 0 && element.Value != 1) {
							if (element.Key.vessel == FlightGlobals.ActiveVessel) {
								loops.Add (element.Key, element.Value - 1); //Only re-add if still has alarms
							} else {
								element.Key.AlarmsDoneCallback ();
							}
						} else {
							element.Key.AlarmsDoneCallback ();
						}
					}
				}
			}
		}

		public void RemoveAllAlarmsForModule(FailureModule fm)
		{
			print ("[CDangIt] [AlarmManager] Removing alarms...");
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

