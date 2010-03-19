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
    private RadioTimeStation _station = null;
    private RadioTimeNowPlaying _nowPlaying = null;
    private string _currentFileName = string.Empty;
    private RadioTimeOutline _currentItem = null;

    public BaseGui()
    {
      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      g_Player.PlayBackChanged += g_player_PlayBackChanged;
      g_Player.PlayBackEnded += g_player_PlayBackEnded;
      g_Player.PlayBackStopped += g_Player_PlayBackStopped;
    }

    protected virtual void doAdditionalStuffOnStarted()
    {
    }

    protected void g_player_PlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
    {
      //ClearInternalVariables();
      ClearProps();
    }

    protected void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      ClearInternalVariables();
      ClearProps();
    }

    protected void g_player_PlayBackEnded(g_Player.MediaType type, string filename)
    {
      ClearInternalVariables();
      ClearProps();
    }

    protected void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (_currentItem == null || _nowPlaying == null || _station == null)
        return;

      if (g_Player.CurrentFile == _currentFileName || string.IsNullOrEmpty(g_Player.CurrentFile))
      {
        Settings.NowPlaying = _nowPlaying.Clone();
        Settings.NowPlayingStation = _station.Clone();
        
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", DownloadStationLogo(_currentItem));

        GUIPropertyManager.SetProperty("#RadioTime.Play.Station", _nowPlaying.Name);
        //GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", GetStationLogoFileName(nowPlaying.Image));
        GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", _nowPlaying.Duration.ToString());
        GUIPropertyManager.SetProperty("#RadioTime.Play.Description", _nowPlaying.Description);
        GUIPropertyManager.SetProperty("#duration", ToMinutes(_nowPlaying.Duration.ToString()));
        GUIPropertyManager.SetProperty("#RadioTime.Play.Location", _nowPlaying.Location);
        GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", _station.Slogan);
        GUIPropertyManager.SetProperty("#RadioTime.Play.Language", _station.Slogan);

        string titleString = _nowPlaying.Name;
        if (!string.IsNullOrEmpty(_nowPlaying.Description))
          if (!string.IsNullOrEmpty(titleString))
            titleString = titleString + " / " + _nowPlaying.Description;
          else
            titleString = _nowPlaying.Description;
        if (!string.IsNullOrEmpty(_nowPlaying.Location))
          if (!string.IsNullOrEmpty(titleString))
            titleString = titleString + " / " + _nowPlaying.Location;
          else
            titleString = _nowPlaying.Location;

        if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Album").Trim()))
          GUIPropertyManager.SetProperty("#Play.Current.Album", titleString);
        //_nowPlaying.Name + "/" + _nowPlaying.Description + "/" + _nowPlaying.Location);

        if (_setting.FormatNames.ContainsKey(_currentItem.Formats))
          GUIPropertyManager.SetProperty("#RadioTime.Play.Format", _setting.FormatNames[_currentItem.Formats]);
        else
          GUIPropertyManager.SetProperty("#RadioTime.Play.Format", " ");

        GUIPropertyManager.SetProperty("#RadioTime.Play.Image", DownloadStationLogo(_currentItem));

        doAdditionalStuffOnStarted();

        ClearInternalVariables();
      }
    }

    protected void ClearInternalVariables()
    {
      _currentFileName = string.Empty;
      _currentItem = null;
      _nowPlaying = null;
      _station = null;
    }

    protected void ClearProps()
    {
      Settings.NowPlaying = new RadioTimeNowPlaying();
      Settings.NowPlayingStation = new RadioTimeStation();
      GUIPropertyManager.SetProperty("#RadioTime.Play.Station", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Description", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Location", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Language", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Format", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Image", " ");
    }

    protected void ClearPlayProps()
    {
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", " ");
      GUIPropertyManager.SetProperty("#Play.Current.Artist", " ");
      GUIPropertyManager.SetProperty("#Play.Current.Title", " ");
      GUIPropertyManager.SetProperty("#Play.Current.Track", " ");
      GUIPropertyManager.SetProperty("#Play.Current.Album", " ");
      GUIPropertyManager.SetProperty("#Play.Current.Year", " ");
      GUIPropertyManager.SetProperty("#Play.Current.Rating", "0");
    }

    public void OnDownloadTimedEvent(object source, ElapsedEventArgs e)
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

      _currentItem = item.Clone();

      //RadioTimeStation station = Settings.NowPlayingStation;
      _station = new RadioTimeStation();
      _station.Grabber = grabber;
      _station.Get(item.GuidId);
      
      if (!_station.IsAvailable)
      {
        GUIWaitCursor.Hide();
        Err_message(Translation.StationNotAvaiable);
        return;
      }
      //var nowPlaying = Settings.NowPlaying;
      _nowPlaying = new RadioTimeNowPlaying();
      _nowPlaying.Grabber = grabber;
      _nowPlaying.Get(item.GuidId);

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
          //string s = Path.GetTempFileName();
          //client.DownloadFile(playList[0].FileName, s);
          IPlayListIO loader1 = new PlayListPLSEIO();
          loader1.Load(playList, playList[0].FileName);
          //File.Delete(s);
        }

        if (playList[0].FileName.ToLower().Contains(".m3u"))
        {
          IPlayListIO loader1 = new PlayListM3uIO();
          string files = playList[0].FileName;
          loader1.Load(playList, playList[0].FileName);
          if (playList.Count == 0)
          {
            IPlayListIO loader2 = new PlayListPLSEIO();
            loader2.Load(playList, files);
          }
        }

        _currentFileName = playList[0].FileName;
        switch (playerType)
        {
          case PlayerType.Audio:
            ClearPlayProps();
            g_Player.PlayAudioStream(_currentFileName);
            break;
          case PlayerType.Video:
            // test if the station have tv group
            ClearPlayProps();
            if (item.GenreId == "g260" || item.GenreId == "g83" || item.GenreId == "g374" || item.GenreId == "g2769")
              g_Player.PlayVideoStream(_currentFileName);
            else
              g_Player.Play(_currentFileName, g_Player.MediaType.Unknown);
            break;
          case PlayerType.Unknow:
            break;
          default:
            break;
        }

        // moved to PLAYBACKSTARTED EVENT
        //if  (isPlaying && g_Player.CurrentFile == playList[0].FileName)

        GUIWaitCursor.Hide();
      }
      catch (Exception exception)
      {
        _currentItem = null; 
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

    public void LoadLocalPresetStations()
    {
      for (int i = 0; i < 10; i++)
      {
        if (_setting.PresetStations.Count < i + 1)
          _setting.PresetStations.Add(new RadioTimeStation());

        if (_setting.PresetIds.Count > i )
        {
          _station = _setting.PresetStations[i];
          _station.Grabber = grabber;
          _station.Get(_setting.PresetIds[i]);
        }
      }
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
