using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadingGame : MonoBehaviour
{
    public static LoadingGame loadingGame;
    public static bool playerLostLife = false; //player lost life before scene change

    public static float timeFromLastSave = 0f;
    public bool editMode = false; //while normal gameplay - check to FALSE

    [Header("Character")]
    public GameObject character;

    [Header("Backpack")]
    public GameObject devResources; //object that stores items as a childrens
    public Backpack backpack;

    [Header("Items you can equip")]
    public GameObject itemsYouCanEquip; //objct where you can find equipable items

    public GameObject portalClose;

    void Start()
    {
        loadingGame = this;

        //string path = Application.persistentDataPath + "/Saves";
        //Directory.CreateDirectory(path); //create directory for storing saves

        if (!NewGameSet.newGameSet)
        {
            GameScripts.loadingGame.LoadGame();

            UIManager.hideIngameUI = false;

            if (playerLostLife)
            {
                Player.livesCount--;
                SaveGame();
                playerLostLife = false;

                if (Player.livesCount <= 0) //check if player can continue
                {
                    if(Player.continueCount < 0) SceneChanger.sceneChanger.LoadScene("MainMenu");
                    else
                    {
                        Player.livesCount = 5;
                        SaveGame();
                    }
                    Debug.Log("You lost all life points");
                }
            }
        }
    }

    private void Update()
    {
        if (editMode) //saving game manually (only in edit mode!)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveGame();
                Debug.Log("Saved manually!");
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadGame();
                Debug.Log("Loaded manually!");
            }
        }

        timeFromLastSave += Time.deltaTime;
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(character.GetComponent<Player>(), itemsYouCanEquip, 0, true, true, true);
        UIManager.saveInfo.SetActive(false);
        UIManager.saveInfo.SetActive(true);
    }

    public void LoadGame()
    {
        FirstFrameStart.alreadyStarted = false;

        if (!NewGameSet.newGameSet)
        {
            bool loadGame = true;

            //loading data
            PlayerData playerData = SaveSystem.LoadPlayerData(0);
            PlayerPositionData playerPositionData = SaveSystem.LoadPlayerPositionData(0);
            SettingsData settingsData = SaveSystem.LoadSettingsData(0);
            ItemsData itemsData = SaveSystem.LoadItemsData(0);

            //checking if save data is corrupted
            if (settingsData == null)
            {
                Debug.Log("Settings data is missing!");
                loadGame = false;
            }
            if (playerData == null)
            {
                Debug.Log("Player data is missing!");
                loadGame = false;
            }
            if (playerPositionData == null)
            {
                Debug.Log("Player position data is missing!");
                loadGame = false;
            }
            if (itemsData == null)
            {
                Debug.Log("Items data is missing!");
                loadGame = false;
            }

            if (loadGame)
            {
                LoadSettings(settingsData); //settings

                LoadBackpack(itemsData); //backpack

                LoadItems(itemsData); //items to equip
                LoadDestinationsOfItems(itemsData); //destinations of items
                LoadSwitches(itemsData); //switches
                LoadCollectables(itemsData); //collectables
                LoadCharacterDialogues(itemsData);

                LoadPlayerStats(playerData); //player stats
                LoadPlayerPosition(playerPositionData); //player position

                portalClose.SetActive(false); //close intro portal while loading

                Debug.Log("Loading data finished succesfully!");
            }
        }
        else
        {
            NewGameSet.newGameSet = false;
        }
    }

    //
    //SETTINGS
    //
    public void SaveSettings()
    {
        SaveSystem.SaveGame(null, null, 0, false, true, false); //saving only settings
    }

    public void LoadSettings()
    {
        SettingsData settingsData = SaveSystem.LoadSettingsData(0);
        if(settingsData != null) LoadSettings(settingsData);
    }

    public void LoadSettings(SettingsData settingsData)
    {
        //LANGUAGE
        GameScripts.languagePL = settingsData.languagePL;

        //HINTS
        GameScripts.isHintsActive = settingsData.isHintsActive;

        //QUALITY
        GameScripts.quality = settingsData.quality;
        if (GameScripts.quality == 0)
        {
            QualitySettings.SetQualityLevel(0, true);
            Application.targetFrameRate = 30;
        }
        else if (GameScripts.quality == 1) QualitySettings.SetQualityLevel(1, true);
        else if (GameScripts.quality == 2) QualitySettings.SetQualityLevel(2, true);
        else QualitySettings.SetQualityLevel(1, true);

        //AUDIO
        //music
        AudioSettings.musicVolume = settingsData.musicVolume;
        AudioSettings.musicSlider.value = settingsData.musicVolume;
        AudioSettings.isMusicMuted = settingsData.isMusicMuted;
        //sfx
        AudioSettings.sfxSource.volume = settingsData.sfxVolume;
        AudioSettings.sfxSlider.value = settingsData.sfxVolume;
        AudioSettings.isSfxMuted = settingsData.isSfxMuted;

        //OTHER
        Player.lastSavePointId = settingsData.lastSavePointId;
    }

    //
    //PLAYER
    //
    public void SavePlayerStatsData()
    {
        SaveSystem.SaveGame(character.GetComponent<Player>(), itemsYouCanEquip, 0, true, true, false); //saving only player data

        Debug.Log("Saved player lives:" + Player.livesCount);
    }

    public void LoadPlayerData()
    {
        PlayerData playerData = SaveSystem.LoadPlayerData(0);
        if (playerData != null) LoadPlayerStats(playerData);
    }

    void LoadPlayerStats(PlayerData playerData)
    {
        //player stats
        Player.livesCount = playerData.livesCount;
        Player.collectablesCount = playerData.collectablesCount;
        Player.continueCount = playerData.continueCount;
        Player.timeSpent = playerData.timeSpent;

        Player.canUseUmbrella = playerData.canUseUmbrella;
        Player.isInCave = playerData.isInCave;
    }

    void LoadPlayerPosition(PlayerPositionData playerPositionData)
    {
        //player position
        Vector3 savedPosition = new Vector3(
            playerPositionData.playerPosition[0],
            playerPositionData.playerPosition[1],
            playerPositionData.playerPosition[2]);
        character.transform.position = savedPosition;
    }

    //
    //BACKPACK
    //
    void LoadBackpack(ItemsData itemsData)
    {
        Item[] items = new Item[3];
        backpack.ClearBackpack();

        for (int i = 0; i < 3; i++)
        {
            if (itemsData.backpackItemsStrings[i] != "-EMPTY-")
            {
                items[i] = devResources.transform.Find(itemsData.backpackItemsStrings[i]).gameObject.GetComponent<Item>();
                
                backpack.AddItem(items[i]);
                if (itemsYouCanEquip.transform.Find(items[i].itemName)) Destroy(itemsYouCanEquip.transform.Find(items[i].itemName).gameObject);
            }
        } // load items to backpack
    }

    //
    //ITEMS
    //
    void LoadItems(ItemsData itemsData)
    {
        for (int i = 0; i < itemsData.itemsYouCanEquipNameList.Count; i++)
        {
            Transform itemObject = itemsYouCanEquip.transform.Find(itemsData.itemsYouCanEquipNameList[i]);

            if (itemObject)
            {
                //DestroyItems(itemsData, itemObject.GetComponent<Item>());

                itemObject.position = new Vector3(itemsData.itemsYouCanEquipPositionList[i][0], itemsData.itemsYouCanEquipPositionList[i][1], itemsData.itemsYouCanEquipPositionList[i][2]);
            } // position set if item is avaliable
            else
            {
                GameObject item = Instantiate(devResources.transform.Find(itemsData.itemsYouCanEquipNameList[i]).gameObject, itemsYouCanEquip.transform);
                Vector3 itemPosition = new Vector3(itemsData.itemsYouCanEquipPositionList[i][0], itemsData.itemsYouCanEquipPositionList[i][1], itemsData.itemsYouCanEquipPositionList[i][2]);

                item.transform.position = itemPosition;
            } // create item and set position if item is currently in backpack
        } // load items position
    }

    void LoadDestinationsOfItems(ItemsData itemsData)
    {
        for (int i = 0; i < itemsData.destinationsOfItemsBoolList.Count; i++)
        {
            GameObject deliverItem = GameScripts.destinationsOfItems.transform.Find(itemsData.destinationsOfItemsNameList[i]).gameObject;
            Transform findItemYouCanEquip = itemsYouCanEquip.transform.Find(deliverItem.GetComponentInChildren<ItemChecker>().wantedItem.itemName);

            deliverItem.GetComponentInChildren<ItemChecker>().isDelivered = itemsData.destinationsOfItemsBoolList[i];

            if (itemsData.destinationsOfItemsBoolList[i] && findItemYouCanEquip != null)
            {
                if (deliverItem.transform.Find("Collider")) deliverItem.transform.Find("Collider").gameObject.SetActive(false);

                Destroy(findItemYouCanEquip.gameObject);
            }
            else if (!itemsData.destinationsOfItemsBoolList[i] && deliverItem.transform.Find("Collider")) deliverItem.transform.Find("Collider").gameObject.SetActive(true);
        } // load items deliver state

        ItemChecker.alreadySet = false;
    }

    //
    //COLLECTABLES
    //
    void LoadCollectables(ItemsData itemsData)
    {
        for (int i = 0; i < itemsData.collectablesBoolList.Count; i++)
        {
            GameObject collectable = GameScripts.collectables.transform.Find(itemsData.collectablesNameList[i]).gameObject;

            if (collectable)
            {
                if (collectable.transform || collectable.activeSelf)
                {
                    if (!itemsData.collectablesBoolList[i])
                    {
                        collectable.SetActive(false);
                    }
                    else
                    {
                        collectable.SetActive(true);
                    }
                }
            }
        }
    }

    //
    //SWITCHES
    //
    void LoadSwitches(ItemsData itemsData)
    {
        for (int i = 0; i < itemsData.switchesBoolList.Count; i++)
        {
            Switch targetSwitch = GameScripts.switches.transform.Find(itemsData.switchesNameList[i]).GetComponent<Switch>();

            targetSwitch.isActivated = itemsData.switchesBoolList[i];
        } // load switches values
    }

    //
    //CHARACTER DIALOGUES
    //
    void LoadCharacterDialogues(ItemsData itemsData)
    {
        for (int i = 0; i < itemsData.characterDialoguesBoolList.Count; i++)
        {
            GameObject characterDialogue = GameScripts.characterDialogues.transform.Find(itemsData.characterDialoguesNameList[i]).gameObject;

            if (characterDialogue)
            {
                if (itemsData.characterDialoguesBoolList[i] == false)
                {
                    characterDialogue.SetActive(false);
                }
                else
                {
                    characterDialogue.SetActive(true);
                }
            }
        }
    }
}
