using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using LL.FSM;

using GNode = UnityEditor.Experimental.GraphView.Node;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using LL.Events;
using System;

public class BehaviorNode : GNode
{
    public StateData CurrentState { get; set; }

    public string InfoText { get; set; }


    private Label stateNameLabel;
    private ObjectField stateDataField;

    public BehaviorNode() {
        //ObjectField stateDataField = new ObjectField {
        //    objectType = typeof(StateData),
        //    allowSceneObjects = false,
        //    //value = CurrentState,
        //};

        ////stateDataField.RegisterValueChangedCallback(OnStateChanged);

        ////stateDataField.RegisterValueChangedCallback<ChangeEvent<StateData>>(OnStateChanged);
        //stateDataField.RegisterValueChangedCallback(eventData => {
        //    //Debug.Log("Field changed: " + stateDataField.value.ToString());
        //    Draw();

        //});

        //titleContainer.Add(stateDataField);
    }

    private void OnStateChanged(ChangeEvent<UnityEngine.Object> evt) {

        
    }

    private void OnStateChanged(ChangeEvent<StateData> eventData){
        Debug.Log("State Changed: " + eventData.newValue.stateName);
    }


    public void UpdateTitle() {
        if(stateDataField.value != null) {
            CurrentState = stateDataField.value as StateData;

        }
        
        if (CurrentState != null) {
            stateNameLabel.text = CurrentState.stateName;
        }
    }

    public void Draw() {


        stateNameLabel = new Label();

        if (CurrentState != null) {
            stateNameLabel.text = CurrentState.stateName;
        }

        titleContainer.Insert(0, stateNameLabel);

        stateDataField = new ObjectField {
            objectType = typeof(StateData),
            allowSceneObjects = false,
            value = CurrentState,
        };

        //stateDataField.RegisterCallback<ChangeEvent<StateData>>(OnStateChanged);

        stateDataField.RegisterValueChangedCallback(eventData => {
            //Debug.Log("Field changed: " + stateDataField.value.ToString());
            UpdateTitle();

        });

        titleContainer.Add(stateDataField);

        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "Previous State";

        inputContainer.Add(inputPort);


        VisualElement customDataContainer = new VisualElement();

        Foldout infoFoldout = new Foldout() {
            text = "Info Text"  
        };

        TextField infoTextField = new TextField() {
            value = InfoText
        };

        infoFoldout.Add(infoTextField);

        customDataContainer.Add(infoFoldout);
        extensionContainer.Add(customDataContainer);
    }


}
