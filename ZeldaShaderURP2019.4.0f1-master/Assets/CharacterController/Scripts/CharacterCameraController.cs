using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraController : MonoBehaviour
{
    public Transform cameraRootStartPoint;
    public Transform cameraRootThirdPersonPoin;
    private void Awake() {
        if(cameraRootStartPoint == null) {
            Debug.LogError("Camera Root Setup Point Non Trovato!");
        }
        if (cameraRootThirdPersonPoin == null) {
            Debug.LogError("Camera Root Third Person Point Non Trovato!");
        }
    }
}
