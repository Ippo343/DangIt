using DangIt.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DangIt
{
    [RequireComponent(typeof(AudioSource))]
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class AlarmManager : MonoBehaviour
	{
		public Dictionary<FailureModule, int> loops;
        AudioSource audio;

		public void Start()
		{
            print("[DangIt] [AlarmManager] Starting...");
            this.audio = this.gameObject.AddComponent<AudioSource>();
            
            print("[DangIt] [AlarmManager] Creating Clip");
            this.audio.clip = GameDatabase.Instance.GetAudioClip("DangIt/Sounds/alarm");  //Load alarm sound
            
            print("[DangIt] [AlarmManager] Setting Volume...");
			this.audio.spatialBlend = 0f; //This disable the game scaling volume with distance from source
			this.audio.volume = 1f;            

			print ("[DangIt] [AlarmManager] Creating Dictionary");
			this.loops=new Dictionary<FailureModule, int>(); //Reset counter, so on logic pass we play it
		}

		public void UpdateSettings(){
			float scaledVolume = CDangIt.Instance.CurrentSettings.AlarmVolume / 100f;
			print ("[DangIt][AlarmManager] Rescaling Volume (at UpdateSettings queue)..., now at " + scaledVolume);
			this.audio.volume = scaledVolume;
		}

		public void AddAlarm(FailureModule fm, int number)
		{
			this.audio.volume = CDangIt.Instance.CurrentSettings.GetMappedVolume(); //This seems like an OK place for this, because if I put it in the constructor...
			                                                                       // ...you would have to reboot to change it, but I don't want to add lag by adding it to each frame in Update()
			if (number != 0) {
				CUtils.Log("[AlarmManager] Adding '" + number.ToString () + "' alarms from '" + fm.ToString () + "'");
				loops.Add (fm, number);
			} else {
                CUtils.Log("[AlarmManager] No alarms added: Would have added 0 alarms");
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
			print ("[DangIt][AlarmManager] Removing alarms...");
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

