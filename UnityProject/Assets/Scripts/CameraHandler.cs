using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: make this prettier.
public class CameraHandler : MonoBehaviour
{
    //Camera camera;
    float movementSpeed;
    float rotationSpeed;

    Transform capsuleTransform;

    void Awake()
    {
        //camera = Camera.current;

        movementSpeed = 0.1f;
        rotationSpeed = 60f;

        Cursor.lockState = CursorLockMode.Locked;
        capsuleTransform = this.transform.parent;
    }

    // Handle the camera movement.
    void Update()
    {
        this.transform.position = capsuleTransform.position;

        if (Input.GetKeyDown("x"))
        {
            movementSpeed /= Mathf.Sqrt(10f);
            print(movementSpeed);
        }

        if (Input.GetKeyDown("c"))
        {
            movementSpeed *= Mathf.Sqrt(10f);
            print(movementSpeed);
        }

        if (Input.GetKeyDown("v"))
        {
            rotationSpeed -= 5f;
            rotationSpeed = Mathf.Max(rotationSpeed, 0f);
            print(rotationSpeed);
        }

        if (Input.GetKeyDown("b"))
        {
            rotationSpeed += 5f;
            print(rotationSpeed);
        }

        if (Input.GetKeyDown("r"))
        {
            this.transform.localPosition = new Vector3(0f,0f,0f);
            this.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }

        Vector3 currentPosition = GetComponent<Camera>().transform.localPosition;
        Vector3 currentRotation = GetComponent<Camera>().transform.localEulerAngles;

        currentRotation = currentRotation + new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0)  * rotationSpeed * Time.deltaTime;

        GetComponent<Camera>().transform.localEulerAngles = currentRotation;

        if (Input.GetKey("space"))
        {
            Vector3 direction = GetComponent<Camera>().transform.forward;
            currentPosition += direction * movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 direction = GetComponent<Camera>().transform.forward;
            currentPosition -= direction * movementSpeed * Time.deltaTime;
        }

        this.transform.localPosition = currentPosition;

        if (Input.GetKey("escape"))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        capsuleTransform.position = this.transform.position;
       
    }
}
