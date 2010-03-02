using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Timers;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using RadioTimeOpmlApi;

namespace RadioTimePlugin
{
  public class BaseGui : GUIWindow
  {
    public Queue downloaQueue = new Queue();
    public WebClient Client = new WebClient();
    public Timer updateStationLogoTimer = new Timer(0.2 * 1000);
    public DownloadFileObject curentDownlodingFile;
    public RadioTime grabber = new RadioTime();
    public Settings _setting = new Settings();



    public  void OnDownloadTimedEvent(object source, ElapsedEventArgs e)
    {
      if (!Client.IsBusy && downloaQueue.Count > 0)
      {
        curentDownlodingFile = (DownloadFileObject)downloaQueue.Dequeue();
        Client.DownloadFileAsync(new Uri(curentDownlodingFile.Url), Path.GetTempPath() + @"\station.png");
      }
    }
    
    static public string GetLocalImageFileName(string strURL)
    {
      if (strURL == "")
        return string.Empty;
      string url = String.Format("radiotime-{0}.png", MediaPortal.Util.Utils.EncryptLine(strURL));
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url); ;
    }

    public string ToMinutes(string minutes)
    {
      if (string.IsNullOrEmpty(minutes))
        return " ";
      int min = 0;
      int.TryParse(minutes, out min);
      int hh = (min/3600);
      min = min - (hh*3600);
      int mm = (min/60);
      min = min - (mm*60);
      if (hh > 0)
        return string.Format("{0}:{1}:{2}", hh.ToString(), mm.ToString("00"), min.ToString("00"));
      return string.Format("{0}:{1}", mm.ToString("00"), min.ToString("00"));
    }

    /// <summary>
    /// Does the play.
    /// </summary>
    /// <param name="item">The item.</param>
    public void DoPlay(RadioTimeOutline item)
    {
      GUIWaitCursor.Show();
      RadioTimeStation station = Settings.NowPlayingStation;
      station.Grabber = grabber;
      station.Get(item.GuidId);
      if (!station.IsAvailable)
      {
        Err_message(Translation.StationNotAvaiable);
        return;
      }
      var nowPlaying = Settings.NowPlaying;
      nowPlaying.Grabber = grabber;
      nowPlaying.Get(item.GuidId);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", GetStationLogoFileName(item));

      PlayerType playerType = PlayerType.Video;
      if (_setting.FormatPlayer.ContainsKey(item.Formats))
        playerType = _setting.FormatPlayer[item.Formats];
      try
      {
        string TargetFile = Path.GetTempFileName();
        WebClient client = new WebClient();
        client.DownloadFile(item.Url, TargetFile);
        IPlayListIO loader = new PlayListM3uIO();
        PlayList playList = new PlayList();
        loader.Load(playList, TargetFile);
        File.Delete(TargetFile);

        if (playList[0].FileName.ToLower().Contains(".pls"))
        {
          string s = Path.GetTempFileName();
          client.DownloadFile(playList[0].FileName, s);
          IPlayListIO loader1 = new PlayListPLSIO();
          loader1.Load(playList, s);
          File.Delete(s);
        }

        switch (playerType)
        {
          case PlayerType.Audio:
            g_Player.PlayAudioStream(playList[0].FileName);
            break;
          case PlayerType.Video:
            // test if the station have tv group
            if (item.GenreId == "g260" || item.GenreId == "g83" || item.GenreId == "g374" || item.GenreId == "g2769")
              g_Player.PlayVideoStream(playList[0].FileName);
            else
              g_Player.Play(playList[0].FileName, g_Player.MediaType.Unknown);
            break;
          case PlayerType.Unknow:
            break;
          default:
            break;
        }
        GUIPropertyManager.SetProperty("#RadioTime.Play.Station", nowPlaying.Name);
        //GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", GetStationLogoFileName(nowPlaying.Image));
        GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", nowPlaying.Duration.ToString());
        GUIPropertyManager.SetProperty("#RadioTime.Play.Description", nowPlaying.Description);
        GUIPropertyManager.SetProperty("#duration", ToMinutes(nowPlaying.Duration.ToString()));
        GUIPropertyManager.SetProperty("#RadioTime.Play.Location", nowPlaying.Location);
        GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", station.Slogan);
        GUIPropertyManager.SetProperty("#RadioTime.Play.Language", station.Slogan);

        GUIPropertyManager.SetProperty("#Play.Current.Title",
                                       nowPlaying.Name + "/" + nowPlaying.Description + "/" + nowPlaying.Location);

        if (_setting.FormatNames.ContainsKey(item.Formats))
          GUIPropertyManager.SetProperty("#RadioTime.Play.Format", _setting.FormatNames[item.Formats]);
        else
          GUIPropertyManager.SetProperty("#RadioTime.Play.Format", " ");

        GUIPropertyManager.SetProperty("#RadioTime.Play.Image", DownloadStationLogo(item));
        GUIWaitCursor.Hide();
      }
      catch (Exception exception)
      {
        GUIWaitCursor.Hide();
        Err_message(string.Format(Translation.PlayError, exception.Message));
        return;
      }
    }

    public void Err_message(string langid)
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(Translation.Message);
        dlgOK.SetLine(1, langid);
        dlgOK.SetLine(2, "");
        dlgOK.DoModal(GetID);
      }
    }

    internal static void SetProperty(string property, string value)
    {
      if (property == null)
        return;


      //// If the value is empty always add a space
      //// otherwise the property will keep 
      //// displaying it's previous value
      if (String.IsNullOrEmpty(value))
        value = " ";

      GUIPropertyManager.SetProperty(property, value);
    }

    public string GetStationLogoFileName(RadioTimeOutline radioItem)
    {
      if (string.IsNullOrEmpty(radioItem.Image))
        return string.Empty;
      return Utils.GetCoverArtName(Thumbs.Radio, radioItem.Text);
    }


    public string DownloadStationLogo(RadioTimeOutline radioItem)
    {
      string localFile = GetStationLogoFileName(radioItem);
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(radioItem.Image))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, radioItem.Image.Replace("q.png", ".png")));
      }
      return localFile;
    }

  }
}
