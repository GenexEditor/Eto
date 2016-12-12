using System;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.Test.Sections.Controls
{
	[Section("Controls", typeof(TreeGridView))]
	public class TreeGridViewSection : Scrollable
	{
		int expanded;
		readonly CheckBox allowCollapsing;
		readonly CheckBox allowExpanding;
		readonly TreeGridView treeView;
		int newItemCount;
		static readonly Image Image = TestIcons.TestIcon;
		Label hoverNodeLabel;
		bool cancelLabelEdit;

		public TreeGridViewSection()
		{
			var layout = new DynamicLayout { DefaultSpacing = new Size(5, 5), Padding = new Padding(10) };
			treeView = ImagesAndMenu();

			layout.AddSeparateRow(
				null,
				allowExpanding = new CheckBox { Text = "Allow Expanding", Checked = true },
				allowCollapsing = new CheckBox { Text = "Allow Collapsing", Checked = true },
				RefreshButton(),
				null
			);
			layout.AddSeparateRow(null, InsertButton(), AddChildButton(), RemoveButton(), ExpandButton(), CollapseButton(), null);
			layout.AddSeparateRow(null, EnabledCheck(), AllowMultipleSelect(), null);

			layout.Add(treeView, yscale: true);
			layout.Add(HoverNodeLabel());

			Content = layout;
		}

		Control HoverNodeLabel()
		{
			hoverNodeLabel = new Label();

			treeView.MouseMove += (sender, e) =>
			{
				var cell = treeView.GetCellAt(e.Location);
				if (cell != null)
					hoverNodeLabel.Text = $"Item under mouse: {((TreeGridItem)cell.Item)?.Values[1] ?? "(no item)"}, Column: {cell.Column?.HeaderText ?? "(no column)"}";
			};

			return hoverNodeLabel;
		}

		Control InsertButton()
		{
			var control = new Button { Text = "Insert" };
			control.Click += (sender, e) =>
			{
				var item = treeView.SelectedItem as TreeGridItem;
				var parent = (item?.Parent ?? (ITreeGridItem)treeView.DataStore) as TreeGridItem;
				if (parent != null)
				{
					var index = item != null ? parent.Children.IndexOf(item) : 0;
					parent.Children.Insert(index, CreateComplexTreeItem(0, "New Item " + newItemCount++, null));
					if (item != null)
						treeView.RefreshItem(parent);
					else
						treeView.RefreshData();
				}
			};
			return control;
		}

		Control AddChildButton()
		{
			var control = new Button { Text = "Add Child" };
			control.Click += (sender, e) =>
			{
				var item = treeView.SelectedItem as TreeGridItem;
				if (item != null)
				{
					item.Children.Add(CreateComplexTreeItem(0, "New Item " + newItemCount++, null));
					treeView.RefreshItem(item);
				}
			};
			return control;
		}

		Control RemoveButton()
		{
			var control = new Button { Text = "Remove" };
			control.Click += (sender, e) =>
			{
				var item = treeView.SelectedItem as TreeGridItem;
				if (item != null)
				{
					var parent = item.Parent as TreeGridItem;
					parent.Children.Remove(item);
					if (parent.Parent == null)
						treeView.RefreshData();
					else
						treeView.RefreshItem(parent);
				}
			};
			return control;
		}

		Control RefreshButton()
		{
			var control = new Button { Text = "Refresh" };
			control.Click += (sender, e) =>
			{
				foreach (var tree in Children.OfType<TreeGridView>())
				{
					tree.RefreshData();
				}
			};
			return control;
		}

		Control ExpandButton()
		{
			var control = new Button { Text = "Expand" };
			control.Click += (sender, e) =>
			{
				var item = treeView.SelectedItem;
				if (item != null)
				{
					item.Expanded = true;
					treeView.RefreshItem(item);
				}
			};
			return control;
		}

		Control CollapseButton()
		{
			var control = new Button { Text = "Collapse" };
			control.Click += (sender, e) =>
			{
				var item = treeView.SelectedItem;
				if (item != null)
				{
					item.Expanded = false;
					treeView.RefreshItem(item);
				}
			};
			return control;
		}

		Control CancelLabelEdit()
		{
			var control = new CheckBox { Text = "Cancel Edit" };
			control.CheckedChanged += (sender, e) => cancelLabelEdit = control.Checked ?? false;
			return control;
		}

		Control EnabledCheck()
		{
			var control = new CheckBox { Text = "Enabled", Checked = treeView.Enabled };
			control.CheckedChanged += (sender, e) => treeView.Enabled = control.Checked ?? false;
			return control;
		}

		Control AllowMultipleSelect()
		{
			var control = new CheckBox { Text = "AllowMultipleSelection" };
			control.CheckedBinding.Bind(treeView, t => t.AllowMultipleSelection);
			return control;
		}

		TreeGridItem CreateComplexTreeItem(int level, string name, Image image)
		{
			var item = new TreeGridItem
			{
				Expanded = expanded++ % 2 == 0
			};
			item.Values = new object[] { image, "col 0 - " + name, "col 1 - " + name };
			if (level < 4)
			{
				for (int i = 0; i < 4; i++)
				{
					item.Children.Add(CreateComplexTreeItem(level + 1, name + " " + i, image));
				}
			}
			return item;
		}

		TreeGridView ImagesAndMenu()
		{
			var control = new TreeGridView
			{
				Size = new Size(100, 150)
			};

			control.Columns.Add(new GridColumn { DataCell = new ImageTextCell(0, 1), HeaderText = "Image and Text", AutoSize = true, Resizable = true, Editable = true });
			control.Columns.Add(new GridColumn { DataCell = new TextBoxCell(2), HeaderText = "Text", AutoSize = true, Width = 150, Resizable = true, Editable = true });

			if (Platform.Supports<ContextMenu>())
			{
				var menu = new ContextMenu();
				var item = new ButtonMenuItem { Text = "Click Me!" };
				item.Click += delegate
				{
					if (control.SelectedItem != null)
						Log.Write(item, "Click, Rows: {0}", control.SelectedItem);
					else
						Log.Write(item, "Click, no item selected");
				};
				menu.Items.Add(item);

				control.ContextMenu = menu;
			}

			control.DataStore = CreateComplexTreeItem(0, "", Image);
			LogEvents(control);
			return control;
		}

		string GetDescription(ITreeGridItem item)
		{
			var treeItem = item as TreeGridItem;
			if (treeItem != null)
				return Convert.ToString(string.Join(", ", treeItem.Values.Select(r => Convert.ToString(r))));
			return Convert.ToString(item);
		}

		void LogEvents(TreeGridView control)
		{
			control.Activated += (sender, e) =>
			{
				Log.Write(control, "Activated, Item: {0}", GetDescription(e.Item));
			};
			control.SelectionChanged += delegate
			{
				Log.Write(control, "SelectionChanged, Rows: {0}", string.Join(", ", control.SelectedRows.Select(r => r.ToString())));
			};
			control.SelectedItemChanged += delegate
			{
				Log.Write(control, "SelectedItemChanged, Item: {0}", control.SelectedItem != null ? GetDescription(control.SelectedItem) : "<none selected>");
			};

			control.Expanding += (sender, e) =>
			{
				Log.Write(control, "Expanding, Item: {0}", GetDescription(e.Item));
				e.Cancel = !(allowExpanding.Checked ?? true);
			};
			control.Expanded += (sender, e) =>
			{
				Log.Write(control, "Expanded, Item: {0}", GetDescription(e.Item));
			};
			control.Collapsing += (sender, e) =>
			{
				Log.Write(control, "Collapsing, Item: {0}", GetDescription(e.Item));
				e.Cancel = !(allowCollapsing.Checked ?? true);
			};
			control.Collapsed += (sender, e) =>
			{
				Log.Write(control, "Collapsed, Item: {0}", GetDescription(e.Item));
			};
			control.ColumnHeaderClick += delegate (object sender, GridColumnEventArgs e)
			{
				Log.Write(control, "Column Header Clicked: {0}", e.Column);
			};

			control.CellClick += (sender, e) =>
			{
				Log.Write(control, "Cell Clicked, Row: {0}, Column: {1}, Item: {2}, ColInfo: {3}", e.Row, e.Column, e.Item, e.GridColumn);
			};

			control.CellDoubleClick += (sender, e) =>
			{
				Log.Write(control, "Cell Double Clicked, Row: {0}, Column: {1}, Item: {2}, ColInfo: {3}", e.Row, e.Column, e.Item, e.GridColumn);
			};
		}
	}
}

