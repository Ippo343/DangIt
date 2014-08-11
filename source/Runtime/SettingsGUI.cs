using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ippo
{
    public partial class DangIt
    {
        ApplicationLauncherButton appBtn;
        DangIt.Settings newSettings = null;

        private Rect windowRect = new Rect(20, 20, 200, 200);
        bool showGUI = false;


        void OnLauncherReady()
        {
            if (ApplicationLauncher.Ready)
            {
                try
                {
                    Debug.Log("About to add the app button...");

                    Texture btnTex = GameDatabase.Instance.GetTexture("DangIt/Textures/appBtn", false);
                    if (btnTex == null)
                        throw new Exception("The texture wasn't loaded!");

                    appBtn = ApplicationLauncher.Instance.AddModApplication(
                                onAppBtnToggleOn,
                                onAppBtnToggleOff,
                                dummyVoid,
                                dummyVoid,
                                dummyVoid,
                                dummyVoid,
                                ApplicationLauncher.AppScenes.SPACECENTER,
                                btnTex);
                }
                catch (Exception e)
                {
                    Debug.Log("[DangIt]: Error! " + e.Message);
                    throw;
                }
            }
        }


        void onAppBtnToggleOn()
        {
            newSettings = this.currentSettings.ShallowClone();
            this.showGUI = true;            
        }


        void onAppBtnToggleOff()
        {
            newSettings = null;
            this.showGUI = false;
        }


        void dummyVoid()
        { 
            return;
        }


        void OnGUI()
        {
            if (showGUI)
            {
                GUI.skin = HighLogic.Skin;
                windowRect = GUILayout.Window("DangItSettings".GetHashCode(), windowRect, WindowFunction, "DangIt! Settings",
                    GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); 
            }
        }


        // This is where the GUI is actually drawn
        void WindowFunction(int windowID)
        {
            // Display the toggles and controls to read the new settings
            newSettings.ManualFailures = GUILayout.Toggle(newSettings.ManualFailures, "Manual failures");
            newSettings.Glow = GUILayout.Toggle(newSettings.Glow, "Glow");
            newSettings.Messages = GUILayout.Toggle(newSettings.Messages, "Messages");

            if (GUILayout.Button("OK"))
            {
                Debug.Log("[DangIt]: Applying new settings from GUI");
                DangIt.Instance.currentSettings = this.newSettings;
                showGUI = false;
            }

            GUI.DragWindow();
        }

    }
}
