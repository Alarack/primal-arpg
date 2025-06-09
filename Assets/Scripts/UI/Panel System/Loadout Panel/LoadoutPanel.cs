using UnityEngine;

public class LoadoutPanel : BasePanel
{


    private LoadoutEntry[] loadouts;



    protected override void Awake() {
        base.Awake();

        loadouts = GetComponentsInChildren<LoadoutEntry>(true);
        for (int i = 0; i < loadouts.Length; i++) {
            loadouts[i].Setup();
        }
    }





    public void OnStartClicked() {
        for (int i = 0; i < loadouts.Length; i++) {
            loadouts[i].SpawnSelectedItem();
        }

        PanelManager.GetPanel<CharacterSelectPanel>().StartGame();
        Close();
    }

}
