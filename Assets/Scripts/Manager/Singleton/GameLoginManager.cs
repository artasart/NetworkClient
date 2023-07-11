using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase;
using MEC;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GameLoginManager : SingletonManager<GameLoginManager>
{
	#region Members

	FirebaseAuth firebaseAuth;
	FirebaseUser firebaseUser;

	public string email;
	public string username;
	public string password;

	bool isRecieved = false;
	bool isResult = false;

	public List<Selectable> selectables = new List<Selectable>();

	#endregion



	#region Initialize

	private void OnDestroy()
	{
		Logout();
	}

	private void Start()
	{
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
		{
			if (task.Exception != null)
			{
				Debug.LogError($"Firebase initialization failed: {task.Exception}");
			}
			else
			{
				firebaseAuth = FirebaseAuth.DefaultInstance;
			}
		});
	}


#if UNITY_EDITOR
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (EventSystem.current.currentSelectedGameObject == null) return;

			Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
			Selectable next = GetNextSelectable(current);

			if (next != null)
			{
				next.Select();
			}
		}
	}

	private Selectable GetNextSelectable(Selectable _current)
	{
		if (_current == null)
		{
			if (selectables.Count > 0)
			{
				return selectables[0];
			}
			else
			{
				return null;
			}
		}

		int currentIndex = -1;
		for (int i = 0; i < selectables.Count; i++)
		{
			if (selectables[i] == _current)
			{
				currentIndex = i;
				break;
			}
		}

		if (currentIndex != -1)
		{
			int nextIndex = (currentIndex + 1) % selectables.Count;
			return selectables[nextIndex];
		}

		return null;
	}
#endif

	#endregion




	public void Login(string _userName, string _password)
	{
		DebugManager.ClearLog($"TRY LOGIN. EMAIL : {_userName} + PASSWORD : {_password}");

		GameManager.Scene.Dim(true);

		isRecieved = false;

		firebaseAuth.SignInWithEmailAndPasswordAsync(_userName, _password).ContinueWith(task =>
		{
			if (task.IsCanceled)
			{
				Debug.Log("LOGIN CANCELED.");
			}

			else if (task.IsFaulted)
			{
				Debug.Log("LOGIN FAILED.");
			}

			else
			{
				FirebaseUser newUser = task.Result.User;

				Debug.Log("LOGIN SUCCESS.");

				username = _userName;
				password = _password;

				isResult = true;
			}

			isRecieved = true;
		});

		Timing.RunCoroutine(Co_Request());
	}

	public void SignUp(string _userName, string _password)
	{
		DebugManager.ClearLog($"TRY SIGNUP. EMAIL : {_userName} + PASSWORD : {_password}");

		GameManager.Scene.Dim(true);

		isRecieved = false;

		firebaseAuth.CreateUserWithEmailAndPasswordAsync(_userName, _password).ContinueWith(task =>
		{
			if (task.IsCanceled)
			{
				Debug.Log("SIGN UP CANCELED.");
			}

			else if (task.IsFaulted)
			{
				Debug.Log("SIGN UP FAILED.");

				if (task.Exception != null)
				{
					foreach (Exception innerException in task.Exception.InnerExceptions)
					{
						if (innerException is FirebaseException firebaseException)
						{

						}

						else
						{
							Debug.Log("ERROR : " + innerException.ToString());

							string errorMessage = innerException.Message;

							if (errorMessage.Contains("Email already exists"))
							{
								Debug.Log("Email already exists error.");
							}
							else if (errorMessage.Contains("Invalid email"))
							{
								Debug.Log("Invalid email address.");
							}
							else if (errorMessage.Contains("Weak password"))
							{
								Debug.Log("Weak password.");
							}
							else if (errorMessage.Contains("Wrong password"))
							{
								Debug.Log("Wrong password.");
							}
							else if (errorMessage.Contains("User not found"))
							{
								Debug.Log("User not found.");
							}
							else if (errorMessage.Contains("Missing email"))
							{
								Debug.Log("Please enter an email address.");
							}
							else if (errorMessage.Contains("Missing password"))
							{
								Debug.Log("Please enter a password.");
							}
							else if (errorMessage.Contains("Too many requests"))
							{
								Debug.Log("Too many requests. Please try again later.");
							}
						}
					}
				}
			}

			else
			{
				FirebaseUser newUser = task.Result.User;

				Debug.Log("SIGN UP SUCCESS.");
			}

			isRecieved = true;
		});

		Timing.RunCoroutine(Co_Request());
	}

	IEnumerator<float> Co_Request()
	{
		yield return Timing.WaitUntilTrue(() => isRecieved);

		GameManager.Scene.Dim(false);

		if (isResult)
		{
			yield return Timing.WaitForSeconds(1f);

			GameManager.Scene.LoadScene(SceneName.Main);

			isResult = false;
		}
	}

	public void Logout()
	{
		firebaseAuth.SignOut();

		DebugManager.ClearLog("LOGOUT.");
	}
}
