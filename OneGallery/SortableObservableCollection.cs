using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneGallery
{
    internal class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public SortableObservableCollection() : base()
        {
        }

        public SortableObservableCollection(List<T> list) : base(list)
        {
        }

        public SortableObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, System.ComponentModel.ListSortDirection direction)
        {
            switch (direction)
            {
                case System.ComponentModel.ListSortDirection.Ascending:
                    {
                        ApplySort(Items.OrderBy(keySelector));
                        break;
                    }
                case System.ComponentModel.ListSortDirection.Descending:
                    {
                        ApplySort(Items.OrderByDescending(keySelector));
                        break;
                    }
            }
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            ApplySort(Items.OrderBy(keySelector, comparer));
        }

        private void ApplySort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            for (int i = 0; i < sortedItemsList.Count; i++)
            {
                Items[i] = sortedItemsList[i];
            }
        }
    }
}
