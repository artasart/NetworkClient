using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using MEC;

public class GameVolumeController : MonoBehaviour
{
	#region Singleton

	public static GameVolumeController Instance
	{
		get
		{
			if (instance != null) return instance;
			instance = FindObjectOfType<GameVolumeController>();
			return instance;
		}
	}
	private static GameVolumeController instance;

	#endregion

	Volume volume;

	Volumes<Vignette> vignette = new Volumes<Vignette>();
	Volumes<Bloom> bloom = new Volumes<Bloom>();

	private void Awake()
	{
		volume = FindObjectOfType<Volume>();
		volume.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();

		vignette.current = vignette.origin = AddVolume<Vignette>();
		bloom.current = bloom.origin = AddVolume<Bloom>();

		SetVignette(.45f, Color.black);
		SetBloom(5f, .9f);
	}

	public T AddVolume<T>(bool _isActivate = true) where T : VolumeComponent
	{
		T component = volume.sharedProfile.Add<T>();
		component.active = _isActivate;

		return component;
	}

	public void RemoveVolume<T>() where T : VolumeComponent
	{
		volume.sharedProfile.Remove<T>();
	}

	public void SetVignette(float _intensity, Color _color)
	{
		vignette.current.intensity.Override(_intensity);
		vignette.current.color.Override(_color);
	}

	public void SetBloom(float _intensity, float _threshold)
	{
		bloom.current.intensity.Override(_intensity);
		bloom.current.threshold.Override(_threshold);
	}

	public void OnDestroy()
	{
		if (vignette.origin) SetVignette(vignette.origin.intensity.value, vignette.origin.color.value);
		if (bloom.current) SetBloom(bloom.origin.intensity.value, bloom.origin.threshold.value);
	}
}

class Volumes<T> where T : VolumeComponent
{
	public T origin;
	public T current;
}
