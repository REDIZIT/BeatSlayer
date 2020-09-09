using InGame.Models;
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
        public IEnumerable<MapsData> OnSearch(IEnumerable<MapsData> data)
        {
            DateTime d1 = DateTime.Now;

            IEnumerable<MapsData> ls = new List<MapsData>();

            ls = data;

            string searchText = searchInputField.text;


            // Searching for matches in correct type (Map or player)
            ls = ls.Where(t => 
                searchingForType == SearchingForType.Map ?
                    (t.Author + "-" + t.Name).ToLower().Contains(searchText.ToLower()) :
                    (t.MappersNicks == null ? true : t.MappersNicks.Any(c => c != null && c.ToLower().Contains(searchText.ToLower()))));
            /*ls = ls.Where(t => 
                searchingForType == SearchingForType.Map ?
                    (t.author + "-" + t.name).ToLower().Contains(searchText.ToLower()) :
                    (t.nicks == null ? false : true));*/


            // Selecting ShowType
            if (showType != ShowType.All)
            {
                ls = ls.Where(t =>
                    showType == ShowType.OnlyNew ? t.IsNew : 
                    showType == ShowType.Played ? AccountManager.LegacyAccount.playedMaps.Any(m => m.author + "-" + m.name == t.Author + "-" + t.Name) : 
                    AccountManager.LegacyAccount.playedMaps.Any(m => m.author + "-" + m.name != t.Author + "-" + t.Name));
            }


            // Sorting
            Func<MapsData, long> keySelector = c => sortByType == SortByType.Popularity ?
                GetPopularity(c) : sortByType == SortByType.Likes ?
                c.Likes : sortByType == SortByType.Dislikes ?
                c.Dislikes : sortByType == SortByType.Downloads ?
                c.Downloads : sortByType == SortByType.MapsCount ?
                c.MappersNicks.Count : c.UpdateTime.Ticks;

            ls = listSortDirection == ListSortDirection.Descending ? ls.OrderByDescending(keySelector) : ls.OrderBy(keySelector);

            if (showType == ShowType.All) ls = ls.OrderByDescending(c => c.IsNew);
            
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








        private int GetPopularity(MapsData group)
        {
            return (group.Likes * 20 + group.PlayCount * 15 + group.Downloads * 10 + group.Dislikes * 4);
        }
    }

}