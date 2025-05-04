using System;
using UnityEngine;

public class WebSocketEncoder
{
    [Serializable]
    public class MessageData
    {
        public string id;
        public string value;
    }

    public static string Encode(MessageData data)
    {
        return JsonUtility.ToJson(data);
    }

    public static MessageData Decode(string data)
    {
        try
        {
            return JsonUtility.FromJson<MessageData>(data);
        } 
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }
}