using MEC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Casino : Panel_Base
{
	Button btn_Close;
	Button btn_Spin;

	TMP_Text txtmp_Credit;

	public Image[,] slots;

	List<int> prize = new List<int>();

	private void OnEnable()
	{
		txtmp_Credit.text = GameLogicManager.Instance.credit.ToString();
	}

	protected override void Awake()
	{
		base.Awake();

		btn_Close = GetUI_Button(nameof(btn_Close), () => GameManager.UI.PopPanel());
		btn_Spin = GetUI_Button(nameof(btn_Spin), () => GameLogicManager.Instance.Play());

		txtmp_Credit = GetUI_TMPText(nameof(txtmp_Credit), string.Empty);

		slots = new Image[Define.ROW, Define.COLUMN];
	}

	public void InitSlots()
	{
		for (int i = 0; i < Define.ROW; i++)
		{
			for (int j = 0; j < Define.COLUMN; j++)
			{
				Timing.KillCoroutines(slots[i, j].GetHashCode());

				slots[i, j].GetComponent<Image>().color = Color.white;
			}
		}

		prize.Clear();
	}

	public void FlickSlots(List<int> _row)
	{
		foreach(var item in _row)
		{
			if (prize.Contains(slots[item, 1].GetHashCode())) continue;

			prize.Add(slots[item, 1].GetHashCode());

			Timing.RunCoroutine(Util.Co_Flik(slots[item, 1], 3, 2f), slots[item, 1].GetHashCode());
		}
	}

	public void FlickSlots(int _index)
	{
		for (int i = 0; i < Define.COLUMN; i++)
		{
			Timing.RunCoroutine(Util.Co_Flik(slots[_index, i], 3, 2f), slots[_index, i].GetHashCode());
		}
	}

	public void SetCreditUI(int _start, int _end, float _duration = .25f) => Timing.RunCoroutine(Co_SetCreditUI(_start, _end, _duration));

	private IEnumerator<float> Co_SetCreditUI(int _start, int _end, float _duration = .25f)
	{
		yield return Timing.WaitForOneFrame;

		float elapsedTime = 0f;

		while (elapsedTime < _duration)
		{
			float time = Mathf.SmoothStep(0f, 1f, elapsedTime / _duration);
			int value = Mathf.RoundToInt(Mathf.Lerp(_start, _end, time));

			txtmp_Credit.text = value.ToString();

			elapsedTime += Time.deltaTime;

			yield return Timing.WaitForOneFrame;
		}

		txtmp_Credit.text = _end.ToString();
	}
}
