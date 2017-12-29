using BS.Plugin.V3.Common;
using BS.Plugin.V3.Output;
using BS.Plugin.V3.Utilities;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BugShooting.Output.Abload
{
  public class OutputPlugin: OutputPlugin<Output>
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

    protected override OutputValues SerializeOutput(Output Output)
    {

      OutputValues outputValues = new OutputValues();

      outputValues.Add("Name", Output.Name);
      outputValues.Add("UserName", Output.UserName);
      outputValues.Add("Password",Output.Password, true);
      outputValues.Add("OpenItemInBrowser", Convert.ToString(Output.OpenItemInBrowser));
      outputValues.Add("CopyItemUrl", Convert.ToString(Output.CopyItemUrl));
      outputValues.Add("FileName", Output.FileName);
      outputValues.Add("FileFormat", Output.FileFormat);

      return outputValues;
      
    }

    protected override Output DeserializeOutput(OutputValues OutputValues)
    {

      return new Output(OutputValues["Name", this.Name],
                        OutputValues["UserName", ""],
                        OutputValues["Password", ""], 
                        OutputValues["FileName", "Screenshot"], 
                        OutputValues["FileFormat", ""],
                        Convert.ToBoolean(OutputValues["OpenItemInBrowser", Convert.ToString(true)]),
                        Convert.ToBoolean(OutputValues["CopyItemUrl", Convert.ToString(true)]));

    }

    protected override async Task<SendResult> Send(IWin32Window Owner, Output Output, ImageData ImageData)
    {

      try
      {

        string url = "http://www.abload.de";

        string userName = Output.UserName;
        string password = Output.Password;
        bool showLogin = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password);
        bool rememberCredentials = false;

        string fileName = AttributeHelper.ReplaceAttributes(Output.FileName, ImageData);

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
              return new SendResult(Result.Canceled);
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
            return new SendResult(Result.Canceled);
          }

          string fullFileName = String.Format("{0}.{1}", send.FileName, FileHelper.GetFileExtention(Output.FileFormat));
          string fileMimeType = FileHelper.GetMimeType(Output.FileFormat);
          byte[] fileBytes = FileHelper.GetFileBytes(Output.FileFormat, ImageData);

          UploadResult uploadResult = await AbloadProxy.Upload(url, loginResult.LoginSession,fullFileName, fileMimeType, fileBytes);
          if (!uploadResult.Success)
          {
            return new SendResult(Result.Failed, uploadResult.FailedMessage);
          }

          string imageUrl = String.Format("{0}/image.php?img={1}", url, uploadResult.ImageName);

          // Open item in browser
          if (Output.OpenItemInBrowser)
          {
            WebHelper.OpenUrl(imageUrl);
          }

          // Copy item url
          if (Output.CopyItemUrl)
          {
            Clipboard.SetText(imageUrl);
          }
            
          return new SendResult(Result.Success,
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
        return new SendResult(Result.Failed, ex.Message);
      }

    }

  }
}
