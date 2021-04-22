using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;

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
    [SerializeField] ScrollRect scroll;

    [SerializeField] InputField inputFieldCookie;
    [SerializeField] Button buttonCookie;

    UnityWebRequest webRequest;
    readonly string uri = "http://arioncerceau.epizy.com/SendMessage.php?";
    readonly string uriGetUsers = "http://arioncerceau.epizy.com/UsersInRoom.php?";

    public UnityWebRequest WebRequest { get => webRequest; private set => webRequest = value; }
    public string Message { get; set; }

    Coroutine active;

    List<int> currentMessages = new List<int>();
    List<string> current = new List<string>();


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) //Apertar Enter pra mandar msg
        {
            if (button.interactable){
            SendMessage();
            }
        }
    }
    void OnEnable()
    {
        chat.SetActive(true);
        connection.SetActive(false);
        ReadMessage();
    }

    void OnDisable()
    {
        StopCoroutine(active);
        if(chat != null)
            chat.SetActive(false);


        if(connection != null)
            connection.SetActive(true);

        if(rooms.HostOptions != null)
            rooms.HostOptions.SetActive(false);

        for (int i = 0; i < chatTransform.childCount; i++)
        {
            int id = i;
            Destroy(chatTransform.GetChild(id).gameObject);
        }
    }

    public void SetCookie()
    {
        cookieValue = inputFieldCookie.text;
    }

    public void Sair()
    {
        Application.Quit();
    }
    public void SendMessage()
    {
        //Message
        string completeUrl = uri + "idUser=" + rooms.MyId + "&isAlert=0" + "&roomID=" + rooms.CurrentRoomID + "&message=" + Message;

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

        string msg = inner.Split('=')[6];
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

                    if (webRequest.downloadHandler.text == "")
                    {
                        Debug.Log("Skip");
                    }
                    else
                    {
                        doc.LoadXml(webRequest.downloadHandler.text);

                        XmlNode result = doc.FirstChild;
                        XmlNodeList itemList = result.ChildNodes;

                        current = new List<string>();

                        for (int i = 0; i < itemList.Count; i++)
                        {
                            Debug.Log(itemList[i].InnerXml);

                            string msgId = itemList[i].InnerXml.Split('=')[5];
                            msgId = msgId.Split('"')[1];
                            msgId = msgId.Split('"')[0];

                            string msgAlert = itemList[i].InnerXml.Split('=')[3];
                            msgAlert = msgAlert.Split('"')[1];
                            msgAlert = msgAlert.Split('"')[0];

                            if (!currentMessages.Contains(int.Parse(msgId)))
                            {
                                GameObject chat = Instantiate(chatPrefab, chatTransform);
                                string msg = ConvertMessage(itemList[i].InnerXml);

                                msg = ReplaceTextWithEmoji(msg, ":feliz");
                                msg = ReplaceTextWithEmoji(msg, ":triste");
                                msg = ReplaceTextWithEmoji(msg, ":nervoso");
                                msg = ReplaceTextWithEmoji(msg, ":pensativo");
                                msg = ReplaceTextWithEmoji(msg, ":bravo");
                                msg = ReplaceTextWithEmoji(msg, ":8");
                                msg = ReplaceTextWithEmoji(msg, ":fumando");
                                msg = ReplaceTextWithEmoji(msg, ":surpresa");
                                msg = ReplaceTextWithEmoji(msg, ":apaixonado");
                                msg = ReplaceTextWithEmoji(msg, ":contente");
                                msg = ReplaceTextWithEmoji(msg, ":ideia");
                                msg = ReplaceTextWithEmoji(msg, ":chaplin");
                                msg = ReplaceTextWithEmoji(msg, ":tirado");
                                msg = ReplaceTextWithEmoji(msg, ":assobiar");
                                msg = ReplaceTextWithEmoji(msg, ":doente");
                                msg = ReplaceTextWithEmoji(msg, ":louco");


                                chat.GetComponentInChildren<TextMeshProUGUI>().text = msg;
                                if (msgAlert == "1")
                                {
                                    chat.GetComponentInChildren<TextMeshProUGUI>().color = new Vector4(0.9f, 0.9f, 0.016f, 1);
                                }

                                if (GetLinks(msg, out string link))
                                {
                                    chat.AddComponent<Button>().onClick.AddListener(() => Application.OpenURL(link));
                                    chat.GetComponentInChildren<TextMeshProUGUI>().color = Color.cyan;
                                    chat.GetComponentInChildren<TextMeshProUGUI>().text = "<i>" + msg + "</i>";
                                }
                                currentMessages.Add(int.Parse(msgId));

                                yield return new WaitForEndOfFrame();
                                scroll.normalizedPosition = new Vector2(0, 0);
                            }
                        }
                    }
                }
            }
            yield return StartCoroutine(GetUsers());
        }
    }

    IEnumerator GetUsers()
    {
        yield return new WaitForSeconds(updateSpeed);
        webRequest = UnityWebRequest.Get(uriGetUsers + "roomID=" + rooms.CurrentRoomID);

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

                if (webRequest.downloadHandler.text == "")
                {
                    Debug.Log("Skip");
                }
                else
                {
                    doc.LoadXml(webRequest.downloadHandler.text);

                    XmlNode result = doc.FirstChild;
                    XmlNodeList itemList = result.ChildNodes;

                    current = new List<string>();
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        string id = itemList[i].InnerXml.Split('"')[1];
                        id = id.Split('"')[0];

                        if (rooms.MyId != int.Parse(id))
                        {
                            current.Add(id);

                            if (!rooms.UsersId.Contains(int.Parse(id)))
                            {
                                rooms.UsersId.Add(int.Parse(id));

                                string name = itemList[i].InnerXml.Split('=')[2];
                                name = name.Split('"')[1];
                                name = name.Split('"')[0];

                                rooms.UsersName.Add(name);

                                Toggle b = Instantiate(rooms.UsuarioPrefab, rooms.UsuarioList);
                                b.onValueChanged.AddListener((value) => rooms.BlockUser(int.Parse(id), value, name));
                                b.GetComponentInChildren<Text>().text = name;

                                rooms.Users.Add(b.gameObject);
                            }
                        }
                    }

                    for (int i = 0; i < rooms.UsersId.Count; i++)
                    {
                        int id = i;
                        if (!current.Contains(rooms.UsersId[id].ToString()))
                        {
                            Destroy(rooms.Users[id].gameObject);
                            rooms.Users.RemoveAt(id);

                            rooms.UsersId.RemoveAt(id);

                            rooms.UsersName.RemoveAt(id);
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

    public void VerifyIfCookieIsEmpty()
    {
        buttonCookie.interactable = inputFieldCookie.text != "";
    }

    public void VerifyIfMessageIsEmpty()
    {
        button.interactable = inputField.text != "";
    }

    public bool GetLinks(string message, out string link)
    {
        List<string> list = new List<string>();
        Regex urlRx = new Regex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);

        bool have = false;

        MatchCollection matches = urlRx.Matches(message);
        foreach (Match match in matches)
        {
            have = true;
            list.Add(match.Value);
        }

        if (have)
        {
            link = list[0];
        }
        else
        {
            link = "";
        }

        return have;
    }

    public string ReplaceTextWithEmoji(string text, string toReplace)
    {
        string input = text;
        string pattern = toReplace;
        string replace = "<sprite name=" + toReplace + ">";
        return Regex.Replace(input, pattern, replace);
    }
}
