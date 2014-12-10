using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    class SettingsWindow
    {
		private Rect settingsRect = new Rect(20, 20, 300, 150);
		string evaDistanceString = string.Empty;    // temp variable to edit the Max Distance in the GUI
		string SoundLoopsString_Low = string.Empty;    // temp variable to edit the Sound Loops in the GUI
		string SoundLoopsString_Medium = string.Empty;    // temp variable to edit the Sound Loops in the GUI
		string SoundLoopsString_High = string.Empty;    // temp variable to edit the Sound Loops in the GUI

        DangIt.Settings newSettings;


        private bool enabled;
        public bool Enabled 
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (value) // Copy the current settings when the window is enabled
                {
                    this.newSettings = DangIt.Instance.CurrentSettings.ShallowClone();
					this.evaDistanceString = newSettings.MaxDistance.ToString();
					this.SoundLoopsString_Low = newSettings.Pri_Low_SoundLoops.ToString();
					this.SoundLoopsString_Medium = newSettings.Pri_Medium_SoundLoops.ToString();
					this.SoundLoopsString_High = newSettings.Pri_High_SoundLoops.ToString();
                }
            }
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
            // Display the toggles and controls to read the new settings
            newSettings.ManualFailures = GUILayout.Toggle(newSettings.ManualFailures, "Manual failures");
            newSettings.Glow = GUILayout.Toggle(newSettings.Glow, "Glow");
			newSettings.Messages = GUILayout.Toggle(newSettings.Messages, "Messages");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max EVA distance: ");
            evaDistanceString = GUILayout.TextField(evaDistanceString);
            GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("# Times to beep for Priorities (-1=>Inf) of Failures");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("LOW: ");
			SoundLoopsString_Low = GUILayout.TextField(SoundLoopsString_Low);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("MEDIUM: ");
			SoundLoopsString_Medium = GUILayout.TextField(SoundLoopsString_Medium);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("HIGH: ");
			SoundLoopsString_High = GUILayout.TextField(SoundLoopsString_High);
			GUILayout.EndHorizontal();

            // Creates the button and returns true when it is pressed
            if (GUILayout.Button("Apply"))
            {
				// Parse the strings
				this.newSettings.MaxDistance = DangIt.Parse<float>(evaDistanceString, defaultTo: 2f);
				this.newSettings.Pri_Low_SoundLoops = DangIt.Parse<int>(SoundLoopsString_Low, defaultTo: 0);
				this.newSettings.Pri_Medium_SoundLoops = DangIt.Parse<int>(SoundLoopsString_Medium, defaultTo: 2);
				this.newSettings.Pri_High_SoundLoops = DangIt.Parse<int>(SoundLoopsString_High, defaultTo: -1);
                DangIt.Instance.CurrentSettings = this.newSettings;
            }

            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }
    }
}
