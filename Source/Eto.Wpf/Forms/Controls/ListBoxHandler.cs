using System;
using System.Linq;
using Eto.Forms;
using swc = System.Windows.Controls;
using sw = System.Windows;
using swd = System.Windows.Data;
using System.Collections.ObjectModel;

namespace Eto.Wpf.Forms.Controls
{
	public class ListBoxHandler : WpfControl<swc.ListBox, ListBox, ListBox.ICallback>, IListBox
	{
		IListStore store;
		ContextMenu contextMenu;

		public override sw.Size GetPreferredSize(sw.Size constraint)
		{
			return base.GetPreferredSize(sw.Size.Empty);
		}

		public ListBoxHandler()
		{
			Control = new swc.ListBox();
			Control.HorizontalAlignment = sw.HorizontalAlignment.Stretch;
			//Control.DisplayMemberPath = "Text";
			var template = new sw.DataTemplate(typeof(IListItem));

			template.VisualTree = WpfListItemHelper.ItemTemplate(false);
			Control.ItemTemplate = template;
			Control.SelectionChanged += delegate
			{
				Callback.OnSelectedIndexChanged(Widget, EventArgs.Empty);
			};
			Control.MouseDoubleClick += delegate
			{
				if (SelectedIndex >= 0)
					Callback.OnActivated(Widget, EventArgs.Empty);
			};
			Control.KeyDown += (sender, e) =>
			{
				if (e.Key == sw.Input.Key.Return)
				{
					if (SelectedIndex >= 0)
					{
						Callback.OnActivated(Widget, EventArgs.Empty);
						e.Handled = true;
					}
				}
			};
		}

		public override void Focus()
		{
			if (Control.IsLoaded)
			{
				var item = Control.ItemContainerGenerator.ContainerFromIndex(Math.Max(0, SelectedIndex)) as sw.FrameworkElement;
				if (item != null)
					item.Focus();
				else
					Control.Focus();
			}
			else
			{
				Control.Loaded += Control_Loaded;
			}
		}

		void Control_Loaded(object sender, sw.RoutedEventArgs e)
		{
			Focus();
			Control.Loaded -= Control_Loaded;
		}

		public override bool UseMousePreview { get { return true; } }

		public override bool UseKeyPreview { get { return true; } }

		public IListStore DataStore
		{
			get { return store; }
			set
			{
				store = value;
				var source = store as ObservableCollection<IListItem>; 
				Control.ItemsSource = source ?? new ObservableCollection<IListItem>(store.AsEnumerable());
			}
		}

		public int SelectedIndex
		{
			get { return Control.SelectedIndex; }
			set
			{
				Control.SelectedIndex = value;
				if (value >= 0)
				{
					var item = store.AsEnumerable().Skip(value).FirstOrDefault();
					Control.ScrollIntoView(item);
				}
			}
		}

		public ContextMenu ContextMenu
		{
			get { return contextMenu; }
			set
			{
				contextMenu = value;
				Control.ContextMenu = contextMenu != null ? contextMenu.ControlObject as sw.Controls.ContextMenu : null;
			}
		}
	}
}