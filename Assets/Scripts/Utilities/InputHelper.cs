using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class InputHelper 
{
 
    public enum GameButtonType {
        None,
        PrimaryAttack,
        SecondaryAttack,
        Jump,
        Dash,
        Skill1,
        Skill2,
        Skill3,
        Skill4
    }
    

    public static Dictionary<GameButtonType, CustomBind> bindDict = new Dictionary<GameButtonType, CustomBind>();
    public static bool initBinds;

    public static void InitDefaultBinds() {

        GameButtonType[] buttons = System.Enum.GetValues(typeof(GameButtonType)) as GameButtonType[];

        foreach (var entry in buttons) {
            CustomBind bind = entry switch {
                GameButtonType.None => null,
                GameButtonType.PrimaryAttack => new CustomBind(0),
                GameButtonType.SecondaryAttack => new CustomBind(1),
                GameButtonType.Jump => null,
                GameButtonType.Dash => new CustomBind(KeyCode.Space),
                GameButtonType.Skill1 => new CustomBind(KeyCode.Alpha1),
                GameButtonType.Skill2 => new CustomBind(KeyCode.Alpha2),
                GameButtonType.Skill3 => new CustomBind(KeyCode.Alpha3),
                GameButtonType.Skill4 => new CustomBind(KeyCode.Alpha4),
                _ => null,
            };

            if(bind != null ) {
                bindDict.Add(entry, bind);
            }
        }

        initBinds = true;
    }

    public static GameButtonType GetCustomInput(Event e) {

        if (e == null)
            return GameButtonType.None;

        if (e.isMouse) {
            int mouseButton = e.button;

            foreach (var item in bindDict) {
                if(item.Value.type == CustomBind.Type.MouseButton && item.Value.mouseButton == mouseButton) {
                    return item.Key;
                }
            }
        }
        else if(e.isKey) {
            KeyCode code = e.keyCode;

            foreach (var item in bindDict) {
                if (item.Value.type == CustomBind.Type.KeyCode && item.Value.keyCode == code) {
                    return item.Key;
                }
            }
        }



        return GameButtonType.None;
    }


    public static KeyCode GetKeyCodeFromInput() {
        if (string.IsNullOrEmpty(Input.inputString) == false) {

            string input = Input.inputString.ToUpper();

            StringBuilder builder = new StringBuilder();
            builder.Append(input);

            if (int.TryParse(input, out int number) == true) {
                builder.Insert(0, "Alpha");
            }

            KeyCode code = (KeyCode)Enum.Parse(typeof(KeyCode), builder.ToString());

            if (Input.GetKeyDown(code)) {
                Debug.Log("You pressed: " + Input.inputString);
            }

            return code;
        }

        return KeyCode.None;
    }




    public class CustomBind {
        public enum Type {
            KeyCode,
            MouseButton
        }
        public Type type;
        public KeyCode keyCode;
        public int mouseButton;

        public CustomBind(KeyCode keycode) {
            this.type = Type.KeyCode;
            this.keyCode = keycode;
        }

        public CustomBind(int mouseButton) {
            this.type = Type.MouseButton;
            this.mouseButton = mouseButton;
        }

    }

}
