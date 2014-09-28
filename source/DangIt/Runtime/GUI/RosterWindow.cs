using CrewFilesInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ippo
{
    class RosterWindow
    {
        delegate bool RosterFilter(ProtoCrewMember k);

        Rect rosterRect = new Rect(300, 100, 800, 300);

        int kerbalSelectionIdx = 0;
        Vector2 kerbalScrollPos = new Vector2(0, 0);

        int perkSelectionIdx = 0;
        Vector2 perksScrollPos = new Vector2(0, 0);

        KerbalFilter activeFilters;
        KerbalFilter previousFilters;

        public bool Enabled { get; set; }


        public RosterWindow()
        {
            // Initialize the default filters
            activeFilters.Crew = HighLogic.LoadedSceneIsFlight;
            activeFilters.Assigned = !HighLogic.LoadedSceneIsFlight;
            activeFilters.Hired = !HighLogic.LoadedSceneIsFlight;
            activeFilters.Applicants = !HighLogic.LoadedSceneIsFlight;

            previousFilters = activeFilters;
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
            GUILayout.BeginHorizontal();

            // Each of these methods creates its own GUI components
            RosterFilter filter = CreateFilter();
            ProtoCrewMember kerbal = SelectKerbal(filter);
            if (kerbal != null) ListAndUpgradePerks(kerbal);

            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }



        private RosterFilter CreateFilter()
        {
            GUILayout.BeginVertical();

            // Save state
            previousFilters = activeFilters;

            // crew is not available when not in flight
            activeFilters.Crew = (HighLogic.LoadedSceneIsFlight) ? GUILayout.Toggle(activeFilters.Crew, "Crew") : false;
            activeFilters.Assigned = GUILayout.Toggle(activeFilters.Assigned, "Assigned");
            activeFilters.Hired = GUILayout.Toggle(activeFilters.Hired, "Hired");
            activeFilters.Applicants = GUILayout.Toggle(activeFilters.Applicants, "Applicants");

            GUILayout.EndVertical();

            return k =>
                (activeFilters.Crew && (HighLogic.LoadedSceneIsFlight) ? FlightGlobals.ActiveVessel.GetVesselCrew().Contains(k) : false)
             || (activeFilters.Assigned && k.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
             || (activeFilters.Hired && HighLogic.CurrentGame.CrewRoster.Crew.Contains(k))
             || (activeFilters.Applicants && HighLogic.CurrentGame.CrewRoster.Applicants.Contains(k));
        }



        private ProtoCrewMember SelectKerbal(RosterFilter filter)
        {
            // Filter the roster using the filter selected by the user
            var allKerbals = HighLogic.CurrentGame.CrewRoster.Applicants.Concat(
                             HighLogic.CurrentGame.CrewRoster.Crew);

            var selectedKerbals = allKerbals.Where(k => filter(k));

            // The user has changed the toggles: reset the index so that it doesn't go out of range
            if (activeFilters != previousFilters)
                kerbalSelectionIdx = 0;

            if (selectedKerbals.Count() > 0)
            {
                kerbalScrollPos = GUILayout.BeginScrollView(kerbalScrollPos, GUIStyle.none);

                kerbalSelectionIdx = GUILayout.SelectionGrid(kerbalSelectionIdx,
                                                             selectedKerbals.Select(k => k.name).ToArray(),
                                                             xCount: 1);
                GUILayout.EndScrollView();

                return selectedKerbals.ElementAt(kerbalSelectionIdx);
            }
            else
            {
                GUILayout.Label("No kerbal matches your filter.", HighLogic.Skin.button);
                return null;
            }
        }



        private void ListAndUpgradePerks(ProtoCrewMember kerbal)
        {
            try 
	        {
                // Fetch the perks from crewfiles
		        List<Perk> perks = Perk.FromNode(kerbal.GetPerksNode());

                // List them in a selection grid with scrollview
                perksScrollPos = GUILayout.BeginScrollView(perksScrollPos, false, false);
                perkSelectionIdx = GUILayout.SelectionGrid(perkSelectionIdx,
                                                           perks.Select(p => p.Specialty.ToString()).ToArray(),
                                                           xCount: 1);
                GUILayout.EndScrollView();

                // Show the button to upgrade perks
                UpgradePerkButton(kerbal, perks, perkSelectionIdx);
	        }
	        catch (ServerNotInstalledException)
	        {
                GUILayout.Label("CrewFiles is not installed!");
                return;
	        }
            catch (ServerUnavailableException)
            {
                GUILayout.Label("Something is wrong with CrewFiles");
                return;
            }
            catch (Exception e)
            {
                GUILayout.Label("An exception occurred: " + e.Message + e.StackTrace);
                return;
            }
        }


        private void UpgradePerkButton(ProtoCrewMember kerbal, List<Perk> perks, int idx)
        {
            GUILayout.BeginVertical();


            // First, show a label (styled like a button) with the current level
            GUILayout.Label("Current:\n" + perks[idx].SkillLevel.ToString(),
                            HighLogic.Skin.button);

            
            SkillLevel nextLevel = GetNextLevel(perks[idx].SkillLevel);

            if (nextLevel == perks[idx].SkillLevel) // max level reached
            {
                GUILayout.Label("Max level", HighLogic.Skin.button);
            }
            else // Create upgrade button
            {
                Perk.UpgradeCost cost = DangIt.Instance.trainingCosts[nextLevel];

                string btnLabel = "Upgrade to " + nextLevel.ToString() + "\n" +
                                  "Funds: " + cost.Funds + "\n" +
                                  "Science: " + cost.Science;

                if (GUILayout.Button(btnLabel))
                {
                    Debug.Log("Requested upgrade to " + nextLevel.ToString());

                    if (CheckOutAndSpendResources(cost))
                    {
                        perks[idx].SkillLevel++;
                        kerbal.SetPerks(perks);
                    }
                    else
                    {
                        DangIt.Broadcast("You don't have enough resources for the training!", true);
                    }
                }
            }

            GUILayout.EndVertical();
        }



        private static bool CheckOutAndSpendResources(Perk.UpgradeCost cost)
        {
            switch (HighLogic.CurrentGame.Mode)
            {
                case Game.Modes.CAREER:

                    if (Funding.Instance.Funds < cost.Funds) return false;
                    if (ResearchAndDevelopment.Instance.Science < cost.Science) return false;

                    Funding.Instance.Funds -= cost.Funds;
                    ResearchAndDevelopment.Instance.Science -= cost.Science;

                    return true;


                case Game.Modes.SCIENCE_SANDBOX:

                    if (ResearchAndDevelopment.Instance.Science < cost.Science) return false;
                    ResearchAndDevelopment.Instance.Science -= cost.Science;

                    return true;


                case Game.Modes.SANDBOX:
                    return true;


                default:
                    return true;
            }
        }



        private static SkillLevel GetNextLevel(SkillLevel current)
        {
            int maxLevel = Enum.GetValues(typeof(SkillLevel)).Cast<int>().Max();

            // Sum 1, clamp to maxLevel, cast to SkillLevel
            SkillLevel nextLevel = (SkillLevel)(Math.Min((int)current + 1, maxLevel));

            return nextLevel;
        }
    }


    struct KerbalFilter : IEquatable<KerbalFilter>
    {
        public bool Crew;
        public bool Assigned;
        public bool Hired;
        public bool Applicants;

        #region Standard overrides

        public bool Equals(KerbalFilter other)
        {
            return this.Crew == other.Crew &&
                   this.Assigned == other.Assigned &&
                   this.Hired == other.Hired &&
                   this.Applicants == other.Applicants;
        }

        public override int GetHashCode() { return base.GetHashCode(); }

        public override string ToString()
        {
            return String.Format("(Crew: {0}, Assigned: {1}, Hired: {2}, Applicants: {3})", Crew, Assigned, Hired, Applicants);
        }

        public override bool Equals(object obj)
        {
            if (obj is KerbalFilter) { return Equals((KerbalFilter)obj); }
            return false;
        }

        public static bool operator ==(KerbalFilter a, KerbalFilter b) { return a.Equals(b); }
        public static bool operator !=(KerbalFilter a, KerbalFilter b) { return !(a == b); }

        #endregion
    }

}
