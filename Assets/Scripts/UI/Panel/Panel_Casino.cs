using MEC;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Casino : Panel_Base
{
	Transform group_Win;

	Button btn_Close;
	Button btn_Spin;
	Button btn_Exchange;

	Button btn_Menu_1;
	Button btn_Menu_2;
	Button btn_Menu_3;

	Button btn_Waste;
	Button btn_Stop;

	TMP_Text txtmp_Gold;
	TMP_Text txtmp_Credit;

	public Image[,] reels;

	List<int> prize = new List<int>();

	private void OnEnable()
	{
		txtmp_Gold.text = GameLogicManager.Instance.gold.ToString();
		txtmp_Credit.text = GameLogicManager.Instance.credit.ToString();

		SpinUI(GameLogicManager.Instance.credit > 0);
	}

	protected override void Awake()
	{
		base.Awake();

		btn_Close = GetUI_Button(nameof(btn_Close),
			() => GameManager.UI.PopPanel(),
			() => GameManager.Sound.PlaySound("Click_1")
		);

		btn_Spin = GetUI_Button(nameof(btn_Spin),
			() => GameLogicManager.Instance.Play(),
			() => GameManager.Sound.PlaySound("Reel")
		);

		btn_Menu_1 = GetUI_Button(nameof(btn_Menu_1),
			() => GameLogicManager.Instance.SetGold(GameLogicManager.Instance.gold + 10000),
			() => GameManager.Sound.PlaySound("Register")
		);

		btn_Menu_2 = GetUI_Button(nameof(btn_Menu_2),
			() => GameLogicManager.Instance.SetGold(GameLogicManager.Instance.gold + 100000),
			() => GameManager.Sound.PlaySound("Register")
		);

		btn_Menu_3 = GetUI_Button(nameof(btn_Menu_3),
			() => GameLogicManager.Instance.SetGold(GameLogicManager.Instance.gold + 100000000),
			() => GameManager.Sound.PlaySound("Register")
		);

		btn_Waste = GetUI_Button(nameof(btn_Waste), () =>
		{
			GameLogicManager.Instance.PlayUntilJackpot();
			btn_Waste.interactable = false;
		});

		btn_Stop = GetUI_Button(nameof(btn_Stop), () =>
		{
			GameLogicManager.Instance.KillJackPot();
			btn_Waste.interactable = true;
		});

		btn_Exchange = GetUI_Button(nameof(btn_Exchange), () => GameLogicManager.Instance.ExchangeAll());
		btn_Exchange.onClick.RemoveListener(PlaySound);

		txtmp_Gold = GetUI_TMPText(nameof(txtmp_Gold), string.Empty);
		txtmp_Credit = GetUI_TMPText(nameof(txtmp_Credit), string.Empty);

		reels = new Image[Define.ROW, Define.COLUMN];

		group_Win = this.transform.Search(nameof(group_Win));

		for (int i = 0; i < group_Win.childCount; i++)
		{
			group_Win.GetChild(i).gameObject.SetActive(false);
		}

		group_Win.gameObject.SetActive(false);
	}

	public void InitReels()
	{
		for (int i = 0; i < Define.ROW; i++)
		{
			for (int j = 0; j < Define.COLUMN; j++)
			{
				Timing.KillCoroutines(reels[i, j].GetHashCode());

				reels[i, j].GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.07f);
			}
		}

		prize.Clear();
	}

	public void FlickSlots(List<int> _row)
	{
		foreach (var item in _row)
		{
			if (prize.Contains(reels[item, 1].GetHashCode())) continue;

			prize.Add(reels[item, 1].GetHashCode());

			Timing.RunCoroutine(Util.Co_Flik(reels[item, 1], 3, 2f), reels[item, 1].GetHashCode());
		}
	}

	public void FlickSlots(int _index)
	{
		for (int i = 0; i < Define.COLUMN; i++)
		{
			Timing.RunCoroutine(Util.Co_Flik(reels[_index, i], 3, 2f), reels[_index, i].GetHashCode());
		}
	}

	public void CheckWin(List<int> _numbers)
	{
		var numbers = _numbers.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

		if (numbers.Count <= 0) return;

		int maxCount = numbers.Values.Max();

		switch (numbers.Count)
		{
			case 3:
				Win(WinPrice.Big);
				break;
			case 4:
				Win(WinPrice.Mega);
				break;
			case 5:
				Win(WinPrice.JackPot);
				break;
		}
	}


	public void SetGoldUI(int _start, int _end, float _duration = .25f) => Timing.RunCoroutine(Co_AnimateText(txtmp_Gold, _start, _end, _duration));

	public void SetCreditUI(int _start, int _end, float _duration = .25f)
	{
		Timing.RunCoroutine(Co_AnimateText(txtmp_Credit, _start, _end, _duration));
	}

	private IEnumerator<float> Co_AnimateText(TMP_Text _txtmp, int _start, int _end, float _duration = .25f)
	{
		yield return Timing.WaitForOneFrame;

		float elapsedTime = 0f;

		while (elapsedTime < _duration)
		{
			float time = Mathf.SmoothStep(0f, 1f, elapsedTime / _duration);
			int value = Mathf.RoundToInt(Mathf.Lerp(_start, _end, time));

			_txtmp.text = value.ToString();

			elapsedTime += Time.deltaTime;

			yield return Timing.WaitForOneFrame;
		}

		_txtmp.text = _end.ToString();
	}



	public void Win(WinPrice _winPrice)
	{
		group_Win.gameObject.SetActive(true);

		Timing.RunCoroutine(Co_Win(_winPrice), "WinPrice");
	}

	private IEnumerator<float> Co_Win(WinPrice _winPrice)
	{
		Transform winPrice = null;

		switch (_winPrice)
		{
			case WinPrice.Big:
				winPrice = group_Win.Search("group_BigWin");
				break;
			case WinPrice.Mega:
				winPrice = group_Win.Search("group_MegaWin");
				break;
			case WinPrice.JackPot:
				winPrice = group_Win.Search("group_JackPot");
				break;
		}

		GameManager.UI.Show(winPrice.gameObject, true);

		yield return Timing.WaitForSeconds(1.5f);

		GameManager.UI.Show(winPrice.gameObject, false);

		yield return Timing.WaitUntilTrue(() => winPrice.GetComponent<CanvasGroup>().alpha <= 0f);

		group_Win.gameObject.SetActive(false);
	}

	public void KillWin()
	{
		Timing.KillCoroutines("WinPrice");

		for (int i = 0; i < group_Win.childCount; i++)
		{
			GameManager.UI.Show(group_Win.GetChild(i).gameObject, false);
		}

		group_Win.gameObject.SetActive(false);
	}


	public void SpinUI(bool _interactable)
	{
		btn_Spin.GetComponent<CanvasGroup>().alpha = _interactable ? 1f : .75f;
		btn_Spin.GetComponent<CanvasGroup>().blocksRaycasts = _interactable;
	}
}

public enum WinPrice
{
	Big,
	Mega,
	JackPot,
}
