using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputHelper;

public class GameManager : Singleton<GameManager>
{
    public bool debugMode;
    public DebugPanel debugPanel;
    public TileDatabase tileDatabase;
    public StatTooltipData tooltipData;
    public MasteryDefiniiton masteryDatabase;

    public SerializableDictionary<GameButtonType, Sprite> buttonDict = new SerializableDictionary<GameButtonType, Sprite>();


    public int totalThreatFromKilledEnemies = 0;

    public static bool AllowDuiplicatSkills { get { return CheckDuplicateAllowance(); } }

    private static bool CheckDuplicateAllowance() {
        int dupeSkills = PlayerPrefs.GetInt("Duplicate Skills");

        return dupeSkills == 1;
    }

    private void Update() {
        if(debugMode == false) 
            return;

        if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
            debugPanel.Toggle();
        }
    }


    

    public static void StartGame() {
        //EntityManager.Instance.CreatePlayer();

    }
}
