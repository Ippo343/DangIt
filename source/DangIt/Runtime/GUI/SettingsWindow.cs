using DangIt.Utilities;
using UnityEngine;

namespace DangIt
{
    class SettingsWindow
    {
		private Rect settingsRect = new Rect(20, 20, 300, 150);
		string evaDistanceString = string.Empty;    // temp variable to edit the Max Distance in the GUI
		string SoundLoopsString_Low = string.Empty;    // temp variable to edit the Sound Loops in the GUI
		string SoundLoopsString_Medium = string.Empty;    // temp variable to edit the Sound Loops in the GUI
		string SoundLoopsString_High = string.Empty;    // temp variable to edit the Sound Loops in the GUI
		string SoundVolumeString = string.Empty;    // temp variable to edit the Sound Volume

		bool lastEnabledValue;
		bool waitingForConfirm=false;

        CDangIt.Settings newSettings;


        private bool enabled;
        public bool Enabled 
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (value) // Copy the current settings when the window is enabled
                {
					ReInitilize ();
                }
            }
        }

		private void ReInitilize(){ //Set our string data mirrors at start, and when we change settings
			this.newSettings = CDangIt.Instance.CurrentSettings.ShallowClone();
			this.evaDistanceString = newSettings.MaxDistance.ToString();
			this.SoundLoopsString_Low = newSettings.Pri_Low_SoundLoops.ToString();
			this.SoundLoopsString_Medium = newSettings.Pri_Medium_SoundLoops.ToString();
			this.SoundLoopsString_High = newSettings.Pri_High_SoundLoops.ToString();
			this.SoundVolumeString = newSettings.AlarmVolume.ToString ();
			this.lastEnabledValue = newSettings.EnabledForSave;
		}

        public void Draw()
        {
            // The settings are only available in the space center
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                settingsRect = GUILayout.Window("DangItSettings".GetHashCode(),
                                                settingsRect,
                                                SettingsWindowFcn,
                                                "Dang It! Settings",
                                                GUILayout.ExpandWidth(true),
                                                GUILayout.ExpandHeight(true));
            }
        }

        void SettingsWindowFcn(int windowID)
        {
			if (waitingForConfirm) {
				GUILayout.BeginVertical ();
				GUILayout.Label ("WARNING! Changing the state of DangIt! while ships are in flight is not supported.");
				GUILayout.Label ("There is no gaurentee that ships will remain in a stable state after toggle, ESPECIALLY if they currently have failed parts.");
				GUILayout.Label ("It is reccomended that this option is only changed immediatley after the start of a game AND while no ships are in flight");
				GUILayout.Label ("You currently have " + (FlightGlobals.Vessels.Count-1).ToString () + " vessels in flight. Are you sure you want to proceed?");
				GUILayout.Space (50);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Yes")) {
					lastEnabledValue = newSettings.EnabledForSave;
					waitingForConfirm = false;
					CDangIt.Instance.CurrentSettings = this.newSettings;
				}
				if (GUILayout.Button ("No")) {
					newSettings.EnabledForSave = lastEnabledValue;
					waitingForConfirm = false;
				}
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical ();
			} else {

				// Display the toggles and controls to read the new settings
				newSettings.EnabledForSave = GUILayout.Toggle (newSettings.EnabledForSave, "Enable");
				if (newSettings.EnabledForSave != this.lastEnabledValue) {
					waitingForConfirm = true;
				}

				if (newSettings.EnabledForSave) {
					newSettings.ManualFailures = GUILayout.Toggle (newSettings.ManualFailures, "Manual failures");
					newSettings.DebugStats = GUILayout.Toggle (newSettings.DebugStats, "Show Debug Stats");
					newSettings.Glow = GUILayout.Toggle (newSettings.Glow, "Glow");
					newSettings.RequireExperience = GUILayout.Toggle (newSettings.RequireExperience, "Check Experience");
					newSettings.Messages = GUILayout.Toggle (newSettings.Messages, "Messages");

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Max EVA distance: ");
					evaDistanceString = GUILayout.TextField (evaDistanceString);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Alarm Volume (0-100): ");
					SoundVolumeString = GUILayout.TextField (SoundVolumeString);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("# Times to beep for Priorities (-1=>Inf) of Failures");
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("LOW: ");
					SoundLoopsString_Low = GUILayout.TextField (SoundLoopsString_Low);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("MEDIUM: ");
					SoundLoopsString_Medium = GUILayout.TextField (SoundLoopsString_Medium);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("HIGH: ");
					SoundLoopsString_High = GUILayout.TextField (SoundLoopsString_High);
					GUILayout.EndHorizontal ();
				} else {
					GUILayout.Label ("DangIt! is disabled");
				}

				// Creates the button and returns true when it is pressed
				if (GUILayout.Button ("Apply")) {
					// Parse the strings
					this.newSettings.MaxDistance = CUtils.Parse<float> (evaDistanceString, defaultTo: 2f);
					this.newSettings.Pri_Low_SoundLoops = CUtils.Parse<int> (SoundLoopsString_Low, defaultTo: 0);
					this.newSettings.Pri_Medium_SoundLoops = CUtils.Parse<int> (SoundLoopsString_Medium, defaultTo: 2);
					this.newSettings.Pri_High_SoundLoops = CUtils.Parse<int> (SoundLoopsString_High, defaultTo: -1);
					int av = CUtils.Parse<int> (SoundVolumeString, defaultTo: 100);
					//av = (av < 0) ? 0 : (av > 100) ? 100 : av;  //This clamps it between 0 and 100 (or not)
					if (av < 1) {
						av = 1;
					} else if (av > 100) {
						av = 100;
					}
					this.newSettings.AlarmVolume = av;
					CDangIt.Instance.CurrentSettings = this.newSettings;
	                
					ReInitilize (); //Reinit string data in case you entered a invalid value (or went over cap in volume)
				}
			}

            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }
    }
}
