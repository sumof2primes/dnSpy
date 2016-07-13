﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using dnSpy.Contracts.Text.Editor;
using Microsoft.VisualStudio.Text.Editor;

namespace dnSpy.Text.Editor {
	sealed class WpfTextViewHost : ContentControl, IDnSpyWpfTextViewHost {
		public bool IsClosed { get; set; }
		IWpfTextView IWpfTextViewHost.TextView => TextView;
		public IDnSpyWpfTextView TextView { get; }
		public event EventHandler Closed;
		public Control HostControl => this;

		readonly IWpfTextViewMargin[] containerMargins;
		readonly Grid grid;

		public WpfTextViewHost(IWpfTextViewMarginProviderCollectionCreator wpfTextViewMarginProviderCollectionCreator, IDnSpyWpfTextView wpfTextView, bool setFocus) {
			if (wpfTextViewMarginProviderCollectionCreator == null)
				throw new ArgumentNullException(nameof(wpfTextViewMarginProviderCollectionCreator));
			if (wpfTextView == null)
				throw new ArgumentNullException(nameof(wpfTextView));
			this.grid = CreateGrid();
			TextView = wpfTextView;
			Focusable = false;
			Content = this.grid;

			UpdateBackground();
			TextView.BackgroundBrushChanged += TextView_BackgroundBrushChanged;

			this.containerMargins = new IWpfTextViewMargin[5];
			containerMargins[0] = CreateContainerMargin(wpfTextViewMarginProviderCollectionCreator, PredefinedMarginNames.Top, true, 0, 0, 3);
			containerMargins[1] = CreateContainerMargin(wpfTextViewMarginProviderCollectionCreator, PredefinedMarginNames.Bottom, true, 0, 0, 2);
			containerMargins[2] = CreateContainerMargin(wpfTextViewMarginProviderCollectionCreator, PredefinedMarginNames.BottomRightCorner, true, 0, 2, 1);
			containerMargins[3] = CreateContainerMargin(wpfTextViewMarginProviderCollectionCreator, PredefinedMarginNames.Left, false, 1, 0, 1);
			containerMargins[4] = CreateContainerMargin(wpfTextViewMarginProviderCollectionCreator, PredefinedMarginNames.Right, false, 1, 2, 1);
			Add(TextView.VisualElement, 1, 1, 1);
			Debug.Assert(!containerMargins.Any(a => a == null));

			if (setFocus) {
				Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
					if (!TextView.IsClosed)
						TextView.VisualElement.Focus();
				}));
			}
		}

		IWpfTextViewMargin CreateContainerMargin(IWpfTextViewMarginProviderCollectionCreator wpfTextViewMarginProviderCollectionCreator, string name, bool isHorizontal, int row, int column, int columnSpan) {
			var margin = new WpfTextViewContainerMargin(wpfTextViewMarginProviderCollectionCreator, this, name, isHorizontal);
			Add(margin.VisualElement, row, column, columnSpan);
			return margin;
		}

		void Add(UIElement elem, int row, int column, int columnSpan) {
			grid.Children.Add(elem);
			if (row != 0)
				Grid.SetRow(elem, row);
			if (column != 0)
				Grid.SetColumn(elem, column);
			if (columnSpan != 1)
				Grid.SetColumnSpan(elem, columnSpan);
		}

		void TextView_BackgroundBrushChanged(object sender, BackgroundBrushChangedEventArgs e) => UpdateBackground();
		void UpdateBackground() => grid.Background = TextView.Background;

		static Grid CreateGrid() {
			var grid = new Grid();

			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength() });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength() });

			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength() });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength() });

			return grid;
		}

		public void Close() {
			if (IsClosed)
				throw new InvalidOperationException();
			TextView.Close();
			IsClosed = true;
			Closed?.Invoke(this, EventArgs.Empty);
			TextView.BackgroundBrushChanged -= TextView_BackgroundBrushChanged;
			foreach (var margin in containerMargins)
				margin.Dispose();
		}

		public IWpfTextViewMargin GetTextViewMargin(string marginName) {
			foreach (var margin in containerMargins) {
				var result = margin.GetTextViewMargin(marginName) as IWpfTextViewMargin;
				if (result != null)
					return result;
			}
			return null;
		}
	}
}
