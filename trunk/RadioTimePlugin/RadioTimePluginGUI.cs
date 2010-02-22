using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Timers;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Localisation;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Playlists;
using RadioTimeOpmlApi;
using RadioTimeOpmlApi.com.radiotime.services;

namespace RadioTimePlugin
{
  public class RadioTimePluginGUI : BaseGui, ISetupForm
  {

    #region MapSettings class
    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _SortAscending;

      public MapSettings()
      {
        // Set default view
        _SortBy = 0;
        _ViewAs = (int)View.List;
        _SortAscending = true;
      }

      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy; }
        set { _SortBy = value; }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }
    }
    #endregion

    #region Base variables

    #endregion

    enum View
    {
      List = 0,
      Icons = 1,
      BigIcons = 2,
      Albums = 3,
      Filmstrip = 4,
    }



    #region locale vars

    private RadioTime grabber = new RadioTime();
    private RadioTimeWebService websrv = new RadioTimeWebService();
    private Settings _setting = new Settings();
    private Identification iden = new Identification();
    MapSettings mapSettings = new MapSettings();
    private int oldSelection = 0;
    private StationSort.SortMethod curSorting = StationSort.SortMethod.name;

    #endregion

    #region skin connection
    [SkinControlAttribute(50)]
    protected GUIFacadeControl listControl = null;
    [SkinControlAttribute(2)]
    protected GUISortButtonControl sortButton = null;
    [SkinControlAttribute(4)]
    protected GUIButtonControl homeButton = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnSwitchView = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl searchButton = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl presetsButton = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl searchArtistButton = null;

    [SkinControlAttribute(51)]
    protected GUIImage logoImage = null;
    #endregion

    public RadioTimePluginGUI()
    {
      GetID = GetWindowId();
      updateStationLogoTimer.AutoReset = true;
      updateStationLogoTimer.Enabled = false;
      updateStationLogoTimer.Elapsed += OnDownloadTimedEvent;
      Client.DownloadFileCompleted += DownloadLogoEnd;
      g_Player.PlayBackStopped += g_Player_PlayBackStopped;
      _setting.Load();
      iden.UserName = _setting.User;
      iden.PasswordKey = RadioTimeWebServiceHelper.HashMD5(_setting.Password);
      iden.PartnerId = "MediaPortal";
      iden.PartnerKey = "NVNxA8N$6VD1";
    }

    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      ClearProps();
    }

    void ClearProps()
    {
      GUIPropertyManager.SetProperty("#RadioTime.Play.Station", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Description", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Play.Location", " "); 
    }
    
    #region ISetupForm Members
    // return name of the plugin
    public string PluginName()
    {
      return "RadioTime";
    }
    // returns plugin description
    public string Description()
    {
      return "RadioTime";
    }
    // returns author
    public string Author()
    {
      return "Dukus";
    }
    // shows the setup dialog
    public void ShowPlugin()
    {
      SetupForm setup = new SetupForm();
      setup.ShowDialog();
    }
    // enable / disable
    public bool CanEnable()
    {
      return true;
    }
    // returns the unique id again
    public int GetWindowId()
    {
      return 25650;
    }
    // default enable?
    public bool DefaultEnabled()
    {
      return true;
    }
    // has setup gui?
    public bool HasSetup()
    {
      return true ;
    }
    // home button
    public bool GetHome(out string strButtonText, out string strButtonImage,
      out string strButtonImageFocus, out string strPictureImage)
    {
      // set the values for the buttom
      strButtonText = _setting.PluginName;

      // no image or picture
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;

      return true;
    }
    // init the skin
    public override bool Init()
    {
      // show the skin
      return Load(GUIGraphicsContext.Skin + @"\radiotime.xml");
    }
     //do the init before page load
    protected override void OnPageLoad()
    {
      updateStationLogoTimer.Enabled = true;
      if (grabber.Body.Count < 1)
      {
        if (_setting.ShowPresets)
        {
          grabber.GetData(_setting.PresetsUrl, false, false);
        }
        else
        {
          Log.Info("RadioTime page loading :{0}", _setting.StartupUrl);
          grabber.GetData(_setting.StartupUrl);
        }
      }
      UpdateList();
      listControl.SelectedItem = oldSelection;
      GUIPropertyManager.SetProperty("#header.label", " ");
      GUIPropertyManager.SetProperty("#RadioTime.Selected.NowPlaying", " ");
      if (sortButton != null)
      {
        sortButton.SortChanged += SortChanged;
      }

      // set the sort button label
      switch (curSorting)
      {
        case StationSort.SortMethod.bitrate:
          sortButton.Label = Translation.SortByBitrate;
          break;
        case StationSort.SortMethod.name:
          sortButton.Label = Translation.SortByName;
          break;
      }
      
      foreach (string name in Translation.Strings.Keys)
      {
        SetProperty("#RadioTime.Translation." + name + ".Label", Translation.Strings[name]);
      }

      base.OnPageLoad();
    }
    // remeber the selection on page leave
    protected override void OnPageDestroy(int new_windowId)
    {
      oldSelection = listControl.SelectedItem;
      base.OnPageDestroy(new_windowId);
    }
    //// do the clicked action
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      ////
      //// look for button pressed
      ////
      //// record ?
      if (actionType == Action.ActionType.ACTION_RECORD)
      {
        //ExecuteRecord();
      }
      else if (control == btnSwitchView)
      {
        switch ((View)mapSettings.ViewAs)
        {
          case View.List:
            mapSettings.ViewAs = (int)View.Icons;
            break;
          case View.Icons:
            mapSettings.ViewAs = (int)View.BigIcons;
            break;
          case View.BigIcons:
            mapSettings.ViewAs = (int)View.Albums;
            break;
          case View.Albums:
            mapSettings.ViewAs = (int)View.Filmstrip;
            break;

          case View.Filmstrip:
            mapSettings.ViewAs = (int)View.List;
            break;
        }
        ShowPanel();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == listControl)
      {
        // execute only for enter keys
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          // station selected
          DoListSelection();
        }
      }
      else if (control == sortButton)
      {
        //sort button selected
        OnShowSortOptions();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == searchArtistButton)
      {
        DoSearchArtist();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == searchButton)
      {
        DoSearch();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == homeButton)
      {
        mapSettings.ViewAs = (int)View.List;
        ShowPanel();
        DoHome();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      else if (control == presetsButton)
      {
        DoPresets();
        GUIControl.FocusControl(GetID, listControl.GetID);
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void DoHome()
    {
      Log.Debug("RadioTime page loading :{0}", _setting.StartupUrl);
      grabber.Reset();
      grabber.GetData(_setting.StartupUrl);
      UpdateList();
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

    //// override action responses
    //// override action responses
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (listControl.Focus)
        {
          if (grabber.Parent != null && grabber.Parent.Body.Count > 0)
          {
            DoBack();
            return;
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = listControl[0];

        if ((item != null) && item.IsFolder && (item.Label == ".."))
        {
          DoBack();
          return;
        }
      }
      UpdateGui();
      base.OnAction(action);
    }
    // do regulary updates
    public override void Process()
    {
      // update the gui
      UpdateGui();
    }

    protected void OnShowSortOptions()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(Translation.Sorting); // Sort options

      dlg.Add(Translation.SortByName); // name
      dlg.Add(Translation.SortByBitrate); // bitrate
      // set the focus to currently used sort method
      dlg.SelectedLabel = (int)curSorting;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;

      if(dlg.SelectedLabelText==Translation.SortByBitrate)
      {
          curSorting = StationSort.SortMethod.bitrate;
          sortButton.Label = Translation.SortByBitrate;
      }
      else
      {
        curSorting = StationSort.SortMethod.name;
        sortButton.Label = Translation.SortByName;
      }

      sortButton.IsAscending = mapSettings.SortAscending;
      UpdateList();
    }
 
    #endregion
    #region helper func's

    private void DoPresets()
    {
      grabber.GetData(_setting.PresetsUrl,false,false);
      UpdateList();
    }

    private void DoListSelection()
    {
      GUIWaitCursor.Show();
      GUIListItem selectedItem = listControl.SelectedListItem;
      if (selectedItem != null)
      {
        if (selectedItem.Label != "..")
        {
          RadioTimeOutline radioItem = ((RadioTimeOutline)selectedItem.MusicTag);
          switch (radioItem.Type)
          {
            case RadioTimeOutline.OutlineType.link:
              grabber.GetData(radioItem.Url);
              UpdateList();
              break;
            case RadioTimeOutline.OutlineType.audio:
              DoPlay(radioItem);
              break;
            default:
              break;
          }
        }
        else
        {
          grabber.Prev();
          UpdateList();
        }
      }
      GUIWaitCursor.Hide();
      //throw new Exception("The method or operation is not implemented.");
    }

    /// <summary>
    /// Does the play.
    /// </summary>
    /// <param name="item">The item.</param>
    private void DoPlay(RadioTimeOutline item)
    {
      var nowPlaying = new RadioTimeNowPlaying();
      nowPlaying.Grabber = grabber;
      nowPlaying.Get(item.GuidId);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", GetStationLogoFileName(item));
      PlayerType playerType = PlayerType.Video;
      if (_setting.FormatPlayer.ContainsKey(item.Formats))
        playerType = _setting.FormatPlayer[item.Formats];
      switch (playerType)
      {
        case PlayerType.Audio:
          g_Player.PlayAudioStream(item.Url);
          break;
        case PlayerType.Video:
          g_Player.Play(item.Url);
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
      GUIPropertyManager.SetProperty("#RadioTime.Play.Location", nowPlaying.Location); 

      GUIPropertyManager.SetProperty("#Play.Current.Title", nowPlaying.Name + "/" + nowPlaying.Description + "/" + nowPlaying.Location);
    }

    private void DoSearchArtist()
    {
      string searchString = "";

      // display an virtual keyboard
      var keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = searchString;
      keyboard.DoModal(GetWindowId());
      if (keyboard.IsConfirmed)
      {
        // input confirmed -- execute the search
        searchString = keyboard.Text;
      }

      if ("" != searchString)
      {
        grabber.SearchArtist(searchString);
        UpdateList();
      }
    }

    private void DoSearch()
    {
      string searchString = "";

      // display an virtual keyboard
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard) return;
      keyboard.Reset();
      keyboard.Text = searchString;
      keyboard.DoModal(GetWindowId());
      if (keyboard.IsConfirmed)
      {
        // input confirmed -- execute the search
        searchString = keyboard.Text;
      }

      if ("" != searchString)
      {
        grabber.Search(searchString);
        UpdateList();
      }
    }
    
    private void DoBack()
    {
      if (grabber.Parent != null)
      {
        grabber.Prev();
        UpdateList();
      }
      else
      {
        GUIWindowManager.ShowPreviousWindow();
      }
    }
    
    public void UpdateList()
    {
      updateStationLogoTimer.Enabled = false;
      downloaQueue.Clear();
      GUIControl.ClearControl(GetID, listControl.GetID);
      if (grabber.Parent != null && grabber.Parent.Body.Count > 0)
      {
        GUIListItem item = new GUIListItem();
        // and add station name & bitrate
        item.Label = "..";
        item.Label2 = "(" + grabber.Parent.Body.Count.ToString() + ")";
        item.IsFolder = true;
        item.IconImage = "defaultFolderBack.png";
        //item.MusicTag = head;
        listControl.Add(item);
      }
      foreach (RadioTimeOutline body in grabber.Body)
      {
        GUIListItem item = new GUIListItem();
        // and add station name & bitrate
        item.Label = body.Text;
        item.Label2 = body.Bitrate;
        item.ThumbnailImage = GetStationLogoFileName(body);
        item.IconImage = GetStationLogoFileName(body);
        item.IsFolder = false;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        item.MusicTag = body;
        listControl.Add(item);
        DownloadStationLogo(body);
        switch (body.Type)
        {
          case RadioTimeOutline.OutlineType.audio:
            if (string.IsNullOrEmpty(item.IconImage))
              item.IconImage = "DefaultMyradio.png";
            item.IsFolder = false;
            break;
          case RadioTimeOutline.OutlineType.link:
            if (string.IsNullOrEmpty(item.IconImage))
              item.IconImage = "defaultFolderNF.png";
            item.IsFolder = true;
            break;
          case RadioTimeOutline.OutlineType.unknow:
            break;
          default:
            break;
        }
      }
      updateStationLogoTimer.Enabled = true;
      listControl.Sort(new StationSort(curSorting, mapSettings.SortAscending));
      ShowPanel();
    }

    void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      listControl.FilmstripView.InfoImageFileName = item.ThumbnailImage;
    }

    void ShowPanel()
    {
      int itemIndex = listControl.SelectedListItemIndex;
      if (mapSettings.ViewAs == (int)View.BigIcons)
      {
        listControl.View = GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.Albums)
      {
        listControl.View = GUIFacadeControl.ViewMode.AlbumView;
      }
      else if (mapSettings.ViewAs == (int)View.Icons)
      {
        listControl.View = GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (mapSettings.ViewAs == (int)View.List)
      {
        listControl.View = GUIFacadeControl.ViewMode.List;
      }
      else if (mapSettings.ViewAs == (int)View.Filmstrip)
      {
        listControl.View = GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (itemIndex > -1)
      {
        GUIControl.SelectItemControl(GetID, listControl.GetID, itemIndex);
      }
     
    }

    protected override void OnShowContextMenu()
    {    
     GUIListItem selectedItem = listControl.SelectedListItem;
     if (selectedItem != null)
     {
       //if (selectedItem.IsFolder)
       //  return;
       try
       {
         GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
         if (dlg == null)
           return;
         dlg.Reset();
         dlg.SetHeading(498); // menu
         TuneResponse resp = new TuneResponse();
         TuneRequest req = new TuneRequest();
         try
         {
           req.StationId = ((RadioTimeOutline)selectedItem.MusicTag).StationIdAsInt;
           req.Identification = iden;
           req.Settings = new RadioTimeOpmlApi.com.radiotime.services.Settings();
           Log.Debug("[Radiotime]Geting info for id {0}", req.StationId.ToString());
           resp = websrv.Tuner_Tune(req);
           if (!resp.TunerResponseXmlView.Station.IsFavorite)
           {
             dlg.Add(Translation.RemoveFromFavorites);
           }
         }
         catch
         {
         }
           //add to favoritest
         dlg.Add("Show guid");
         dlg.Add(Translation.AddToFavorites);
         dlg.DoModal(GetID);
         if (dlg.SelectedId == -1)
           return;
         if (dlg.SelectedLabelText == Translation.AddToFavorites)
           AddToFavorites(req.StationId);
         if (dlg.SelectedLabelText == Translation.RemoveFromFavorites)
           RemoveFavorites(req.StationId); ;
         if (dlg.SelectedLabelText == "Show guid")
         {
           MiniGuide miniGuide = (MiniGuide) GUIWindowManager.GetWindow(25651);
           miniGuide.GuidId = ((RadioTimeOutline)selectedItem.MusicTag).GuidId;
           miniGuide.Grabber = grabber;
           miniGuide.DoModal(GetID);
         }
       }
       catch(System.Web.Services.Protocols.SoapException ex)
       {
         Log.Error("[RadioTime] Comunication error or wrong user name or password ");
         Log.Error(ex);
       }
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
    /// <summary>
    /// Removes the favorites.
    /// </summary>
    /// <param name="p">The Station id.</param>
    private void RemoveFavorites(int p)
    {
      Err_message(Translation.UseWebInterfaceToEditFavorites);
    }

    /// <summary>
    /// Adds to favorites.
    /// </summary>
    /// <param name="p">The station id.</param>
    private void AddToFavorites(int p)
    {
      try
      {
        FavoriteFolderUpdateRequest req = new FavoriteFolderUpdateRequest();
        req.ItemIds = new int[] { p };
        req.Identification = iden;
        websrv.Favorite_StationListAdd(req);
        grabber.GetData(grabber.CurentUrl, grabber.CacheIsUsed, false);
        UpdateList();
      }
      catch (Exception)
      {
        Err_message(Translation.ComunicationError);
      }
    }

    public void UpdateGui()
    {
      GUIListItem selectedItem = listControl.SelectedListItem;
      if (selectedItem != null)
      {
        RadioTimeOutline radioItem = ((RadioTimeOutline)selectedItem.MusicTag);
        if (radioItem != null && !string.IsNullOrEmpty(radioItem.Image))
        {
          logoImage.SetFileName(DownloadStationLogo(radioItem));
          GUIPropertyManager.SetProperty("#RadioTime.Selected.NowPlaying", radioItem.CurrentTrack);
          GUIPropertyManager.SetProperty("#RadioTime.Selected.Subtext", radioItem.Subtext);
          if (_setting.FormatNames.ContainsKey(radioItem.Formats))
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", _setting.FormatNames[radioItem.Formats]);
          else
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", " ");
        }
        else
        {
          logoImage.SetFileName(string.Empty);
          GUIPropertyManager.SetProperty("#RadioTime.Selected.NowPlaying", " ");
          GUIPropertyManager.SetProperty("#RadioTime.Selected.Subtext", " ");
          GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", " ");
        }
      }

      string textLine = string.Empty;
      View view = (View)mapSettings.ViewAs;
      bool sortAsc = mapSettings.SortAscending;
      switch (view)
      {
        case View.List:
          textLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          textLine = GUILocalizeStrings.Get(100);
          break;
        case View.BigIcons:
          textLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          textLine = GUILocalizeStrings.Get(529);
          break;
        case View.Filmstrip:
          textLine = GUILocalizeStrings.Get(733);
          break;
      }
      
      GUIControl.SetControlLabel(GetID, btnSwitchView.GetID, textLine);

    }

    void SortChanged(object sender, SortEventArgs e)
    {
      // save the new state
      mapSettings.SortAscending = e.Order != SortOrder.Descending;
      // update the list
      UpdateList();
      //UpdateButtonStates();
      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
    }

    #endregion

    #region download manager

    private string GetStationLogoFileName(RadioTimeOutline radioItem)
    {
      if (string.IsNullOrEmpty(radioItem.Image))
        return string.Empty;
      else
        return Utils.GetCoverArtName(Thumbs.Radio, radioItem.Text);
    }

    private string DownloadStationLogo(RadioTimeOutline radioItem )
    {
      string localFile = GetStationLogoFileName(radioItem);
      if (!File.Exists(localFile) && !string.IsNullOrEmpty(radioItem.Image))
      {
        downloaQueue.Enqueue(new DownloadFileObject(localFile, radioItem.Image.Replace("q.png",".png")));
      }
      return localFile;
    }


    private void DownloadLogoEnd(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error == null)
      {
        File.Copy(Path.GetTempPath() + @"\station.png", curentDownlodingFile.FileName, true);
        UpdateGui();
       
      }
    }            

    #endregion
  }
}
