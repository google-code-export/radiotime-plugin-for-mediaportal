using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace RadioTimeOpmlApi
{
  public class RadioTime
  {
    private bool _cacheIsUsed;

    /// <summary>
    /// Gets or sets a value indicating whether [cache is used].
    /// </summary>
    /// <value><c>true</c> if [cache is used]; otherwise, <c>false</c>.</value>
    public bool CacheIsUsed
    {
      get { return _cacheIsUsed; }
      set { _cacheIsUsed = value; }
    }


    private String _curentUrl;
    /// <summary>
    /// Gets or sets the curent URL.
    /// </summary>
    /// <value>The curent URL.</value>
    public String CurentUrl
    {
      get { return _curentUrl; }
      set { _curentUrl = value; }
    }

    private RadioTime _parent;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    /// <value>The parent.</value>
    public RadioTime Parent
    {
      get { return _parent; }
      set { _parent = value; }
    }


    private string _parentUrl;
    /// <summary>
    /// Gets or sets the parent URL.
    /// </summary>
    /// <value>The parent URL.</value>
    public string ParentUrl
    {
      get { return _parentUrl; }
      set { _parentUrl = value; }
    }

    private RadioTimeHead _head;

    /// <summary>
    /// Gets or sets the head description.
    /// </summary>
    /// <value>The head.</value>
    public RadioTimeHead Head
    {
      get { return _head; }
      set { _head = value; }
    }


    private List<RadioTimeOutline> _body;
    /// <summary>
    /// Gets or sets the body elements.
    /// </summary>
    /// <value>The body.</value>
    public List<RadioTimeOutline> Body
    {
      get { return _body; }
      set { _body = value; }
    }

    private RadioTimeSetting _settings;
    /// <summary>
    /// Gets or sets the settings.
    /// </summary>
    /// <value>The settings.</value>
    public RadioTimeSetting Settings
    {
      get { return _settings; }
      set { _settings = value; }
    }

    private Dictionary<string,RadioTime> _cache;

    /// <summary>
    /// Gets or sets the cache.
    /// </summary>
    /// <value>The cache.</value>
    public Dictionary<string,RadioTime> Cache
    {
      get { return _cache; }
      set { _cache = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioTime"/> class.
    /// </summary>
    public RadioTime()
    {
      CurentUrl = "http://opml.radiotime.com/Index.aspx";
      ParentUrl = CurentUrl;
      Head = new RadioTimeHead();
      Body = new List<RadioTimeOutline>();
      Settings = new RadioTimeSetting();
      Cache = new Dictionary<string, RadioTime>();
      Parent = null;
    }
    
    public RadioTime(RadioTime parent)
    {
      this.Head =new RadioTimeHead(parent.Head);
      this.Body = new List<RadioTimeOutline>(parent.Body);
      this.Settings = new RadioTimeSetting(parent.Settings);
      this.ParentUrl = parent.ParentUrl;
      this.CurentUrl = parent.CurentUrl;
      this.Parent = parent.Parent;
      this.Cache = parent.Cache;
    }

    /// <summary>
    /// Prevs this instance.
    /// </summary>
    public void Prev()
    {
      if (Parent != null)
      {
        this.Head = Parent.Head;
        this.Body = Parent.Body;
        this.Settings = Parent.Settings;
        this.ParentUrl = Parent.ParentUrl;
        this.CurentUrl = Parent.CurentUrl;
        this.Parent = Parent.Parent;
      }
    }

    /// <summary>
    /// Resets all variabiles and clear the cache.
    /// </summary>
    public void Reset()
    {
      ClearCache();
      this.Parent = null;
      Body.Clear();
      ParentUrl = string.Empty;
      CurentUrl = string.Empty;
    }
    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void ClearCache()
    {
      Cache.Clear();
    }

    /// <summary>
    /// Searches the specified search string.
    /// </summary>
    /// <param name="sStr">The search string.</param>
    public void Search(string sStr)
    {
      string s = string.Format("http://opml.radiotime.com/Search.aspx?query={0}&{1}", sStr, Settings.GetParamString());
      GetData(s);
    }

    /// <summary>
    /// Gets the online data.
    /// </summary>
    /// <param name="sUrl">The s URL.</param>
    /// <returns></returns>
    public bool GetData(string sUrl)
    {
      return GetData(sUrl, true);
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="sUrl">The s URL.</param>
    /// <param name="useCache">if set to <c>true</c> [use cache].</param>
    /// <returns></returns>
    public bool GetData(string sUrl, bool useCache)
    {
      return GetData(sUrl, useCache, true);
    }

    /// <summary>
    /// Get and parse the online data.
    /// </summary>
    /// <param name="sUrl">The s URL.</param>
    /// <param name="useCache">if set to <c>true</c> [use cache].</param>
    /// <param name="useNavigationLogic">if set to <c>true</c> [use navigation logic].</param>
    /// <returns></returns>
    public bool GetData(string sUrl,bool useCache,bool useNavigationLogic)
    {
      if (string.IsNullOrEmpty(sUrl))
        return false;
      
      CacheIsUsed = useCache;

      if (useCache && Cache.ContainsKey(sUrl))
      {
        if (useNavigationLogic)
          Parent = new RadioTime(this);
        ParentUrl = CurentUrl;
        CurentUrl = sUrl;
        Head = new RadioTimeHead(Cache[sUrl].Head);
        Body = new List<RadioTimeOutline>(Cache[sUrl].Body);
        return true;
      }
      else
      {
        Stream response = RetrieveData(sUrl);
        if (response != null)
        {
          if (sUrl != CurentUrl)
          {
            if (useNavigationLogic)
              Parent = new RadioTime(this);
            ParentUrl = CurentUrl;
            CurentUrl = sUrl;
          }
          // Get the stream associated with the response.
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
            XmlNode headnodetitle = root.SelectSingleNode("head/title");
            if (headnodetitle != null)
            {
              Head.Title = headnodetitle.InnerText;
            }
            else
            {
              Head.Title = "";
            }
            XmlNodeList bodynodes = root.SelectNodes("body/outline");
            Body.Clear();
            foreach (XmlNode node in bodynodes)
            {
              if (node.HasChildNodes)
              {
                foreach (XmlNode childnode in node.ChildNodes)
                {
                  Body.Add(new RadioTimeOutline(childnode));
                }
              }
              else
              {
                Body.Add(new RadioTimeOutline(node));
              }
            }
            
            if (useCache)
            {
              Cache.Add(sUrl, new RadioTime(this));
            }

          }
          catch (XmlException)
          {
            return false;
          }
          response.Close();
          return true;
        }
        return true;
      }
    }

    /// <summary>
    /// Retrieves the online data.
    /// </summary>
    /// <param name="sUrl">The s URL.</param>
    /// <returns></returns>
    private Stream RetrieveData(String sUrl)
    {
      if (sUrl == null || sUrl.Length < 1 || sUrl[0] == '/')
      {
        return null;
      }
      //sUrl = this.Settings.UpdateUrl(sUrl);
      HttpWebRequest request = null;
      HttpWebResponse response = null;
      try
      {
        request = (HttpWebRequest)WebRequest.Create(sUrl);
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
