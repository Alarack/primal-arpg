using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

using Object = UnityEngine.Object;
using UnityEditorInternal.VersionControl;
using System.Linq;

public static class EditorHelper 
{

    #region TEXTURES AND STYLES

    private static Texture2D _addTexture;
    private static Texture2D _removeTexture;
    private static Texture2D _deleteTexture;
    private static Texture2D _dropdownTexture;
    private static Texture2D _eyeTexture;
    private static Texture2D _moveUpTexture;
    private static Texture2D _moveDownTexture;
    private static Texture2D _moveLeftTexture;
    private static Texture2D _moveRightTexture;
    private static Texture2D _helpPromptTexture;
    private static Texture2D _helpPromptActiveTexture;
    private static Texture2D _dialTexture;
    private static Texture2D _iconOkayTexture;
    private static Texture2D _iconWarningTexture;
    private static Texture2D _iconErrorTexture;
    private static Texture2D _iconDisabledTexture;
    private static Texture2D _iconConfigIssueTexture;

    private static GUIStyle _iconButtonStyle;
    private static GUIStyle _iconStyle;

    private static Texture2D AddTexture { get { return _addTexture ?? (_addTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "addButtonIcon" : "addButtonIconLight")); } }
    private static Texture2D RemoveTexture { get { return _removeTexture ?? (_removeTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "removeButtonIcon" : "removeButtonIconLight")); } }
    private static Texture2D DeleteTexture { get { return _deleteTexture ?? (_deleteTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "deleteButtonIcon" : "deleteButtonIconLight")); } }
    private static Texture2D DropdownTexture { get { return _dropdownTexture ?? (_dropdownTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "lookupButtonIcon" : "lookupButtonIconLight")); } }
    private static Texture2D EyeTexture { get { return _eyeTexture ?? (_eyeTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "eyeButtonIcon" : "eyeButtonIconLight")); } }
    private static Texture2D MoveUpTexture { get { return _moveUpTexture ?? (_moveUpTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "moveUpButtonIcon" : "moveUpButtonIconLight")); } }
    private static Texture2D MoveDownTexture { get { return _moveDownTexture ?? (_moveDownTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "moveDownButtonIcon" : "moveDownButtonIconLight")); } }
    private static Texture2D MoveLeftTexture { get { return _moveLeftTexture ?? (_moveLeftTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "moveLeftButtonIcon" : "moveLeftButtonIconLight")); } }
    private static Texture2D MoveRightTexture { get { return _moveRightTexture ?? (_moveRightTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "moveRightButtonIcon" : "moveRightButtonIconLight")); } }
    private static Texture2D HelpPromptTexture { get { return _helpPromptTexture ?? (_helpPromptTexture = Resources.Load<Texture2D>("helpPromptIcon")); } }
    private static Texture2D HelpPromptTextureActive { get { return _helpPromptActiveTexture ?? (_helpPromptActiveTexture = Resources.Load<Texture2D>("helpPromptActiveIcon")); } }
    private static Texture2D DialTexture { get { return _dialTexture ?? (_dialTexture = Resources.Load<Texture2D>("dialIcon")); } }

    private static Texture2D IconOkayTexture { get { return _iconOkayTexture ?? (_iconOkayTexture = Resources.Load<Texture2D>("okayIcon")); } }
    private static Texture2D IconWarningTexture { get { return _iconWarningTexture ?? (_iconWarningTexture = Resources.Load<Texture2D>("warningIcon")); } }
    private static Texture2D IconErrorTexture { get { return _iconErrorTexture ?? (_iconErrorTexture = Resources.Load<Texture2D>("errorIcon")); } }
    private static Texture2D IconDisabledTexture { get { return _iconDisabledTexture ?? (_iconDisabledTexture = Resources.Load<Texture2D>("disabledIcon")); } }
    private static Texture2D IconConfigIssueTexture { get { return _iconConfigIssueTexture ?? (_iconConfigIssueTexture = Resources.Load<Texture2D>("configurationProblemIcon")); } }

    public static GUIStyle IconButtonStyle { get { return _iconButtonStyle ?? (_iconButtonStyle = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(2, 2, 1, 1) }); } }
    private static GUIStyle IconStyle { get { return _iconStyle ?? (_iconStyle = new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 0, 0) }); } }


    #endregion

    #region BUTTONS

    public static bool AddButton(string tooltip = "Add new entry", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(AddTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    /// <summary> Shows a small toolbar-like button with a "-" symbol </summary>
    public static bool RemoveButton(string tooltip = "Remove entry", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(RemoveTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    /// <summary> Shows a small toolbar-like button with an "x" symbol </summary>
    public static bool DeleteButton(string tooltip = "Delete?", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(DeleteTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    /// <summary> Shows a small toolbar-like button with an eye symbol </summary>
    public static bool EyeButton(string tooltip = "Toggle visivility", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(EyeTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    /// <summary> Shows a small toolbar-like button colored red with an "x" symbol </summary>
    public static bool DeleteButtonConfirm(string tooltip = "DELETE?", int width = 20, int height = 18) {
        GUI.backgroundColor = Color.red;
        bool b = GUILayout.Button(new GUIContent(DeleteTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
        GUI.backgroundColor = Color.white;
        return b;
    }

    private static object _lastDeleteObject;

    public static bool DeleteButtonDouble(object deleteObject) {
        if (_lastDeleteObject == deleteObject) {
            if (DeleteButtonConfirm()) {
                _lastDeleteObject = null;
                return true;
            }
        }
        else {
            if (DeleteButton()) {
                _lastDeleteObject = deleteObject;
            }
        }
        return false;
    }

    public static bool MoveUpButton(string tooltip = "Move up", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(MoveUpTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    public static bool MoveDownButton(string tooltip = "Move down", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(MoveDownTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    public static bool MoveLeftButton(string tooltip = "Move left", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(MoveLeftTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }

    public static bool MoveRightButton(string tooltip = "Move right", int width = 20, int height = 18) {
        return GUILayout.Button(new GUIContent(MoveRightTexture, tooltip), IconButtonStyle, GUILayout.Width(width), GUILayout.Height(height));
    }


    #endregion

    #region LIST CONTROLS
    private static object _lastRemoveList;
    private static int _lastRemoveIndex;

    public static bool DrawListControls<T>(List<T> list, int index, T defaultValue, bool add = true, bool remove = true, bool move = true, int width = 20, int height = 18) {

        if (add) {
            if (AddButton("Add new entry", width, height)) {

                list.Insert(index + 1, defaultValue);

                if (_lastRemoveList == list) {
                    _lastRemoveList = null;
                    _lastRemoveIndex = -1;
                }

                GUI.changed = true; // we have "changed" a "field" with this action, so mark GUI as dirty
            }
        }

        if (remove) {
            if (_lastRemoveList == (list) && _lastRemoveIndex == index) {
                if (DeleteButtonConfirm("DELETE?", width, height)) {

                    list.RemoveAt(index);
                    _lastRemoveList = null;
                    _lastRemoveIndex = -1;

                    GUI.changed = true; // we have "changed" a "field" with this action, so mark GUI as dirty

                    return true;
                }
            }
            else {
                if (DeleteButton("Delete?", width, height)) {
                    _lastRemoveList = list;
                    _lastRemoveIndex = index;

                    GUI.changed = true; // we have "changed" a "field" with this action, so mark GUI as dirty

                    return true;
                }
            }
        }

        if (move) {
            if (index == 0) 
                GUI.enabled = false;
            
            if (MoveUpButton("Move up", width, height)) {
                T temp = list[index];
                list[index] = list[index - 1];
                list[index - 1] = temp;

                if (_lastRemoveList == list) {
                    _lastRemoveList = null;
                    _lastRemoveIndex = -1;
                }

                GUI.changed = true; // we have "changed" a "field" with this action, so mark GUI as dirty

                return true;
            }
            GUI.enabled = true;

            if (index == list.Count - 1) 
                GUI.enabled = false;
            
            if (MoveDownButton("Move down", width, height)) {
                T temp = list[index];
                list[index] = list[index + 1];
                list[index + 1] = temp;

                if (_lastRemoveList == list) {
                    _lastRemoveList = null;
                    _lastRemoveIndex = -1;
                }

                GUI.changed = true; // we have "changed" a "field" with this action, so mark GUI as dirty

                return true;
            }
            GUI.enabled = true;
        }

        return false;
    }

    public static bool DrawListMoveUpDownControls<T>(List<T> list, T item) {
        int index = list.IndexOf(item);

        if (index == 0) 
            GUI.enabled = false;
        
        if (MoveUpButton()) {
            T temp = list[index];
            list[index] = list[index - 1];
            list[index - 1] = temp;
            return true;
        }

        GUI.enabled = true;

        if (index == list.Count - 1) 
            GUI.enabled = false;
        
        if (MoveDownButton()) {
            T temp = list[index];
            list[index] = list[index + 1];
            list[index + 1] = temp;
            return true;
        }

        GUI.enabled = true;

        return false;
    }

    private static T MakeDefaultValue<T>(T defaultValue, bool givenDefaultValue) {
        if (givenDefaultValue)
            return defaultValue;

        if (!typeof(T).IsClass)
            return default(T);

        if (typeof(T).IsSubclassOf(typeof(Object))) // Anything Unity should be made by Unity or referenced, so -> null
            return default(T);

        return Activator.CreateInstance<T>();
    }
    #endregion

    #region LISTS

    public static List<T> DrawList<T>(string listTitle, List<T> list,T defaultValue, Func<List<T>, int, T> drawCallback, bool add = true, bool remove = true) {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        if (listTitle != null)
            EditorGUILayout.LabelField(listTitle, EditorStyles.boldLabel);

        if (list == null) // this will happen when new object doesn't create a list and expects the inspector to create it
            list = new List<T>();

        if (list.Count > 0) {
            for (int i = 0; i < list.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                list[i] = drawCallback(list, i);

                if (DrawListControls(list, i, defaultValue, add, remove))
                    break;

                EditorGUILayout.EndHorizontal();
            }
        }
        else {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Empty");

            if (AddButton())
                list.Insert(0, defaultValue);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        return list;
    }

    public static List<T> DrawList<T>(string listTitle, string entryLabel, List<T> list, T defaultValue, Func<List<T>, int, string, T> drawCallback,bool add = true, bool remove = true) {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        if (listTitle != null)
            EditorGUILayout.LabelField(listTitle, EditorStyles.boldLabel);

        if (list == null) // this will happen when new object doesn't create a list and expects the inspector to create it
            list = new List<T>();

        if (list.Count > 0) {
            for (int i = 0; i < list.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                list[i] = drawCallback(list, i, entryLabel);

                if (DrawListControls(list, i, defaultValue, add, remove))
                    break;

                EditorGUILayout.EndHorizontal();
            }
        }
        else {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Empty");

            if (AddButton())
                list.Insert(0, defaultValue);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        return list;
    }


    public static List<T> DrawListInline<T>(string listTitle, List<T> list, T defaultValue, Func<List<T>, int, T> drawCallback) {
        if (list == null) // this will happen when new object doesn't create a list and expects the inspector to show/create it
            list = new List<T>();

        if (list.Count > 0) {
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < list.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                if (listTitle != null)
                    EditorGUILayout.PrefixLabel(i == 0 ? listTitle : i == 0 ? "Entries" : " ", EditorStyles.label);

                list[i] = drawCallback(list, i);


                if (DrawListControls(list, i, defaultValue, true, true, true, 16, 14))
                    break;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
        else {
            EditorGUILayout.BeginHorizontal();

            if (listTitle != null)
                EditorGUILayout.LabelField(listTitle, "Empty");
            else
                GUILayout.Label("Empty");

            DrawListControls(list, -1, defaultValue, true, false, false, 16, 14);

            EditorGUILayout.EndHorizontal();
        }

        return list;
    }

    public static List<T> DrawExtendedList<T>(List<T> list, string itemCaption,
        Func<T, T> drawCallback) {
        return DrawExtendedList(
            null,
            list,
            delegate (T item, int index) { return itemCaption + " #" + index; },
            default(T),
            drawCallback);
    }

    public static List<T> DrawExtendedList<T>(string listTitle, List<T> list, Func<T, int, string> itemCaptionCallback,
       T defaultValue, Func<T, T> drawCallback) {
        
        if (listTitle != null)
            EditorGUILayout.LabelField(listTitle, EditorStyles.boldLabel);

        if (list == null) // this will happen when new object doesn't create a list and expects the inspector to create it
            list = new List<T>();

        if (list.Count > 0) {
            for (int i = 0; i < list.Count; i++) {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(itemCaptionCallback(list[i], i));

                if (DrawListControls(list, i, defaultValue))
                    break;

                EditorGUILayout.EndHorizontal();

                list[i] = drawCallback(list[i]);

                EditorGUILayout.EndVertical();
            }
        }
        else {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            EditorGUILayout.LabelField("None");

            DrawListControls(list, -1, defaultValue, true, false, false, 20, 18);

            EditorGUILayout.EndHorizontal();
        }
        return list;
    }


    #endregion

    #region ENUMS

    public static T EnumPopup<T>(string label, T value, List<T> badValues = null, params GUILayoutOption[] options) where T : struct {
        Type enumType = typeof(T);

        string[] names = Enum.GetNames(enumType);
        string[] titles = names.Select(n => ObjectNames.NicifyVariableName(n)).ToArray();

        string selectedName = Enum.GetName(enumType, value);
        int selectedIndex = Array.FindIndex(names, n => n == selectedName);

        bool bad = false;
        if (badValues != null)
            if (selectedIndex >= 0 && selectedIndex < names.Length)
                if (badValues.Contains((T)Enum.Parse(enumType, names[selectedIndex])))
                    bad = true;
        if (bad == true) 
            GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f);

        if (label != null)
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, titles, options);
        else
            selectedIndex = EditorGUILayout.Popup(selectedIndex, titles, options);

        if (bad == true) 
            GUI.backgroundColor = Color.white;


        if (selectedIndex < 0 || selectedIndex >= names.Length)
            return (T)Enum.Parse(enumType, names[0]);
        else
            return (T)Enum.Parse(enumType, names[selectedIndex]);

    }

    public static T DrawListOfEnums<T>(List<T> list, int index, string label) where T : struct, System.IFormattable, System.IConvertible {
        T result = EnumPopup(label, list[index]);

        return result;
    }

    #endregion

    #region OBJECT FIELDS

    public static T ObjectField<T>(string label, T value, params GUILayoutOption[] options) where T : Object {
        return EditorGUILayout.ObjectField(label, value, typeof(T), false, options) as T;
    }

    public static T ObjectField<T>(T value, params GUILayoutOption[] options) where T : Object {
        return EditorGUILayout.ObjectField(value, typeof(T), false, options) as T;
    }

    #endregion
}
