using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{

    private static int _lastUid = 0;
    public int getUID { get { _lastUid++; return _lastUid; } }
    public int uid;
    public UnityEvent onInteract;
    public UnityEvent onHover;
    public UnityEvent onStopHover;
    private void Awake()
    {
        uid = getUID;
    }
    public void DebugStr(string a)
    {
        Debug.Log(a);
    }
    private void Start()
    {
        PlayerInteractManager.current.AddInteractable(this);
    }
}
