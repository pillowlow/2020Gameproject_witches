using System;

public class CustomEventFactory
{
    //return CustomEvent Instance
    //T is for CustomEvent type
    //K is for script type
    public static T GetEvent<T,K>(K script, String str) where T : CustomEvent
    {
        return (T) Activator.CreateInstance(typeof(T),script,str);
    }
    
}