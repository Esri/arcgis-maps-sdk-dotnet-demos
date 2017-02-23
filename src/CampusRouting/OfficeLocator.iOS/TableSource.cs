using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using System.Linq;

namespace OfficeLocator
{
    // These classes are here to provide the office geocoding with auto completion 

	internal class TableSource<T> : UITableViewSource
	{
		readonly IEnumerable<T> _items;
		readonly Func<T, string> _labelFunc;
		readonly string _cellIdentifier;

		public TableSource(IEnumerable<T> items, Func<T, string> labelFunc)
		{
			_items = items;
			_labelFunc = labelFunc;
			_cellIdentifier = Guid.NewGuid().ToString();
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell(_cellIdentifier);
			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

			var item = _items.ElementAt(indexPath.Row);

			cell.TextLabel.Text = _labelFunc(item);

			return cell;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			OnTableRowSelected(indexPath);
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return _items.Count();
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			return 30f;
		}

		public event EventHandler<TableRowSelectedEventArgs<T>> TableRowSelected;

		private void OnTableRowSelected(NSIndexPath itemIndexPath)
		{
			var item = _items.ElementAt(itemIndexPath.Row);
			var label = _labelFunc(item);
			TableRowSelected?.Invoke(this, new TableRowSelectedEventArgs<T>(item, label, itemIndexPath));
		}
	}

	internal class TableRowSelectedEventArgs<T> : EventArgs
	{
		public TableRowSelectedEventArgs(T selectedItem, string selectedItemLabel, NSIndexPath selectedItemIndexPath)
		{
			SelectedItem = selectedItem;
			SelectedItemLabel = selectedItemLabel;
			SelectedItemIndexPath = selectedItemIndexPath;
		}

		public T SelectedItem { get; }
		public string SelectedItemLabel { get; }
		public NSIndexPath SelectedItemIndexPath { get; }
	}
}
