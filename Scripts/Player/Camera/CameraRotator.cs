using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraRotator : MonoBehaviour
{
    public static CameraRotator instance;

    [SerializeField]
    private float sensibilityX, sensibilityY = 400f;

    private Transform playerOrientation;
    [SerializeField]
    private Transform camHolder;

    private float xRotation, yRotation;

    private CinemachineVirtualCamera cmVCam;

    private void Awake()
    {
        instance = this;

        playerOrientation = GameObject.Find("PlayerOrientation").transform;
    }

    private void Start()
    {
        cmVCam = GetComponent<CinemachineVirtualCamera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensibilityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensibilityY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerOrientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float targetFOV = 75f) 
    {
        DOTween.To(() => cmVCam.m_Lens.FieldOfView,
                   x => cmVCam.m_Lens.FieldOfView = x,
                   targetFOV, 0.25f)
            .SetEase(Ease.Linear);
    }

    public void DoTilt(float zTilt) 
    {
        transform.DOLocalRotate(new Vector3(0,0,zTilt), 0.25f);
    }
}
