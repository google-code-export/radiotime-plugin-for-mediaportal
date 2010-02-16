using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using RadioTimeOpmlApi.com.radiotime.services;

namespace RadioTimeOpmlApi
{
  public class RadioTimeOutline
  {
    public enum OutlineType
    {
      audio,
      link,
      unknow
    };

    public RadioTimeOutline()
    {

    }

    public RadioTimeOutline(XmlNode node)
    {
      if (node.Attributes["type"].Value == "link")
        Type = OutlineType.link;
      else
        if (node.Attributes["type"].Value == "audio")
          Type = OutlineType.audio;
        else Type = OutlineType.unknow;
      Text = node.Attributes["text"].Value;
      if (node.Attributes["url"] != null)
        Url = node.Attributes["url"].Value;
      if (node.Attributes["URL"] != null)
        Url = node.Attributes["URL"].Value;
      
      if (node.Attributes["image"] != null)
        Image = node.Attributes["image"].Value;
      else
        Image = string.Empty;

      if (node.Attributes["bitrate"] != null)
        Bitrate = node.Attributes["bitrate"].Value;
      else
        Bitrate = string.Empty;

      if (node.Attributes["subtext"] != null)
        Subtext = node.Attributes["subtext"].Value;
      else
        Subtext = string.Empty;

      if (node.Attributes["formats"] != null)
        Formats = node.Attributes["formats"].Value;
      else
        Formats = string.Empty;

      if (node.Attributes["current_track"] != null)
        CurrentTrack = node.Attributes["current_track"].Value;
      else
        CurrentTrack = string.Empty;

      if (Type == OutlineType.audio)
      {
        Now_playing_id = node.Attributes["now_playing_id"].Value;
      }
      else
        Now_playing_id = string.Empty;


    }

    private OutlineType _type;
    public OutlineType Type
    {
      get { return _type; }
      set { _type = value; }
    }

    private string _text;
    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    private string _url;
    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    private string _image;
    public string Image
    {
      get { return _image; }
      set { _image = value; }
    }

    private string _now_playing_id;
    public string Now_playing_id
    {
      get { return _now_playing_id; }
      set { _now_playing_id = value; }
    }

    private string _bitrate;
    public string Bitrate
    {
      get { return _bitrate; }
      set { _bitrate = value; }
    }

    private string _current_track;
    public string CurrentTrack
    {
      get { return _current_track; }
      set { _current_track = value; }
    }

    public string Subtext { get; set; }
    public string Formats { get; set; }

    /// <summary>
    /// Gets the station id.
    /// </summary>
    /// <value>The station id.</value>
    public string StationId
    {
      get
      {
        if (Now_playing_id.Contains("station:"))
        {
          return Now_playing_id.Substring("station:".Length);
        }
        return string.Empty;
      }
    }

    /// <summary>
    /// Gets the station id as int.
    /// </summary>
    /// <value>The station id as int.</value>
    public int StationIdAsInt
    {
      get
      {
        int i = 0;
        int.TryParse(StationId, out i);
        return i;
      }
    }

  }
}
