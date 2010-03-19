using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using RadioTimeOpmlApi;

namespace RadioTimePlugin
{
  public class NowPlayingGUI:BaseGui
  {

    [SkinControl(166)]
    protected GUIListControl facadeGenres = null;
    [SkinControl(155)]
    protected GUIListControl facadeSimilar = null;

    public override int GetID
    {
      get
      {
        return 25652;
      }

      set
      {
      }
    }

    override protected void doAdditionalStuffOnStarted()
    {
      Refresh();
    }

    public NowPlayingGUI()
    {
      Settings _setting = new Settings();
      _setting.Load();
      grabber.Settings.User = _setting.User;
      grabber.Settings.Password = _setting.Password;
      grabber.Settings.PartnerId = _setting.PartnerId;
    }

    void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        File.Copy(Path.GetTempPath() + @"\station.png", curentDownlodingFile.FileName, true);
        
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", " ");
        GUIPropertyManager.SetProperty("#RadioTime.Play.Image", " ");
        
        Process();

        GUIPropertyManager.SetProperty("#Play.Current.Thumb", curentDownlodingFile.FileName);
        GUIPropertyManager.SetProperty("#RadioTime.Play.Image", curentDownlodingFile.FileName);
      }
    }

    public override void Process()
    {
      if (!g_Player.Playing)
      {
        facadeGenres.Visible = false;
        facadeSimilar.Visible = false;
      }
      else
      {
        facadeGenres.Visible = true;
        facadeSimilar.Visible = true;
      }

      if (facadeGenres.Count < 1)
        facadeGenres.Visible = false;
      if (facadeSimilar.Count < 1)
        facadeSimilar.Visible = false;
    }
    
    public override bool Init()
    {
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = true;
      updateStationLogoTimer.Elapsed -= new ElapsedEventHandler(OnDownloadTimedEvent);
      updateStationLogoTimer.Elapsed += new ElapsedEventHandler(OnDownloadTimedEvent);
      Client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Client_DownloadFileCompleted);

      // show the skin
      return Load(GUIGraphicsContext.Skin + @"\radiotimenowplaying.xml");
    }

    protected override void OnPageLoad()
    {
      g_Player.PlayBackEnded += g_player_PlayBackEndedNPGUI;
      g_Player.PlayBackStopped += g_Player_PlayBackStoppedNPGUI;

      Refresh();
      foreach (string name in Translation.Strings.Keys)
      {
        SetProperty("#RadioTime.Translation." + name + ".Label", Translation.Strings[name]);
      }
      base.OnPageLoad();
    }

    private void Refresh()
    {
      if (Settings.NowPlayingStation == null) return;

      facadeGenres.ListItems.Clear();
      GUIControl.ClearControl(GetID, facadeGenres.GetID);
      foreach (var station in Settings.NowPlayingStation.Genres)
      {
        GUIListItem listItem = new GUIListItem();
        listItem.MusicTag = station;
        listItem.Label = station.Text;
        facadeGenres.Add(listItem);
      }
      facadeSimilar.ListItems.Clear();
      GUIControl.ClearControl(GetID, facadeSimilar.GetID);
      foreach (var station in Settings.NowPlayingStation.Similar)
      {
        GUIListItem listItem = new GUIListItem();
        listItem.MusicTag = station;
        listItem.Label = station.Text;
        facadeSimilar.Add(listItem);
      }
   }

    protected override void OnPageDestroy(int new_windowId)
    {
      g_Player.PlayBackEnded -= g_player_PlayBackEndedNPGUI;
      g_Player.PlayBackStopped -= g_Player_PlayBackStoppedNPGUI;
      base.OnPageDestroy(new_windowId);
    }

    void g_Player_PlayBackStoppedNPGUI(g_Player.MediaType type, int stoptime, string filename)
    {
      GUIWindowManager.ShowPreviousWindow();
    }

    void g_player_PlayBackEndedNPGUI(g_Player.MediaType type, string filename)
    {
      GUIWindowManager.ShowPreviousWindow();
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_STOP)
        GUIWindowManager.ShowPreviousWindow();
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == facadeSimilar)
      {
        GUIListItem selectedItem = facadeSimilar.SelectedListItem;
        GUIWaitCursor.Show();
        if (selectedItem != null)
        {
          RadioTimeOutline radioItem = ((RadioTimeOutline) selectedItem.MusicTag);
          if (radioItem != null)
          {
            DoPlay(radioItem);
            Refresh();
          }
        }
        GUIWaitCursor.Hide();
      }

      if (control == facadeGenres)
      {
        GUIListItem selectedItem = facadeGenres.SelectedListItem;
        if (selectedItem != null)
        {
          RadioTimeOutline radioItem = ((RadioTimeOutline) selectedItem.MusicTag);
          if (radioItem != null)
          {
            Settings.GuideId = radioItem.GuidId;
            Settings.GuideIdDescription = Translation.Genres + ": " + selectedItem.Label;
            GUIWindowManager.ActivateWindow(25650);
          }
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

  }
}
