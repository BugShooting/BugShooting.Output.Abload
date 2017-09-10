using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;

namespace BS.Output.Abload
{

  internal class AbloadProxy
  {

    static internal async Task<LoginResult> Login(string url, string userName, string password)
    {

      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/login?name={1}&password={2}", url, HttpUtility.UrlEncode(userName), HttpUtility.UrlEncode(password)));
      request.Method = "GET";

      using (WebResponse response = await request.GetResponseAsync())
      { 
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {

          string responseData = await reader.ReadToEndAsync();

          XmlDocument xmlDoc = new XmlDocument();
          xmlDoc.LoadXml(responseData);

          XmlNodeList loginNodes = xmlDoc.GetElementsByTagName("login");

          if (loginNodes.Count == 0)
          {
            return new LoginResult(false, null);
          }

          return new LoginResult(true, loginNodes[0].Attributes["session"].Value);

        }

      }
    }
    
    static internal async Task<UploadResult> Upload(string url, string loginSession, string fullFileName, string fileMimeType, byte[] fileBytes)
    {

      try
      {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("session", loginSession);

        string resultData = await SendFile(String.Format("{0}/api/upload", url), parameters, fullFileName, fileMimeType, fileBytes);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(resultData);

        string imagename = xmlDoc.GetElementsByTagName("image")[0].Attributes["newname"].Value;

        return new UploadResult(true, imagename, null);

      }
      catch (WebException ex) when (ex.Response is HttpWebResponse)
      {

        HttpWebResponse response = (HttpWebResponse)ex.Response;

        return new UploadResult(true, null, response.StatusDescription);

      }

    }

    private static async Task<string> SendFile(string url, Dictionary<string, string> parameters, string fullFileName, string fileMimeType, byte[] fileBytes)
    {

      string boundary = String.Format("----------{0}", DateTime.Now.Ticks.ToString("x"));
      
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
      request.Method = "POST";
      request.ContentType =String.Format("multipart/form-data; boundary={0}", boundary);

      StringBuilder postData = new StringBuilder();

      foreach (string key in parameters.Keys)
      {
        postData.AppendFormat("--{0}",boundary);
        postData.AppendLine();
        postData.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n", key);
        postData.AppendLine();
        postData.AppendFormat("{0}\r\n", parameters[key]);
      }

      postData.AppendFormat("--{0}", boundary);
      postData.AppendLine();
      postData.AppendFormat("Content-Disposition: file; name=\"img0\"; filename=\"{0}\"\r\n", fullFileName);
      postData.AppendFormat("Content-Type: {0}\r\n", fileMimeType);
      postData.AppendLine();

      byte[] postBytes = Encoding.UTF8.GetBytes(postData.ToString());
      byte[] boundaryBytes = Encoding.ASCII.GetBytes(String.Format("\r\n--{0}\r\n", boundary));
      
      request.ContentLength = postBytes.Length + fileBytes.Length + boundaryBytes.Length;

      using (Stream requestStream = await request.GetRequestStreamAsync())
      {
        requestStream.Write(postBytes, 0, postBytes.Length);
        requestStream.Write(fileBytes, 0, fileBytes.Length);
        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
        requestStream.Close();
      }

      using (WebResponse response = await request.GetResponseAsync())
      { 
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
          return await reader.ReadToEndAsync();
        }
      }

    }

  }

  internal class LoginResult
  {

    bool success;
    string loginSession;

    public LoginResult(bool success,
                       string loginSession)
    {
      this.success = success;
      this.loginSession = loginSession;
    }

    public bool Success
    {
      get { return success; }
    }

    public string LoginSession
    {
      get { return loginSession; }
    }
    
  }

  internal class UploadResult
  {

    bool success;
    string imageName;
    string failedMessage;

    public UploadResult(bool success,
                       string imageName,
                       string failedMessage)
    {
      this.success = success;
      this.imageName = imageName;
      this.failedMessage = failedMessage;
    }

    public bool Success
    {
      get { return success; }
    }

    public string ImageName
    {
      get { return imageName; }
    }

    public string FailedMessage
    {
      get { return failedMessage; }
    }

  }


}
