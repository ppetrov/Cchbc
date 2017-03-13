using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Cchbc.AppBuilder.DDL;
using Cchbc.Common;


namespace Cchbc.AppBuilder.UI
{
	public sealed partial class MainPage
	{
		//public AgendaViewModel ViewModel { get; } = new AgendaViewModel();
		public DesignTableViewModel ViewModel { get; } = new DesignTableViewModel();

		public MainPage()
		{
			this.InitializeComponent();
		}

		private void LvColumnTypesItemClick(object sender, ItemClickEventArgs e)
		{
			this.ViewModel.Select(e.ClickedItem as DbColumnTypeViewModel);
		}

		private void LvTablesItemClick(object sender, ItemClickEventArgs e)
		{
			this.ViewModel.Select(e.ClickedItem as DbTableViewModel);
		}
	}

	public sealed class DbTableViewModelConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value as DbTableViewModel;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value as DbTableViewModel;
		}
	}

	public sealed class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value is Visibility && (Visibility)value == Visibility.Visible;
		}
	}

	public sealed class DesignTableViewModel : ViewModel
	{
		private string _tableName = string.Empty;
		public string TableName
		{
			get { return _tableName; }
			set { this.SetProperty(ref _tableName, value); }
		}

		private DbTableViewModel _selectedTable;
		public DbTableViewModel SelectedTable
		{
			get { return _selectedTable; }
			set
			{
				this.SetProperty(ref _selectedTable, value);
				this.Select(value);
			}
		}

		public ICommand AddTableCommand { get; }

		public ObservableCollection<DbColumnViewModel> Columns { get; } = new ObservableCollection<DbColumnViewModel>();
		public ObservableCollection<DbTableViewModel> Tables { get; } = new ObservableCollection<DbTableViewModel>();

		public ObservableCollection<DbColumnTypeViewModel> ColumnTypes { get; } = new ObservableCollection<DbColumnTypeViewModel>();

		public DesignTableViewModel()
		{
			this.AddTableCommand = new RelayCommand(this.AddTable);

			this.ColumnTypes.Add(new DbColumnTypeViewModel(DbColumnType.Integer, this));
			this.ColumnTypes.Add(new DbColumnTypeViewModel(DbColumnType.Decimal, this));
			this.ColumnTypes.Add(new DbColumnTypeViewModel(DbColumnType.String, this));
			this.ColumnTypes.Add(new DbColumnTypeViewModel(DbColumnType.DateTime, this));
			this.ColumnTypes.Add(new DbColumnTypeViewModel(DbColumnType.Bytes, this));
			this.ColumnTypes.Add(new DbColumnTypeViewModel(new DbColumnType(@"ForeignKey"), this));
		}

		private void AddTable()
		{
			if (string.IsNullOrWhiteSpace(this.TableName)) return;
			if (this.Columns.Count == 0) return;

			var columns = new List<DbColumn>(this.Columns.Count);
			foreach (var c in this.Columns)
			{
				columns.Add(c.Model);
			}
			this.Tables.Add(new DbTableViewModel(new DbTable(this.TableName, columns)));

			this.ClearTableAdd();
			this.Columns.Clear();
		}

		private void ClearTableAdd()
		{
			this.TableName = string.Empty;
		}

		public void CreateColumn(string name, DbColumnTypeViewModel columnTypeViewModel)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (columnTypeViewModel == null) throw new ArgumentNullException(nameof(columnTypeViewModel));

			// Automatically add primary key if it does't have
			var hasPrimaryKey = false;
			foreach (var column in this.Columns)
			{
				if (column.Model.IsPrimaryKey)
				{
					hasPrimaryKey = true;
					break;
				}
			}
			if (!hasPrimaryKey)
			{
				this.Columns.Add(new DbColumnViewModel(DbColumn.PrimaryKey(), this));
			}

			//this.FkTable != null ? DbColumn.ForeignKey(this.FkTable.Model) :
			this.Columns.Add(new DbColumnViewModel(new DbColumn(char.ToUpperInvariant(name[0]) + name.Substring(1), columnTypeViewModel.Model), this));
		}

		public void Delete(DbColumnViewModel columnViewModel)
		{
			if (columnViewModel == null) throw new ArgumentNullException(nameof(columnViewModel));

			this.Columns.Remove(columnViewModel);
		}

		public void ToggleNullability(DbColumnViewModel columnViewModel)
		{
			if (columnViewModel == null) throw new ArgumentNullException(nameof(columnViewModel));

			var index = this.Columns.IndexOf(columnViewModel);
			var model = columnViewModel.Model;
			this.Columns[index] = new DbColumnViewModel(new DbColumn(model.Name, model.Type, !model.IsNullable), this);
		}

		private void DisplayDdl()
		{
			var cls = new List<DbColumn>();
			foreach (var c in this.Columns)
			{
				cls.Add(c.Model);
			}
			var table = new DbTable(@"Unknown", cls);

			var prj = new DbProject(new DbSchema(@"Unknown", new[] { table }));
			var ddl = DbScript.CreateTable(table);
			var entities = prj.CreateEntities();
			var sourceCode = prj.CreateEntityClass(entities[0]);
		}

		public void Select(DbColumnTypeViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			foreach (var type in this.ColumnTypes)
			{
				if (type != viewModel)
				{
					type.IsSelected = false;
				}
			}

			viewModel.IsSelected = !viewModel.IsSelected;
		}

		public void Select(DbTableViewModel viewModel)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			this.Columns.Clear();
			foreach (var column in viewModel.Model.Columns)
			{
				this.Columns.Add(new DbColumnViewModel(column, this));
			}
		}
	}

	public sealed class DbColumnTypeViewModel : ViewModel<DbColumnType>
	{
		private DesignTableViewModel DesignTableViewModel { get; }

		public string Name { get; }
		private string _inputName = string.Empty;
		public string InputName
		{
			get { return _inputName; }
			set { this.SetProperty(ref _inputName, value); }
		}
		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { this.SetProperty(ref _isSelected, value); }
		}
		public string IconPath { get; }
		public ICommand CreateCommand { get; }

		public DbColumnTypeViewModel(DbColumnType model, DesignTableViewModel designTableViewModel) : base(model)
		{
			if (designTableViewModel == null) throw new ArgumentNullException(nameof(designTableViewModel));

			this.DesignTableViewModel = designTableViewModel;
			this.Name = model.Name;
			this.IconPath = $@"Assets/Icons/Type{model.Name}.png";
			this.CreateCommand = new RelayCommand(this.Create);
		}

		private void Create()
		{
			if (string.IsNullOrWhiteSpace(this.InputName)) return;

			this.DesignTableViewModel.CreateColumn(this.InputName, this);

			this.InputName = string.Empty;
			this.IsSelected = false;
		}
	}

	public sealed class DbColumnViewModel : ViewModel<DbColumn>
	{
		private DesignTableViewModel ParentViewModel { get; }

		public string Type { get; }
		public string Name { get; }
		public string Nullability { get; }
		public Symbol NullabilityIconSymbol { get; }

		public ICommand DeleteCommand { get; }
		public ICommand ToggleNullabilityCommand { get; }

		public DbColumnViewModel(DbColumn model, DesignTableViewModel parentViewModel) : base(model)
		{
			if (parentViewModel == null) throw new ArgumentNullException(nameof(parentViewModel));

			this.ParentViewModel = parentViewModel;
			this.Type = model.Type.Name;
			this.Name = model.Name;
			this.Nullability = model.IsNullable ? @"NULL" : @"NOT NULL";

			this.NullabilityIconSymbol = Symbol.Accept;
			if (model.IsNullable)
			{
				this.NullabilityIconSymbol = Symbol.Stop;
			}

			this.DeleteCommand = new RelayCommand(this.Delete);
			this.ToggleNullabilityCommand = new RelayCommand(this.ToggleNullability);
		}

		private void ToggleNullability()
		{
			this.ParentViewModel.ToggleNullability(this);
		}

		private void Delete()
		{
			this.ParentViewModel.Delete(this);
		}
	}

	public sealed class DbTableViewModel : ViewModel<DbTable>
	{
		public string Name { get; }

		public DbTableViewModel(DbTable model) : base(model)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));

			this.Name = model.Name;
		}
	}
}
