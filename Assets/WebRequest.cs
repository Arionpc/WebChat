using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    [SerializeField] int itemID = 1;

    string uri = "http://arioncerceau.epizy.com/testedb.php?id=";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetRequest(uri + itemID.ToString()));
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);

        using (webRequest)
        {
            webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.57");
            webRequest.SetRequestHeader("Cookie", "__test=87b1b2e0e5bce20ae27d1c65cde386d0");

            //Wait communication
            yield return webRequest.SendWebRequest();

            //Handle connection errors 
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Connected");

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webRequest.downloadHandler.text);

                XmlNode result = doc.FirstChild;
                XmlNodeList itemList = result.ChildNodes;

                for (int i = 0; i < itemList.Count; i++)
                {
                    Debug.Log(itemList[i].InnerXml);

                    XmlAttributeCollection attributes = itemList[i].Attributes;

                    Debug.Log(itemList[i].InnerXml);
                }
            }
        }
    }
}