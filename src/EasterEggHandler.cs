using Modding;
using Satchel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChristmasInDirtmouth
{
    internal class EasterEggHandler
    {
        // Easter Egg
        public static bool CuteMas = false;

        private SceneManager sm;
        private KeyCode[] code = new KeyCode[]{ KeyCode.N, KeyCode.O, KeyCode.B };
        int codeIndex = 0;

        // Class just used for handling little easter eggs
        // If you've found this type "nob" in the christmas shop
        public EasterEggHandler()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }

        public void OnHeroUpdate()
        {

            if (ChristmasShopSceneHandler.SceneActive)
            {
                // If theres a Key pressed
                if (Input.anyKeyDown)
                {
                    if (Input.GetKeyDown(code[codeIndex]))
                    {
                        codeIndex++;
                        if (codeIndex == code.Length)
                        {
                            Logger.Success("Easter Egg Found!");
                            codeIndex = 0;
                            CuteMas = !CuteMas;
                            if (CuteMas)
                            {
                                HeroController.instance.gameObject.AddComponent<NerdBehavior>();
                            }
                            ChangeNPCLines();
                        }
                    }
                    else
                    {
                        // Reset, wrong code
                        codeIndex = 0;
                    }
                }
            }
        }

        private void ChangeNPCLines()
        {
            // Changed NPC lines for easter egg
            if (CuteMas)
            {
                ModItems.NPCMap["MERRY_DIALOG_1"] = "Its nobberin time!";
                ModItems.NPCMap["MERRY_DIALOG_2"] = "Noboh, noboh nobu";
                ModItems.NPCMap["MERRY_DIALOG_3"] = "Erm, What the nob?";
            }
            else
            {
                ModItems.NPCMap["MERRY_DIALOG_1"] = "Bless you, little one. You’ve a good eye for merriment. Safe travels now, and may your path be merry and bright!";
                ModItems.NPCMap["MERRY_DIALOG_2"] = "I’ve gifts for the festive of heart! Trinkets, baubles, even charms to add some jolly jingle to your journey. Come now, take a look. Bring a bit of brightness to this gloomy kingdom.";
                ModItems.NPCMap["MERRY_DIALOG_3"] = "Still here? Or perhaps you’re savoring the cheer? No rush, no rush... The season is for sharing, after all!";
            }
            // Rebuild dialog manager in satchel
            ChristmasInDirtmouth.MerryDialogueManager.Conversations.Clear();
            ModItems.AddManagerConversations(ChristmasInDirtmouth.MerryDialogueManager);
        }

        private void OnSceneChange(Scene from, Scene to)
        {
            if (CuteMas)
            {
                HeroController.instance.gameObject.AddComponent<NerdBehavior>();
            }
        }

        private class NerdBehavior : MonoBehaviour
        {
            private GameObject nerdArrowPrompt;
            private void Awake()
            {
                nerdArrowPrompt = CreatePromptPrehab();
                nerdArrowPrompt.SetActive(true);
                nerdArrowPrompt.GetComponent<CustomArrowPromptBehaviour>().Show();
                ModHooks.HeroUpdateHook += OnHeroUpdate;
            }

            private GameObject CreatePromptPrehab()
            {
                // Needs preloads["Cliffs_01"]["Cornifer Card"]; on mod init
                // Assumes CustomArrowPrompt.Prepare(CardPrefab); has been called
                // https://github.com/PrashantMohta/Satchel/blob/2e922c2939ae35af0a256b5edd1792db0dbf0c92/Custom/CustomArrowPrompt.cs
                GameObject prefab = new GameObject("Arrow");
                CustomArrowPrompt.GetAddCustomArrowPrompt(prefab, "NERD", null);
                DontDestroyOnLoad(prefab);

                prefab.transform.parent = this.transform;
                //prefab.transform.position = HeroController.instance.transform.position;
                prefab.transform.localPosition = new Vector3(0, 0.5f, 0);
                return prefab;
            }

            public void OnHeroUpdate()
            {
                nerdArrowPrompt.transform.localScale = new Vector3(HeroController.instance.cState.facingRight ? -1f : 1f, 1f, 1f);
            }
        }
    }
}
