using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Timers;
using MediaPortal.GUI.Library;

namespace RadioTimePlugin
{
  public class BaseGui : GUIWindow
  {
    public Queue downloaQueue = new Queue();
    public WebClient Client = new WebClient();
    public Timer updateStationLogoTimer = new Timer(0.2 * 1000);
    public DownloadFileObject curentDownlodingFile;




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
        return string.Empty;
      int min = 0;
      int.TryParse(minutes, out min);
      minutes = (min/60).ToString();
      minutes += ":" + (min - ((min/60)*60)).ToString("00");
      return minutes;
    }
  }
}
