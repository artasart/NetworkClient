using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab_Setting : Tab_Base
{
	Slider slider_BGM;
	Slider slider_SoundEffect;

	protected override void Awake()
	{
		base.Awake();

		slider_BGM = GetUI_Slider(nameof(slider_BGM), OnSliderValueChanged_BGM);
		slider_SoundEffect = GetUI_Slider(nameof(slider_SoundEffect), OnSliderValueChanged_SoundEffect);
	}

	private void Start()
	{
		slider_BGM.value = GameManager.Sound.bgmVolume;
		slider_SoundEffect.value = GameManager.Sound.effectVolume;
	}

	private void OnSliderValueChanged_BGM(float _value)
	{
		Debug.Log("BGM value is changed : " + _value);

		GameManager.Sound.bgmVolume = _value;
		GameManager.Sound.bgm.volume = _value;
	}

	private void OnSliderValueChanged_SoundEffect(float _value)
	{
		Debug.Log("SoundEffect value is changed : " + _value);

		GameManager.Sound.effectVolume = _value;
		GameManager.Sound.soundEffect.volume = _value;
	}
}
