using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;
using RadioTimeOpmlApi; 


namespace RadioTimePlugin
{
  class StationSort : IComparer<GUIListItem>
  {
    SortMethod currentSortMethod;
    bool sortAscending = true;

    public enum SortMethod
    {
      name,
      bitrate,
      none
    }

    public StationSort(SortMethod method, bool asc)
    {
      currentSortMethod = method;
      sortAscending = asc;
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;

      switch (currentSortMethod)
      {
        case SortMethod.name:
          if (sortAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }
        case SortMethod.bitrate:
          int bit1=0;
          int.TryParse(item1.Label2, out bit1);
          int bit2 = 0;
          int.TryParse(item2.Label2, out bit2);
          if (sortAscending)
          {
            return (int)(bit1 - bit2);
          }
          else
          {
            return (int)(bit2 - bit1);
          }
      }
      return 0;
    }

  }
}
