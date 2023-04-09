using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UltraFunGuns;
public class Freecam : MonoBehaviour
{
    public delegate void OnFreecamStateChangeHandler(bool enabled);
    public static OnFreecamStateChangeHandler OnFreecamStateChange;

    public static bool Enabled { get; private set; }

    //Static stuff
    private static Freecam freecam;
    public static Freecam Instance
    {
        get
        {
            if (freecam == null)
            {
                freecam = GameObject.FindObjectOfType<Freecam>();
                if (freecam == null)
                    SpawnFreecam();
            }
            return freecam;
        }
    }
    public static void SpawnFreecam()
    {
        if (freecam == null)
            freecam = new GameObject().AddComponent<Freecam>();
    }

    //Component stuff
    private Camera camera;
    private Transform cameraTransform;

    private float minZoom = 1e-05f, maxZoom = 179f;
    private float currentZoom = 77.0f;
    private float zoomSpeed = 25.0f;

    private float moveSpeed = 50.0f;

    private float lookSpeed = 16.0f;

    private Vector3 currentLocalRotation;

    private Vector3 moveInput, lookInput;
    private float zoomInput;

    private Vector3 lastMousePos;


    private Dictionary<Camera, bool> cachedCameras = new Dictionary<Camera, bool>();
    private Dictionary<Canvas, bool> cachedCanvases = new Dictionary<Canvas, bool>();


    private void Awake()
    {
        gameObject.name = "Freecam";
        Debug.LogWarning("Freecam Spawned");
        DontDestroyOnLoad(gameObject);
        ConstructFreecam();
    }

    private void Update()
    {
        GetInputs();
        if (!Enabled)
            return;
        Smooth();
        Look();
        Move();
        Zoom();
    }


    public void SetEnabled(bool enabled)
    {
        SetOtherCameras(!enabled);
        SetCanvases(!enabled);
        SetLockmode(enabled);
        camera.enabled = enabled;
        Enabled = enabled;
        Debug.LogWarning($"Freecam {((Enabled)?"Enabled":"Disabled")}");
    }


    private void SetOtherCameras(bool enabled)
    {
        if (enabled)
        {
            foreach (KeyValuePair<Camera, bool> cam in cachedCameras)
            {
                if (cam.Key == null)
                    continue;

                cam.Key.enabled = cam.Value;
            }

            cachedCameras.Clear();
        }
        else
        {
            List<Camera> otherCameras = GameObject.FindObjectsOfType<Camera>().Where(x => x != camera).ToList();

            foreach (Camera cam in otherCameras)
            {
                cam.enabled = false;
                cachedCameras.Add(cam, cam.enabled);
            }
        }
    }

    private void SetCanvases(bool enabled)
    {
        if (enabled)
        {
            foreach (KeyValuePair<Canvas, bool> canvas in cachedCanvases)
            {
                if (canvas.Key == null)
                    continue;

                canvas.Key.enabled = canvas.Value;
            }

            cachedCanvases.Clear();
        }
        else
        {
            List<Canvas> otherCanvases = GameObject.FindObjectsOfType<Canvas>().ToList();

            foreach (Canvas canvas in otherCanvases)
            {
                canvas.enabled = false;
                cachedCanvases.Add(canvas, canvas.enabled);
            }
        }
    }


    private CursorLockMode lastLockMode;
    private void SetLockmode(bool enabled)
    {
        if(enabled)
        {
            lastLockMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.Locked;
        }else
        {
            Cursor.lockState = lastLockMode;
        }
    }

    private void GetInputs()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SetEnabled(!Enabled);
        }

        if (!Enabled)
            return;

        moveInput.z = ((Input.GetKey(KeyCode.W)) ? 1 : 0) + ((Input.GetKey(KeyCode.S)) ? -1 : 0);
        moveInput.x = ((Input.GetKey(KeyCode.A)) ? -1 : 0) + ((Input.GetKey(KeyCode.D)) ? 1 : 0);
        moveInput.y = ((Input.GetKey(KeyCode.Space)) ? 1 : 0) + ((Input.GetKey(KeyCode.LeftControl)) ? -1 : 0);

        Vector3 mousePos = Input.mousePosition;
        Vector2 mouseDelta = mousePos - lastMousePos;
        lastMousePos = mousePos;

        lookInput.x = (-mouseDelta.x) + ((Input.GetKey(KeyCode.LeftArrow)) ? 1 : 0) + ((Input.GetKey(KeyCode.RightArrow)) ? -1 : 0);
        lookInput.y = (-mouseDelta.y) + ((Input.GetKey(KeyCode.UpArrow)) ? -1 : 0) + ((Input.GetKey(KeyCode.DownArrow)) ? 1 : 0);
        lookInput.z = ((Input.GetKey(KeyCode.Q)) ? -1 : 0) + ((Input.GetKey(KeyCode.E)) ? 1 : 0);

        zoomInput = Input.mouseScrollDelta.y + ((Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus)) ? 1 : 0) + ((Input.GetKey(KeyCode.Minus) || (Input.GetKey(KeyCode.KeypadMinus)) ? -1 : 0));
    }

    private void Smooth()
    {

    }

    private void Look()
    {
        currentLocalRotation.x += lookInput.y * lookSpeed * Time.deltaTime;
        currentLocalRotation.y += lookInput.x * lookSpeed * Time.deltaTime;
        currentLocalRotation.z += lookInput.z * lookSpeed * Time.deltaTime;

        cameraTransform.localRotation = Quaternion.Euler(currentLocalRotation);
    }

    private void Move()
    {
        Vector3 moveVector = moveInput * moveSpeed * Time.deltaTime;
        transform.position += cameraTransform.rotation * moveVector;
    }

    private void Zoom()
    {
        currentZoom = Mathf.Clamp(currentZoom + (zoomInput * zoomSpeed * Time.deltaTime), minZoom, maxZoom);
        camera.fieldOfView = currentZoom;
    }

    private void ConstructFreecam()
    {
        cameraTransform = new GameObject("CameraHolder").transform;
        cameraTransform.parent = transform;
        cameraTransform.localPosition = Vector3.zero;
        cameraTransform.localRotation = Quaternion.identity;

        camera = cameraTransform.gameObject.AddComponent<Camera>();
        camera.fieldOfView = currentZoom;
        camera.nearClipPlane = 0.01f;
        camera.enabled = false;
    }

}