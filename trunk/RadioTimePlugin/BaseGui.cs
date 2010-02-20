using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using MediaPortal.GUI.Library;

namespace RadioTimePlugin
{
  public class BaseGui : GUIWindow
  {
    public Queue downloaQueue = new Queue();
    public WebClient Client = new WebClient();
    public System.Timers.Timer updateStationLogoTimer = new System.Timers.Timer(0.2 * 1000);
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
      return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url); ;
    }

  }
}
