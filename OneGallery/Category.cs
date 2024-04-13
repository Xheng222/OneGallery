﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace OneGallery
{
    public class Category: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsFolder = false;

        public bool IsGallery = false;

        public bool IsHomePage = false;

        public bool IsFolderInfo = false;

        public bool IsAddSelection = false;


        public string Name { get; set; }

        public string   _name
        {
            get => Name;
            set
            {
                Name = value;
                OnPropertyChanged();
            }
        }

        public FontIcon Icon = new();

        public string Glyph { set; get; }

        public void SetFontIcon(string _glyph)
        {
            Icon.Glyph = _glyph;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            if (IsHomePage)
            {
                Glyph = "&#xE80F;";
            }
            else if (IsGallery)
            {
                Glyph = "&#xE8EC;";
            }
            else if (IsFolder)
            {
                Glyph = "&#xE8B7;";
                
            }
            else if (IsFolderInfo)
            {
                Glyph = "&#xEC50;";
            }
            else
            {
                Glyph = "&#xE8B9;";
            }
        }

        public ObservableCollection<object> Children = new();

        public string PageType { get; set; }


        public Category()
        {
            Icon.Glyph = "\uE80F";
        }


    }
}
