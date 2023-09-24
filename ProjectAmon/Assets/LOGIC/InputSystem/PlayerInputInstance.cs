using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputInstance : MonoBehaviour
{
    public static PlayerInputInstance Instance { get; set; }
    public PlayerInput _playerInput;
    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There's more than one PlayerInputInstance " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
