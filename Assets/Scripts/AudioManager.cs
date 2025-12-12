using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public AudioSource Wind;
    public AudioSource Jump;
    public AudioSource Landing;
    public AudioSource Grind;
    public AudioSource Points;

    // Dictionary to store original volumes for reset
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();
    private Dictionary<AudioSource, Coroutine> activeFades = new Dictionary<AudioSource, Coroutine>();

    void Awake()
    {
        // Cache original volumes
        if (Wind) originalVolumes[Wind] = Wind.volume;
        if (Jump) originalVolumes[Jump] = Jump.volume;
        if (Landing) originalVolumes[Landing] = Landing.volume;
        if (Grind) originalVolumes[Grind] = Grind.volume;
        if (Points) originalVolumes[Points] = Points.volume;
    }
    
    public void PlayAudio(string audioName)
    {
        AudioSource source = GetSource(audioName);
        if (source != null)
        {
            // Cancel active fade if any
            if (activeFades.ContainsKey(source))
            {
                if (activeFades[source] != null) StopCoroutine(activeFades[source]);
                activeFades.Remove(source);
            }

            // Ensure volume is reset before playing
            if (originalVolumes.ContainsKey(source))
            {
                source.volume = originalVolumes[source];
            }
            source.Play();
        }
    }

    public void StopAudio(string audioName, bool fade = false, float duration = 1.0f)
    {
        AudioSource source = GetSource(audioName);
        if (source != null)
        {
            // Cancel previous fade if switching to new stop
            if (activeFades.ContainsKey(source))
            {
                if (activeFades[source] != null) StopCoroutine(activeFades[source]);
                activeFades.Remove(source);
            }

            if (fade && source.isPlaying)
            {
                activeFades[source] = StartCoroutine(FadeOutRoutine(source, duration));
            }
            else
            {
                source.Stop();
            }
        }
    }

    private AudioSource GetSource(string name)
    {
        switch (name)
        {
            case "Wind": return Wind;
            case "Jump": return Jump;
            case "Landing": return Landing;
            case "Grind": return Grind;
            case "Points": return Points;
            default:
                Debug.LogWarning("Audio not found: " + name);
                return null;
        }
    }

    private IEnumerator FadeOutRoutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        
        // Reset volume for next time
        if (originalVolumes.ContainsKey(source))
        {
            source.volume = originalVolumes[source];
        }
        
        if (activeFades.ContainsKey(source))
        {
            activeFades.Remove(source);
        }
    }
}
