using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using SimpleJSON;

public class DropboxItem
{
    public string name { get; set; }
    public string type { get; set; }
}

public class DropboxAPIModule : MonoBehaviour {
    public string ClientID = "rnj3c5emjhj6qzs";
    public string ClientSecret;
    public string testToken = "3sfXSVeeyKwAAAAAAAAJO-BSICNhdYrmbhziIdRx7I2WWY72qbYRdtAzi6ZQji4x";
    public string RedirectURI = "http://localhost";
    const int SERVER_PORT = 9271;

    public Text fileList;
    public InputField customPath;

    Uri AuthorizationURL
    {
        get
        {
            return new Uri("https://www.dropbox.com/1/oauth2/authorize?" +
                    "response_type=token" + 
                    "&client_id=" + ClientID +
                    "&redirect_uri=" + RedirectURI);
        }
    }

    public bool Validator(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors policyErrors)
    {
        return true;
    }

    public void updateList ()
    {
        listofFiles.Clear();
        if (customPath.text == "/" || customPath.text == "")
        {
            //UnityEngine.Debug.Log("Visit root");
            updateFileList("");
        }
        else
        {
            //UnityEngine.Debug.Log("Visit " + customPath.text);
            updateFileList(customPath.text);
        }
        fileList.text = "";
        foreach (DropboxItem dbItem in listofFiles)
        {
            fileList.text += "("+dbItem.type+") "+dbItem.name+"\n";
        }
    }

    private List<DropboxItem> listofFiles;

    //private string getToken()
    //{
    //    ServicePointManager.ServerCertificateValidationCallback = Validator;
    //    WebRequest request = WebRequest.Create("https://api.dropboxapi.com/2/files/list_folder");
    //    request.Method = "GET";

    //    request.Headers.Add("Authorization: Bearer " + testToken);
    //    //        request.Headers.Add("Content-Type: applicatoin/json");
    //    request.ContentType = "application/json; charset=UTF-8";

    //    string postData = "{\"path\": \"" + path + "\",\"recursive\": false,\"include_media_info\": false,\"include_deleted\": false}";
    //    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
    //    request.ContentLength = byteArray.Length;

    //    Stream dataStream = request.GetRequestStream();
    //    dataStream.Write(byteArray, 0, byteArray.Length);

    //    WebResponse response = request.GetResponse();
    //    dataStream = response.GetResponseStream();

    //    //UnityEditor.EditorUtility.DisplayDialog("Safe Landing", "4", "Ok"); 

    //    StreamReader reader = new StreamReader(dataStream);
    //    string resultJSON = reader.ReadToEnd();

    //    //parsing JSON
    //    var parseResult = JSON.Parse(resultJSON);
    //    for (int index = 0; index < parseResult["entries"].Count; index++)
    //    {
    //        DropboxItem dbItem = new DropboxItem();
    //        dbItem.name = parseResult["entries"][index]["name"];
    //        dbItem.type = parseResult["entries"][index][".tag"];
    //        listofFiles.Add(dbItem);
    //    }

    //    reader.Close();
    //    dataStream.Close();
    //    response.Close();

    //    return "";
    //}

    private void updateFileList (string path)
    {
        ServicePointManager.ServerCertificateValidationCallback = Validator;
        WebRequest request = WebRequest.Create("https://api.dropboxapi.com/2/files/list_folder");
        request.Method = "POST";

        request.Headers.Add("Authorization: Bearer " + testToken);
        //        request.Headers.Add("Content-Type: applicatoin/json");
        request.ContentType = "application/json; charset=UTF-8";

        string postData = "{\"path\": \""+path+"\",\"recursive\": false,\"include_media_info\": false,\"include_deleted\": false}";
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentLength = byteArray.Length;

        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);

        WebResponse response = request.GetResponse();
        dataStream = response.GetResponseStream();

        //UnityEditor.EditorUtility.DisplayDialog("Safe Landing", "4", "Ok"); 

        StreamReader reader = new StreamReader(dataStream);
        string resultJSON = reader.ReadToEnd();

        //parsing JSON
        var parseResult = JSON.Parse(resultJSON);
        for (int index = 0; index < parseResult["entries"].Count; index++ )
        {
            DropboxItem dbItem = new DropboxItem();
            dbItem.name = parseResult["entries"][index]["name"];
            dbItem.type = parseResult["entries"][index][".tag"];
            listofFiles.Add(dbItem);
        }

        reader.Close();
        dataStream.Close();
        response.Close();
    }

	// Use this for initialization
	void Start () {
        listofFiles = new List<DropboxItem>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
