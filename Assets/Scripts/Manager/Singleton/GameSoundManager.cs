using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSoundManager : SingletonManager<GameSoundManager>
{
	GameSound gameSound;

	AudioSource bgm;
	AudioSource soundEffect;

	float pausedTime = 0;

	CoroutineHandle handle_bgm;

	private void Awake()
	{
		gameSound = Resources.Load<GameSound>(Define.PATH_SOUND + "GameSound");

		bgm = gameObject.AddComponent<AudioSource>();
		soundEffect = gameObject.AddComponent<AudioSource>();

		bgm.volume = 0;
	}

	public void PlaySoundEffect(string _filename, float _volume = 1f)
	{
		AudioClip clip = GetSoundEffect(_filename);

		if (clip != null)
		{
			soundEffect.PlayOneShot(clip, _volume);
		}

		else { DebugManager.Log("There is no audio clip."); }
	}

	public void PlayBGM(string _filename, float _volume = 1f)
	{
		AudioClip clip = GetBGM(_filename);

		if (clip != null)
		{
			if (pausedTime == 0)
			{
				pausedTime = bgm.time;
			}

			bgm.clip = clip;
			bgm.time = pausedTime;
			bgm.Play();

			Timing.KillCoroutines(handle_bgm);

			handle_bgm = Timing.RunCoroutine(Co_SetVolume(bgm, _volume), Define.BGM);
		}

		else { DebugManager.Log("There is no audio clip."); }
	}

	public void StopBGM()
	{
		if (bgm.clip == null) return;

		Timing.KillCoroutines(handle_bgm);

		handle_bgm = Timing.RunCoroutine(Co_SetVolume(bgm, 0f, bgm.Stop), Define.BGM);
	}

	public void SwitchBGM(string _filename, float _volume = 1f)
	{
		AudioClip clip = GetBGM(_filename);

		if (clip != null)
		{
			Timing.KillCoroutines(handle_bgm);

			handle_bgm = Timing.RunCoroutine(Co_SetVolume(bgm, 0f, () => PlayBGM(_filename, _volume)), Define.BGM);
		}

		else { DebugManager.Log("There is no audio clip."); }
	}

	public void CrossDissolveBGM(string _filename, float _volume = 1f)
	{
		if (handle_bgm.IsRunning) return;

		AudioClip clip = GetBGM(_filename);

		if (clip != null)
		{
			AudioSource target = gameObject.AddComponent<AudioSource>();
			target.clip = clip;
			target.volume = 0f;
			target.Play();

			Timing.KillCoroutines(handle_bgm);

			handle_bgm = Timing.RunCoroutine(Co_CrossDissolveBGM(bgm, target, _volume), Define.BGM);
		}
		else
		{
			DebugManager.Log("There is no audio clip.");
		}
	}

	private IEnumerator<float> Co_CrossDissolveBGM(AudioSource _current, AudioSource _target, float _volume)
	{
		float elapsedTime = 0f;
		float lerpSpeed = .5f;

		float elapsedTime2 = 0f;

		float total = _current.volume * .25f;

		while (Mathf.Abs(_target.volume - _volume) >= 0.001f)
		{
			_current.volume = Mathf.Lerp(_current.volume, 0f, elapsedTime += lerpSpeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;

			if (_current.volume <= total)
			{
				_target.volume = Mathf.Lerp(_target.volume, _volume, elapsedTime2 += lerpSpeed * Time.deltaTime);
			}
		}
		_target.volume = _volume;

		_current.Stop();

		Destroy(_current);

		bgm = _target;
	}



	private AudioClip GetSoundEffect(string _filename)
	{
		if (gameSound != null && gameSound.soundEffects != null)
		{
			foreach (var clip in gameSound.soundEffects)
			{
				if (clip.name == _filename)
				{
					return clip;
				}
			}
		}

		return null;
	}

	private AudioClip GetBGM(string _filename)
	{
		if (gameSound != null && gameSound.bgm != null)
		{
			foreach (var clip in gameSound.bgm)
			{
				if (clip.name == _filename)
				{
					return clip;
				}
			}
		}
		return null;
	}




	public void SetBGMVolume(AudioSource _bgm, float _volume)
	{
		Timing.KillCoroutines(handle_bgm);

		handle_bgm = Timing.RunCoroutine(Co_SetVolume(_bgm, _volume), Define.BGM);
	}

	public void SetSoundEffectVolume(float _volume)
	{
		Timing.RunCoroutine(Co_SetVolume(soundEffect, _volume), Define.SOUNDEFFECT);
	}

	IEnumerator<float> Co_SetVolume(AudioSource _audioSource, float _volume, Action _action = null)
	{
		float elapsedTime = 0f;
		float lerpSpeed = .5f;

		while (Mathf.Abs(_audioSource.volume - _volume) >= 0.001f)
		{
			_audioSource.volume = Mathf.Lerp(_audioSource.volume, _volume, elapsedTime += lerpSpeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		_audioSource.volume = _volume;

		_action?.Invoke();
	}
}