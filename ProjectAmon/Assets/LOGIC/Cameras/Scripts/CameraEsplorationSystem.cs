using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraEsplorationSystem : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    private void Awake() {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    public void SetCameraFollow() {
        if(CharacterManager.Instance.currentCharacterManagerSingle != null) {
            virtualCamera.Follow = CharacterManager.Instance.currentCharacterManagerSingle.characterCameraController.cameraRootStartPoint;
        }
    }
    public void SetUnFollow() {
        virtualCamera.Follow = null;
    }
}
