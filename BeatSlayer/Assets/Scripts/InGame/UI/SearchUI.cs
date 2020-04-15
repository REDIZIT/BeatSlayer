using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Searching
{
    public class SearchUI : MonoBehaviour
    {
        public TrackListUI ui { get { return GetComponent<TrackListUI>(); } }

        public InputField searchInputField;


        public ToggleGroup searchingForGroup;
        public ToggleGroup sortByGroup;
        public ToggleGroup showGroup;
        public ToggleGroup directionGroup;


        SearchingForType searchingForType;
        SortByType sortByType;
        ShowType showType;
        ListSortDirection listSortDirection = ListSortDirection.Descending;


        public enum SortByType
        {
            Popularity, Date, Likes, Dislikes, Downloads, MapsCount
        }
        public enum ShowType
        {
            All, OnlyNew, NotPlayed, Played
        }
        public enum SearchingForType
        {
            Map, Player
        }



        public void OnSearchFieldChanged()
        {
            ui.Refresh();
        }
        public IEnumerable<GroupInfoExtended> OnSearch(IEnumerable<GroupInfoExtended> data)
        {
            DateTime d1 = DateTime.Now;

            IEnumerable<GroupInfoExtended> ls = new List<GroupInfoExtended>();

            ls = data;

            string searchText = searchInputField.text;


            // Searching for matches in correct type (Map or player)
            ls = ls.Where(t => 
                searchingForType == SearchingForType.Map ?
                    (t.author + "-" + t.name).ToLower().Contains(searchText.ToLower()) :
                    t.nicks.Any(c => c.ToLower().Contains(searchText.ToLower())));


            // Selecting ShowType
            if (showType != ShowType.All)
            {
                ls = ls.Where(t => showType == ShowType.OnlyNew ?
                    t.IsNew : showType == ShowType.Played ?
                    AccountManager.account.playedMaps.Any(m => m.author + "-" + m.name == t.author + "-" + t.name) :
                    AccountManager.account.playedMaps.Any(m => m.author + "-" + m.name != t.author + "-" + t.name));
            }


            // Sorting
            Func<GroupInfoExtended, long> keySelector = c => sortByType == SortByType.Popularity ?
                GetPopularity(c) : sortByType == SortByType.Likes ?
                c.allLikes : sortByType == SortByType.Dislikes ?
                c.allDislikes : sortByType == SortByType.Downloads ?
                c.allDownloads : sortByType == SortByType.MapsCount ?
                c.mapsCount : c.updateTime.Ticks;

            ls = listSortDirection == ListSortDirection.Descending ? ls.OrderByDescending(keySelector) : ls.OrderBy(keySelector);

            Debug.Log("Search time is " + (DateTime.Now - d1).TotalMilliseconds);

            return ls;
        }


        public void OnToggleChange()
        {
            Toggle searchingForToggle = searchingForGroup.ActiveToggles().First();
            searchingForType = (SearchingForType)int.Parse(searchingForToggle.name);

            Toggle sortByToggle = sortByGroup.ActiveToggles().First();
            sortByType = (SortByType)int.Parse(sortByToggle.name);

            Toggle showToggle = showGroup.ActiveToggles().First();
            showType = (ShowType)int.Parse(showToggle.name);

            Toggle directionToggle = directionGroup.ActiveToggles().First();
            listSortDirection = (ListSortDirection)int.Parse(directionToggle.name);


            OnSearchFieldChanged();
        }








        int GetPopularity(GroupInfoExtended group)
        {
            return (group.allLikes * 20 + group.allPlays * 15 + group.allDownloads * 10 + group.allDislikes * 4);
        }
    }

}