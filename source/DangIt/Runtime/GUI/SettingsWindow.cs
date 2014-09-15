using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    class SettingsWindow
    {
        private Rect settingsRect = new Rect(20, 20, 200, 150);
        string evaDistanceString = string.Empty;    // temp variable to edit the Max Distance in the GUI

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
            evaDistanceString = GUILayout.TextField(evaDistanceString);
            GUILayout.EndHorizontal();

            // Creates the button and returns true when it is pressed
            if (GUILayout.Button("OK"))
            {
                // Parse the string
                this.newSettings.MaxDistance = DangIt.Parse<float>(evaDistanceString, defaultTo: 2f);
                DangIt.Instance.CurrentSettings = this.newSettings;
                this.Enabled = false;
            }

            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }
    }
}
