using UnityEngine;
using UnityEngine.Events;

using System;
using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;

[Serializable]
public class DownloadManagerEvent : UnityEvent<bool, string> { }

public class DownloadManager : GlobalBehaviorBase
{
    private PassData _passData;

    private readonly object _hashSetLock = new object();
    private HashSet<string> _existedSlides;
    private string _persistentDataPath;

    public bool IsPathExisted(string path)
    {
        lock (_hashSetLock) return _existedSlides.Contains(path);
    }

    public void AddExistedPath(string path)
    {
        lock(_hashSetLock) _existedSlides.Add(path);
    }

    public List<string> ExportExistedName()
    {
        string[] names;
        lock (_hashSetLock)
        {
            names = new string[_existedSlides.Count];
            _existedSlides.CopyTo(names);
        }
        return new List<string>(names);
    }

    public DownloadManagerEvent OnPostDownload;
    public int timeOutMs = 10000;

    protected override void Awake()
    {
        base.Awake();
        if (_passData == null)
            _passData = new PassData();

        _persistentDataPath = Application.persistentDataPath;

        if (_existedSlides == null)
        {
            _existedSlides = new HashSet<string>();
            //extract all existed slides names
            DirectoryInfo info = new DirectoryInfo(_persistentDataPath);
            _existedSlides.UnionWith(Directory.GetDirectories(_persistentDataPath, "*", SearchOption.TopDirectoryOnly));
            _existedSlides.UnionWith(Directory.GetFiles(_persistentDataPath, "*", SearchOption.TopDirectoryOnly));
        }
        
    }

    private void OnDestroy()
    {
        if (_passData.Status == ThreadedJob.JobStatus.Started)
            _passData.Abort(); //TODO: abort exception handling
    }

    public void LaunchDownload (string urlString, string dbNumber)
	{
        _passData.Setup(urlString, dbNumber, _persistentDataPath, timeOutMs, this);
        _passData.Start ();
        StartCoroutine(WaitForDownload());
	}

    private IEnumerator WaitForDownload()
    {
        ThreadedJob.JobStatus status;
        do
        {
            status = _passData.Status;
            yield return null;
        }
        while (status == ThreadedJob.JobStatus.NotStarted || status == ThreadedJob.JobStatus.Started);

        if (status == ThreadedJob.JobStatus.Succeeded)
        {
            ZipUtil.Unzip(_passData.tmpFileLocation, _passData.targetLocation);
                AddExistedPath(_passData.targetLocation);

            //5.1 succeeded
            OnPostDownload.Invoke(status == ThreadedJob.JobStatus.Succeeded, _passData.targetLocation);
        }

        //6. clean up
        if (File.Exists(_passData.tmpFileLocation))
            File.Delete(_passData.tmpFileLocation); //delete temp zip file  
    }
}

public class PassData : ThreadedJob
{
    //input
	private string m_urlStr;
	private string m_dbStr;
    private string _persistentDataPath;
    private int _timeOut;
    public DownloadManager _manager;


    //output

    public string tmpFileLocation { get; private set; }
    public string targetLocation { get; private set; }

    public PassData() : base() { }

    public void Setup(string firstStr, string secondStr, string persistentDataPath, int timeOut, DownloadManager manager)
    {
        m_urlStr = firstStr;
        m_dbStr = secondStr;
        _persistentDataPath = persistentDataPath;
        _timeOut = timeOut;
        _manager = manager;
    }

	private static bool Validator (
		object sender,
		X509Certificate certificate,
		X509Chain chain,
		SslPolicyErrors policyErrors)
	{
		return true;
	}

    private string ResolveDownloadLocation(string idealPath)
    {
        if (_manager == null)
            return idealPath;

        string modifiedPath = idealPath;
        int i = 0;
        while (_manager.IsPathExisted(modifiedPath))
        {
            modifiedPath = idealPath + "_" + i;
            i++;
        }
        return modifiedPath;
    }

	private void Download ()
	{
        //1. send request
		ServicePointManager.ServerCertificateValidationCallback = Validator;
		
		WebRequest request =  WebRequest.Create ("https://" + m_urlStr + ".localtunnel.me/unity/");
        request.Timeout = _timeOut;
        request.Method = "POST";
        string postData = string.Format("dbNumber={0}", m_dbStr);// "<@" +   + "@>";
       
		byte[] byteArray = Encoding.UTF8.GetBytes (postData);
		request.ContentType = "application/x-www-form-urlencoded";
		request.ContentLength = byteArray.Length;
    
        using (Stream dataStream = request.GetRequestStream())
        {
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
        }

        // Google Voice Recognition
        /*
		request.ContentType = "audio/x-flac; rate=" + 44100;
		request.ContentType = "application/json";
		request.ContentLength = byteArray.Length;
		forTest = byteArray.ToString ();
		
		dataStream = request.GetRequestStream ();
		dataStream.Write (byteArray, 0, byteArray.Length);
		dataStream.Flush();
		dataStream.Close ();
		*/

        //2. get request
        try
        {
            using (WebResponse response = request.GetResponse()) //sychronized here
            {
                //3. store       
                tmpFileLocation = Path.Combine(_persistentDataPath,
                    string.Format("{0}{1}", "Coffee", "_tmp"));

                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[4097];
                        byte[] result = null;
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                            if (count == 0) { break; }
                        }
                        while (true);

                        result = memoryStream.ToArray();
                        using (FileStream fs = new FileStream(tmpFileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            fs.Write(result, 0, result.Length);
                        }
                    }
                }

                targetLocation = ResolveDownloadLocation(Path.Combine(_persistentDataPath, "Coffee"));
            }

            Status = JobStatus.Succeeded;
        }
        catch (WebException e)
        {

#if DEBUG
            string errStr = e.Status.ToString();
            Console.Error.WriteLine(string.Format("Download Network Error: {0}", errStr));
#endif
            //5.2 failed
            if (e.Status == WebExceptionStatus.Timeout)
            {
                //...

            }
            Status = JobStatus.Failed;
        }
    }

	protected override void ThreadFunction()
    {
        Download();
	}
}
