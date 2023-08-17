using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraCombatSystem : MonoBehaviour
{
    public Transform followTransform;
    public float weitghFollow = 0.5f;
    public Transform cameraTransform;

    public float normalSpeed = 3f;
    public float fastlSpeed = 8;
    public float movementSpeed = 3f;
    public float movementTime = 5f;
    public float rotationAmount = 1f;
    public Vector3 zoomAmount = new Vector3(0, -1f, 1f);


    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 newZoom;
    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 rotateStartPosition;
    public Vector3 rotateCurrentPosition;

    private InputSystemCustom inputSystemCustom;
    InputAction movementInput;
    InputAction rotationInput;
    InputAction sprint;

    private void Awake() {
        inputSystemCustom = new InputSystemCustom();
    }
    void Start() {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;

        movementInput = inputSystemCustom.Camera.Move;
        rotationInput = inputSystemCustom.Camera.Look;
        sprint = inputSystemCustom.Camera.Sprint;
    }
    private void OnEnable() {
        inputSystemCustom.Enable();
    }
    private void OnDisable() {
        inputSystemCustom.Disable();
    }
    // Update is called once per frame
    void Update() {
        if (followTransform != null) {
            newPosition = transform.position;
            transform.position = Vector3.Lerp(transform.position, followTransform.position, Time.deltaTime * movementTime);
        }

        HandleMomeventInput();
        HandleMouseInput();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            followTransform = null;
        }

    }
    void HandleMouseInput() {
        if (Input.mouseScrollDelta.y != 0) {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }

        if (Input.GetMouseButtonDown(1)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;
            if (plane.Raycast(ray, out entry)) {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(1)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (plane.Raycast(ray, out entry)) {
                dragCurrentPosition = ray.GetPoint(entry);
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                if (Mathf.Abs((Mathf.Abs(newPosition.x) - Mathf.Abs(transform.position.x))) > weitghFollow || Mathf.Abs((Mathf.Abs(newPosition.z) - Mathf.Abs(transform.position.z))) > weitghFollow) followTransform = null;
            }
        }

        if (Input.GetMouseButtonDown(2)) {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2)) {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }

    }

    void HandleMomeventInput() {

        // Movimento
        Vector2 moveInput = movementInput.ReadValue<Vector2>();
        if (moveInput != Vector2.zero) followTransform = null;
        newPosition += transform.forward * moveInput.y * movementSpeed * Time.deltaTime;
        newPosition += transform.right * moveInput.x * movementSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift) || sprint.IsPressed()) movementSpeed = fastlSpeed;
        else movementSpeed = normalSpeed;

        // Rotazione
        Vector2 rotateInput = rotationInput.ReadValue<Vector2>();
        //newRotation *= Quaternion.Euler(Vector3.up * rotateInput.x * rotationAmount);
        Vector2 zoomInput = rotationInput.ReadValue<Vector2>();
        //newZoom += -zoomInput.y * zoomAmount * 2 * Time.deltaTime;

        if (Mathf.Abs(zoomInput.y) < Mathf.Abs(rotateInput.x)) {
            newRotation *= Quaternion.Euler(Vector3.up * rotateInput.x * rotationAmount);
        } else {
            newZoom += -zoomInput.y * zoomAmount * 3 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            followTransform = null;
            newPosition += (transform.forward * movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            followTransform = null;
            newPosition -= (transform.forward * movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            followTransform = null;
            newPosition += (transform.right * movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            followTransform = null;
            newPosition -= (transform.right * movementSpeed * Time.deltaTime);
        }
        //if (Input.GetKey(KeyCode.Q)) {
        //    followTransform = null;
        //    newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        //}
        //if (Input.GetKey(KeyCode.E)) {
        //    followTransform = null;
        //    newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        //}

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    public void SetFollowTransform(Transform obj) {
        followTransform = obj;
    }
    public void SetFollowTransformToNull() {
        followTransform = null;
    }
}
