using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManagerEventArgs : EventArgs {
    public CharacterManagerSingle characterManagerSingle; // Campo per passare un intero (puoi aggiungere più campi se necessario).
}
public class CharacterControllerModeEventArgs : EventArgs {
    public CharacterControllerMode characterControllerMode; // Campo per passare un intero (puoi aggiungere più campi se necessario).
}
public enum CharacterControllerMode {
    ThirdPersonController = 1,
    ClickToMove = 5
}
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; set; }

    public CharacterManagerSingle currentCharacterManagerSingle;

    public List<CharacterManagerSingle> characterManagerSingleList;

    private InputSystemCustom inputSystemCustom;

    public int currentCharacterIndex = 0;

    public event EventHandler<CharacterManagerEventArgs> OnGameCurrentCharacterChanged;
    public event EventHandler<CharacterControllerModeEventArgs> OnCharacterControllerModeChanged;
    public CharacterControllerMode characterControllerMode;

    public bool allCharacterFollowingPlayer = false;

    public bool thridControllerMode = true;
    [Header("AI Variable")]
    public float stoppingDistance = 2f;
    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There's more than one CharacterManager " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        inputSystemCustom = new InputSystemCustom();
    }
    private void Start() {

        inputSystemCustom.Player.CharacterSwitchNext.performed += CharacterSwitchNext_performed;
        inputSystemCustom.Player.CharacterSwitchPreviews.performed += CharacterSwitchPreviews_performed;
        inputSystemCustom.Player.CharacterFollow.performed += CharacterFollow_performed;
        inputSystemCustom.Player.SwitchControlMode.performed += SwitchControlMode_performed;

        SetMainCharacter(currentCharacterIndex);
        // Aggiorna tutti i parametri
        UpdateAllCharacterController();
        UpdateFollowPlayer();
        StartCoroutine(EnableMainCharacterMove());
    }
    void SetMainCharacter(int characterIndex) {
        // Assicurati che l'indice sia valido
        if (characterIndex >= 0 && characterIndex < characterManagerSingleList.Count) {
            if (characterManagerSingleList[characterIndex] == currentCharacterManagerSingle) return;
            // Attiva il personaggio corrente
            MakeCharacterStanAlone(currentCharacterManagerSingle);
            // Attiva il personaggio dell'index
            currentCharacterManagerSingle = characterManagerSingleList[characterIndex];

            // Aggiorna tutti i parametri
            UpdateAllCharacterController();
            UpdateFollowPlayer();
            StartCoroutine(EnableMainCharacterMove());

            
        }
        // Aggiorna tutti i parametri
        UpdateAllCharacterController();
        UpdateFollowPlayer();
        StartCoroutine(EnableMainCharacterMove());

        // Crea un'istanza della classe personalizzata e inizializza i campi.
        CharacterManagerEventArgs args = new CharacterManagerEventArgs {
            characterManagerSingle = this.currentCharacterManagerSingle
        };
        // Controlla se ci sono sottoscrittori all'evento prima di inviarlo.
        if (OnGameCurrentCharacterChanged != null) {
            // Invia l'evento con i dati attraverso gli sottoscrittori.
            OnGameCurrentCharacterChanged(this, args);
        }
    }
    void MakeCharacterStanAlone(CharacterManagerSingle characterManagerSingle) {
        characterManagerSingle.thirdPersonControllerEsplorations.ResetCameraRoot();
        characterManagerSingle.thirdPersonControllerEsplorations.yourTurn = false;
        characterManagerSingle.characterControllerMode = CharacterControllerMode.ClickToMove;
    }
    void UpdateAllCharacterController() {
        foreach (CharacterManagerSingle character in characterManagerSingleList) {
            if (character == currentCharacterManagerSingle) {
                currentCharacterManagerSingle.characterControllerMode = this.characterControllerMode;
                currentCharacterManagerSingle.thirdPersonControllerEsplorations.ResetCameraRoot();
            } else {
                character.characterControllerMode = CharacterControllerMode.ClickToMove;
                character.thirdPersonControllerEsplorations.ResetCameraRoot();
            }
        }
    }
    void UpdateFollowPlayer() {
        foreach (CharacterManagerSingle character in characterManagerSingleList) {
            if (character == currentCharacterManagerSingle) {
                currentCharacterManagerSingle.isFollowingPlayer = false;
            } else {
                character.isFollowingPlayer = allCharacterFollowingPlayer;
            }
        }
    }
    IEnumerator EnableMainCharacterMove() {
        yield return new WaitForSeconds(CameraManager.Instance.cameraTransitionTime);
        currentCharacterManagerSingle.thirdPersonControllerEsplorations.yourTurn = true;
    }
    private void SwitchControlMode_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if(GamePlayManager.Instance.GetGamePlayMode() == GamePlayMode.Esploration) {
            thridControllerMode = !thridControllerMode;
            if (thridControllerMode) {
                characterControllerMode = CharacterControllerMode.ThirdPersonController;
                SetCursorState(true);
            } else {
                characterControllerMode = CharacterControllerMode.ClickToMove;
                SetCursorState(false);
            } 

            // Crea un'istanza della classe personalizzata e inizializza i campi.
            CharacterControllerModeEventArgs args = new CharacterControllerModeEventArgs {
                characterControllerMode = this.characterControllerMode
            };

            // Controlla se ci sono sottoscrittori all'evento prima di inviarlo.
            if (OnCharacterControllerModeChanged != null) {
                // Invia l'evento con i dati attraverso gli sottoscrittori.
                OnCharacterControllerModeChanged(this, args);
            }
            UpdateAllCharacterController();
        }
    }
    void UpdateCharacterControllerMode() {

        foreach (CharacterManagerSingle character in characterManagerSingleList) {
            if (character == currentCharacterManagerSingle) {
                currentCharacterManagerSingle.characterControllerMode = this.characterControllerMode;
            } else {
                character.characterControllerMode = CharacterControllerMode.ClickToMove;
            }
        }
    }

    private void CharacterFollow_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        allCharacterFollowingPlayer = !allCharacterFollowingPlayer;
        UpdateFollowPlayer();
    }
    
    private void CharacterSwitchPreviews_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (GamePlayManager.Instance.GetGamePlayMode() != GamePlayMode.Esploration) return;

        if (characterManagerSingleList.Count < 1) return;

        currentCharacterIndex--; // Aumenta l'indice del personaggio corrente

        // Se l'indice supera la dimensione della lista dei personaggi, ritorna al primo personaggio
        if (currentCharacterIndex < 0) {
            currentCharacterIndex = characterManagerSingleList.Count -1;
        }
        // Imposta il personaggio corrente in base all'indice calcolato
        SetMainCharacter(currentCharacterIndex);
    }

    private void CharacterSwitchNext_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {

        if (GamePlayManager.Instance.GetGamePlayMode() != GamePlayMode.Esploration) return;

        if (characterManagerSingleList.Count < 1) return;

        currentCharacterIndex++; // Aumenta l'indice del personaggio corrente

        // Se l'indice supera la dimensione della lista dei personaggi, ritorna al primo personaggio
        if (currentCharacterIndex >= characterManagerSingleList.Count) {
            currentCharacterIndex = 0;
        }
        // Imposta il personaggio corrente in base all'indice calcolato
        SetMainCharacter(currentCharacterIndex);
    }
    void SetCurrentCharacter(int characterIndex) {
        // Assicurati che l'indice sia valido
        if (characterIndex >= 0 && characterIndex < characterManagerSingleList.Count) {
            currentCharacterManagerSingle.thirdPersonControllerEsplorations.ResetCameraRoot();
            currentCharacterManagerSingle.thirdPersonControllerEsplorations.yourTurn = false;
            currentCharacterManagerSingle.characterControllerMode = CharacterControllerMode.ClickToMove;
            // Attiva il personaggio corrente
            currentCharacterManagerSingle = characterManagerSingleList[characterIndex];
            currentCharacterManagerSingle.characterControllerMode = this.characterControllerMode;
            currentCharacterManagerSingle.thirdPersonControllerEsplorations.ResetCameraRoot();
            UpdateFollowPlayer();
            StartCoroutine(SetCanMove());
            // Crea un'istanza della classe personalizzata e inizializza i campi.
            CharacterManagerEventArgs args = new CharacterManagerEventArgs {
                characterManagerSingle = this.currentCharacterManagerSingle
            };
            // Controlla se ci sono sottoscrittori all'evento prima di inviarlo.
            if (OnGameCurrentCharacterChanged != null) {
                // Invia l'evento con i dati attraverso gli sottoscrittori.
                OnGameCurrentCharacterChanged(this, args);
            }
        }
    }
    IEnumerator SetCanMove() {
        yield return new WaitForSeconds(CameraManager.Instance.cameraTransitionTime);
        currentCharacterManagerSingle.thirdPersonControllerEsplorations.yourTurn = true;
    }
    private void SetCursorState(bool newState) {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
    private void OnEnable() {
        inputSystemCustom.Enable();
    }
    private void OnDisable() {
        inputSystemCustom.Disable();
    }
    private void OnDestroy() {
        inputSystemCustom.Player.CharacterSwitchNext.performed -= CharacterSwitchNext_performed;
        inputSystemCustom.Player.CharacterSwitchPreviews.performed -= CharacterSwitchPreviews_performed;
        inputSystemCustom.Player.CharacterFollow.performed -= CharacterFollow_performed;
        inputSystemCustom.Player.SwitchControlMode.performed -= SwitchControlMode_performed;
    }
}
