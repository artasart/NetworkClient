using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Util
{
	public static Transform Search(this Transform _target, string _name)
	{
		if (_target.name == _name) return _target;

		for (int i = 0; i < _target.childCount; ++i)
		{
			var result = Search(_target.GetChild(i), _name);

			if (result != null) return result;
		}

		return null;
	}

	public static T GetComponentInSibilings<T>(Transform _transform) where T : Component
	{
		var parent = _transform.parent;

		return parent.GetComponentInChildren<T>();
	}


	public static IEnumerator<float> Co_Flik<T>(T _image, int _interval, float _lerpSpeed = 1f) where T : MaskableGraphic
	{
		float alpha = _image.color.a;

		for (int i = 0; i < _interval; i++)
		{
			CoroutineHandle handle = Timing.RunCoroutine(Co_FadeColor(_image, .25f, _lerpSpeed), Define.FLICK);

			yield return Timing.WaitUntilDone(handle);

			handle = Timing.RunCoroutine(Co_FadeColor(_image, alpha, _lerpSpeed), Define.FLICK);

			yield return Timing.WaitUntilDone(handle);
		}
	}

	public static IEnumerator<float> Co_FadeColor<T>(T _image, float _alpha, float _lerpSpeed = 1f) where T : MaskableGraphic
	{
		var targetColor = new Color(_image.color.r, _image.color.g, _image.color.b, _alpha);
		var lerpvalue = 0f;

		while (GetColorDistance(_image.color, targetColor) >= 0.001f)
		{
			_image.color = Color.Lerp(_image.color, targetColor, lerpvalue += _lerpSpeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		_image.color = targetColor;
	}

	public static float GetColorDistance(Color _colorA, Color _colorB)
	{
		float r = _colorA.r - _colorB.r;
		float g = _colorA.g - _colorB.g;
		float b = _colorA.b - _colorB.b;
		float a = _colorA.a - _colorB.a;

		return Mathf.Sqrt(r * r + g * g + b * b + a * a);
	}
}
