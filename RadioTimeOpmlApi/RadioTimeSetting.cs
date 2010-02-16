using System;
using System.Collections.Generic;
using System.Text;

namespace RadioTimeOpmlApi
{
  public class RadioTimeSetting
  {
    public RadioTimeSetting()
    {
      User = string.Empty;
      Mp3 = true;
      Wma = true;
      Real = false;
    }

    public RadioTimeSetting(RadioTimeSetting parent)
    {
      User = parent.User; ;
      Mp3 = parent.Mp3;
      Wma = parent.Wma;
      Real = parent.Real;
    }

    private string _user;
    public string User
    {
      get { return _user; }
      set { _user = value; }
    }

    private bool _mp3;
    public bool Mp3
    {
      get { return _mp3; }
      set { _mp3 = value; }
    }

    private bool _wma;
    public bool Wma
    {
      get { return _wma; }
      set { _wma = value; }
    }

    private bool _real;
    public bool Real
    {
      get { return _real; }
      set { _real = value; }
    }

    private string _partnerId;

    public string PartnerId
    {
      get { return _partnerId; }
      set { _partnerId = value; }
    }

    public string UpdateUrl(string sUrl)
    {
      if (sUrl.EndsWith(".aspx"))
      {
        sUrl += "?";
      }
      string opUser = "&username=";
      if (!string.IsNullOrEmpty(User))
      {
        int ipos = sUrl.IndexOf(opUser);
        if (ipos > 0)
        {
          sUrl = sUrl.Remove(ipos);
          sUrl= sUrl + opUser + User.Trim();
        }
        else
        {
          sUrl = sUrl + opUser + User.Trim();
        }
      } 
      return sUrl;
    }

    /// <summary>
    /// Gets the startup URL.
    /// </summary>
    /// <value>The startup URL.</value>
    public string StartupUrl
    {
      get
      {
        if (!string.IsNullOrEmpty(GetParamString()))
        {
          return "http://opml.radiotime.com/Index.aspx?" + GetParamString();
        }
        else
        {
          return "http://opml.radiotime.com/Index.aspx";
        }
      }
    }

    /// <summary>
    /// Gets the presets URL.
    /// </summary>
    /// <value>The presets URL.</value>
    public string PresetsUrl
    {
      get
      {
        if (!string.IsNullOrEmpty(GetParamString()))
        {
          return "http://opml.radiotime.com/GroupList.aspx?type=favorite&" + GetParamString();
        }
        else
        {
          return "http://opml.radiotime.com/GroupList.aspx?type=favorite";
        }
      }
    }
	
    public string GetParamString()
    {
      string s = "";
      if (!string.IsNullOrEmpty(User.Trim()))
      {
        s += "username=" + User;
      }
      string ext = string.Empty;
      if (Mp3)
        ext += "mp3,";
      if (Wma)
        ext += "wma,";
      if (Real)
        ext += "real";
      //s += "&formats=" + ext;
      if (!string.IsNullOrEmpty(PartnerId))
      {
        s += "&partnerID=" + PartnerId;
      }
      return s;
    }
  }
}
