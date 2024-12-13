using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public TileDatabase tileDatabase;
    public StatTooltipData tooltipData;
    public MasteryDefiniiton masteryDatabase;

    public int totalThreatFromKilledEnemies = 0;

    public static bool AllowDuiplicatSkills { get { return CheckDuplicateAllowance(); } }

    private static bool CheckDuplicateAllowance() {
        int dupeSkills = PlayerPrefs.GetInt("Duplicate Skills");

        return dupeSkills == 1;
    }



    public static void StartGame() {
        //EntityManager.Instance.CreatePlayer();

    }
}
