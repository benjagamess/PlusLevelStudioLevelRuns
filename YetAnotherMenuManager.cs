using PlusLevelStudio.Menus;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using Rewired.Interfaces;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.LevelRuns
{
    public class CreateRunScreenManager : MonoBehaviour
    {
        private bool initialized = false;
        private int floorAmount = 2;
        private List<PlayableEditorLevel> hideSeekLevels;
        private LevelSortingType levelSortingType = LevelSortingType.Random;
        private LifeMode lifeMode = LifeMode.Normal;

        private GameObject createRunButton;
        private GameObject createRunScreen;
        private TMP_Text numFloorsTxt;
        private TMP_Text levelSortTypeTxt;
        private TMP_Text lifeModeTxt;
        private EditorPlayScreenManager playScreenManager;

        public void Initialize()
        {
            playScreenManager = transform.Find("PlayScreen").GetComponent<EditorPlayScreenManager>();
            createRunButton = playScreenManager.transform.Find("CreateRunButton").gameObject;
            createRunScreen = transform.Find("CreateRunScreen").gameObject;
            numFloorsTxt = createRunScreen.transform.Find("NumFloors").GetComponent<TMP_Text>();
            levelSortTypeTxt = createRunScreen.transform.Find("LevelSortType").GetComponent<TMP_Text>();
            lifeModeTxt = createRunScreen.transform.Find("LifeMode").GetComponent<TMP_Text>();

            initialized = true;
        }

        private void Update()
        {
            if (initialized)
            {
                List<PlayableEditorLevel> levels = new List<PlayableEditorLevel>();
                foreach (PlayableEditorLevel lvl in playScreenManager.playableLevels)
                {
                    if (LocalizationManager.Instance.GetLocalizedText(LevelStudioPlugin.Instance.gameModeAliases[lvl.meta.gameMode].nameKey) == "Hide & Seek")
                    {
                        levels.Add(lvl);
                    }
                }
                hideSeekLevels = levels.ToList();

                if (playScreenManager.gameObject.activeInHierarchy && hideSeekLevels.Count >= 2)
                {
                    createRunButton.SetActive(true);
                }
                else if (createRunButton.activeInHierarchy)
                {
                    createRunButton.SetActive(false);
                }

                if (createRunScreen.activeInHierarchy)
                {
                    if (numFloorsTxt != null)
                    {
                        numFloorsTxt.text = floorAmount.ToString();
                    }
                    if (levelSortTypeTxt != null)
                    {
                        string[] sortingTypes = new string[]
                        {
                            "Random",
                            "Book Count",
                            "Level Size"
                        };

                        levelSortTypeTxt.text = "Level Order: " + sortingTypes[(int)levelSortingType];
                        lifeModeTxt.text = "Life Mode: " + lifeMode.ToString();
                    }
                }
                else
                {
                    floorAmount = hideSeekLevels.Count;
                }
            }
        }

        public void IncreaseFloorAmount()
        {
            floorAmount++;

            if (floorAmount > hideSeekLevels.Count)
            {
                floorAmount = 2;
            }
        }

        public void DecreaseFloorAmount()
        {
            floorAmount--;

            if (floorAmount < 2)
            {
                floorAmount = hideSeekLevels.Count;
            }
        }

        public void CycleLevelSortingType()
        {
            switch (levelSortingType)
            {
                case LevelSortingType.Random:
                    levelSortingType = LevelSortingType.NotebookCount;
                    break;
                case LevelSortingType.NotebookCount:
                    levelSortingType = LevelSortingType.LevelSize;
                    break;
                case LevelSortingType.LevelSize:
                    levelSortingType = LevelSortingType.Random;
                    break;
            }
        }

        public void CycleLifeMode()
        {
            switch (lifeMode)
            {
                case LifeMode.Normal:
                    lifeMode = LifeMode.Arcade;
                    break;
                case LifeMode.Arcade:
                    lifeMode = LifeMode.Intense;
                    break;
                case LifeMode.Intense:
                    lifeMode = LifeMode.Normal;
                    break;
            }
        }

        public void Play()
        {
            LevelStudioRunsPlugin.Instance.inCustomRun = true;
            LevelStudioRunsPlugin.Instance.levelOrder.Clear();
            LevelStudioRunsPlugin.Instance.currentLevel = 0;

            switch (levelSortingType)
            {
                case LevelSortingType.Random:
                    LevelStudioRunsPlugin.Instance.levelOrder = hideSeekLevels.ToList();
                    LevelStudioRunsPlugin.Instance.levelOrder.Shuffle();
                    break;
                case LevelSortingType.NotebookCount:
                    Dictionary<PlayableEditorLevel, int> levelAndItsNBCount = new Dictionary<PlayableEditorLevel, int>();
                    foreach (PlayableEditorLevel lvl in hideSeekLevels)
                    {
                        int count = 0;
                        foreach (RoomInfo info in lvl.data.rooms)
                        {
                            if (info.activity != null)
                            {
                                count++;
                            }
                        }
                        levelAndItsNBCount.Add(lvl, count);
                    }
                    var levels = levelAndItsNBCount.OrderBy(x => x.Value).ToList();
                    for (int i = 0; i < levels.Count; i++)
                    {
                        LevelStudioRunsPlugin.Instance.levelOrder.Add(levels.ElementAt(i).Key);
                    }
                    break;
                case LevelSortingType.LevelSize:
                    Dictionary<PlayableEditorLevel, int> levelAndItsSize = new Dictionary<PlayableEditorLevel, int>();
                    foreach (PlayableEditorLevel lvl in hideSeekLevels)
                    {
                        levelAndItsSize.Add(lvl, (lvl.data.levelSize.x * lvl.data.levelSize.y));
                    }
                    var levelss = levelAndItsSize.OrderBy(x => x.Value).ToList();
                    for (int i = 0; i < levelAndItsSize.Count; i++)
                    {
                        LevelStudioRunsPlugin.Instance.levelOrder.Add(levelss.ElementAt(i).Key);
                    }
                    break;
            }

            LevelStudioRunsPlugin.Instance.lifeMode = lifeMode;

            for (int i = 0; i < hideSeekLevels.Count - floorAmount; i++)
            {
                LevelStudioRunsPlugin.Instance.levelOrder.RemoveAt(0);
            }
            if (LevelStudioRunsPlugin.Instance.pitstop == null)
            {
                LevelStudioRunsPlugin.Instance.pitstop = GetPitstop();
            }

            // I need to do sceneobject changes and other small changes here. So no, I won't call EditorPlayModeManager.LoadLevel
            PlayableEditorLevel level = LevelStudioRunsPlugin.Instance.levelOrder[0];

            EditorPlayModeManager pmm = GameObject.Instantiate<EditorPlayModeManager>(LevelStudioPlugin.Instance.assetMan.Get<EditorPlayModeManager>("playModeManager"));
            pmm.customContent = new EditorCustomContent();
            pmm.customContent.LoadFromPackage(level.meta.contentPackage);
            SceneObject sceneObj = LevelImporter.CreateSceneObject(level.data);
            sceneObj.manager = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab;
            sceneObj.levelTitle = "F1";
            sceneObj.levelNo = 0;
            sceneObj.totalShopItems = Random.Range(4, 7);
            sceneObj.shopItems = Resources.FindObjectsOfTypeAll<SceneObject>().First(x => x.name == "MainLevel_1").shopItems;
            sceneObj.mapPrice = 250 * LevelStudioRunsPlugin.Instance.levelOrder.Count;
            GameLoader loader = GameObject.Instantiate<GameLoader>(LevelStudioPlugin.Instance.assetMan.Get<GameLoader>("gameLoaderPrefab"));
            ElevatorScreen screen = GameObject.Instantiate<ElevatorScreen>(LevelStudioPlugin.Instance.assetMan.Get<ElevatorScreen>("elevatorScreenPrefab"));
            pmm.returnToEditor = false;
            if (level.meta.modeSettings != null)
            {
                BaseGameManager modifiedManager = GameObject.Instantiate<BaseGameManager>(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab, MTM101BaldAPI.MTM101BaldiDevAPI.prefabTransform);
                modifiedManager.name = modifiedManager.name.Replace("(Clone)", "_Customized");
                level.meta.modeSettings.ApplySettingsToManager(modifiedManager);
                sceneObj.manager = modifiedManager;
                pmm.customContent.gameManagerPre = modifiedManager;
            }
            pmm.sceneObjectsToCleanUp.Add(sceneObj);
            pmm.editorLevelToLoad = null;
            pmm.editorModeToLoad = "full";
            loader.AssignElevatorScreen(screen);
            loader.Initialize(lifeMode == LifeMode.Intense ? 0 : 2);
            loader.SetMode(0);
            loader.LoadLevel(sceneObj);
            screen.Initialize();
            loader.SetSave(false);
        }

        private SceneObject GetPitstop()
        {
            SceneObject pitstop = Resources.FindObjectsOfTypeAll<SceneObject>().First(x => x.name == "Pitstop");
            SceneObject obj = ScriptableObject.Instantiate<SceneObject>(pitstop);
            LevelAsset asset = LevelAsset.Instantiate(pitstop.levelAsset);
            obj.levelAsset = asset;

            foreach (CellData cell in obj.levelAsset.tile)
            {
                if (cell.pos.x == 30 && cell.pos.z == 3)
                {
                    cell.type = 12;
                }
            }
            obj.levelAsset.posters = new List<PosterData>();
            obj.levelAsset.rooms[0].basicObjects.RemoveAt(0);

            return obj;
        }
    }
}