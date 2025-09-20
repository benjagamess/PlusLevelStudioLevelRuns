using UnityEngine;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using PlusLevelStudio;
using System.Collections.Generic;

namespace PlusLevelStudio.LevelRuns
{
    [BepInPlugin("benjagamess.plusmods.customlevelruns", "PLS Level Runs", "1.0.0.0")]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudio")]
    public class LevelStudioRunsPlugin : BaseUnityPlugin
    {
        public static LevelStudioRunsPlugin Instance { get; private set; }

        public bool inCustomRun = false;
        public List<PlayableEditorLevel> levelOrder = new List<PlayableEditorLevel>();
        public int currentLevel = 0;
        public LifeMode lifeMode;
        public SceneObject pitstop = null;

        private void Awake()
        {
            new Harmony("benjagamess.plusmods.customlevelruns").PatchAllConditionals();

            Instance = this;
        }
    }
}
