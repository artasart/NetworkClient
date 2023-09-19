using FrameWork.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkObserver : NetworkComponent
{
    [SerializeField] private List<NetworkComponent> networkComponents = new();

    public void SetNetworkObject( Client client, int id, bool isMine, bool isPlayer, string ownerId )
    {
        Client = client;

        Client.packetHandler.AddHandler(OnOwnerChanged);

        this.id = id;
        this.isMine = isMine;
        this.isPlayer = isPlayer;

        if (isPlayer)
        {
            networkComponents.Add(GetComponent<NetworkTransform_DeadReckoning>());
            //networkComponents.Add(GetComponent<NetworkTransform>());
            networkComponents.Add(GetComponent<NetworkAnimator>());

            transform.GetComponentInChildren<TMP_Text>().text = ownerId;

            if (!isMine)
            {
                Destroy(GetComponent<PlayerController>());
                Destroy(GetComponentInChildren<CameraShake>().gameObject);
            }
        }
        else
        {
            networkComponents.Add(GetComponent<NetworkTransform_Rigidbody>());
        }

        foreach (NetworkComponent item in networkComponents)
        {
            item.Client = Client;
            item.id = id;
            item.isMine = isMine;
            item.isPlayer = isPlayer;
        }
    }

    private void OnOwnerChanged( Protocol.S_SET_GAME_OBJECT_OWNER pkt )
    {
        if (pkt.GameObjectId != id)
        {
            return;
        }

        isMine = pkt.OwnerId == Client.ClientId;
    }
}
