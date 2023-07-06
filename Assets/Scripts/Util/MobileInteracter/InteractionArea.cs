using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 변고경 BKK
/// 상호작용 가능한 영역을 설정해주는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractionArea : MonoBehaviour
{
    public UnityEvent<Collider> _ontriggerEnter;
    public UnityEvent<Collider> _ontriggerStay;
    public UnityEvent<Collider> _ontriggerExit;

    public TriggerCheckType checkType = TriggerCheckType.Tag;
    
    public LayerMask collisionLayer;
    
    public string CollisionTag = "Player";

    public bool entered = false;

    public bool playOnce = false;

    private bool checkPlayOnce = false;

    public bool setLayerOnAwake = true; 

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if(setLayerOnAwake) this.gameObject.layer = LayerMask.NameToLayer("InteractionArea");
    }

    private void OnValidate()
    {
        //this.gameObject.layer = LayerMask.NameToLayer("InteractionArea");
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (playOnce)
        // {
        //     if(checkPlayOnce) return;
        //     checkPlayOnce = true;
        // }

        switch (checkType)
        {
            case TriggerCheckType.Tag:
                if (other.CompareTag(CollisionTag))
                {
                    if (playOnce)
                    {
                        if(checkPlayOnce) return;
                        checkPlayOnce = true;
                    }
                    _ontriggerEnter?.Invoke(other);
                    entered = true;
                }
                break;
            case TriggerCheckType.Layer:
                if (((1 << other.gameObject.layer) & collisionLayer) != 0)
                {
                    if (playOnce)
                    {
                        if(checkPlayOnce) return;
                        checkPlayOnce = true;
                    }
                    _ontriggerEnter?.Invoke(other);
                    entered = true;
                }
                break;
            case TriggerCheckType.Both:
                if (other.CompareTag(CollisionTag) && ((1 << other.gameObject.layer) & collisionLayer) != 0)
                {
                    if (playOnce)
                    {
                        if(checkPlayOnce) return;
                        checkPlayOnce = true;
                    }
                    _ontriggerEnter?.Invoke(other);
                    entered = true;
                }
                break;
            case TriggerCheckType.None:
            default:
                if (playOnce)
                {
                    if(checkPlayOnce) return;
                    checkPlayOnce = true;
                }
                _ontriggerEnter?.Invoke(other);
                entered = true;
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (checkType)
        {
            case TriggerCheckType.Tag:
                if (other.CompareTag(CollisionTag))
                {
                    _ontriggerStay?.Invoke(other);
                    entered = true;
                }
                break;
            case TriggerCheckType.Layer:
                if (((1 << other.gameObject.layer) & collisionLayer) != 0)
                {
                    _ontriggerStay?.Invoke(other);
                    entered = true;
                }
                break;
            case TriggerCheckType.Both:
                if (other.CompareTag(CollisionTag) && ((1 << other.gameObject.layer) & collisionLayer) != 0)
                {
                    _ontriggerStay?.Invoke(other);
                    entered = true;
                }
                break;
            case TriggerCheckType.None:
            default:
                _ontriggerStay?.Invoke(other);
                entered = true;
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (checkType)
        {
            case TriggerCheckType.Tag:
                if (other.CompareTag(CollisionTag))
                {
                    _ontriggerExit?.Invoke(other);
                    entered = false;
                }
                break;
            case TriggerCheckType.Layer:
                if (((1 << other.gameObject.layer) & collisionLayer) != 0)
                {
                    _ontriggerExit?.Invoke(other);
                    entered = false;
                }
                break;
            case TriggerCheckType.Both:
                if (other.CompareTag(CollisionTag) && ((1 << other.gameObject.layer) & collisionLayer) != 0)
                {
                    _ontriggerExit?.Invoke(other);
                    entered = false;
                }
                break;
            case TriggerCheckType.None:
            default:
                _ontriggerExit?.Invoke(other);
                entered = false;
                break;
        }
    }
}

public enum TriggerCheckType
{
    Tag,
    Layer,
    Both,
    None
}

