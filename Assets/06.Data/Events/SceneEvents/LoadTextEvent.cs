using System;
using UnityEngine.UI;

namespace CustomEventNamespace
{
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
    }

}

