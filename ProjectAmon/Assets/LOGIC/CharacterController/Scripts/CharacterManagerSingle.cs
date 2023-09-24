using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterCameraController))]
[RequireComponent(typeof(ThirdPersonControllerEsplorations))]
public class CharacterManagerSingle : MonoBehaviour
{
    public CharacterCameraController characterCameraController;
    public ThirdPersonControllerEsplorations thirdPersonControllerEsplorations;

    public CharacterControllerMode characterControllerMode;

    public bool isFollowingPlayer = false;
    public Transform followPlayerPoistion;

    private void Awake() {
        characterCameraController = GetComponent<CharacterCameraController>();
        thirdPersonControllerEsplorations = GetComponent<ThirdPersonControllerEsplorations>();
    }

    public void SetToFollowPlayer(bool isFollowingPlayer) {
        this.isFollowingPlayer = isFollowingPlayer;
    }
    public void SetToThirdPersonController(bool isFollowingPlayer) {
        this.isFollowingPlayer = isFollowingPlayer;
        SetToFollowPlayer(false);
    }
    public void SetToClickToMoveController(bool isFollowingPlayer) {
        this.isFollowingPlayer = isFollowingPlayer;
    }
}

