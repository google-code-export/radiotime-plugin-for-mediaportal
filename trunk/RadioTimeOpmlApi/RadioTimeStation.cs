using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;


namespace RadioTimeOpmlApi
{
  public class RadioTimeStation
  {

    public void Get(string stationid)
    {
      StationId = stationid;
      string sUrl = string.Format("http://opml.radiotime.com/NowPlaying.aspx?stationId={0}", StationId);
      Stream response = RetrieveData(sUrl);
      if (response != null)
      {
        StreamReader reader = new StreamReader(response, System.Text.Encoding.UTF8, true);
        String sXmlData = reader.ReadToEnd().Replace('\0', ' ');
        response.Close();
        reader.Close();
        try
        {
          XmlDocument doc = new XmlDocument();
          doc.LoadXml(sXmlData);
          // skip xml node
          XmlNode root = doc.FirstChild.NextSibling;
          XmlNode bodynodes = root.SelectSingleNode("body");
          int i = 1;
          foreach (XmlNode node in bodynodes)
          {

            
            i++;
          }
        }
        catch
        {
        }
      }
    }

    private System.IO.Stream RetrieveData(String sUrl)
    {
      if (sUrl == null || sUrl.Length < 1 || sUrl[0] == '/')
      {
        return null;
      }
      HttpWebRequest request = null;
      HttpWebResponse response = null;
      try
      {
        request = (HttpWebRequest)WebRequest.Create(sUrl);
        // Note: some network proxies require the useragent string to be set or they will deny the http request
        // this is true for instance for EVERY thailand internet connection (also needs to be set for banners/episodethumbs and any other http request we send)
        //request.UserAgent = Settings.UserAgent;
        request.Timeout = 20000;
        response = (HttpWebResponse)request.GetResponse();

        if (response != null) // Get the stream associated with the response.
          return response.GetResponseStream();

      }
      catch (Exception e)
      {
        // can't connect, timeout, etc
      }
      finally
      {
        //if (response != null) response.Close(); // screws up the decompression
      }

      return null;
    }      
  }
}
