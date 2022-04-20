using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    //Camera camera;
    float movementSpeed;
    float rotationSpeed;
    // Start is called before the first frame update
    void Awake()
    {
        //camera = Camera.current;

        movementSpeed = 0.1f;
        rotationSpeed = 60f;
    }

    // Handle the camera movement.
    void Update()
    {
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

        if (Input.GetKey("n"))
        {
            this.transform.Rotate(new Vector3(0,0, rotationSpeed * Time.deltaTime));
        }

        if (Input.GetKey("m"))
        {
            this.transform.Rotate(new Vector3(0,0, -rotationSpeed * Time.deltaTime));
        }

        if (Input.GetKeyDown("r"))
        {
            this.transform.localPosition = new Vector3(0f,0f,0f);
            this.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }

        Vector3 currentPosition = GetComponent<Camera>().transform.localPosition;

        this.transform.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime,0));
        this.transform.Rotate(new Vector3(-Input.GetAxis("Vertical") * rotationSpeed * Time.deltaTime, 0,0));

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
            Application.Quit();
        }


       
    }
}
