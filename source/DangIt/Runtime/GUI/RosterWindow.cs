using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CrewFilesInterface;

namespace ippo
{
    class RosterWindow
    {
        int selectionIdx = 0;
        Rect rosterRect = new Rect(300, 100, 400, 200);
        Vector2 scrollPos = new Vector2(0, 0);

        public bool Enabled { get; set; }


        public void Draw()
        {
            rosterRect = GUILayout.Window("DangItRoster".GetHashCode(),
                                          this.rosterRect,
                                          this.WindowFcn,
                                          "Dang It! Crew management",
                                          GUILayout.ExpandHeight(true),
                                          GUILayout.ExpandWidth(true)); 
        }


        public void WindowFcn(int windowID)
        {
            string kerbalName = string.Empty;

            GUILayout.BeginHorizontal();

            #region Left side: list of kerbals
            
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

            // In flight only show assigned crew
            ProtoCrewMember[] crew = HighLogic.LoadedSceneIsFlight ? 
                HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Assigned).ToArray() :
                HighLogic.CurrentGame.CrewRoster.Crew.ToArray();

            selectionIdx = GUILayout.SelectionGrid(selectionIdx,
                                                   crew.Select(k => k.name).ToArray(),
                                                   xCount: 1);

            kerbalName = crew.ElementAt(selectionIdx).name;

            GUILayout.EndScrollView(); 

            #endregion

            #region Right side: kerbal data

            string text = "No information available";
            if (CrewFilesManager.CrewFilesInstalled &&
                CrewFilesManager.Server != null &&
                CrewFilesManager.Server.Contains(kerbalName))
            {
                text = CrewFilesManager.Server.GetKerbalFile(kerbalName).GetNode(PerkGenerator.NodeName).ToString();
            }

            GUILayout.TextArea(text, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)); 

            #endregion

            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
    }
}
