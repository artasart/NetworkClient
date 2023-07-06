using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MEC;
using System.Collections.Generic;

public class GameLogicManager : MonoBehaviour
{
	#region Singleton

	public static GameLogicManager Instance
	{
		get
		{
			if (instance != null) return instance;
			instance = FindObjectOfType<GameLogicManager>();
			return instance;
		}
	}
	private static GameLogicManager instance;

	#endregion




	#region Members

	[SerializeField] float[] weights;

	public Sprite[] sprites;
	
	private int[,] array;
	private int[,] origin;

	int jackpotMultiplier = 16;
	public int playCount = 0;
	public int jackpotCount = 0;

	int[] current;
	int[] prev;
	int[] next;

	int winIndex = 0;
	int prevIndex = 0;
	int nextIndex = 0;

	Dictionary<int, int> wildCards = new Dictionary<int, int>();
	List<int> columnIndexes = new List<int>();

	public int gold { set; get; }
	public int credit { set; get; }

	CoroutineHandle handle_spin;
	CoroutineHandle handle_display;

	#endregion




	#region Initialize

	private void Awake()
	{
		array = new int[Define.ROW, Define.PICTURECOUNT];
		current = new int[Define.ROW];
		prev = new int[Define.ROW];
		next = new int[Define.ROW];

		sprites = new Sprite[Define.PICTURECOUNT + Define.BONUSCOUNT];

		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i] = Resources.Load<Sprite>("Sprite/" + (i + 1));
		}
	}

	private void Start()
	{
		weights = GenerateWeights(Define.PICTURECOUNT + Define.BONUSCOUNT);

		for (int j = 0; j < Define.ROW; j++)
		{
			var name = "group_Row_" + (j + 1);
			var panel = GameManager.UI.GetPanel<Panel_Casino>();
			var group_Slots = panel.transform.Search(name);

			for (int i = 0; i < 3; i++)
			{
				panel.reels[j, i] = group_Slots.GetChild(i).GetComponent<Image>();
			}
		}

		RefreshReels();
	}

	#endregion




	#region Core Methods

	public void Play()
	{
		if (credit <= 0)
		{
			DebugManager.ClearLog("You need more 'CREDIT'. Go for exchange.");

			return;
		}

		SetCredit(credit - 1);

		InitReels();

		RunReels();

		GameManager.UI.GetPanel<Panel_Casino>().SpinUI(false);
	}

	private void InitReels()
	{
		Timing.KillCoroutines(handle_display);
		Timing.KillCoroutines(Define.FLICK);

		GameManager.UI.GetPanel<Panel_Casino>().InitReels();
	}

	private void RunReels()
	{
		for (int i = 0; i < Define.ROW; i++)
		{
			for (int j = 0; j < Define.PICTURECOUNT; j++)
			{
				array[i, j] = GenerateRandomNumberWithWeights(weights);
			}
		}

		handle_spin = Timing.RunCoroutine(Co_RunReels());
	}

	private IEnumerator<float> Co_RunReels()
	{
		float elapsedTime = 0;

		while (elapsedTime <= .05f)
		{
			elapsedTime += Time.deltaTime;

			RefreshReels();

			yield return Timing.WaitForSeconds(.01f);
		}

		Calculate();

		Display();
	}

	private void RefreshReels()
	{
		for (int i = 0; i < Define.ROW; i++)
		{
			for (int j = 0; j < Define.COLUMN; j++)
			{
				array[i, j] = GenerateRandomNumberWithWeights(weights);

				GameManager.UI.GetPanel<Panel_Casino>().reels[i, j].sprite = sprites[array[i, j] - 1];
			}
		}
	}

	private void Calculate()
	{
		GetWinIndex();

		CheckScatter();

		CheckRowReward();

		CheckColumnReward();

		CheckJackPot();
	}

	private void Display()
	{
		GameManager.UI.GetPanel<Panel_Casino>().KillWin();

		var panel = GameManager.UI.GetPanel<Panel_Casino>();

		for (int i = 0; i < Define.ROW; i++)
		{
			panel.reels[i, 0].sprite = sprites[origin[i, prevIndex] - 1];
			panel.reels[i, 1].sprite = sprites[origin[i, winIndex] - 1];
			panel.reels[i, 2].sprite = sprites[origin[i, nextIndex] - 1];
		}
				
		handle_display = Timing.RunCoroutine(Co_Display(), "Display");
	}

	private IEnumerator<float> Co_Display()
	{
		yield return Timing.WaitForSeconds(.75f);

		GameManager.UI.GetPanel<Panel_Casino>().SpinUI(true);

		var panel = GameManager.UI.GetPanel<Panel_Casino>();

		if (wildCards.Count <= 0)
		{
			panel.FlickSlots(columnIndexes);
			panel.CheckWin(columnIndexes);

			yield break;
		}

		foreach (var item in wildCards)
		{
			var index = item.Key;
			var spriteIndex = item.Value - 1;

			for (int i = 0; i < Define.COLUMN; i++)
			{
				panel.reels[index, i].sprite = sprites[spriteIndex];
			}

			panel.FlickSlots(index);
		}

		panel.FlickSlots(columnIndexes);
		panel.CheckWin(columnIndexes);
	}

	#endregion




	#region Basic Methods

	private void CheckScatter()
	{
		int symbolCount = 0;

		for (int i = 0; i < Define.ROW; i++)
		{
			if (array[i, winIndex] == Define.SCATTER || array[i, prevIndex] == Define.SCATTER || array[i, nextIndex] == Define.SCATTER)
			{
				symbolCount++;
			}
		}

		if (symbolCount == 2)
		{
			SetGold(gold + Define.PAYMENT);
		}

		else if (symbolCount >= 3)
		{
			credit += 1;
		}
	}

	private void CheckRowReward()
	{
		wildCards.Clear();
		columnIndexes.Clear();

		origin = array;

		for (int i = 0; i < Define.ROW; i++)
		{
			current[i] = array[i, winIndex];
			prev[i] = array[i, prevIndex];
			next[i] = array[i, nextIndex];

			if (current[i] == prev[i] && current[i] == next[i])
			{
				SetGold(gold + (current[i] * 3 * Define.UNIT));
			}

			else
			{
				List<int> numbers = new List<int>();

				numbers.Add(current[i]);
				numbers.Add(prev[i]);
				numbers.Add(next[i]);

				var value = GetNumber(numbers);

				if (value != 0)
				{
					current[i] = prev[i] = next[i] = value;

					wildCards.Add(i, value);

					SetGold(gold + (value * 3 * Define.UNIT));
				}

				numbers.Clear();
			}
		}
	}

	private void CheckColumnReward()
	{
		var numbers = new int[Define.ROW];

		for (int i = 0; i < Define.ROW; i++)
		{
			numbers[i] = array[i, winIndex];
		}

		SetGold(gold + GetReward(numbers) * Define.UNIT);
	}

	private void CheckJackPot()
	{
		if (IsJackPot(winIndex))
		{
			SetGold(gold + (array[0, winIndex] * Define.UNIT * jackpotMultiplier));

			DebugManager.ClearLog($"JackPot..! You just won {(array[0, winIndex] * Define.UNIT * jackpotMultiplier)} Golds.");
		}
	}



	public void SetGold(int _gold)
	{
		GameManager.UI.GetPanel<Panel_Casino>().SetGoldUI(gold, _gold);

		gold = _gold;
	}

	public void SetCredit(int _credit)
	{
		GameManager.UI.GetPanel<Panel_Casino>().SetCreditUI(credit, _credit);

		credit = _credit;
	}



	private void GetWinIndex()
	{
		winIndex = Random.Range(0, Define.PICTURECOUNT);
		prevIndex = (winIndex - 1 + Define.PICTURECOUNT) % Define.PICTURECOUNT;
		nextIndex = (winIndex + 1) % Define.PICTURECOUNT;
	}

	public int GetNumber(List<int> _numbers)
	{
		int wildCount = 0;
		int first = -1;

		for (int i = 0; i < _numbers.Count; i++)
		{
			int number = _numbers[i];

			if (number == Define.SCATTER) return 0;

			if (number >= 1 && number <= Define.PICTURECOUNT)
			{
				if (first == -1)
				{
					first = number;
				}

				else if (first != number)
				{	
					return 0;
				}
			}

			else if (number == Define.WILD)
			{
				wildCount++;
			}
		}

		if (wildCount == 2 || (wildCount == 1 && first != -1))
		{
			return first;
		}

		else if (wildCount == 3)
		{
			return Random.Range(1, 11);
		}

		return 0;
	}

	public int GetReward(int[] _numbers)
	{
		var numberCount = _numbers.Select((x, index) => new { Number = x, Index = index })
			.Where(item => item.Number >= 1 && item.Number <= Define.PICTURECOUNT)
			.GroupBy(item => item.Number)
			.ToDictionary(g => g.Key, g => g.Select(item => item.Index).ToList()
		);

		int result = numberCount.Where(item => item.Value.Count > 1).Sum(item =>
		{
			columnIndexes = item.Value;
						
			return item.Key * item.Value.Count;
		});

		return result;
	}



	public bool IsJackPot(int index)
	{
		int valueToCompare = array[0, index];

		for (int i = 1; i < array.GetLength(0); i++)
		{
			if (array[i, index] != valueToCompare)
			{
				return false;
			}
		}

		return true;
	}

	private float[] GenerateWeights(int length)
	{
		float[] weights = new float[length];

		for (int i = 0; i < length; i++)
		{
			float t = i / (float)(length - 1);
			float value = 0.75f - Mathf.Sin(t * Mathf.PI / 2f) * .25f;
			weights[i] = value;
		}

		return weights;
	}

	private int GenerateRandomNumberWithWeights(float[] weights)
	{
		float totalWeight = weights.Sum();
		float randomNumber = Random.Range(0f, totalWeight);
		float cumulativeWeight = 0f;

		for (int i = 0; i < Define.PICTURECOUNT + Define.BONUSCOUNT; i++)
		{
			cumulativeWeight += weights[i];
			if (randomNumber <= cumulativeWeight)
			{
				return i + 1;
			}
		}

		return 1;
	}



	public void Exchange(int _gold)
	{
		int amount = _gold / Define.PAYMENT;

		if (amount == 0)
		{
			Debug.Log("Not enough gold.");

			return;
		}

		SetGold(gold - _gold);
		SetCredit(credit + amount);

		Debug.Log($"Purchased, Total : {credit}");
	}

	public void ExchangeAll()
	{
		int amount = gold / Define.PAYMENT;

		if (amount == 0)
		{
			Debug.Log("Not enough gold.");

			return;
		}

		SetGold(gold - (amount * Define.PAYMENT));
		SetCredit(credit + amount);

		Debug.Log($"Purchased, Total : {credit}");
	}

	#endregion




	#region Test Methods

	CoroutineHandle handl_JackPot;

	public void KillJackPot()
	{
		Timing.KillCoroutines(handl_JackPot);
	}

	public void PlayUntilJackpot() => handl_JackPot = Timing.RunCoroutine(Co_PlayUntilJackpot());

	IEnumerator<float> Co_PlayUntilJackpot()
	{
		DebugManager.ClearLog();

		while (true)
		{
			if (credit <= 0)
			{
				DebugManager.ClearLog("Wasted.");

				yield break;
			}

			Play();

			playCount++;

			yield return Timing.WaitUntilDone(handle_display);
		}
	}

	#endregion
}