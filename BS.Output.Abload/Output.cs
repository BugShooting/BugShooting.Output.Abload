namespace BS.Output.Abload
{

  public class Output: IOutput 
  {
    
    string name;
    string userName;
    string password;
    string fileName;
    string fileFormat;
    bool openItemInBrowser;
    bool copyItemUrl;

    public Output(string name, 
                  string userName,
                  string password, 
                  string fileName, 
                  string fileFormat,
                  bool openItemInBrowser,
                  bool copyItemUrl)
    {
      this.name = name;
      this.userName = userName;
      this.password = password;
      this.fileName = fileName;
      this.fileFormat = fileFormat;
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

    public string FileFormat
    {
      get { return fileFormat; }
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
