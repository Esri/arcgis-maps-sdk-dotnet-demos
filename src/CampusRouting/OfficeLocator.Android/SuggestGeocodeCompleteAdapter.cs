using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace OfficeLocator
{
    internal class SuggestGeocodeCompleteAdapter : ArrayAdapter, IFilterable
    {
        private List<string> resultList;
        public SuggestGeocodeCompleteAdapter(Context context, int textViewResourceId) : base(context, textViewResourceId)
        {
            resultList = new List<string>();
            SetNotifyOnChange(true);
        }
        public void UpdateList(IEnumerable<string> list)
        {
            resultList = list.ToList();
            NotifyDataSetChanged();
        }

        public override int Count
        {
            get
            {
                return resultList.Count;
            }
        }

        public override Filter Filter
        {
            get
            {
                return new SuggestFilter(resultList);
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return resultList[position];
        }

        public override long GetItemId(int position)
        {
            return base.GetItemId(position);
        }


        private class SuggestFilter : Filter
        {
            private List<string> resultList;
            public SuggestFilter(List<string> list) { resultList = list; }
            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                return new FilterResults() { Count = resultList.Count, Values = resultList.ToArray() };
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                //
            }
        }
    }
}