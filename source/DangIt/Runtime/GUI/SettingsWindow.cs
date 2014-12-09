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
		string SoundLoopsString = string.Empty;    // temp variable to edit the Sound Loops in the GUI

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
			evaDistanceString = GUILayout.TextField(DangIt.Instance.CurrentSettings.MaxDistance.ToString());
            GUILayout.EndHorizontal();

			newSettings.SoundNotifications = GUILayout.Toggle(newSettings.SoundNotifications, "Sound Notification");

			GUILayout.BeginHorizontal();
			GUILayout.Label("# Loops (-1=Inf): ");
			SoundLoopsString = GUILayout.TextField(DangIt.Instance.CurrentSettings.SoundLoops.ToString());
			GUILayout.EndHorizontal();

            // Creates the button and returns true when it is pressed
            if (GUILayout.Button("Apply"))
            {
                // Parse the string
				this.newSettings.MaxDistance = DangIt.Parse<float>(evaDistanceString, defaultTo: 2f);
				this.newSettings.SoundLoops = DangIt.Parse<int>(SoundLoopsString, defaultTo: 10);
                DangIt.Instance.CurrentSettings = this.newSettings;
            }

            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }
    }
}
