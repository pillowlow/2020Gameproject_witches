using UnityEngine;

namespace CustomEventNamespace
{
    public class OpenDoorEvent : MonoBehaviour, CustomEvent
    {
        private Door door;
        public OpenDoorEvent(OnInteract.DataStruct data)
        {
            door = data.gameObject.GetComponent<Door>();
        }
        
        public void StartEvent(OnInteract action)
        {
            if (door.isClosed)
            {
                if (door.isClosed)
                {
                    //Check whether player has the key
                    if (true)
                    {
                        door.QueryToUnlockDoor(action);
                    }
                }
                else
                {
                    door.QueryToOpenDoor(action);
                }
            }
        }
    }
}