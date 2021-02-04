using System;

public interface CustomEvent
{ 
    
    //StartEvent
    //You need to implement a deserializer in constructor
    //Pass OnInteract Instance to set action done
    void StartEvent(OnInteract action);
}