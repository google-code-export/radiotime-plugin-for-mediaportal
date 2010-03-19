using System;
using System.Collections.Generic;
using System.Text;
using RadioTimeOpmlApi;
using MediaPortal.Configuration;

namespace RadioTimePlugin
{
  public enum PlayerType
  {
    Audio = 0,
    Video = 1,
    Unknow = 2
  }

  public class Settings : RadioTimeSetting
  {

    static public RadioTimeStation NowPlayingStation { get; set; }
    static public RadioTimeNowPlaying NowPlaying { get; set; }
    static public string GuideId { get; set; }
    static public string GuideIdDescription { get; set; }
    public Dictionary<string, PlayerType> FormatPlayer { get; set; }
    public Dictionary<string, string> FormatNames { get; set; }
    public List<string> SearchHistory { get; set; }
    public List<string> ArtistSearchHistory { get; set; }
 
    private string _password;
    public new string Password
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
      FormatPlayer = new Dictionary<string, PlayerType>();
      FormatPlayer.Add("wma", PlayerType.Video);
      FormatPlayer.Add("mp3", PlayerType.Audio);
      FormatPlayer.Add("aac", PlayerType.Audio);
      FormatPlayer.Add("real", PlayerType.Video);
      FormatPlayer.Add("flash", PlayerType.Video);
      FormatPlayer.Add("html", PlayerType.Unknow);
      FormatPlayer.Add("wmpro", PlayerType.Audio);
      FormatPlayer.Add("wmvoice", PlayerType.Audio);
      FormatPlayer.Add("wmvideo", PlayerType.Video);
      FormatPlayer.Add("ogg", PlayerType.Audio);
      FormatPlayer.Add("qt", PlayerType.Video);

      FormatNames = new Dictionary<string, string>();
      FormatNames.Add("wma", "WMAudio v8/9/10");
      FormatNames.Add("mp3", "standard MP3");
      FormatNames.Add("aac", "AAC and AAC+");
      FormatNames.Add("real", "Real Media");
      FormatNames.Add("flash", "RTMP (usually MP3 or AAC encoded)");
      FormatNames.Add("html", "Usually desktop players");
      FormatNames.Add("wmpro", "Windows Media Professional");
      FormatNames.Add("wmvoice", "Windows Media Voice");
      FormatNames.Add("wmvideo", "Windows Media Video v8/9/10");
      FormatNames.Add("ogg", "Ogg Vorbis");
      FormatNames.Add("qt", "Quicktime");

      SearchHistory = new List<string>();
      ArtistSearchHistory = new List<string>();
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

        string s = "";
        foreach (string history in SearchHistory)
        {
          s += history + "|";
        }
        xmlwriter.SetValue("radiotime", "searchHistory", s);

        s = "";
        foreach (string history in ArtistSearchHistory)
        {
          s += history + "|";
        }
        xmlwriter.SetValue("radiotime", "artistSearchHistory", s);

      }
    }

    public void Load()
    {
      using (
        MediaPortal.Profile.Settings xmlreader =
          new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        Mp3 = xmlreader.GetValueAsBool("radiotime", "mp3", true);
        Wma = xmlreader.GetValueAsBool("radiotime", "wma", true);
        Real = xmlreader.GetValueAsBool("radiotime", "real", false);
        ShowPresets = xmlreader.GetValueAsBool("radiotime", "showpresets", false);
        UseVideo = xmlreader.GetValueAsBool("radiotime", "UseVideo", false);
        User = xmlreader.GetValueAsString("radiotime", "user", string.Empty);
        Password = xmlreader.GetValueAsString("radiotime", "password", string.Empty);
        PluginName = xmlreader.GetValueAsString("radiotime", "pluginname", "RadioTime");

        SearchHistory.Clear();
        ArtistSearchHistory.Clear();
        string searchs = xmlreader.GetValueAsString("radiotime", "searchHistory", "");
        if (!string.IsNullOrEmpty(searchs))
        {
          string[] array = searchs.Split('|');
          for (int i = 0; i < array.Length && i < 25; i++)
          {
            if (!string.IsNullOrEmpty(array[i]))
              SearchHistory.Add(array[i]);
          }
        }

        searchs = xmlreader.GetValueAsString("radiotime", "artistSearchHistory", "");
        if (!string.IsNullOrEmpty(searchs))
        {

          string[] array = searchs.Split('|');
          for (int i = 0; i < array.Length && i < 25; i++)
          {
            if (!string.IsNullOrEmpty(array[i]))
              ArtistSearchHistory.Add(array[i]);
          }

        }
        PartnerId = "41";
      }
    }
  }
}
