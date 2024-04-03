using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneGallery
{
    public class SortableObservableCollection<T> : ObservableCollection<T>
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

        public void Sort<TKey>(Func<T, TKey> keySelector)
        {
            ApplySort(Items.OrderByDescending(keySelector));
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            ApplySort(Items.OrderBy(keySelector, comparer));
        }

        public void Union(SortableObservableCollection<T> other)
        {
            foreach (var item in other)
            {
                Items.Add(item);
            }
        }

        public void Except(SortableObservableCollection<T> other)
        {
            foreach(var item in other)
            {
                Items.Remove(item);
            }
        }

        public int Find<Tkey>(Func<T, string> _keySelector, string _path)
        {
            int _index = 0;
            foreach (var _item in Items)
            {
                if (_path == _keySelector.Invoke(_item))
                    return _index;
                
                _index++;
                //Debug.Print(_keySelector(_item) + "");
                
            }
            return -1;
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
