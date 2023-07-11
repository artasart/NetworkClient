using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RideController : MonoBehaviour
{
	TouchInteractable touch;

	Vector3 offset = new Vector3(0f, .5f, 0f);

	BoxCollider boxCollider;

	private void Awake()
	{
		touch = GetComponent<TouchInteractable>();
		boxCollider = GetComponent<BoxCollider>();
		touch.AddEvent(Ride);
	}

	public void Ride()
	{
		FindObjectOfType<PlayerController>().transform.LookAt(this.transform);

		FindObjectOfType<PlayerController>().MoveToTarget(this.transform);

		Timing.RunCoroutine(Test());
	}

	IEnumerator<float> Test()
	{
		var playerController = FindObjectOfType<PlayerController>();

		yield return Timing.WaitUntilTrue(() => !playerController.isPathFinding);

		yield return Timing.WaitForSeconds(1f);

		Debug.Log("Ride");

		boxCollider.enabled = false;

		FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, playerController.transform.position, Quaternion.identity);

		playerController.transform.position = this.transform.position + offset;

		playerController.transform.rotation = this.transform.rotation;

		FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, this.transform.position + offset, Quaternion.identity);

		playerController.animator.SetBool("Sit", true);

		this.transform.SetParent(playerController.transform);

		this.transform.localPosition = Vector3.zero;
		this.transform.localRotation = Quaternion.identity;

		playerController.walkSpeed += 20;
		playerController.runSpeed += 40;

		FindObjectOfType<CinemachineTPSController>().ShowCursor(false);
	}

	public void UnRide()
	{
		boxCollider.enabled = true;
		this.transform.SetParent(null);

		var playerController = FindObjectOfType<PlayerController>();

		playerController.walkSpeed -= 20;
		playerController.runSpeed -= 40;

		FindObjectOfType<CinemachineTPSController>().ShowCursor(true);

		Destroy(this.gameObject);
	}
}
