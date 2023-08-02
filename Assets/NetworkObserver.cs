using FrameWork.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkAnimator))]
public class NetworkObserver : NetworkComponent
{
    [SerializeField] private List<NetworkComponent> networkComponents = new();

    protected void Awake()
    {
        if (!isMine)
        {
            Destroy(GetComponent<PlayerController>());
            Destroy(GetComponentInChildren<CameraShake>().gameObject);
        }

        networkComponents.Add(GetComponent<NetworkTransform>());
        networkComponents.Add(GetComponent<NetworkAnimator>());

        foreach (NetworkComponent item in networkComponents)
        {
            item.objectId = objectId;
            item.isMine = isMine;
        }
    }

    public void SetConnection( Client client )
    {
        Client = client;

        if (isPlayer)
        {
            transform.GetComponentInChildren<TMP_Text>().text = Client.ClientId;
        }

        foreach (NetworkComponent item in networkComponents)
        {
            item.Client = Client;
        }
    }
}
