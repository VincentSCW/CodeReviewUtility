using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using Utility.Library.BusinessObjects;

namespace Utility.Library.Controls
{
	public class ListViewSorter : ViewModelBase
    {
        protected ListView List { get; private set;}
        protected bool IsCtrlKeyDown { get; private set; }
		private IDictionary<SortableGridViewColumn, ListSortDirection> sortedHeaders = new Dictionary<SortableGridViewColumn, ListSortDirection>();

		private void RefreshColumns()
		{
			GridView gridView = this.List.View as GridView;
			if (gridView != null)
			{
				// determine which column is marked as IsDefaultSortColumn. Stops on the first column marked this way.
				foreach (SortableGridViewColumn c in gridView.Columns)
				{
					if (sortedHeaders.ContainsKey(c))
						c.HeaderTemplate = sortedHeaders[c] == ListSortDirection.Ascending ? GetHeaderArrowUp(c) : GetHeaderArrowDown(c);
					else
						c.HeaderTemplate = GetHeaderArrowTransparent(c);
				}
			}
		}

        public ListViewSorter(ListView list)
        {
            this.List = list;
            this.List.Initialized += new EventHandler(OnListInitialized);
			this.List.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(OnListPreviewKeyDown);
			this.List.PreviewKeyUp += new System.Windows.Input.KeyEventHandler(OnListPreviewKeyUp);
        }


		protected virtual ICollectionView CollectionView
		{
			get { return CollectionViewSource.GetDefaultView(this.List.ItemsSource); }
		}

		void OnListPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			this.IsCtrlKeyDown = e.Key == System.Windows.Input.Key.RightCtrl
				|| e.Key ==  System.Windows.Input.Key.LeftCtrl;
		}

		void OnListPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			this.IsCtrlKeyDown = false;
		}

        /// <summary>
        /// Executes when the control is initialized completely the first time through. Runs only once.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnListInitialized(object sender, EventArgs e)
		{

			// add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
			this.List.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnColumnHeaderClicked));
		}

		internal void Setup()
		{
			// cast the ListView's View to a GridView
			GridView gridView = this.List.View as GridView;
			if (gridView != null)
			{
				// determine which column is marked as IsDefaultSortColumn. Stops on the first column marked this way.
				SortableGridViewColumn sortableGridViewColumn = null;
				foreach (GridViewColumn gridViewColumn in gridView.Columns)
				{
					sortableGridViewColumn = gridViewColumn as SortableGridViewColumn;
					if (sortableGridViewColumn != null)
					{
						if (sortableGridViewColumn.IsDefaultSortColumn)
						{
							break;
						}
						sortableGridViewColumn = null;
					}
				}

				// if the default sort column is defined, sort the data and then update the templates as necessary.
				if (sortableGridViewColumn != null)
				{
					sortedHeaders[sortableGridViewColumn] = ListSortDirection.Ascending;
					sortableGridViewColumn.HeaderTemplate = GetHeaderArrowUp(sortableGridViewColumn);

					Sort(sortableGridViewColumn.SortPropertyName, ListSortDirection.Ascending);
					this.List.SelectedIndex = 0;
				}
			}
		}

        /// <summary>
        /// Event Handler for the ColumnHeader Click Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            // ensure that we clicked on the column header and not the padding that's added to fill the space.
            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                // attempt to cast to the sortableGridViewColumn object.
                SortableGridViewColumn sortableGridViewColumn = (headerClicked.Column) as SortableGridViewColumn;

                // ensure that the column header is the correct type and a sort property has been set.
                if (sortableGridViewColumn != null && !string.IsNullOrEmpty(sortableGridViewColumn.SortPropertyName))
                {
                    ListSortDirection direction;

                    // determine if this is a new sort, or a switch in sort direction.
					if(!this.sortedHeaders.ContainsKey(sortableGridViewColumn))
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (this.sortedHeaders[sortableGridViewColumn] == ListSortDirection.Ascending)
                            direction = ListSortDirection.Descending;
                        else
                            direction = ListSortDirection.Ascending;
                    }

					this.Sort(sortableGridViewColumn, direction);
				}
            }
        }

		private void Sort(SortableGridViewColumn sortableGridViewColumn, ListSortDirection direction)
		{
			// ensure that the column header is the correct type and a sort property has been set.
			if (sortableGridViewColumn != null && !string.IsNullOrEmpty(sortableGridViewColumn.SortPropertyName))
			{
				var c = this.List.Cursor;
				this.List.Cursor = Cursors.Wait;
				// get the sort property name from the column's information.
				this.Sort(sortableGridViewColumn.SortPropertyName, direction);
				this.List.Cursor = c;

				// Remove arrow from previously sorted header
				if (!this.IsCtrlKeyDown)
				{
					foreach (var entry in this.sortedHeaders)
					{
						entry.Key.HeaderTemplate = GetHeaderArrowTransparent(entry.Key);
					}
					sortedHeaders.Clear();
				}

				this.sortedHeaders[sortableGridViewColumn] = direction;
				sortableGridViewColumn.HeaderTemplate = direction == ListSortDirection.Ascending ? GetHeaderArrowDown(sortableGridViewColumn) : GetHeaderArrowUp(sortableGridViewColumn);
			}
		}

        /// <summary>
        /// Helper method that sorts the data.
        /// </summary>
        /// <param name="sortBy"></param>
        /// <param name="direction"></param>
        protected virtual void Sort(string sortBy, ListSortDirection direction)
        {
			ICollectionView dataView = this.CollectionView;
			if (dataView != null)
            {				
				if(!this.IsCtrlKeyDown)
					dataView.SortDescriptions.Clear();

                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }


		private DataTemplate GetHeader(SortableGridViewColumn column, PathFigure arrowPath, Brush lineBrush)
		{
			DataTemplate template = new DataTemplate();

			// Grid
			FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));
			FrameworkElementFactory RowDefinitionFactory1 = new FrameworkElementFactory(typeof(RowDefinition));
			gridFactory.AppendChild(RowDefinitionFactory1);
			FrameworkElementFactory RowDefinitionFactory2 = new FrameworkElementFactory(typeof(RowDefinition));
			gridFactory.AppendChild(RowDefinitionFactory2);

			// DockPanel
			FrameworkElementFactory dpFactory = new FrameworkElementFactory(typeof(DockPanel));
			dpFactory.SetValue(Grid.RowProperty, 0);
			dpFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
			gridFactory.AppendChild(dpFactory);

			// TextBlock
			FrameworkElementFactory tbFactory = new FrameworkElementFactory(typeof(TextBlock));
			tbFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
			tbFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top);
			tbFactory.SetBinding(TextBlock.TextProperty, new Binding());
			dpFactory.AppendChild(tbFactory);

			//Path
			PathGeometry pathGeometry = new PathGeometry() { Figures = new PathFigureCollection() { arrowPath } };

			FrameworkElementFactory pFactory = new FrameworkElementFactory(typeof(Path));
			pFactory.SetValue(Path.StrokeThicknessProperty, 1.0);
			pFactory.SetValue(Path.FillProperty, lineBrush);
			pFactory.SetValue(Path.DataProperty, pathGeometry);
			pFactory.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Top);
			dpFactory.AppendChild(pFactory);

			template.VisualTree = gridFactory;

			return template;
		}

		/// <summary>
		/// <DockPanel>
		///    <TextBlock HorizontalAlignment="Center" Text="{Binding}"/>
		///    <Path Name="arrow"
		///   StrokeThickness = "1"					  
		///   Fill            = "gray"
		///   Data            = "M 5,10 L 15,10 L 10,5 L 5,10"/>
		/// </DockPanel>
		/// </summary>
		private DataTemplate GetHeaderArrowUp(SortableGridViewColumn column)
		{
			return GetHeader(
				column,
				new PathFigure() 
				{ 
					StartPoint = new Point(5, 10),
					Segments = new PathSegmentCollection() 
						{ 
							new LineSegment() { Point = new Point(15, 10) }, 
							new LineSegment() { Point = new Point(10, 5) }, 
							new LineSegment() { Point = new Point(5, 10) } 
						}
				},
				Brushes.Gray
			);
		}

		/// <summary>
		/// <DockPanel>
		///     <TextBlock HorizontalAlignment="Center" Text="{Binding }"/>
		///     <Path Name="ArrowDown"
		///       StrokeThickness = "1"					  
		///       Fill            = "gray"
		///       Data            = "M 5,5 L 10,10 L 15,5 L 5,5"/>
		/// </DockPanel>
		/// </summary>
		private DataTemplate GetHeaderArrowDown(SortableGridViewColumn column)
		{
			return GetHeader(
				column,
				new PathFigure()
				{
					StartPoint = new Point(5, 5),
					Segments = new PathSegmentCollection()
					{ 
						new LineSegment() { Point = new Point(10, 10) }, 
						new LineSegment() { Point = new Point(15, 5) }, 
						new LineSegment() { Point = new Point(5, 5) } 
					}
				},
				Brushes.Gray
			);
		}

		/// <summary>
		/// <DockPanel>
		///     <TextBlock HorizontalAlignment="Center" Text="{Binding }"/>
		///     <Path Name="Transparent"
		///       StrokeThickness = "1"					  
		///       Fill            = "gray"
		///       Data            = "M 5,5 L 10,10 L 15,5 L 5,5"/>
		/// </DockPanel>
		/// </summary>
		private DataTemplate GetHeaderArrowTransparent(SortableGridViewColumn column)
		{
			return GetHeader(
				column,
				new PathFigure()
				{
					StartPoint = new Point(5, 5),
					Segments = new PathSegmentCollection() 
					{ 
						new LineSegment() { Point = new Point(10, 10) }, 
						new LineSegment() { Point = new Point(15, 5) }, 
						new LineSegment() { Point = new Point(5, 5) } 
					}
				},
				Brushes.Transparent
			);
		}
    }
}
