using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Panel_NetworkInfo : Panel_Base
{
    TMP_Text label_Ping;

    public static Panel_NetworkInfo Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<Panel_NetworkInfo>();
            return instance;
        }
    }
    private static Panel_NetworkInfo instance;

    protected override void Awake()
    {
        base.Awake();

        label_Ping = this.transform.Search(nameof(label_Ping)).GetComponent<TMP_Text>();
    }

    public void SetPing(int ping)
    {
        label_Ping.text = $"Ping: {ping}ms";
    }
}