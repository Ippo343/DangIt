using KSP.UI.Screens;
using System;
using System.Collections;
using UnityEngine;

namespace DangIt
{
    public partial class CDangIt
    {        
        ApplicationLauncherButton appBtn;
        SettingsWindow settingsWindow = new SettingsWindow();

        void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (settingsWindow.Enabled) settingsWindow.Draw();
        }


        /// <summary>
        /// Coroutine that creates the button in the toolbar. Will wait for the runtime AND the launcher to be ready
        /// before creating the button.
        /// </summary>
        IEnumerator AddAppButton()
        {
            while (!ApplicationLauncher.Ready || !this.IsReady)
                yield return null;

            try
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    // Load the icon for the button
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
                                ApplicationLauncher.AppScenes.ALWAYS,
                                btnTex);
                }
                
            }
            catch (Exception e)
            {
                this.Log("Error! " + e.Message);
                throw e;
            }
        }

        // The AppLauncher requires a callback for some events that are not used by this plugin
        void dummyVoid() { return; }


        void onAppBtnToggleOn()
        {
            this.settingsWindow.Enabled = true;
        }

        void onAppBtnToggleOff()
        {
            this.settingsWindow.Enabled = false;
        }

    }
}
