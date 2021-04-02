using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class WebService : MonoBehaviour
{
    [SerializeField] string cookieValue = "__test=e45efd70a8a86380ec20d847525aab0c";
    [SerializeField] GameObject chat;
    [SerializeField] GameObject connection;
    [SerializeField] GameObject chatPrefab;
    [SerializeField] Transform chatTransform;

    [SerializeField] ShowRooms rooms;
    [SerializeField] InputField inputField;
    [SerializeField] Button button;
    [SerializeField] float updateSpeed = 1;

    UnityWebRequest webRequest;
    readonly string uri = "http://arioncerceau.epizy.com/SendMessage.php?";

    public UnityWebRequest WebRequest { get => webRequest; private set => webRequest = value; }
    public string Message { get; set; }

    Coroutine active;

    public List<int> currentMessages = new List<int>();

    // Start is called before the first frame update
    void OnEnable()
    {
        chat.SetActive(true);
        connection.SetActive(false);
        ReadMessage();
    }

    void OnDisable()
    {
        StopCoroutine(active);
        chat.SetActive(false);
        connection.SetActive(true);
        rooms.hostOptions.SetActive(false);

        for (int i = 0; i < chatTransform.childCount; i++)
        {
            int id = i;
            Destroy(chatTransform.GetChild(id).gameObject);
        }
    }

    public void SendMessage()
    {
        //Message
        string completeUrl = uri + "idUser="  + rooms.MyId + "&isAlert=0" + "&roomID=" + rooms.CurrentRoomID + "&message=" + Message;

        StartCoroutine(SendRequest(completeUrl));
    }
    IEnumerator SendRequest(string completeUrl)
    {
        button.interactable = false;

        webRequest = UnityWebRequest.Get(completeUrl);

        using (webRequest)
        {
            webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            webRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return webRequest.SendWebRequest();

            //Handle connection errors 
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Message Send");
                Clear();
                button.interactable = true;
            }
        }
    }

    public void ReadMessage()
    {
        active = StartCoroutine(GetRequest());
    }

    string ConvertMessage(string inner)
    {
        string name = inner.Split('=')[2];
        name = name.Split('"')[1];
        name = name.Split('"')[0];

        string msg = inner.Split('=')[3];
        msg = msg.Split('"')[1];
        msg = msg.Split('"')[0];

        return name + ": " + msg;
    }

    IEnumerator GetRequest()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateSpeed);
            webRequest = UnityWebRequest.Get(uri + "roomID=" + rooms.CurrentRoomID + "&idUser=-1");

            using (webRequest)
            {
                webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
                webRequest.SetRequestHeader("Cookie", cookieValue);

                //Wait communication
                yield return webRequest.SendWebRequest();

                //Handle connection errors 
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log("Updating Messages");

                    XmlDocument doc = new XmlDocument();

                    doc.LoadXml(webRequest.downloadHandler.text);

                    XmlNode result = doc.FirstChild;
                    XmlNodeList itemList = result.ChildNodes;

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        Debug.Log(itemList[i].InnerXml);

                        string msgId = itemList[i].InnerXml.Split('=')[5];
                        msgId = msgId.Split('"')[1];
                        msgId = msgId.Split('"')[0];

                        string msgAlert = itemList[i].InnerXml.Split('=')[6];
                        msgAlert = msgAlert.Split('"')[1];
                        msgAlert = msgAlert.Split('"')[0];

                        if (!currentMessages.Contains(int.Parse(msgId)))
                        {
                            GameObject chat = Instantiate(chatPrefab, chatTransform);
                            chat.GetComponentInChildren<Text>().text = ConvertMessage(itemList[i].InnerXml);
                            if (msgAlert == "1")
                            {
                                chat.GetComponentInChildren<Text>().color = Color.red;
                            }
                            currentMessages.Add(int.Parse(msgId));
                        }

                        

                        string id = itemList[i].InnerXml.Split('"')[1];
                        id = id.Split('"')[0];

                        if (rooms.MyId != int.Parse(id))
                        {
                            if (!rooms.UsersId.Contains(int.Parse(id)))
                            {
                                if (!rooms.UsersId.Contains(int.Parse(id)))
                                {
                                    rooms.UsersId.Add(int.Parse(id));
                                }

                                string name = itemList[i].InnerXml.Split('=')[2];
                                name = name.Split('"')[1];
                                name = name.Split('"')[0];

                                if (!rooms.UsersName.Contains(name))
                                {
                                    rooms.UsersName.Add(name);
                                }

                                Toggle b = Instantiate(rooms.usuarioPrefab, rooms.usuarioList);
                                b.onValueChanged.AddListener((value) => rooms.BlockUser(int.Parse(id), value, name));
                                b.GetComponentInChildren<Text>().text = name;

                                rooms.users.Add(b.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }

    public void Clear()
    {
        inputField.text = "";
        Message = "";
    }

    public void VerifyIfMessageIsEmpty()
    {
        button.interactable = inputField.text != "";
    }
}
