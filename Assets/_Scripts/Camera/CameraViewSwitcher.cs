using Unity.Cinemachine;
using UnityEngine;

// Alterna entre a câmera de terceira e de primeira pessoa; quem decide quando trocar chama SetFirstPerson (ex.: um trigger dentro de casa).
public class CameraViewSwitcher : MonoBehaviour
{
    public static CameraViewSwitcher Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CinemachineOrbitalFollow thirdPersonOrbit;
    [SerializeField] private CinemachineCamera firstPersonCamera;
    [SerializeField] private CinemachinePanTilt firstPersonPanTilt;
    [SerializeField] private Renderer playerRenderer;

    [SerializeField] private bool IsFirstPerson;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyView();
    }

    private void LateUpdate()
    {
        // Mantém a órbita em standby olhando para onde o player olha, para a volta à terceira pessoa não "girar" a câmera.
        if (IsFirstPerson)
            thirdPersonOrbit.HorizontalAxis.Value = firstPersonPanTilt.PanAxis.Value;
    }

    public void SetFirstPerson(bool isFirstPerson)
    {
        // Evita resetar o tilt se alguém chamar repetidamente (ex.: OnTriggerStay).
        if (isFirstPerson == IsFirstPerson)
            return;

        IsFirstPerson = isFirstPerson;
        ApplyView();
    }

    private void ApplyView()
    {
        // Com Binding Mode = World Space, o eixo horizontal da órbita já é o yaw da câmera, o mesmo valor que o Pan espera.
        if (IsFirstPerson)
        {
            firstPersonPanTilt.PanAxis.Value = thirdPersonOrbit.HorizontalAxis.Value;
            firstPersonPanTilt.TiltAxis.Value = 0f;
        }

        firstPersonCamera.enabled = IsFirstPerson;
        playerRenderer.enabled = !IsFirstPerson;
    }

    public bool getIsFirstPerson()
    {
        return IsFirstPerson;
    }

    public void setIsFirstPerson(bool isActive)
    {
        IsFirstPerson = isActive;
    }
}
