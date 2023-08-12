using FrameWork.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkAnimator))]
public class NetworkObserver : NetworkComponent
{
    [SerializeField] private List<NetworkComponent> networkComponents = new();

    public void SetNetworkObject( Client client, int id, bool isMine, bool isPlayer, string ownerId )
    {
        Client = client;
        this.id = id;
        this.isMine = isMine;
        this.isPlayer = isPlayer;

        if (!isMine)
        {
            Destroy(GetComponent<PlayerController>());
            Destroy(GetComponentInChildren<CameraShake>().gameObject);
        }

        if (isPlayer)
        {
            transform.GetComponentInChildren<TMP_Text>().text = ownerId;
        }

        networkComponents.Add(GetComponent<NetworkTransform>());
        networkComponents.Add(GetComponent<NetworkAnimator>());

        foreach (NetworkComponent item in networkComponents)
        {
            item.Client = Client;
            item.id = id;
            item.isMine = isMine;
            item.isPlayer = isPlayer;
        }
    }
}
