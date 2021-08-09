
public interface ICustomEvent
{ 
    
    //StartEvent
    //You need to implement a deserializer in constructor
    //Pass OnInteract Instance to set action done
    void StartEvent(OnInteract action);

}