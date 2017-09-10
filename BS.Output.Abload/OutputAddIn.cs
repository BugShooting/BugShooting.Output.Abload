using System;
using System.Drawing;
using System.Windows.Forms;
using System.ServiceModel;
using System.Web;
using System.Threading.Tasks;

namespace BS.Output.Abload
{
  public class OutputAddIn: V3.OutputAddIn<Output>
  {

    protected override string Name
    {
      get { return "Abload"; }
    }

    protected override Image Image64
    {
      get  { return Properties.Resources.logo_64; }
    }

    protected override Image Image16
    {
      get { return Properties.Resources.logo_16 ; }
    }

    protected override bool Editable
    {
      get { return true; }
    }

    protected override string Description
    {
      get { return "Upload your screenshots to www.abload.de."; }
    }
    
    protected override Output CreateOutput(IWin32Window Owner)
    {
      
      Output output = new Output(Name, 
                                 String.Empty, 
                                 String.Empty, 
                                 "Screenshot",
                                 String.Empty, 
                                 true,
                                 true);

      return EditOutput(Owner, output);

    }

    protected override Output EditOutput(IWin32Window Owner, Output Output)
    {

      Edit edit = new Edit(Output);

      var ownerHelper = new System.Windows.Interop.WindowInteropHelper(edit);
      ownerHelper.Owner = Owner.Handle;
      
      if (edit.ShowDialog() == true) {

        return new Output(edit.OutputName,
                          edit.UserName,
                          edit.Password,
                          edit.FileName,
                          edit.FileFormat,
                          edit.OpenItemInBrowser,
                          edit.CopyItemUrl);
      }
      else
      {
        return null; 
      }

    }

    protected override OutputValueCollection SerializeOutput(Output Output)
    {

      OutputValueCollection outputValues = new OutputValueCollection();

      outputValues.Add(new OutputValue("Name", Output.Name));
      outputValues.Add(new OutputValue("UserName", Output.UserName));
      outputValues.Add(new OutputValue("Password",Output.Password, true));
      outputValues.Add(new OutputValue("OpenItemInBrowser", Convert.ToString(Output.OpenItemInBrowser)));
      outputValues.Add(new OutputValue("CopyItemUrl", Convert.ToString(Output.CopyItemUrl)));
      outputValues.Add(new OutputValue("FileName", Output.FileName));
      outputValues.Add(new OutputValue("FileFormat", Output.FileFormat));

      return outputValues;
      
    }

    protected override Output DeserializeOutput(OutputValueCollection OutputValues)
    {

      return new Output(OutputValues["Name", this.Name].Value,
                        OutputValues["UserName", ""].Value,
                        OutputValues["Password", ""].Value, 
                        OutputValues["FileName", "Screenshot"].Value, 
                        OutputValues["FileFormat", ""].Value,
                        Convert.ToBoolean(OutputValues["OpenItemInBrowser", Convert.ToString(true)].Value),
                        Convert.ToBoolean(OutputValues["CopyItemUrl", Convert.ToString(true)].Value));

    }

    protected override async Task<V3.SendResult> Send(IWin32Window Owner, Output Output, V3.ImageData ImageData)
    {

      try
      {

        string url = "http://www.abload.de";

        string userName = Output.UserName;
        string password = Output.Password;
        bool showLogin = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password);
        bool rememberCredentials = false;

        string fileName = V3.FileHelper.GetFileName(Output.FileName, Output.FileFormat, ImageData);

        while (true)
        {

          if (showLogin)
          {

            // Show credentials window
            Credentials credentials = new Credentials(url, userName, password, rememberCredentials);

            var credentialsOwnerHelper = new System.Windows.Interop.WindowInteropHelper(credentials);
            credentialsOwnerHelper.Owner = Owner.Handle;

            if (credentials.ShowDialog() != true)
            {
              return new V3.SendResult(V3.Result.Canceled);
            }

            userName = credentials.UserName;
            password = credentials.Password;
            rememberCredentials = credentials.Remember;

          }

          LoginResult loginResult = await AbloadProxy.Login(url, userName, password);
          if (!loginResult.Success)
          {
            showLogin = true;
            continue;
          }

          // Show send window
          Send send = new Send(url, fileName);

          var sendOwnerHelper = new System.Windows.Interop.WindowInteropHelper(send);
          sendOwnerHelper.Owner = Owner.Handle;

          if (!send.ShowDialog() == true)
          {
            return new V3.SendResult(V3.Result.Canceled);
          }

          string fullFileName = String.Format("{0}.{1}", send.FileName, V3.FileHelper.GetFileExtention(Output.FileFormat));
          string fileMimeType = V3.FileHelper.GetMimeType(Output.FileFormat);
          byte[] fileBytes = V3.FileHelper.GetFileBytes(Output.FileFormat, ImageData);

          UploadResult uploadResult = await AbloadProxy.Upload(url, loginResult.LoginSession,fullFileName, fileMimeType, fileBytes);
          if (!uploadResult.Success)
          {
            return new V3.SendResult(V3.Result.Failed, uploadResult.FailedMessage);
          }

          string imageUrl = String.Format("{0}/image.php?img={1}", url, uploadResult.ImageName);

          // Open item in browser
          if (Output.OpenItemInBrowser)
          {
            V3.WebHelper.OpenUrl(imageUrl);
          }

          // Copy item url
          if (Output.CopyItemUrl)
          {
            Clipboard.SetText(imageUrl);
          }
            
          return new V3.SendResult(V3.Result.Success,
                                    new Output(Output.Name,
                                              (rememberCredentials) ? userName : Output.UserName,
                                              (rememberCredentials) ? password : Output.Password,
                                              Output.FileName,
                                              Output.FileFormat,
                                              Output.OpenItemInBrowser,
                                              Output.CopyItemUrl));
        }

      }
      catch (Exception ex)
      {
        return new V3.SendResult(V3.Result.Failed, ex.Message);
      }

    }

  }
}
