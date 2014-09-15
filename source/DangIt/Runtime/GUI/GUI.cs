using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CrewFilesInterface;

namespace ippo
{
    public partial class DangIt
    {
        ApplicationLauncherButton appBtn;
        RosterWindow rosterWindow = new RosterWindow();
        SettingsWindow settingsWindow = new SettingsWindow();


        void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (settingsWindow.Enabled) settingsWindow.Draw();
            if (rosterWindow.Enabled) rosterWindow.Draw();
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
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER ||
                    HighLogic.LoadedSceneIsEditor ||
                    HighLogic.LoadedSceneIsFlight)
                {

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

        void dummyVoid() { return; }


        void onAppBtnToggleOn()
        {
            this.settingsWindow.Enabled = true;
            this.rosterWindow.Enabled = true;
        }

        void onAppBtnToggleOff()
        {
            this.settingsWindow.Enabled = false;
            this.rosterWindow.Enabled = false;
        }

        

    }
}
