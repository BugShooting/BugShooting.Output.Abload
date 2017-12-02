using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace BugShooting.Output.Abload
{
  partial class Send : Window
  {
 
    public Send(string url, string fileName)
    {
      InitializeComponent();

      Url.Text = url;
      FileNameTextBox.Text = fileName;
      
      FileNameTextBox.TextChanged += ValidateData;
      ValidateData(null, null);

    }
    
    public string FileName
    {
      get { return FileNameTextBox.Text; }
    }

    private void ValidateData(object sender, EventArgs e)
    {
      OK.IsEnabled = Validation.IsValid(FileNameTextBox);
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

  }

  internal class ProjectItem
  {
    
    private string projectID;
    private string fullName;

    public ProjectItem(string projectID, string fullName)
    {
      this.projectID = projectID;
      this.fullName = fullName;
    }

    public string ProjectID
    {
      get { return projectID; }
    }

    public override string ToString()
    {
      return fullName;
    }

  }

}
