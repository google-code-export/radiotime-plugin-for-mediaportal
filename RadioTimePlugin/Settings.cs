using System;
using System.Collections.Generic;
using System.Text;
using RadioTimeOpmlApi;
using MediaPortal.Configuration;

namespace RadioTimePlugin
{
  public class Settings : RadioTimeSetting
  {
 
    private string _password;

    public string Password
    {
      get { return _password; }
      set { _password = value; }
    }

    private bool showpresets;

    public bool ShowPresets
    {
      get { return showpresets; }
      set { showpresets = value; }
    }

    public Settings()
      : base()
    {
    }

    private string pluginName;

    public string PluginName
    {
      get
      {
        if (!string.IsNullOrEmpty(pluginName))
        {
          return pluginName;
        }
        else
        {
          return "RadioTime";
        }
      }
      set { pluginName = value; }
    }

    private bool usevideo;

    public bool UseVideo
    {
      get { return usevideo; }
      set { usevideo = value; }
    }

    public void Save()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("radiotime", "mp3", Mp3);
        xmlwriter.SetValueAsBool("radiotime", "wma", Wma);
        xmlwriter.SetValueAsBool("radiotime", "real", Real);
        xmlwriter.SetValueAsBool("radiotime", "UseVideo", UseVideo);
        xmlwriter.SetValue("radiotime", "user", User);
        xmlwriter.SetValue("radiotime", "password", Password);
        xmlwriter.SetValueAsBool("radiotime", "showpresets", ShowPresets);
        xmlwriter.SetValue("radiotime", "pluginname", PluginName);
      }
    }

    public void Load()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        Mp3 = xmlreader.GetValueAsBool("radiotime", "mp3", true);
        Wma = xmlreader.GetValueAsBool("radiotime", "wma", true);
        Real = xmlreader.GetValueAsBool("radiotime", "real", false);
        ShowPresets = xmlreader.GetValueAsBool("radiotime", "showpresets", false);
        UseVideo = xmlreader.GetValueAsBool("radiotime", "UseVideo", false);
        User = xmlreader.GetValueAsString("radiotime", "user", string.Empty);
        Password = xmlreader.GetValueAsString("radiotime", "password", string.Empty);
        PluginName = xmlreader.GetValueAsString("radiotime", "pluginname", "RadioTime");
        PartnerId = "41";
      }
    }
  }
}
