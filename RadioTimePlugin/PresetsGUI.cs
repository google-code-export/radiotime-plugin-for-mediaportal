using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using MediaPortal.GUI.Library;
using RadioTimeOpmlApi;

namespace RadioTimePlugin
{
  public class PresetsGUI : BaseGui
  {
    [SkinControlAttribute(2)]
    protected GUIButtonControl homeButton = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl folderButton = null;

    public override int GetID
    {
      get
      {
        return 25653;
      }

      set
      {
      }
    }

    public PresetsGUI()
    {
    }

    public override bool Init()
    {
      grabber = new RadioTime();
      return Load(GUIGraphicsContext.Skin + @"\RadioTimePresets.xml");
    }

    protected override void OnPageLoad()
    {
    
      _setting = Settings.Instance;
      grabber.Settings.User = _setting.User;
      grabber.Settings.Password = _setting.Password;
      grabber.Settings.PartnerId = _setting.PartnerId;
      // show the skin
      LoadLocalPresetStations();

      foreach (string name in Translation.Strings.Keys)
      {
        SetProperty("#RadioTime.Translation." + name + ".Label", Translation.Strings[name]);
      }

      base.OnPageLoad();
    }
    
    public override bool OnMessage(GUIMessage message)
    {
      Log.Error(message.Message.ToString());
      if (message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS && message.TargetControlId > 100 && message.TargetControlId<Settings.LOCAL_PRESETS_NUMBER+100)
      {
        if (_setting.PresetStations.Count > message.TargetControlId - 100)
          UpdateSelectedLabels(_setting.PresetStations[message.TargetControlId - 100]);
      }
      return base.OnMessage(message);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      
      if (control.GetType() == typeof(GUIButtonControl))
      {
        if (controlId > 100 && controlId < Settings.LOCAL_PRESETS_NUMBER + 100)
        {
          DoPlay(_setting.PresetStations[controlId - 100]);
        }
      }
      if (control == homeButton)
      {
        _setting.FirtsStart = false;
        GUIWindowManager.ActivateWindow(25650);
      }
      else if (control == folderButton)
      {
        string s = GetPresetFolder();
        if (s != null)
        {
          _setting.FolderId = s;
          _setting.Save();
          LoadLocalPresetStations();
        }
      }

      base.OnClicked(controlId, control, actionType);
    }
  }
}
