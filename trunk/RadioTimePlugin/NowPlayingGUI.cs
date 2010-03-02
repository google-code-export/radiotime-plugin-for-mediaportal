using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
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

    public NowPlayingGUI()
    {
    
    }

    public override bool Init()
    {
      // show the skin
      return Load(GUIGraphicsContext.Skin + @"\radiotimenowplaying.xml");
    }

    protected override void OnPageLoad()
    {
      Refresh();
      foreach (string name in Translation.Strings.Keys)
      {
        SetProperty("#RadioTime.Translation." + name + ".Label", Translation.Strings[name]);
      }
      base.OnPageLoad();
    }

    private void Refresh()
    {
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
      base.OnPageDestroy(new_windowId);
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
            GUIWindowManager.ActivateWindow(25650);
          }
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

  }
}
