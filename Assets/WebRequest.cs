using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Xml;

public class WebRequest : MonoBehaviour
{
    public int itemId = 1;
    void Start()
    {
        UnityWebRequest.ClearCookieCache(new System.Uri("http://arioncerceau.epizy.com"));
        StartCoroutine(GetRequest("http://arioncerceau.epizy.com/testedb.php?id=" + itemId.ToString()));
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);

        using (webRequest)
        {
            // PARA BURLAR O PROBLEMA COM O INFINITY FREE
            webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
          // MEU COOKIE, PEGO NO WIRE SHARK --> WIFI (OU ETHERNET) --> GRAVAR --> F5 NA PÁGINA NO EPZY --> ABRIR WIRE SHARK E PROCURAR POR HTTP --> HYPERTEXT TRANSFER PROTOCOL --> COOKIE
           webRequest.SetRequestHeader("Cookie", "__test=226269f34688ea4884f625e7eddcd694");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError) // O mesmo que webRequest.isNetworkError (obsoleto)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                // XML Parse - Em objeto XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webRequest.downloadHandler.text);

                //Dispaly all the book titles.
                XmlNode result = doc.FirstChild;
                XmlNodeList itemList = result.ChildNodes;

                XmlAttributeCollection attributes;

                for (int i = 0; i < itemList.Count; i++)
                {
                    
                    attributes = itemList[i].Attributes;
                   // XmlAttribute attr = (XmlAttribute)attributes.GetNamedItem("name");
                   // Debug.Log("Name:"+ attributes["name"]);
                }
            }
        }
    }

}
