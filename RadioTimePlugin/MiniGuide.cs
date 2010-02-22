using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using RadioTimeOpmlApi;

namespace RadioTimePlugin
{
  public class MiniGuide : BaseGui, IRenderLayer
  {

    // Member variables                                  
    [SkinControl(34)]
    protected GUIButtonControl cmdExit = null;
    [SkinControl(35)]
    protected GUIListControl lstChannelsNoStateIcons = null;
    [SkinControl(36)]
    protected GUISpinControl spinGroup = null;
    [SkinControl(37)]
    protected GUIListControl lstChannelsWithStateIcons = null;

    private bool _canceled = false;
    private bool _running = false;
    private int _parentWindowID = 0;
    private GUIWindow _parentWindow = null;

    public string GuidId { get; set; }
    public RadioTime Grabber { get; set; }

    public MiniGuide()
    {
      GetID = 25651;
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = true;
      updateStationLogoTimer.Elapsed += new ElapsedEventHandler(OnDownloadTimedEvent);
      Client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Client_DownloadFileCompleted);
    }

    void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        File.Copy(Path.GetTempPath() + @"\station.png", curentDownlodingFile.FileName, true);
      }
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\RadioTimeMiniGuide.xml");

     // GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      //_canceled = true;
      //LoadSettings();
      return bResult;
    }

    protected override void OnPageLoad()
    {
      
      Log.Debug("RadioTimeMiniGuide: onpageload");


      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      //GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      AllocResources();
      ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
      
      //FillChannelList();
      FillList();
      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      Log.Debug("RadioTimeMiniGuide: OnPageDestroy");
      base.OnPageDestroy(newWindowId);
      _running = false;
    }

    private void FillList()
    {
      string url = string.Format("http://opml.radiotime.com/Browse.ashx?c=schedule&id={0}", GuidId);
      Grabber.GetData(url, true, false);
      foreach (var body in Grabber.Body)
      {
        GUIListItem item = new GUIListItem("");
        item.Label2 = ToMinutes(body.Duration);
        item.Label = body.Start;
        item.Label3 = body.Text;
        item.MusicTag = body;
        DownloadFile(body);
        item.OnRetrieveArt += item_OnRetrieveArt;
        lstChannelsWithStateIcons.Add(item);
      }
      if (lstChannelsWithStateIcons.GetID == 37)
      {
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, 37, 0, 0, null);
        OnMessage(msg);
      }

    }

    void item_OnRetrieveArt(GUIListItem item)
    {
      RadioTimeOutline tag = item.MusicTag as RadioTimeOutline;
      if (tag != null)
      {
        string file = GetLocalImageFileName(tag.Image.Replace("q.png", ".png"));
        if (File.Exists(file))
        {
          item.IconImageBig = file;
          item.IconImage = file;
        }
      }
    }
  

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (message.SenderControlId == 35 || message.SenderControlId == 37) // listbox
            {
              if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                //// switching logic
                //SelectedChannel = (Channel)lstChannels.SelectedListItem.MusicTag;

                //Channel changeChannel = null;
                //if (AutoZap)
                //{
                //  string selectedChan = (string)lstChannels.SelectedListItem.TVTag;
                //  if ((TVHome.Navigator.CurrentChannel != selectedChan) || g_Player.IsTVRecording)
                //  {
                //    List<Channel> tvChannelList = GetChannelListByGroup();
                //    if (tvChannelList != null)
                //    {
                //      changeChannel = (Channel)tvChannelList[lstChannels.SelectedListItemIndex];
                //    }
                //  }
                //}
                //_canceled = false;
                //Close();

                ////This one shows the zapOSD when changing channel from mini GUIDE, this is currently unwanted.
                ///*
                //TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
                //if (TVWindow != null) TVWindow.UpdateOSD(changeChannel.Name);                
                //*/

                //TVHome.UserChannelChanged = true;

                //if (changeChannel != null)
                //{
                //  TVHome.ViewChannel(changeChannel);
                //}
              }
            }
            else if (message.SenderControlId == 36) // spincontrol
            {
              // switch group              
              //OnGroupChanged();
            }
            else if (message.SenderControlId == 34) // exit button
            {
              // exit
              Close();
              _canceled = true;
            }
            break;
          }
      }
      return base.OnMessage(message);
    }

    private string DownloadFile(RadioTimeOutline radioItem)
    {
      string localFile = GetLocalImageFileName(radioItem.Image.Replace("q.png", ".png"));
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(radioItem.Image))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, radioItem.Image.Replace("q.png", ".png")));
      }
      return localFile;
    }

    public void DoModal(int dwParentId)
    {
      //Log.Debug("TvMiniGuide: domodal");
      _parentWindowID = dwParentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowID);
      if (null == _parentWindow)
      {
        //Log.Debug("TvMiniGuide: parentwindow = null");
        _parentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _running = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }

      Close();
    }

    private void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        _running = false;
        _parentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      GUILayerManager.UnRegisterLayer(this);
    }

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      if (_running)
      {
        Render(timePassed);
      }
    }
  }
}
