using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ShowRooms : MonoBehaviour
{
    readonly string enterRoomUri = "http://arioncerceau.epizy.com/EnterRoom.php?";
    readonly string createRoomUri = "http://arioncerceau.epizy.com/CreateRoom.php?";
    readonly string exitRoomUri = "http://arioncerceau.epizy.com/ExitRoom.php?";
    readonly string insertUserUri = "http://arioncerceau.epizy.com/InsertUser.php?";
    readonly string blockUserUri = "http://arioncerceau.epizy.com/BlockUser.php?";
    readonly string messageUri = "http://arioncerceau.epizy.com/SendMessage.php?";

    [SerializeField] string cookieValue = "__test=e45efd70a8a86380ec20d847525aab0c";

    [SerializeField] RoomData prefab;
    [SerializeField] Transform parentButton;
    [SerializeField] WebService service;
    [Header("Fields")]
    [SerializeField] InputField nameField;
    [SerializeField] InputField roomField;
    [Header("Buttons")]
    [SerializeField] Button createButton;
    [SerializeField] Button SearchButton;
    [SerializeField] Button EnterButton;
    [SerializeField] Text roomName;
    [Header("Host")]
    [SerializeField] GameObject hostOptions;
    [SerializeField] Toggle usuarioPrefab;
    [SerializeField] Transform usuarioList;

    public UnityWebRequest WebRequest { get; private set; }
    public string NickName { get; set; }

    public int CurrentRoomID { get; set; }
    public string CurrentRoomName { get; set; }
    public int MyId { get; set; }
    public List<int> UsersId { get; set; } = new List<int>();
    public List<string> UsersName { get; set; } = new List<string>();
    public List<GameObject> Users { get; set; } = new List<GameObject>();
    public GameObject HostOptions { get => hostOptions; set => hostOptions = value; }
    public Toggle UsuarioPrefab { get => usuarioPrefab; set => usuarioPrefab = value; }
    public Transform UsuarioList { get => usuarioList; set => usuarioList = value; }

    bool isCurrentHost = false;

    XmlNodeList itemList;

    public void SearchRoom()
    {
        for (int i = 0; i < parentButton.childCount; i++)
        {
            int id = i;
            Destroy(parentButton.GetChild(id).gameObject);
        }

        EnterButton.interactable = false;

        //Message
        string completeUrl = enterRoomUri + "nick=" + NickName;
        UnityWebRequest.Get(completeUrl);

        //Set text in URL
        StartCoroutine(ReadSearch(completeUrl));
    }
    IEnumerator ReadSearch(string completeUrl)
    {
        Debug.Log("Searching Rooms...");
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(WebRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                itemList = result.ChildNodes;
                bool created = false;
                for (int i = 0; i < itemList.Count; i++)
                {
                    created = true;
                    RoomData data = Instantiate(prefab, parentButton);

                    data.roomID = int.Parse(GetID(itemList[i].InnerXml));
                    data.roomName = GetName(itemList[i].InnerXml);
                    data.text.text = data.roomName;
                    data.button.onClick.AddListener(() => SelectRoom(data));
                }

                if (!created)
                {
                    Debug.Log("Não há salas.");
                }
            }
        }
    }

    public void JoinRoom()
    {
        string completeUrl = insertUserUri + "idRoom=" + CurrentRoomID + "&name=" + NickName + "&isHost=0";

        StartCoroutine(ReadJoin(completeUrl));
    }
    public void JoinRoomHost()
    {
        string completeUrl = insertUserUri + "idRoom=" + CurrentRoomID + "&name=" + NickName + "&isHost=1";
        isCurrentHost = true;
        StartCoroutine(ReadJoin(completeUrl, isCurrentHost));
    }
    IEnumerator ReadJoin(string completeUrl, bool isHost = false)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                Debug.Log("User Created");

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(WebRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                itemList = result.ChildNodes;

                for (int i = 0; i < itemList.Count; i++)
                {
                    MyId = GetUserId(itemList[i].InnerXml);
                }
            }

            Debug.Log("Current Joined in: " + CurrentRoomName + "/ID: " + CurrentRoomID);

            OpenChat(isCurrentHost);

            OpenChat();
            //xml com bem vindo
            string url = messageUri + "idUser=" + MyId + "&isAlert=1" + "&roomID=" + CurrentRoomID + "&message=" + " entrou na sala.";
            StartCoroutine(GetWhoJoined(url));
        }
    }
    IEnumerator GetWhoJoined(string completeUrl)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                Debug.Log("Message Send");

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(WebRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                itemList = result.ChildNodes;
            }
        }
    }

    public void CreateRoom()
    {
        string completeUrl = createRoomUri + "name=" + CurrentRoomName;

        StartCoroutine(ReadCreate(completeUrl));
    }
    IEnumerator ReadCreate(string completeUrl)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                Debug.Log("Room Created");

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(WebRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                itemList = result.ChildNodes;

                for (int i = 0; i < itemList.Count; i++)
                {
                    CurrentRoomID = int.Parse(GetID(itemList[i].InnerXml));
                    CurrentRoomName = GetName(itemList[i].InnerXml);
                }
            }

            JoinRoomHost();
        }
    }

    public void ExitRoom()
    {
        //send to php that i disconnected, send id to remove from room, If host, disconnect all users and destroy room
        string completeUrl = exitRoomUri + "idRoom=" + CurrentRoomID + "&idUser=" + MyId;

        StartCoroutine(ReadExit(completeUrl));
    }
    IEnumerator ReadExit(string completeUrl)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                Debug.Log("Exit Completed");

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(WebRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                itemList = result.ChildNodes;

                string nameExit = "";

                for (int i = 0; i < itemList.Count; i++)
                {
                    nameExit = itemList[i].InnerXml.Split('"')[1];
                    nameExit = nameExit.Split('"')[0];
                }

                string url;
                if (!isCurrentHost)
                {
                    url = messageUri + "idUser=" + MyId + "&isAlert=1" + "&roomID=" + CurrentRoomID + "&message=" + "O usuario " + nameExit + " saiu da sala.";
                }
                else
                {
                    url = messageUri + "idUser=-2" + "&isAlert=1" + "&roomID=" + CurrentRoomID + "&message=" + "Sistema: O host se desconectou. A sala está offline.";
                }

                StartCoroutine(GetWhoExit(url));
            }

            OpenMenu();
        }
    }
    IEnumerator GetWhoExit(string completeUrl)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                Debug.Log("Exit Send");
            }
        }
    }

    public void SelectRoom(RoomData data)
    {
        CurrentRoomID = data.roomID;
        CurrentRoomName = data.roomName;
        EnterButton.interactable = true;
    }

    int GetUserId(string value)
    {
        string id = value.Split('"')[1];
        string newId = id.Split('"')[0];

        return int.Parse(newId);
    }
    string GetName(string innerXml)
    {
        string name = innerXml.Split('=')[2];
        name = name.Split('"')[1];
        string newName = name.Split('"')[0];

        return newName;
    }
    string GetID(string innerXml)
    {
        string id = innerXml.Split('"')[1];
        string newId = id.Split('"')[0];

        return newId;
    }

    void OpenChat(bool host = false)
    {
        roomName.text = "Sala: " + CurrentRoomName;
        service.enabled = true;

        if (host)
        {
            HostOptions.SetActive(true);
        }

        for (int i = 0; i < parentButton.childCount; i++)
        {
            int id = i;
            Destroy(parentButton.GetChild(id).gameObject);
        }
    }
    void OpenMenu()
    {
        isCurrentHost = false;
        service.enabled = false;
        EnterButton.interactable = false;

        Users.Clear();
        UsersName.Clear();
        UsersId.Clear();
    }

    public void OpenPlayerList()
    {

    }

    public void BlockUser(int idUser, bool value, string name)
    {
        string completeUrl;
        if (value)
        {
            completeUrl = blockUserUri + "idRoom=" + CurrentRoomID + "&idUser=" + idUser + "&isBlocking=1";
        }
        else
        {
            completeUrl = blockUserUri + "idRoom=" + CurrentRoomID + "&idUser=" + idUser + "&isBlocking=0";
        }

        StartCoroutine(ReadBlock(completeUrl, value, idUser, name));
    }
    IEnumerator ReadBlock(string completeUrl, bool value, int idUser, string name)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(WebRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                itemList = result.ChildNodes;
            }

            string url;
            if (value)
            {
                url = messageUri + "idUser=" + MyId + "&isAlert=1" + "&roomID=" + CurrentRoomID + "&message=" + "O moderador bloqueou " + name + ". As mensagens não serão enviadas.";
            }
            else
            {
                url = messageUri + "idUser=" + MyId + "&isAlert=1" + "&roomID=" + CurrentRoomID + "&message=" + "O moderador desbloqueou " + name + ". As mensagens podem ser enviadas.";
            }

            StartCoroutine(GetWhoWasBlocked(url));
        }
    }
    IEnumerator GetWhoWasBlocked(string completeUrl)
    {
        WebRequest = UnityWebRequest.Get(completeUrl);

        using (WebRequest)
        {
            WebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            WebRequest.SetRequestHeader("Cookie", cookieValue);

            //Wait communication
            yield return WebRequest.SendWebRequest();

            //Handle connection errors 
            if (WebRequest.result == UnityWebRequest.Result.ConnectionError || WebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + WebRequest.error);
            }
            else
            {
                Debug.Log("Message Send");
            }
        }
    }

    public void VerifyIfNameIsEmpty()
    {
        SearchButton.interactable = nameField.text != "";
    }
    public void VerifyIfRoomNameAndIsEmpty()
    {
        createButton.interactable = roomField.text != "" && nameField.text != "";
    }
}
