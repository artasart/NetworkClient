using Framework.Network;
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

	protected void Awake()
	{		
		if (!isMine)
		{
			Destroy(this.GetComponent<PlayerController>());
			Destroy(this.GetComponentInChildren<CameraShake>().gameObject);
		}

		networkComponents.Add(this.GetComponent<NetworkTransform>());
		networkComponents.Add(this.GetComponent<NetworkAnimator>());

		foreach (var item in networkComponents)
		{
			item.objectId = objectId;
			item.isMine = isMine;
        }
	}

	public void SetConnection(Client client)
	{
		this.Client = client;

        if (isPlayer)
        {
            this.transform.GetComponentInChildren<TMP_Text>().text = this.Client.ClientId;
        }

        foreach (var item in networkComponents)
        {
			item.Client = this.Client;
        }
    }
}
