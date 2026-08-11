using System;
using TreeEditor;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin cinemachinePerlin;
    private float shakeTimer;
    private float startingdAmplitude;
    private float shakeTimerTotal;

    private void Awake()
    {
        Instance = this;
        cinemachinePerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float amplitude, float frequency, float duration)
    {
        if (cinemachinePerlin == null) return;

        // Guardamos valores iniciales
        startingdAmplitude = amplitude;
        shakeTimerTotal = duration;
        shakeTimer = duration;

        // Aplicamos la intensidad máxima al inicio
        cinemachinePerlin.AmplitudeGain = amplitude;
        cinemachinePerlin.FrequencyGain = frequency;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            // Progreso de 1 (inicio) a 0 (final)
            float progress = Mathf.Clamp01(shakeTimer / shakeTimerTotal);

            // Interpolación lineal de la amplitud: desde startAmplitude hasta 0
            cinemachinePerlin.AmplitudeGain = Mathf.Lerp(0f, startingdAmplitude, progress);

            // También puedes reducir la frecuencia si lo deseas (opcional)
            // perlin.FrequencyGain = Mathf.Lerp(0f, startFrequency, progress);
        }
        else if (shakeTimer < 0f)
        {
            // Finaliza la sacudida (asegurar que todo quede en 0)
            cinemachinePerlin.AmplitudeGain = 0f;
            cinemachinePerlin.FrequencyGain = 0f;
            shakeTimer = 0f; // Evita que entre de nuevo
        }
    }
}
