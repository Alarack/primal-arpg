using System.Collections.Generic;
using UnityEngine;
using System;

namespace LL.Events {
    public class EventData {


        #region FIELDS AND PROPERTIES

        private IDictionary<string, int> _intValues;

        private IDictionary<string, string> _stringValues;

        private IDictionary<string, float> _floatValues;

        private IDictionary<string, bool> _boolValues;

        private IDictionary<string, GameObject> _gameObjectValues;

        private IDictionary<string, MonoBehaviour> _monoBehaviourValues;

        private IDictionary<string, Vector3> _vectors;

        private IDictionary<string, Action> _actions;

        //Game Specific
        private IDictionary<string, Ability> _abilities;
        private IDictionary<string, Effect> _effects;
        private IDictionary<string, Entity> _entities;
        private IDictionary<string, Weapon> _weapons;
        private IDictionary<string, AbilityTrigger> _triggers;



        #endregion

        #region CONSTRUCTION

        public EventData() {

        }

        #endregion

        #region ADDING KEYED VALUES

        public void AddInt(string key, int value) {
            if (_intValues == null)
                _intValues = new Dictionary<string, int>();

            _intValues.Add(key, value);
        }

        public void AddString(string key, string value) {
            if (_stringValues == null)
                _stringValues = new Dictionary<string, string>();

            _stringValues.Add(key, value);
        }

        public void AddFloat(string key, float value) {
            if (_floatValues == null)
                _floatValues = new Dictionary<string, float>();

            _floatValues.Add(key, value);
        }

        public void AddBool(string key, bool value) {
            if (_boolValues == null)
                _boolValues = new Dictionary<string, bool>();

            _boolValues.Add(key, value);
        }

        public void AddGameObject(string key, GameObject value) {
            if (_gameObjectValues == null)
                _gameObjectValues = new Dictionary<string, GameObject>();

            _gameObjectValues.Add(key, value);
        }

        public void AddMonoBehaviour(string key, MonoBehaviour value) {
            if (_monoBehaviourValues == null)
                _monoBehaviourValues = new Dictionary<string, MonoBehaviour>();

            _monoBehaviourValues.Add(key, value);
        }

        public void AddAction(string key, Action value) {
            if (_actions == null)
                _actions = new Dictionary<string, Action>();

            _actions.Add(key, value);
        }

        public void AddVector3(string key, Vector3 value) {
            if (_vectors == null)
                _vectors = new Dictionary<string, Vector3>();

            _vectors.Add(key, value);
        }

        public void AddEntity(string key, Entity value) {
            if (_entities == null)
                _entities = new Dictionary<string, Entity>();

            _entities.Add(key, value);
        }

        public void AddAbility(string key, Ability value) {
            if (_abilities == null)
                _abilities = new Dictionary<string, Ability>();

            _abilities.Add(key, value);
        }

        public void AddEffect(string key, Effect value) {
            if (_effects == null)
                _effects = new Dictionary<string, Effect>();

            _effects.Add(key, value);
        }

        public void AddWeapon(string key, Weapon value) {
            if (_weapons == null)
                _weapons = new Dictionary<string, Weapon>();

            _weapons.Add(key, value);
        }

        public void AddTrigger(string key, AbilityTrigger value) {
            if (_triggers == null)
                _triggers = new Dictionary<string, AbilityTrigger>();

            _triggers.Add(key, value);
        }


        #endregion

        #region GETTING KEYED VALUES

        public int GetInt(string key) {
            if (_intValues == null || !_intValues.TryGetValue(key, out int i)) {
                i = 0;
            }

            return i;
        }

        public string GetString(string key) {
            if (_stringValues == null || !_stringValues.TryGetValue(key, out string s)) {
                s = "";
            }

            return s;
        }

        public float GetFloat(string key) {
            if (_floatValues == null || !_floatValues.TryGetValue(key, out float f)) {
                f = 0f;
            }

            return f;
        }

        public bool GetBool(string key) {
            if (_boolValues == null || !_boolValues.TryGetValue(key, out bool b)) {
                b = false;
            }

            return b;
        }

        public GameObject GetGameObject(string key) {
            if (_gameObjectValues == null || !_gameObjectValues.TryGetValue(key, out GameObject gameObject)) {
                gameObject = null;
            }

            return gameObject;
        }

        public MonoBehaviour GetMonoBehaviour(string key) {
            if (_monoBehaviourValues == null || !_monoBehaviourValues.TryGetValue(key, out MonoBehaviour monoBehaviour)) {
                return null;
            }

            return monoBehaviour;
        }

        public Action GetAction(string key) {
            if (_actions == null || !_actions.TryGetValue(key, out Action action)) {
                return null;
            }

            return action;
        }

        public Vector3 GetVector3(string key) {
            if (_vectors == null || !_vectors.TryGetValue(key, out Vector3 vector3)) {
                vector3 = new Vector3();
            }

            return vector3;
        }

        public Ability GetAbility(string key) {
            if (_abilities == null || !_abilities.TryGetValue(key, out Ability ability)) {
                return null;
            }

            return ability;
        }

        public Effect GetEffect(string key) {
            if (_effects == null || !_effects.TryGetValue(key, out Effect effect)) {
                return null;
            }

            return effect;
        }

        public Entity GetEntity(string key) {
            if (_entities == null || !_entities.TryGetValue(key, out Entity entity)) {
                return null;
            }

            return entity;
        }

        public Weapon GetWeapon(string key) {
            if (_weapons == null || !_weapons.TryGetValue(key, out Weapon weapon)) {
                return null;
            }

            return weapon;
        }

        public AbilityTrigger GetTrigger(string key) {
            if (_triggers == null || !_triggers.TryGetValue(key, out AbilityTrigger trigger)) {
                return null;
            }

            return trigger;
        }


        #endregion
    }

}
