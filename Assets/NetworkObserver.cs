using FrameWork.Network;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkAnimator))]
public class NetworkObserver : NetworkComponent
{
	[SerializeField] List<NetworkComponent> networkComponents = new List<NetworkComponent>();

	protected override void Awake()
	{
		base.Awake();

		if (!isMine)
		{
			Destroy(this.GetComponent<PlayerController>());
			Destroy(this.GetComponentInChildren<CameraShake>().gameObject);
		}

		if (isPlayer)
		{
			this.transform.GetComponentInChildren<TMP_Text>().text = "MarkerMan_" + objectId;
		}

		networkComponents.Add(this.GetComponent<NetworkTransform>());
		networkComponents.Add(this.GetComponent<NetworkAnimator>());

		foreach (var item in networkComponents)
		{
			item.objectId = objectId;
			item.isMine = isMine;
			item.isPlayer = isPlayer;
			item.connection = connection;
		}
	}
}
