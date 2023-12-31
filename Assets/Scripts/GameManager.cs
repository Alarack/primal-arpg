using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{


    public static bool AllowDuiplicatSkills { get { return CheckDuplicateAllowance(); } }

    private static bool CheckDuplicateAllowance() {
        int dupeSkills = PlayerPrefs.GetInt("Duplicate Skills");

        return dupeSkills == 1;
    }
}
