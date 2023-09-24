using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public enum GamePlayMode {
    Esploration = 1,
    Combat = 10
}
public class GamePlayModeChangedEventArgs : EventArgs {
    public GamePlayMode gamePlayMode; // Campo per passare un intero (puoi aggiungere più campi se necessario).
}

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; set; }
    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There's more than one GamePlayManager " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public GamePlayMode gamePlayMode;
    public bool isTestMode = true;

    public event EventHandler<GamePlayModeChangedEventArgs> OnGamePlayModeChanged;
    //public event EventHandler OnGamePlayModeChanged;
    private void Start() {
        SwitchToEsplorationMode();
    }
    private void Update() {
        //if (isTestMode) {
        //    if (Input.GetKey(KeyCode.T)) {
        //        SwitchToEsplorationMode();
        //    }
        //    if (Input.GetKey(KeyCode.G)) {
        //        SwitchToCombatMode();
        //    }
        //}
    }
    public void SwitchToEsplorationMode() {
        gamePlayMode = GamePlayMode.Esploration;
        SetCursorState(true);
        UpdateSubscriberChanged();
    }
    public void SwitchToCombatMode() {
        gamePlayMode = GamePlayMode.Combat;
        SetCursorState(false);
        UpdateSubscriberChanged();
    }
    public void UpdateSubscriberChanged() {
        // Crea un'istanza della classe personalizzata e inizializza i campi.
        GamePlayModeChangedEventArgs args = new GamePlayModeChangedEventArgs {
            gamePlayMode = this.gamePlayMode
        };

        // Controlla se ci sono sottoscrittori all'evento prima di inviarlo.
        if (OnGamePlayModeChanged != null) {
            // Invia l'evento con i dati attraverso gli sottoscrittori.
            OnGamePlayModeChanged(this, args);
        }
        //OnGamePlayModeChanged?.Invoke(this, EventArgs.Empty);


        // Funzione di esempio per gestire l'evento quando viene innescato.
        //private void HandleGamePlayModeChanged(object sender, GamePlayModeChangedEventArgs e) {
        //    // Esempio di come accedere ai campi passati con l'evento.
        //    int valoreRicevuto = e.someValue;
        //    Debug.Log("Valore ricevuto dall'evento: " + valoreRicevuto);
        //}
    }
    public GamePlayMode GetGamePlayMode() {
        return gamePlayMode;
    }
    private void SetCursorState(bool newState) {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GamePlayManager))]
public class CharacterActionManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GamePlayManager camePlayManager = (GamePlayManager)target;
        if (GUILayout.Button("SwitchToEsplorationMode")) {
            camePlayManager.SwitchToEsplorationMode();
        }
        if (GUILayout.Button("SwitchToCombatMode")) {
            camePlayManager.SwitchToCombatMode();
        }

    }
}
#endif