using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Cchbc.AppBuilder.DDL;
using Cchbc.Common;
using Cchbc.Objects;


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

		private void ColumnTypeOnChecked(object sender, RoutedEventArgs e)
		{
			var value = ((sender as RadioButton).Tag) as string;
			switch (value)
			{
				case @"1":
					this.ViewModel.ColumnType = new DbColumnTypeViewModel(DbColumnType.Integer);
					break;
				case @"2":
					this.ViewModel.ColumnType = new DbColumnTypeViewModel(DbColumnType.Decimal);
					break;
				case @"3":
					this.ViewModel.ColumnType = new DbColumnTypeViewModel(DbColumnType.String);
					break;
				case @"4":
					this.ViewModel.ColumnType = new DbColumnTypeViewModel(DbColumnType.DateTime);
					break;
				case @"5":
					this.ViewModel.ColumnType = new DbColumnTypeViewModel(DbColumnType.Bytes);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
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

	public sealed class DesignTableViewModel : ViewModel
	{
		private string _tableName = string.Empty;
		public string TableName
		{
			get { return _tableName; }
			set { this.SetField(ref _tableName, value); }
		}

		private string _columnName = string.Empty;
		public string ColumnName
		{
			get { return _columnName; }
			set { this.SetField(ref _columnName, value); }
		}

		private DbColumnTypeViewModel _columnType;
		public DbColumnTypeViewModel ColumnType
		{
			get { return _columnType; }
			set
			{
				this.SetField(ref _columnType, value);
				this.CanSelectType = value != null;
			}
		}

		private DbTableViewModel _fkTable;
		public DbTableViewModel FkTable
		{
			get { return _fkTable; }
			set
			{
				this.SetField(ref _fkTable, value);
				this.CanSelectType = value == null;
			}
		}

		private bool _canSelectType = true;
		public bool CanSelectType
		{
			get { return _canSelectType; }
			set { this.SetField(ref _canSelectType, value); }
		}

		public ICommand AddColumnCommand { get; }
		public ICommand AddTableCommand { get; }

		public ObservableCollection<DbColumnViewModel> Columns { get; } = new ObservableCollection<DbColumnViewModel>();
		public ObservableCollection<DbTableViewModel> Tables { get; } = new ObservableCollection<DbTableViewModel>();


		public DesignTableViewModel()
		{
			this.AddColumnCommand = new RelayCommand(this.AddColumn);
			this.AddTableCommand = new RelayCommand(this.AddTable);
		}

		private void AddTable()
		{
			if (string.IsNullOrWhiteSpace(this.TableName)) return;
			if (this.Columns.Count == 0) return;

			var columns = new DbColumn[this.Columns.Count];
			for (var i = 0; i < this.Columns.Count; i++)
			{
				columns[i] = this.Columns[i].Model;
			}
			this.Tables.Add(new DbTableViewModel(new DbTable(this.TableName, columns)));

			this.ClearColumnAdd();
			this.ClearTableAdd();
			this.Columns.Clear();
		}

		private void ClearTableAdd()
		{
			this.TableName = string.Empty;
		}

		private void AddColumn()
		{
			var canAddRegularColumn = !string.IsNullOrWhiteSpace(this.ColumnName) && this.ColumnType != null;
			var canAddForeignKeyColumn = this.FkTable != null;
			if (!canAddRegularColumn && !canAddForeignKeyColumn) return;

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

			var dbColumn = this.FkTable != null ? DbColumn.ForeignKey(this.FkTable.Model) : new DbColumn(this.ColumnName, this.ColumnType.Model);
			this.Columns.Add(new DbColumnViewModel(dbColumn, this));

			this.ClearColumnAdd();
		}

		private void ClearColumnAdd()
		{
			// Clear column name to allow the addtion of another column
			this.ColumnName = string.Empty;
			this.FkTable = null;
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
			var cls = new DbColumn[this.Columns.Count];
			for (var i = 0; i < this.Columns.Count; i++)
			{
				cls[i] = this.Columns[i].Model;
			}
			var table = new DbTable(@"Unknown", cls);

			var prj = new DbProject(new DbSchema(@"Unknown", new[] { table }));
			var ddl = DbScript.CreateTable(table);
			var entities = prj.CreateEntities();
			var sourceCode = prj.CreateEntityClass(entities[0]);
		}
	}

	public sealed class DbColumnTypeViewModel : ViewModel<DbColumnType>
	{
		public DbColumnTypeViewModel(DbColumnType model) : base(model)
		{
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
