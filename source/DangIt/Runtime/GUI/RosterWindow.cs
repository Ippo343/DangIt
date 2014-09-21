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
        delegate bool RosterFilter(ProtoCrewMember k);

        int selectionIdx = 0;
        Rect rosterRect = new Rect(300, 100, 600, 300);
        Vector2 scrollPos = new Vector2(0, 0);

        bool showCrew;
        bool showAssigned;
        bool showHired;
        bool showApplicants;

        public bool Enabled { get; set; }


        public RosterWindow()
        {
            showCrew = HighLogic.LoadedSceneIsFlight;
            showAssigned = !HighLogic.LoadedSceneIsFlight;
            showHired = !HighLogic.LoadedSceneIsFlight;
            showApplicants = !HighLogic.LoadedSceneIsFlight;
        }


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

            #region Filters
            GUILayout.BeginVertical();
             
            // If no filter is selected, apply a default filter
            if (!(this.showCrew || this.showAssigned || this.showHired || this.showApplicants))
            {
                if (HighLogic.LoadedSceneIsFlight)
                    showCrew = true;    // in flight, default filter is crew
                else
                    showHired = true;   // outside, default filter is hired
            }

            // Create the toggles to select the filter
            showCrew = (HighLogic.LoadedSceneIsFlight) ? GUILayout.Toggle(showCrew, "Crew") : false; // crew is not available when not in flight
            showAssigned = GUILayout.Toggle(showAssigned, "Assigned");
            showHired = GUILayout.Toggle(showHired, "Hired");
            showApplicants = GUILayout.Toggle(showApplicants, "Applicants");            

            // Filter function that selects the kerbals from the roster based on the user's selection
            RosterFilter filter = 
                k => (showCrew && (FlightGlobals.ActiveVessel != null) ? FlightGlobals.ActiveVessel.GetVesselCrew().Contains(k) : false)
                  || (showAssigned && k.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                  || (showHired && HighLogic.CurrentGame.CrewRoster.Crew.Contains(k))
                  || (showApplicants && HighLogic.CurrentGame.CrewRoster.Applicants.Contains(k));

            GUILayout.EndVertical();

            #endregion

            #region List of kerbals

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

            // Join the crew and applicants rosters into one
            var allKerbals = HighLogic.CurrentGame.CrewRoster.Applicants.Concat(
                             HighLogic.CurrentGame.CrewRoster.Crew);

            // Filter them using the closure created above
            ProtoCrewMember[] selectedKerbals = allKerbals.Where(k => filter(k)).ToArray();

            if (selectedKerbals.Count() > 0)
            {
                selectionIdx = GUILayout.SelectionGrid(selectionIdx,
                                                           selectedKerbals.Select(k => k.name).ToArray(),
                                                           xCount: 1);
                kerbalName = selectedKerbals.ElementAt(selectionIdx).name; 
            }

            GUILayout.EndScrollView(); 

            #endregion

            #region Right side: kerbal data

            // Fetch the data from CrewFiles
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
