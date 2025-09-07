using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using PlusLevelStudio;
using System.Collections;
using MTM101BaldAPI.UI;

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

            // Modifying existing objects in the new screen

            Object.Destroy(createRunScreen.transform.Find("FullButton").gameObject);
            Object.Destroy(createRunScreen.transform.Find("ComplaintButton").gameObject);

            GameObject roomsBtn = createRunScreen.transform.Find("RoomsButton").gameObject;
            roomsBtn.name = "StartButton";
            roomsBtn.transform.localPosition = new Vector2(0, -120);
            roomsBtn.GetComponentInChildren<TMP_Text>().text = Singleton<LocalizationManager>.Instance.GetLocalizedText("But_Start");

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

            TextMeshProUGUI numFloorsTxt = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans18, "Number of Floors", createRunScreen.transform, new Vector3(0, 20, 0));
            numFloorsTxt.name = "NumFloorsTxt";
            numFloorsTxt.rectTransform.sizeDelta = new Vector2(200f, 64f);
            numFloorsTxt.alignment = TextAlignmentOptions.Center;



            // Adding the button on the level select screen

            GameObject openFolderButton = editorPlayScreen.Find("OpenFolderButton").gameObject;
            GameObject createRunButton = Object.Instantiate(openFolderButton);
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
            createRunButtonBtn.OnPress = new UnityEvent();
            createRunButtonBtn.OnPress.AddListener(() =>
            {
                editorPlayScreen.gameObject.SetActive(false);
                createRunScreen.SetActive(true);
            });
        }
    }
}
