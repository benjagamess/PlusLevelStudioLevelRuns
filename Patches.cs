using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using PlusLevelStudio;
using System.Collections;
using MTM101BaldAPI.UI;
using MTM101BaldAPI.Reflection;
using PlusStudioLevelLoader;
using System.Linq;
using System.Net.Security;
using System.Collections.Generic;

namespace PlusLevelStudio.LevelRuns
{
    [HarmonyPatch(typeof(MainMenu), "Start")]
    class AddCreateRunScreenPatch
    {
        static void Postfix(MainMenu __instance)
        {
            __instance.StartCoroutine(BuildMod());
        }

        static IEnumerator BuildMod()
        {
            while (Singleton<AdditiveSceneManager>.Instance.Busy)
            {
                yield return null;
            }

            // Creating the "Create Run" screen

            Transform editorModeSelection = null;
            bool screenFound = false;
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (obj.name == "EditorModeSelection")
                {
                    editorModeSelection = obj.transform;
                    screenFound = true;
                    break;
                }
            }
            if (!screenFound)
            {
                Debug.LogWarning("EditorModeSelection was not found! Do you even have Level Studio installed?");
                yield break;
            }
            Transform editorTypeSelection = editorModeSelection.Find("EditorTypeSelection");
            Transform editorPlayScreen = editorModeSelection.Find("PlayScreen");

            GameObject createRunScreen = Object.Instantiate(editorTypeSelection.gameObject);
            createRunScreen.name = "CreateRunScreen";
            createRunScreen.transform.SetParent(editorModeSelection);
            createRunScreen.transform.localPosition = Vector2.zero;
            editorModeSelection.Find("Bottom").SetAsLastSibling();

            CreateRunScreenManager yamm = editorModeSelection.gameObject.AddComponent<CreateRunScreenManager>(); // yamm here stands for 'Yet Another Menu Manager'

            // Modifying existing objects in the new screen

            createRunScreen.transform.Find("FullButton").gameObject.SetActive(false);
            createRunScreen.transform.Find("ComplaintButton").gameObject.SetActive(false);

            GameObject roomsBtn = createRunScreen.transform.Find("RoomsButton").gameObject;
            roomsBtn.name = "StartButton";
            roomsBtn.transform.localPosition = new Vector2(0, -120);
            roomsBtn.GetComponentInChildren<TMP_Text>().text = Singleton<LocalizationManager>.Instance.GetLocalizedText("But_Start");
            roomsBtn.GetComponent<StandardMenuButton>().OnPress = new UnityEvent();
            roomsBtn.GetComponent<StandardMenuButton>().OnPress.AddListener(() =>
            {
                yamm.Play();
            });

            StandardMenuButton backButton = createRunScreen.transform.Find("Back").GetComponent<StandardMenuButton>();
            backButton.transitionOnPress = true;
            backButton.transitionTime = 0.0167f;
            backButton.transitionType = UiTransition.Dither;
            backButton.OnPress = new UnityEvent();
            backButton.OnPress.AddListener(() =>
            {
                createRunScreen.SetActive(false);
                editorPlayScreen.gameObject.SetActive(true);
            });

            // Adding new elements for the screen

            TextMeshProUGUI titleTxt = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.BoldComicSans24, "Create Run", createRunScreen.transform, new Vector3(0, 120, 0));
            titleTxt.name = "Title";
            titleTxt.rectTransform.sizeDelta = new Vector2(200f, 64f);
            titleTxt.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI lifeMode = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans24, "Life Mode: Normal", createRunScreen.transform, new Vector3(0, 80, 0));
            lifeMode.name = "LifeMode";
            lifeMode.rectTransform.sizeDelta = new Vector2(500f, 64f);
            lifeMode.alignment = TextAlignmentOptions.Center;
            lifeMode.raycastTarget = true;
            StandardMenuButton lifeModeBtn = lifeMode.gameObject.ConvertToButton<StandardMenuButton>();
            lifeModeBtn.underlineOnHigh = true;
            lifeModeBtn.OnPress.AddListener(() =>
            {
                yamm.CycleLifeMode();
            });

            TextMeshProUGUI numFloorsTxt = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans18, "Number of Floors", createRunScreen.transform, new Vector3(0, 40, 0));
            numFloorsTxt.name = "NumFloorsTxt";
            numFloorsTxt.rectTransform.sizeDelta = new Vector2(200f, 64f);
            numFloorsTxt.alignment = TextAlignmentOptions.Center;

            TextMeshProUGUI numFloors = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.BoldComicSans24, "2", createRunScreen.transform, new Vector3(0, 0, 0));
            numFloors.name = "NumFloors";
            numFloors.rectTransform.sizeDelta = new Vector2(200f, 64f);
            numFloors.alignment = TextAlignmentOptions.Center;

            GameObject decreaseNumFloors = new GameObject("DecreaseFloorNum");
            decreaseNumFloors.transform.SetParent(createRunScreen.transform);
            decreaseNumFloors.transform.localPosition = new Vector3(-30, 0, 0);
            decreaseNumFloors.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            Image decreaseNumFloorsImg = decreaseNumFloors.AddComponent<Image>();
            decreaseNumFloorsImg.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowLeft");
            StandardMenuButton decreaseNumFloorsBtn = decreaseNumFloors.ConvertToButton<StandardMenuButton>();
            decreaseNumFloorsBtn.unhighlightedSprite = decreaseNumFloorsImg.sprite;
            decreaseNumFloorsBtn.swapOnHigh = true;
            decreaseNumFloorsBtn.highlightedSprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowLeftHigh");
            decreaseNumFloorsBtn.OnPress = new UnityEvent();
            decreaseNumFloorsBtn.OnPress.AddListener(() =>
            {
                yamm.DecreaseFloorAmount();
            });

            GameObject increaseNumFloors = new GameObject("DecreaseFloorNum");
            increaseNumFloors.transform.SetParent(createRunScreen.transform);
            increaseNumFloors.transform.localPosition = new Vector3(30, 0, 0);
            increaseNumFloors.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            Image increaseNumFloorsImg = increaseNumFloors.AddComponent<Image>();
            increaseNumFloorsImg.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowRight");
            StandardMenuButton increaseNumFloorsBtn = increaseNumFloors.ConvertToButton<StandardMenuButton>();
            increaseNumFloorsBtn.unhighlightedSprite = increaseNumFloorsImg.sprite;
            increaseNumFloorsBtn.swapOnHigh = true;
            increaseNumFloorsBtn.highlightedSprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowRightHigh");
            increaseNumFloorsBtn.OnPress.AddListener(() =>
            {
                yamm.IncreaseFloorAmount();
            });

            TextMeshProUGUI levelSortType = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans24, "Level Order: Random", createRunScreen.transform, new Vector3(0, -60, 0));
            levelSortType.name = "LevelSortType";
            levelSortType.rectTransform.sizeDelta = new Vector2(500f, 64f);
            levelSortType.alignment = TextAlignmentOptions.Center;
            levelSortType.raycastTarget = true;
            StandardMenuButton levelSortTypeBtn = levelSortType.gameObject.ConvertToButton<StandardMenuButton>();
            levelSortTypeBtn.underlineOnHigh = true;
            levelSortTypeBtn.OnPress.AddListener(() =>
            {
                yamm.CycleLevelSortingType();
            });

            // Adding the button on the level select screen

            GameObject openFolderButton = editorPlayScreen.Find("OpenFolderButton").gameObject;
            GameObject createRunButton = Object.Instantiate(openFolderButton);
            createRunButton.name = "CreateRunButton";
            createRunButton.transform.SetParent(editorPlayScreen);
            createRunButton.transform.localPosition = new Vector2(-220, -155);
            createRunButton.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            createRunButton.GetComponent<Image>().sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButton");
            StandardMenuButton createRunButtonBtn = createRunButton.GetComponent<StandardMenuButton>();
            createRunButtonBtn.transitionOnPress = true;
            createRunButtonBtn.transitionTime = 0.0167f;
            createRunButtonBtn.transitionType = UiTransition.Dither;
            createRunButtonBtn.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButtonHover");
            createRunButtonBtn.unhighlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButton");
            createRunButtonBtn.OnPress.AddListener(() =>
            {
                editorPlayScreen.gameObject.SetActive(false);
                createRunScreen.SetActive(true);
            });

            yamm.Initialize();
        }
    }

    [HarmonyPatch(typeof(MainMenu), "Start")]
    class ReturnToMainMenuChecks
    {
        static void Prefix()
        {
            if (LevelStudioRunsPlugin.Instance.inCustomRun)
            {
                LevelStudioRunsPlugin.Instance.inCustomRun = false;
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), "Initialize")]
    class RestoreLivesPatch
    {
        static void Prefix(BaseGameManager __instance)
        {
            if (!LevelStudioRunsPlugin.Instance.inCustomRun)
            {
                return;
            }

            if (LevelStudioRunsPlugin.Instance.lifeMode == LifeMode.Normal)
            {   
                if ((int)__instance.ReflectionGetVariable("levelNo") > Singleton<CoreGameManager>.Instance.lastLevelNumber)
                {
                    Singleton<CoreGameManager>.Instance.SetLives(2, true);
                    Singleton<CoreGameManager>.Instance.lastLevelNumber = (int)__instance.ReflectionGetVariable("levelNo");
                }
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), "BeginPlay")]
    class LevelInfoShowPatch
    {
        static void Postfix()
        {
            if (!LevelStudioRunsPlugin.Instance.inCustomRun || Singleton<CoreGameManager>.Instance.sceneObject.levelTitle == "PIT")
            {
                return;
            }

            PlayableEditorLevel level = LevelStudioRunsPlugin.Instance.levelOrder[LevelStudioRunsPlugin.Instance.currentLevel];
            Singleton<CoreGameManager>.Instance.GetHud(0).SetTooltip(string.Format("{0}\nby: {1}", level.meta.name, level.meta.author));
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), "ExitedSpawn")]
    class LevelInfoHidePatch
    {
        static void Postfix()
        {
            if (!LevelStudioRunsPlugin.Instance.inCustomRun || Singleton<CoreGameManager>.Instance.sceneObject.levelTitle == "PIT")
            {
                return;
            }

            Singleton<CoreGameManager>.Instance.GetHud(0).CloseTooltip();
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), "RestartLevel")]
    class RestartLevelPatch
    {
        static BaseGameManager instance;

        static bool Prefix(BaseGameManager __instance, ElevatorScreen ___elevatorScreen, ElevatorScreen ___elevatorScreenPre)
        {
            if (!LevelStudioRunsPlugin.Instance.inCustomRun)
            {
                return true;
            }

            instance = __instance;

            Singleton<CoreGameManager>.Instance.saveMapAvailable = true;
            __instance.ReflectionInvoke("PrepareToLoad", new object[] { });
            ___elevatorScreen = Object.Instantiate<ElevatorScreen>(___elevatorScreenPre);
            ___elevatorScreen.OnLoadReady += BaseRestartLevel;
            ___elevatorScreen.Initialize();
            Singleton<CoreGameManager>.Instance.GetPoints(0);

            instance.ReflectionInvoke("PrepareToLoad", new object[] { });
            Singleton<CoreGameManager>.Instance.PrepareForReload();
            Singleton<CoreGameManager>.Instance.BackupMap(instance.Ec.map);
            Singleton<CoreGameManager>.Instance.RestorePlayers();
            AccessTools.Method(instance.GetType(), "LoadSceneObject", new System.Type[] { typeof(SceneObject), typeof(bool) }).Invoke(instance, new object[] { Singleton<CoreGameManager>.Instance.sceneObject, true });

            return false;
        }

        static void BaseRestartLevel()
        {
            Debug.Log("Triggered?");
            instance.ReflectionInvoke("PrepareToLoad", new object[] { });
            Singleton<CoreGameManager>.Instance.PrepareForReload();
            Singleton<CoreGameManager>.Instance.BackupMap(instance.Ec.map);
            Singleton<CoreGameManager>.Instance.RestorePlayers();
            AccessTools.Method(instance.GetType(), "LoadSceneObject", new System.Type[] { typeof(SceneObject), typeof(bool) }).Invoke(instance, new object[] {Singleton<CoreGameManager>.Instance.sceneObject, true});
        }
    }

    [HarmonyPatch(typeof(BaseGameManager), "EnterExit", new System.Type[] { typeof(Elevator) })]
    [HarmonyPatch(MethodType.Enumerator)]
    class LoadNextLevelPatch
    {
        static BaseGameManager instance;

        static bool Prefix(object __instance)
        {
            var bgmField = __instance.GetType().GetField("<>4__this");
            instance = (BaseGameManager)bgmField.GetValue(__instance);

            var elevatorField = __instance.GetType().GetField("elevator");
            Elevator elevator = (Elevator)elevatorField.GetValue(__instance);

            if (!LevelStudioRunsPlugin.Instance.inCustomRun || Singleton<CoreGameManager>.Instance.sceneObject.levelTitle == "PIT")
            {
                return true;
            }

            if (!elevator.InsideCollider.HasPlayer || !elevator.Powered)
            {
                return true;
            }

            LevelStudioRunsPlugin.Instance.currentLevel++;
            if (LevelStudioRunsPlugin.Instance.currentLevel == LevelStudioRunsPlugin.Instance.levelOrder.Count)
            {
                return true;
            }
            PlayableEditorLevel level = LevelStudioRunsPlugin.Instance.levelOrder[LevelStudioRunsPlugin.Instance.currentLevel];

            EditorPlayModeManager pmm = Object.FindObjectOfType<EditorPlayModeManager>();
            pmm.customContent = new EditorCustomContent();
            pmm.customContent.LoadFromPackage(level.meta.contentPackage);
            SceneObject sceneObj = LevelImporter.CreateSceneObject(level.data);
            sceneObj.manager = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab;
            sceneObj.levelTitle = "F" + (LevelStudioRunsPlugin.Instance.currentLevel + 1);
            sceneObj.levelNo = LevelStudioRunsPlugin.Instance.currentLevel;
            sceneObj.totalShopItems = Random.Range(4, 7);
            sceneObj.shopItems = Resources.FindObjectsOfTypeAll<SceneObject>().First(x => x.name == "MainLevel_1").shopItems;
            sceneObj.mapPrice = Singleton<CoreGameManager>.Instance.sceneObject.mapPrice - 250;
            Singleton<CoreGameManager>.Instance.sceneObject.nextLevel = sceneObj;
            if (level.meta.modeSettings != null)
            {
                BaseGameManager modifiedManager = GameObject.Instantiate<BaseGameManager>(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab, MTM101BaldAPI.MTM101BaldiDevAPI.prefabTransform);
                modifiedManager.name = modifiedManager.name.Replace("(Clone)", "_Customized");
                level.meta.modeSettings.ApplySettingsToManager(modifiedManager);
                sceneObj.manager = modifiedManager;
                pmm.customContent.gameManagerPre = modifiedManager;
            }
            pmm.sceneObjectsToCleanUp.Add(sceneObj);

            Singleton<CoreGameManager>.Instance.saveMapAvailable = false;
            for (int i = 0; i < 2 - Singleton<CoreGameManager>.Instance.Attempts; i++)
            {
                Singleton<CoreGameManager>.Instance.AddPoints(Singleton<CoreGameManager>.Instance.GetPointsThisLevel(0), 0, false, false);
            }
            instance.ReflectionInvoke("PrepareToLoad", new object[] { });
            instance.ReflectionSetVariable("elevatorScreen", Object.Instantiate<ElevatorScreen>((ElevatorScreen)instance.ReflectionGetVariable("elevatorScreenPre")));
            ElevatorScreen elevatorScreen = (ElevatorScreen)instance.ReflectionGetVariable("elevatorScreen");
            elevatorScreen.OnLoadReady += BaseLoadNextLevel;
            elevatorScreen.Initialize();

            int problems = (int)instance.ReflectionGetVariable("problems");
            int correctProblems = (int)instance.ReflectionGetVariable("correctProblems");
            float gradeValue = (float)instance.ReflectionGetVariable("gradeValue");
            if (problems > 0)
            {
                Singleton<CoreGameManager>.Instance.GradeVal += -Mathf.RoundToInt(gradeValue * ((float)correctProblems / (float)problems * 2f - 1f));
            }
            Singleton<CoreGameManager>.Instance.AwardGradeBonus();
            elevatorScreen.ShowResults((float)instance.ReflectionGetVariable("time"), 0);

            return false;
        }

        static void BaseLoadNextLevel()
        {
            instance.Ec.gameObject.SetActive(false);
            instance.ReflectionInvoke("PrepareToLoad", new object[] { });
            Singleton<CoreGameManager>.Instance.PrepareForReload();
            if (Singleton<CoreGameManager>.Instance.sceneObject.levelNo > Singleton<CoreGameManager>.Instance.lastLevelNumber)
            {
                Singleton<CoreGameManager>.Instance.tripPlayed = false;
            }
            if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Main)
            {
                foreach (NPC npc in instance.Ec.npcsToSpawn)
                {
                    Singleton<PlayerFileManager>.Instance.Find(Singleton<PlayerFileManager>.Instance.foundChars, (int)npc.Character);
                }
                foreach (Obstacle value in instance.Ec.obstacles)
                {
                    Singleton<PlayerFileManager>.Instance.Find(Singleton<PlayerFileManager>.Instance.foundObstcls, (int)value);
                }
                Singleton<PlayerFileManager>.Instance.Find(Singleton<PlayerFileManager>.Instance.clearedLevels, (int)instance.ReflectionGetVariable("levelNo"));
            }
            Singleton<SubtitleManager>.Instance.DestroyAll();
            AccessTools.Method(instance.GetType(), "LoadSceneObject", new System.Type[] {typeof(SceneObject), typeof(bool)}).Invoke(instance, new object[] { Singleton<CoreGameManager>.Instance.sceneObject.nextLevel, false });
        }
    }

    [HarmonyPatch(typeof(MainGameManager), "LoadSceneObject", new System.Type[] {typeof(SceneObject), typeof(bool)})]
    class ModdedPitstopPatch
    {
        static bool Prefix(MainGameManager __instance, SceneObject sceneObject, bool restarting)
        {
            if (!LevelStudioRunsPlugin.Instance.inCustomRun)
            {
                return true;
            }

            Singleton<CoreGameManager>.Instance.nextLevel = sceneObject;
            if (!__instance.levelObject.finalLevel || restarting)
            {
                Items[] itemBlacklist = new Items[]
                {
                    Items.None,
                    Items.Map,
                    Items.BusPass,
                    Items.Points,
                    Items.CircleKey,
                    Items.TriangleKey,
                    Items.HexagonKey,
                    Items.WeirdKey,
                    Items.SquareKey,
                    Items.lostItem0,
                    Items.lostItem1,
                    Items.lostItem2,
                    Items.lostItem3,
                    Items.lostItem4,
                    Items.lostItem5,
                    Items.lostItem6,
                    Items.lostItem7,
                    Items.lostItem8,
                    Items.lostItem9
                };
                List<Items> itemsInLevel = new List<Items>();
                List<WeightedItemObject> shopItems = new List<WeightedItemObject>();
                foreach (RoomData room in Singleton<CoreGameManager>.Instance.sceneObject.levelAsset.rooms)
                {
                    foreach (ItemData item in room.items)
                    {
                        itemsInLevel.Add(item.item.itemType);
                        if (!itemBlacklist.Contains(item.item.itemType))
                        {
                            shopItems.Add(new WeightedItemObject() { selection = item.item, weight = Mathf.RoundToInt((-0.000125f * (Mathf.Sin(item.item.price) * Mathf.Sin(item.item.price))) + 200) });
                        }
                    }
                }
                if (shopItems.Count < 4)
                {
                    Singleton<CoreGameManager>.Instance.nextLevel.shopItems = Resources.FindObjectsOfTypeAll<SceneObject>().First(x => x.name == "MainLevel_1").shopItems;
                }
                else
                {
                    Singleton<CoreGameManager>.Instance.nextLevel.shopItems = shopItems.ToArray();
                }

                if (itemsInLevel.Contains(Items.BusPass))
                {
                    Singleton<CoreGameManager>.Instance.tripPlayed = false;
                    LevelStudioRunsPlugin.Instance.pitstop.manager.ReflectionSetVariable("tierOneTripLevel", Singleton<CoreGameManager>.Instance.nextLevel.levelNo);
                }
                else
                {
                    LevelStudioRunsPlugin.Instance.pitstop.manager.ReflectionSetVariable("tierOneTripLevel", Singleton<CoreGameManager>.Instance.nextLevel.levelNo + 1);
                }
                

                Singleton<CoreGameManager>.Instance.sceneObject = LevelStudioRunsPlugin.Instance.pitstop;
                Singleton<AdditiveSceneManager>.Instance.LoadScene("Game");
                return false;
            }
            Singleton<CoreGameManager>.Instance.sceneObject = sceneObject;
            Singleton<AdditiveSceneManager>.Instance.LoadScene("Game");
            return false;
        }
    }
}
