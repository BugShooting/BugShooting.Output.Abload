using BS.Plugin.V3.Output;
using System;

namespace BugShooting.Output.Abload
{

  public class Output: IOutput 
  {
    
    string name;
    string userName;
    string password;
    string fileName;
    Guid fileFormatID;
    bool openItemInBrowser;
    bool copyItemUrl;

    public Output(string name, 
                  string userName,
                  string password, 
                  string fileName,
                  Guid fileFormatID,
                  bool openItemInBrowser,
                  bool copyItemUrl)
    {
      this.name = name;
      this.userName = userName;
      this.password = password;
      this.fileName = fileName;
      this.fileFormatID = fileFormatID;
      this.openItemInBrowser = openItemInBrowser;
      this.copyItemUrl = copyItemUrl;
    }
    
    public string Name
    {
      get { return name; }
    }

    public string Information
    {
      get { return string.Empty; }
    }
       
    public string UserName
    {
      get { return userName; }
    }

    public string Password
    {
      get { return password; }
    }
          
    public string FileName
    {
      get { return fileName; }
    }

    public Guid FileFormatID
    {
      get { return fileFormatID; }
    }

    public bool OpenItemInBrowser
    {
      get { return openItemInBrowser; }
    }

    public bool CopyItemUrl
    {
      get { return copyItemUrl; }
    }

  }
}
