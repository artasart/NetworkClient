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

	Sprite[] sprites;
	
	private int[,] array;

	public int credit = 100000;
	int jackpotMultiplier = 16;
	public int playCount = 0;
	public int jackpotCount = 0;

	bool isJackpot = false;

	int[] current;
	int[] prev;
	int[] next;

	CoroutineHandle handle_spin;

	#endregion



	#region Initialize

	private void Awake()
	{
		array = new int[Define.ROW, Define.PICTURECOUNT];
		current = new int[Define.ROW];
		prev = new int[Define.ROW];
		next = new int[Define.ROW];


		sprites = new Sprite[Define.PICTURECOUNT];

		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i] = Resources.Load<Sprite>("Sprite/" + (i + 1));
		}
	}

	private void Start()
	{
		//GameManager.UI.StackPanel<Panel_HUD>();

		weights = GenerateWeights(Define.PICTURECOUNT);

		for (int j = 0; j < Define.ROW; j++)
		{
			var name = "group_Row_" + (j + 1);
			var panel = GameManager.UI.GetPanel<Panel_Casino>();
			var group_Slots = panel.transform.Search(name);

			for (int i = 0; i < 3; i++)
			{
				panel.slots[j, i] = group_Slots.GetChild(i).GetComponent<Image>();
			}
		}

		RefreshSlots();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			PlayUntilJackpot();
		}
	}

	#endregion



	#region Core Methods

	public void Play()
	{
		InitGame();

		SetCredit(credit - Define.PAYMENT);

		for (int i = 0; i < Define.ROW; i++)
		{
			for (int j = 0; j < Define.PICTURECOUNT; j++)
			{
				array[i, j] = GenerateRandomNumberWithWeights(weights);
			}
		}

		handle_spin = Timing.RunCoroutine(Co_Play());	
	}

	IEnumerator<float> Co_Play()
	{
		float elapsedTime = 0;
		
		while(elapsedTime <= .05f)
		{
			elapsedTime += Time.deltaTime;

			RefreshSlots();

			yield return Timing.WaitForSeconds(.01f);
		}

		Calculate();

		ShowResult();
	}

	public void RefreshSlots()
	{
		for (int i = 0; i < Define.ROW; i++)
		{
			for (int j = 0; j < Define.COLUMN; j++)
			{
				array[i, j] = GenerateRandomNumberWithWeights(weights);

				GameManager.UI.GetPanel<Panel_Casino>().slots[i, j].sprite = sprites[array[i, j] - 1];
			}
		}
	}

	private void Calculate()
	{
		var winIndex = Random.Range(0, Define.PICTURECOUNT);

		CheckRowReward(winIndex);

		CheckColumnReward(winIndex);

		CheckJackPot(winIndex);
	}

	private void ShowResult()
	{
		var panel = GameManager.UI.GetPanel<Panel_Casino>();

		for (int i = 0; i < Define.ROW; i++)
		{
			panel.slots[i, 0].sprite = sprites[prev[i] - 1];
			panel.slots[i, 1].sprite = sprites[current[i] - 1];
			panel.slots[i, 2].sprite = sprites[next[i] - 1];
		}
	}

	#endregion



	#region Basic Methods

	private void CheckRowReward(int _winIndex)
	{
		int prevIndex = (_winIndex - 1 + Define.PICTURECOUNT) % Define.PICTURECOUNT;
		int nextIndex = (_winIndex + 1) % Define.PICTURECOUNT;

		for (int i = 0; i < Define.ROW; i++)
		{
			current[i] = array[i, _winIndex];
			prev[i] = array[i, prevIndex];
			next[i] = array[i, nextIndex];

			if (current[i] == prev[i] && current[i] == next[i])
			{
				Debug.Log($"{i + 1}열에 조건을 만족하고 있습니다. 당첨 숫자 {current[i]}");

				Debug.Log($"리워드 추가 : {current[i] * 3 * Define.UNIT}");

				SetCredit(credit + (current[i] * 3 * Define.UNIT));

				GameManager.UI.GetPanel<Panel_Casino>().FlickSlots(i);
			}
		}
	}

	private void CheckColumnReward(int _winIndex)
	{
		var numbers = new int[Define.ROW];

		for (int i = 0; i < Define.ROW; i++)
		{
			numbers[i] = array[i, _winIndex];
		}

		SetCredit(credit + Reward(numbers) * Define.UNIT);
	}

	public static int Reward(int[] _numbers)
	{
		var numberCount = _numbers.Select((x, index) => new { Number = x, Index = index })
			.GroupBy(item => item.Number)
			.ToDictionary(g => g.Key, g => g.Select(item => item.Index).ToList()
		);

		int result = numberCount.Where(kvp => kvp.Value.Count > 1).Sum(kvp =>
		{
			GameManager.UI.GetPanel<Panel_Casino>().FlickSlots(kvp.Value);
			return kvp.Key * kvp.Value.Count;
		});

		return result;
	}

	private void CheckJackPot(int _winIndex)
	{
		if (IsJackPot(_winIndex))
		{
			SetCredit(credit + (array[0, _winIndex] * Define.UNIT * jackpotMultiplier));

			isJackpot = true;

			DebugManager.ClearLog($"JackPot..! You just won {(array[0, _winIndex] * Define.UNIT * jackpotMultiplier)} credits.");
		}
	}


	public void InitGame()
	{
		Timing.KillCoroutines(Define.FLICK);

		GameManager.UI.GetPanel<Panel_Casino>().InitSlots();
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

	public void SetCredit(int _credit)
	{
		GameManager.UI.GetPanel<Panel_Casino>().SetCreditUI(credit, _credit);

		credit = _credit;
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

		for (int i = 0; i < Define.PICTURECOUNT; i++)
		{
			cumulativeWeight += weights[i];
			if (randomNumber <= cumulativeWeight)
			{
				return i + 1;
			}
		}

		return 1;
	}

	#endregion



	#region Test Methods

	private void PlayUntilJackpot() => Timing.RunCoroutine(Co_PlayUntilJackpot());

	IEnumerator<float> Co_PlayUntilJackpot()
	{
		DebugManager.ClearLog();

		if (credit <= 0) SetCredit(1000000);

		isJackpot = false;

		while (!isJackpot)
		{
			if (credit - 1000 < 0)
			{
				Debug.Log("Wasted.");

				InitGame();

				yield break;
			}

			Play();

			playCount++;

			yield return Timing.WaitUntilDone(handle_spin);
		}
	}

	#endregion
}