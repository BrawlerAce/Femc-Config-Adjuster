﻿// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace Femc_Config_Adjuster.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
	private string _applicationTitle = "FEMC Config";

	[ObservableProperty]
	private ObservableCollection<object> _footerMenuItems = new()
	{
		new NavigationViewItem()
		{
			Content = "Settings",
			Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
			TargetPageType = typeof(Views.Pages.SettingsPage)
		}
	};

	[ObservableProperty]
	private ObservableCollection<MenuItem> _trayMenuItems = new()
	{
		new MenuItem { Header = "Home", Tag = "tray_home" }
    };

    public NavigationViewItem[] MenuItems { get; } =
    [
        new NavigationViewItem()
        {
            Content = "2D",
            Icon = new SymbolIcon { Symbol = SymbolRegular.PaintBrush16 },
            TargetPageType = typeof(Views.Pages.Categories.Category_2D),
        },
        new NavigationViewItem()
        {
            Content = "3D",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Cube16 },
            TargetPageType = typeof(Views.Pages.Categories.Category_3D),
        },
        new NavigationViewItem()
        {
            Content = "Audio",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Speaker216 },
            TargetPageType = typeof(Views.Pages.Categories.Category_Audio),
        },
    ];
}
