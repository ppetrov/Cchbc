using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;
using Cchbc.Common;
using Cchbc.Objects;


namespace Cchbc.AppBuilder.UI
{
	public sealed partial class MainPage
	{
		//public AgendaViewModel ViewModel { get; } = new AgendaViewModel();
		public DbTableViewModel ViewModel { get; } = new DbTableViewModel();

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

	public sealed class DbTableViewModel : ViewModel
	{
		private string _ddl;
		public string Ddl
		{
			get { return _ddl; }
			private set { this.SetField(ref _ddl, value); }
		}

		private string _sourceCode;
		public string SourceCode
		{
			get { return _sourceCode; }
			private set { this.SetField(ref _sourceCode, value); }
		}

		private string _columnName = string.Empty;
		public string ColumnName
		{
			get { return _columnName; }
			set
			{
				this.SetField(ref _columnName, value);
				this.AddColumnCommand.RaiseCanExecuteChanged();
			}
		}

		private DbColumnTypeViewModel _columnType;
		public DbColumnTypeViewModel ColumnType
		{
			get { return _columnType; }
			set
			{
				this.SetField(ref _columnType, value);
				this.AddColumnCommand.RaiseCanExecuteChanged();
			}
		}

		private bool? _columnAllowNull = false;
		public bool? ColumnAllowNull
		{
			get { return _columnAllowNull; }
			set { this.SetField(ref _columnAllowNull, value); }
		}

		public RelayCommand AddColumnCommand { get; }

		public ObservableCollection<DbColumnViewModel> Columns { get; } = new ObservableCollection<DbColumnViewModel>();

		public DbTableViewModel()
		{
			this.AddColumnCommand = new RelayCommand(this.AddColumn, this.CanAddColumn);
			this.Columns.Add(new DbColumnViewModel(DbColumn.PrimaryKey(), this));
			this.Columns.Add(new DbColumnViewModel(DbColumn.String(@"Name"), this));
			this.Columns.Add(new DbColumnViewModel(DbColumn.String(@"Description", true), this));
		}

		private bool CanAddColumn()
		{
			return !string.IsNullOrWhiteSpace(this.ColumnName) && this.ColumnType != null;
		}

		private void AddColumn()
		{
			this.Columns.Add(new DbColumnViewModel(new DbColumn(this.ColumnName, this.ColumnType.Model, this.ColumnAllowNull ?? false), this));

			this.ColumnName = string.Empty;
			this.ColumnAllowNull = false;

			this.DisplayDdl();
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
			this.Ddl = DbScript.CreateTable(table);
			var entities = prj.CreateEntities();
			this.SourceCode = prj.CreateEntityClass(entities[0]);
		}

		public void Delete(DbColumnViewModel columnViewModel)
		{
			if (columnViewModel == null) throw new ArgumentNullException(nameof(columnViewModel));

			this.Columns.Remove(columnViewModel);

			this.DisplayDdl();
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
		private DbTableViewModel ParentViewModel { get; }

		public string Type { get; }
		public string Name { get; }
		public string Nullability { get; }

		public ICommand DeleteCommand { get; }

		public DbColumnViewModel(DbColumn model, DbTableViewModel parentViewModel) : base(model)
		{
			if (parentViewModel == null) throw new ArgumentNullException(nameof(parentViewModel));

			this.ParentViewModel = parentViewModel;
			this.Type = model.Type.Name;
			this.Name = model.Name;
			this.Nullability = model.IsNullable ? @"NULL" : @"NOT NULL";

			this.DeleteCommand = new RelayCommand(this.Delete);
		}

		private void Delete()
		{
			this.ParentViewModel.Delete(this);
		}
	}
}
