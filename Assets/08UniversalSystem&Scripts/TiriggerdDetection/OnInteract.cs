using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class OnInteract : MonoBehaviour
{
    
    [Serializable]
    public struct DataStruct
    {
        [CanBeNull] public String path;
        [CanBeNull] public GameObject gameObject;
        [CanBeNull] public Vector2 vec;
        public bool clearEvents;
    }
    
    [Serializable]
    public struct EventsStruct
    {
        [StringInList(typeof(OnInteract),"GetEvents")]
        public List<String> EventsList;
        public List<DataStruct> Data;
    }
    
    [SerializeField]
    private EventsStruct EventsData;
    public Queue<String> EventsQueue=new Queue<String>();

    private bool EventDone=true;
    private int DataIndex=0;
    private LayerMask playerLayer;
    public GameObject Hint;

    
    //get all CustomEvents using Reflection

    public static String[] GetEvents()
    {
        String name = nameof(CustomEventNamespace);
        var clazz = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == name
            select t.ToString();
        return clazz.Select(n=>n.Replace("CustomEventNamespace.","")).ToArray();
    }

    public void Start()
    {
        playerLayer = PlayerManager.instance.layer;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //check if gameObject layer is in playerLayer
        if (((1 << col.gameObject.layer) & playerLayer) != 0)
        {
            Hint.SetActive(true);
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (((1 << col.gameObject.layer) & playerLayer) != 0)
        {
            if (EventsQueue.Count==0 && PlayerManager.instance.input.GetKeyDown(InputAction.Interact))
            {
                if(PlayerManager.state==PlayerManager.StateCode.Stop) return;
                DataIndex = 0;
                foreach (String e in EventsData.EventsList)
                    EventsQueue.Enqueue(e);
                if (EventsQueue.Count!=0) 
                    ExecuteEvent();
                
            }
        }
        
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (((1 << col.gameObject.layer) & playerLayer) != 0)
        {
            Hint.SetActive(false);
        }
    }
    
    
    public void ExecuteEvent()
    {   
        if(!EventDone) return;
        EventDone = false;
        PlayerManager.state = PlayerManager.StateCode.Stop;
        Type t = Type.GetType("CustomEventNamespace." + EventsQueue.Dequeue());
        CustomEvent _event =(CustomEvent)Activator.CreateInstance(t,EventsData.Data[DataIndex]);
        _event.StartEvent(this);
    }

    public void SetEventDone()
    {
        EventDone = true;
        if(EventsQueue.Count==0) PlayerManager.state = PlayerManager.StateCode.Idle;
        
    }

    public OnInteract AddAction(String e,DataStruct data)
    {
        if (GetEvents().Contains(e))
        {
            EventsData.EventsList.Add(e);
            EventsData.Data.Add(data);
        }else Debug.LogWarning("[Warning] No event called "+e+", skip");
        return this;

    }

    public OnInteract AddAction(CustomEvent e, DataStruct data)
    {
        String str = e.ToString().Replace("CustomEventNamespace.", "");
        EventsData.EventsList.Add(str);
        EventsData.Data.Add(data);
        return this;
    }
    public OnInteract ClearEvent()
    {
        EventsData.EventsList.Clear();
        EventsData.Data.Clear();
        return this;
    }

}
