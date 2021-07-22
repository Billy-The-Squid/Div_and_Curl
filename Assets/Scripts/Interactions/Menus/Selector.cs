using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Selector<T> : MonoBehaviour
{
    [SerializeField]
    protected T[] _available;
    public T[] available
    {
        get => _available;
        set
        {
            if(!_available.Equals(value))
            {
                _available = value;
                ChangeAvailable();
            }
        }
    }

    protected int _current;
    public int current {
        get => _current;
        set {
            if(available.Length > 0)
            {
                int val = value % available.Length;
                while (val < 0)
                {
                    val += available.Length;
                }
                if (val != _current)
                {
                    _current = val;
                    ChangeSelection();
                }
            } 
            else
            {
                Debug.LogError(name + " recieved an empty array.");
            }
        }
    }

    protected virtual void Start()
    {
        current = 0;
        if(available.Length > 0)
        {
            ChangeAvailable();
            ChangeSelection();
        }
        else
        {
            Debug.LogWarning(name + " cannot select from an array of zero length ");
        }
    }

    /// <summary>
    /// Update the menu to the current selection. Must be able to gracefully handle null pointers, etc. 
    /// </summary>
    protected abstract void ChangeSelection();

    protected virtual void ChangeAvailable()
    {
        ChangeSelection();
    }

    public virtual void Next() {
        current += 1;
    }

    public virtual void Previous() {
        current -= 1;
    }

    public virtual bool HasNext()
    {
        if (available.Length > 0)
        {
            int val = (current + 1) % available.Length;
            if (val != current)
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool HasPrevious()
    {
        if (available.Length > 0)
        {
            int val = current - 1;
            while (val < 0)
            {
                val += available.Length;
            }
            if (val != current)
            {
                return true;
            }
        }
        return false;
    }
}


[System.Serializable]
public class UIEvent : UnityEvent<Canvas> { }