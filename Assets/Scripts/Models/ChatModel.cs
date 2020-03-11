using System;

[Serializable]
public class ChatModel
{
    private string _id;
    public string Id
    {
        get { return _id; }
        set { _id = value; }
    }

    private string _mesage;
    public string LastMesage
    {
        get { return _mesage; }
        set { _mesage = value; }
    }
}