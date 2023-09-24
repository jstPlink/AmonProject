using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; set; }
    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There's more than one CameraManager " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    [SerializeField] CameraEsplorationSystem cameraEsplorationSystem;
    [SerializeField] CameraCombatSystem cameraCombatSystem;
    private CinemachineBrain cinemachineBrain;
    public float cameraTransitionTime = 1f;
    private void Start() {
        GamePlayManager.Instance.OnGamePlayModeChanged += Instance_OnGamePlayModeChanged;
        //GamePlayManager.Instance.UpdateSubscriberChanged();

        CharacterManager.Instance.OnGameCurrentCharacterChanged += Instance_OnGameCurrentCharacterChanged;
        CharacterManager.Instance.OnCharacterControllerModeChanged += Instance_OnCharacterControllerModeChanged;

        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        cinemachineBrain.m_DefaultBlend.m_Time = cameraTransitionTime;

        // Only for Test
        cameraEsplorationSystem.gameObject.SetActive(true);
        cameraCombatSystem.gameObject.SetActive(false);
        cameraEsplorationSystem.SetCameraFollow();

    }

    private void Instance_OnCharacterControllerModeChanged(object sender, CharacterControllerModeEventArgs e) {
        UpdateCamera(e);
    }

    private void Instance_OnGameCurrentCharacterChanged(object sender, CharacterManagerEventArgs e) {
        if (GamePlayManager.Instance.GetGamePlayMode() != GamePlayMode.Esploration) return;
       
        StartCoroutine(SetCameraFollow());

    }

    private void Instance_OnGamePlayModeChanged(object sender, GamePlayModeChangedEventArgs e) {
        //UpdateCamera(e);
    }
    void UpdateCamera(CharacterControllerModeEventArgs e) {
        if (e.characterControllerMode == CharacterControllerMode.ThirdPersonController) {

            cameraEsplorationSystem.gameObject.SetActive(true);
            cameraCombatSystem.gameObject.SetActive(false);
            cameraEsplorationSystem.SetCameraFollow();
            //StartCoroutine(SetCameraFollow());

        } else {
            cameraEsplorationSystem.SetUnFollow();
            cameraEsplorationSystem.gameObject.SetActive(false);
            cameraCombatSystem.gameObject.SetActive(true);
            cameraCombatSystem.SetFollowTransform(CharacterManager.Instance.currentCharacterManagerSingle.characterCameraController.cameraRootStartPoint);
        }
    }
    IEnumerator SetCameraFollow() {
        yield return new WaitForSeconds(0f);
        cameraEsplorationSystem.SetCameraFollow();
        cameraCombatSystem.SetFollowTransform(CharacterManager.Instance.currentCharacterManagerSingle.characterCameraController.cameraRootStartPoint);
    }
    private void OnDestroy() {
        GamePlayManager.Instance.OnGamePlayModeChanged += Instance_OnGamePlayModeChanged;
    }
}
