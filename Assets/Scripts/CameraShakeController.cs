using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CameraShakeController : MonoBehaviour
{
    private CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noiseModule;

    private CinemachineOrbitalFollow orbitalFollow;

    public float SpeedFov = 70f;
    public float NormalFov = 60f;

    private bool speeding = false;
    private float speedShakeIntensity;
    private bool landing = false;
    private float landingShakeIntensity;

    
    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        if (virtualCamera != null)
        {
            noiseModule = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            orbitalFollow = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
        }
    }

    public void ShakeCameraLanding(float intensity, float duration)
    {
        if (noiseModule != null)
        {
            landing = true;
            landingShakeIntensity = intensity;
            noiseModule.AmplitudeGain = intensity;
            StartCoroutine(WaitTime(duration));
            StartCoroutine(ChangeFOVinNout(NormalFov - 10f, 0.2f));
        }
    }

    IEnumerator WaitTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopShake();
    }

    private void StopShake()
    {
        if (noiseModule != null)
        {
            if (speeding)
            {
                noiseModule.AmplitudeGain = speedShakeIntensity;
            } else
            {
                noiseModule.AmplitudeGain = 0f;
            }
            landing = false;
        }
    }

    public void ShakeCameraSpeed(float intensity)
    {
        if (noiseModule != null)
        {
            if (!landing)
                noiseModule.AmplitudeGain = intensity;
            speedShakeIntensity = intensity;
            speeding = true;
            StartCoroutine(ChangeFOV(SpeedFov, 0.5f));
        }
    }

    public void StopShakeCameraSpeed()
    {
        if (noiseModule != null)
        {
            if (landing)
            {
                noiseModule.AmplitudeGain = landingShakeIntensity;
            } else
            {
                noiseModule.AmplitudeGain = 0f;
            }
            speeding = false;
            StartCoroutine(ChangeFOV(NormalFov, 0.5f));
        }
    }

    IEnumerator ChangeFOV(float endFOV, float duration)
    {
        float startFOV = virtualCamera.Lens.FieldOfView;
        float time = 0;
        while(time < duration)
        {
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, time / duration);
            yield return null;
            time += Time.deltaTime;
        }
    }

    IEnumerator ChangeFOVinNout(float endFOV, float duration)
    {
        float startFOV = virtualCamera.Lens.FieldOfView;
        float time = 0;
        while(time < duration / 2)
        {
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, time / (duration / 2));
            yield return null;
            time += Time.deltaTime;
        }
        time = 0;
        while(time < duration / 2)
        {
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(endFOV, startFOV, time / (duration / 2));
            yield return null;
            time += Time.deltaTime;
        }    
    }
}
