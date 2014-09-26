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
            ListPerks(kerbal);

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


        private void ListPerks(ProtoCrewMember kerbal)
        {
            // No kerbal selected, nothing to do here
            if (kerbal == null) return;

            if (CrewFilesManager.IsReady && CrewFilesManager.Server.Contains(kerbal))
            {
                #region Perks scrollview

                ConfigNode perksNode = CrewFilesManager.Server
                                       .GetKerbalFile(kerbal)
                                       .GetNode(PerkGenerator.NodeName);
                List<Perk> perks = Perk.FromNode(perksNode);

                // List them in a selection grid with scrollview
                perksScrollPos = GUILayout.BeginScrollView(perksScrollPos, false, false);
                perkSelectionIdx = GUILayout.SelectionGrid(perkSelectionIdx,
                                                           perks.Select(p => p.Specialty.ToString()).ToArray(),
                                                           xCount: 1);
                GUILayout.EndScrollView(); 

                #endregion

                // Only show upgrades for kerbals that are available (they are at the KSC, so they have time to study)
                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                    UpgradePerkButton(kerbal, perks[perkSelectionIdx]);

            }
            else
                GUILayout.Label("There seems to be some problem with CrewFiles",
                                GUILayout.ExpandHeight(true),
                                GUILayout.ExpandWidth(true));
        }


        private void UpgradePerkButton(ProtoCrewMember kerbal, Perk selectedPerk)
        {
            // Two lines: a label with the current level
            // and a button to upgrade to the next level
            GUILayout.BeginVertical();

            GUILayout.Label("Current:\n" + selectedPerk.SkillLevel.ToString(), HighLogic.Skin.button);

            SkillLevel nextLevel = GetNextLevel(selectedPerk.SkillLevel);

            if (nextLevel == selectedPerk.SkillLevel) // max level reached
            {
                GUILayout.Label("Max level", HighLogic.Skin.button);
            }
            else // Upgrade the perk
            {
                Perk.UpgradeCost cost = DangIt.Instance.trainingCosts[nextLevel];

                string btnLabel = "Upgrade to " + nextLevel.ToString() + "\n" +
                                  "Funds: " + cost.Funds + "\n" +
                                  "Science: " + cost.Science;

                if (GUILayout.Button(btnLabel))
                {
                    Debug.Log("Requested upgrade to " + nextLevel.ToString());

                    bool hasEnoughResources = true;

                    switch (HighLogic.CurrentGame.Mode)
                    {
                        // In career mode, you need both funds and science
                        case Game.Modes.CAREER:
                            if (Funding.Instance.Funds < cost.Funds) hasEnoughResources = false;
                            if (ResearchAndDevelopment.Instance.Science < cost.Science) hasEnoughResources = false;
                            break;
                        
                        // In science mode, you only need science
                        case Game.Modes.SCIENCE_SANDBOX:
                            if (ResearchAndDevelopment.Instance.Science < cost.Science) hasEnoughResources = false;
                            break;

                        // In sandbox you have no limits
                        case Game.Modes.SANDBOX:
                            hasEnoughResources = true;
                            break;

                        default:
                            hasEnoughResources = true;
                            break;
                    }

                    if (hasEnoughResources)
                    {
                        ConfigNode perksNode = CrewFilesManager.Server
                                              .GetKerbalFile(kerbal)
                                              .GetNode(PerkGenerator.NodeName);
                        List<Perk> perks = Perk.FromNode(perksNode);

                        // Subtract the resources
                        if (Funding.Instance != null) Funding.Instance.Funds -= cost.Funds;
                        if (ResearchAndDevelopment.Instance != null) ResearchAndDevelopment.Instance.Science -= cost.Science;

                        // Increase the skill level
                        perks.Find(p => p == selectedPerk).SkillLevel++;

                        // Re-assign in CrewFiles
                        perksNode = perks.ToNode();
                    }
                    else
                    {
                        DangIt.Broadcast("You don't have enough resources for the training!", true);
                    }
                }
            }

            GUILayout.EndVertical();
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
