using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public partial class DangIt
    {
        // Button object: displayed in the stock toolbar to open the settings window
        ApplicationLauncherButton appBtn;
        
        // Working copy for the new settings, will be copied over the old ones
        DangIt.Settings newSettings = null;

        // temp variable to edit the Max Distance in the GUI
        string evaDistanceString = string.Empty;

        // Default window position / size
        private Rect windowRect = new Rect(20, 20, 200, 200);

        // Enables / disables the window
        bool showGUI = false;


        /// <summary>
        /// Coroutine that creates the button in the toolbar.
        /// Will wait for the runtime AND the launcher to be ready
        /// before creating the button.
        /// </summary>
        IEnumerator AddAppButton()
        {
            while (!ApplicationLauncher.Ready || !this.IsReady)
                yield return null;

            try
            {
                this.Log("About to add the app button...");

                Texture btnTex = GameDatabase.Instance.GetTexture("DangIt/Textures/appBtn", false);
                if (btnTex == null)
                    throw new Exception("The button texture wasn't loaded!");

                appBtn = ApplicationLauncher.Instance.AddModApplication(
                            onAppBtnToggleOn,
                            onAppBtnToggleOff,
                            dummyVoid,  // ignore callbacks for more elaborate events
                            dummyVoid,
                            dummyVoid,
                            dummyVoid,
                            ApplicationLauncher.AppScenes.SPACECENTER,
                            btnTex);
            }
            catch (Exception e)
            {
                this.Log("Error! " + e.Message);
                throw e;
            }
        }

        void dummyVoid() { return; }


        void onAppBtnToggleOn()
        {
            // Copy the current settings to a working copy that can be edited
            // in the GUI without modifying directly the settings
            newSettings = this.CurrentSettings.ShallowClone();
            evaDistanceString = CurrentSettings.MaxDistance.ToString();

            this.showGUI = true;            
        }


        void onAppBtnToggleOff()
        {
            // Delete the working copy
            newSettings = null;
            this.showGUI = false;
        }


        // Unity calls this function at every update if the behaviour is enabled
        // This is the starting point for the GUI code
        void OnGUI()
        {
            if (showGUI)
            {
                // To use the automatic layout, you must use GUILayout.Window
                // and give it a callback to the function that creates the window
                GUI.skin = HighLogic.Skin;
                windowRect = GUILayout.Window("DangItSettings".GetHashCode(), windowRect, WindowFunction, "DangIt! Settings",
                    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); 
            }
        }


        // This is the GUI callback that actually draws the GUI
        void WindowFunction(int windowID)
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
                this.newSettings.MaxDistance = DangIt.Parse<float>(evaDistanceString, defaultTo: 1f);

                this.Log("Applying the new settings selected from GUI. New settings:\n" + newSettings.ToNode().ToString());
                DangIt.Instance.CurrentSettings = this.newSettings;

                //showGUI = false;
                // "Click" on the app button to close the window
                appBtn.SetFalse(makeCall: true);
            }

            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }

    }
}
