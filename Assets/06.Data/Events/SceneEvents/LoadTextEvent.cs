using System;
using UnityEngine.UI;

public class LoadTextEvent : CustomEvent
{
        private TextUIScript TextScript;
        private String TextPath;

        public LoadTextEvent(TextUIScript script,String path)
        {
                TextScript = script;
                TextPath = path;
        }
        public void StartEvent(OnInteract action)
        {
                TextScript.LoadText(TextPath,action);
        }

        public void Deserialize(String str)
        {
                
        }
}
