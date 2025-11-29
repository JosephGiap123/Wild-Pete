using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SGAudioManager : MonoBehaviour
{
	[Header("Audio Source (auto-created if empty)")]
	[SerializeField] private AudioSource sfxSource;

	[Header("Clips")]
	[SerializeField] private AudioClip explodeClip;
	[SerializeField] private AudioClip beepClip; // short beep used for periodic beeping
	[SerializeField] private AudioClip runLoopClip; // looping run sound

	[Header("Mixer (optional)")]
	[SerializeField] private AudioMixerGroup sfxMixerGroup;

	[Header("Tuning")]
	[Range(0f, 1f)] public float sfxVolume = 1f;
	[Range(0f, 0.3f)] public float pitchJitter = 0.03f;
	public float max3dDistance = 20f;
	[SerializeField] private float beepInterval = 0.8f;
	[SerializeField] private float beepSpeedMultiplier = 5.25f; // 3x faster beeps
	[SerializeField] private float runLoopPitchMultiplier = 3.25f; // 10x faster walking

	private AudioSource loopSource; // used for run loop
	private Coroutine beepCoroutine;

	private void Awake()
	{
		if (!sfxSource) sfxSource = gameObject.GetComponent<AudioSource>();
		if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();

		sfxSource.spatialBlend = 1f;
		sfxSource.rolloffMode = AudioRolloffMode.Linear;
		sfxSource.maxDistance = max3dDistance;
		sfxSource.playOnAwake = false;
		sfxSource.outputAudioMixerGroup = sfxMixerGroup;

		loopSource = gameObject.AddComponent<AudioSource>();
		loopSource.loop = true;
		loopSource.spatialBlend = 1f;
		loopSource.rolloffMode = AudioRolloffMode.Linear;
		loopSource.maxDistance = max3dDistance;
		loopSource.playOnAwake = false;
		loopSource.outputAudioMixerGroup = sfxMixerGroup;
		loopSource.volume = sfxVolume;
	}

	private void OnDisable()
	{
		StopBeeping();
		StopRunLoop();
	}

	public void PlayExplode()
	{
		PlayOneShot(explodeClip);
	}

	public void StartBeeping(float intervalOverride = -1f)
	{
		if (beepClip == null) return;

		float interval = intervalOverride > 0f
			? intervalOverride
			: (beepInterval > 0f ? beepInterval : Mathf.Max(0.1f, beepClip.length));
		interval /= Mathf.Max(0.01f, beepSpeedMultiplier); // make beeps faster

		if (beepCoroutine != null) StopCoroutine(beepCoroutine);
		beepCoroutine = StartCoroutine(BeepRoutine(interval));
	}

	public void StopBeeping()
	{
		if (beepCoroutine == null) return;
		StopCoroutine(beepCoroutine);
		beepCoroutine = null;
	}

	public void StartRunLoop()
	{
		if (runLoopClip == null || loopSource == null) return;
		if (loopSource.isPlaying) return;
		loopSource.clip = runLoopClip;
		loopSource.pitch = Mathf.Max(0.01f, runLoopPitchMultiplier);
		loopSource.volume = sfxVolume;
		loopSource.Play();
	}

	public void StopRunLoop()
	{
		if (loopSource == null) return;
		if (loopSource.isPlaying) loopSource.Stop();
	}

	public void SetSfxVolume(float value01)
	{
		sfxVolume = Mathf.Clamp01(value01);
		if (loopSource.isPlaying) loopSource.volume = sfxVolume;
	}

	// Helpers
	private void PlayOneShot(AudioClip clip)
	{
		if (clip == null || sfxSource == null) return;
		float oldPitch = sfxSource.pitch;
		sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
		sfxSource.PlayOneShot(clip, sfxVolume);
		sfxSource.pitch = oldPitch;
	}

	private IEnumerator BeepRoutine(float interval)
	{
		while (true)
		{
			PlayOneShot(beepClip);
			yield return new WaitForSeconds(interval);
		}
	}
}
