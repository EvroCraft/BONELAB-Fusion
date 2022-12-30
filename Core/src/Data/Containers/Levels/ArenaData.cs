﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;
using SLZ.Vehicle;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

namespace LabFusion.Data {
    public static class ArenaData {
        public static Arena_GameController GameController;
        public static GenGameControl_Display GameControlDisplay;
        public static ArenaMenuController MenuController;
        public static GeoManager GeoManager;
        public static ChallengeSelectMenu[] ChallengeSelections;
        public static void OnCacheArenaInfo() {
            GameController = GameObject.FindObjectOfType<Arena_GameController>();

            if (GameController != null) {
                ChallengeSelections = GameObject.FindObjectsOfType<ChallengeSelectMenu>(true);
                GameControlDisplay = GameObject.FindObjectOfType<GenGameControl_Display>(true);
                MenuController = GameObject.FindObjectOfType<ArenaMenuController>(true);
                GeoManager = GameObject.FindObjectOfType<GeoManager>(true);
            }
        }

        public static bool IsInArena => !GameController.IsNOC();

        public static bool HasChallengeSelect(ChallengeSelectMenu menu) {
            foreach (var otherMenu in ChallengeSelections) {
                if (otherMenu == menu)
                    return true;
            }

            return false;
        }

        public static byte? GetIndex(ChallengeSelectMenu menu) {
            for (byte i = 0; i < ChallengeSelections.Length; i++) {
                if (ChallengeSelections[i] == menu) {
                    return i;
                }
            }

            return null;
        }

        public static ChallengeSelectMenu GetMenu(byte index)
        {
            if (ChallengeSelections != null && ChallengeSelections.Length > index)
                return ChallengeSelections[index];
            return null;
        }
    }
}
