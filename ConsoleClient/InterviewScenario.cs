using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Atos.Client;

namespace ConsoleClient
{
	public sealed class User
	{
		public long Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public sealed class UserViewModel : ViewModel
	{
		public UserViewModel(User model)
		{
		}
	}




	public interface IUserDataProvider
	{
		List<User> GetUsers();
	}

	public sealed class DisplayUsersViewModel
	{
		public IUserDataProvider DataProvider { get; }
		public ObservableCollection<UserViewModel> Users { get; } = new ObservableCollection<UserViewModel>();

		public DisplayUsersViewModel(IUserDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void Load()
		{
			this.Users.Clear();
			foreach (var user in this.DataProvider.GetUsers())
			{
				this.Users.Add(new UserViewModel(user));
			}
		}
	}

	public interface IUserCreateDataProvider
	{
		bool Exists(string username);
		void Create(string username);
	}

	public interface IDbQueryContext
	{
		int Execute(DbQuery query);
		T ExecuteSingle<T>(DbQuery<T> query);
		List<T> Execute<T>(DbQuery<T> query);
	}

	public sealed class DbQueryContext : IDbQueryContext
	{
		public int Execute(DbQuery query)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSingle<T>(DbQuery<T> query)
		{
			throw new NotImplementedException();
		}

		public List<T> Execute<T>(DbQuery<T> query)
		{
			throw new NotImplementedException();
		}
	}

	public interface IDbDataReader
	{

	}

	public sealed class DbQuery
	{
		public string Statement { get; }
		public object[] Parameters { get; }

		public DbQuery(string statement, object[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			this.Statement = statement;
			this.Parameters = parameters;
		}
	}

	public sealed class DbQuery<T>
	{
		public string Statement { get; }
		public object[] Parameters { get; }
		public Func<IDbDataReader, T> Creator { get; set; }

		public DbQuery(string statement, object[] parameters, Func<IDbDataReader, T> creator)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));
			if (creator == null) throw new ArgumentNullException(nameof(creator));

			this.Statement = statement;
			this.Parameters = parameters;
			this.Creator = creator;
		}
	}

	public sealed class DbDataSeleteror
	{
		public static DbDataSeleteror DataSelector { get; } = new DbDataSeleteror(new DbQueryContext());

		private IDbQueryContext DbQueryContext { get; }
		private Dictionary<string, object> Cache { get; } = new Dictionary<string, object>();

		public DbDataSeleteror(IDbQueryContext dbQueryContext)
		{
			if (dbQueryContext == null) throw new ArgumentNullException(nameof(dbQueryContext));

			this.DbQueryContext = dbQueryContext;
		}

		public List<T> GetData<T>(DbQuery<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			List<T> data = null;

			var name = default(T).GetType().FullName;

			object cachedValue;
			if (!this.Cache.TryGetValue(name, out cachedValue))
			{
				data = this.QueryData(query);
				this.Cache.Add(name, data);
			}

			return data ?? cachedValue as List<T>;
		}

		public List<T> QueryData<T>(DbQuery<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.DbQueryContext.Execute(query);
		}
	}

	public sealed class UserCreateDataProvider : IUserCreateDataProvider
	{
		public IDbQueryContext QueryContext { get; }

		public UserCreateDataProvider(IDbQueryContext queryContext)
		{
			if (queryContext == null) throw new ArgumentNullException(nameof(queryContext));

			this.QueryContext = queryContext;
		}

		public bool Exists(string username)
		{
			if (username == null) throw new ArgumentNullException(nameof(username));

			return this.QueryContext.ExecuteSingle(new DbQuery<long?>("", null, r => default(long?))).HasValue;
		}

		public void Create(string username)
		{
			if (username == null) throw new ArgumentNullException(nameof(username));

			if (!this.Exists(username))
			{
				this.QueryContext.Execute(new DbQuery(@"insert into users", new[] { "username" }));
			}
		}
	}

	public sealed class CreateUserViewModel
	{
		public IUserCreateDataProvider DataProvider { get; }

		public CreateUserViewModel(IUserCreateDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void Create(string username)
		{
			if (username == null) throw new ArgumentNullException(nameof(username));

			var exists = this.DataProvider.Exists(username);
			if (exists)
			{
				// TODO : Display message
				return;
			}
			this.DataProvider.Create(username);
		}
	}


	public sealed class Product
	{
		public long Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public sealed class ProductViewModel : ViewModel
	{
		public ProductViewModel(Product model)
		{
		}
	}

	public interface IProductDataProvider
	{
		List<Product> GetProducts();
	}

	public sealed class ProductDataProvider : IProductDataProvider
	{
		public DbDataSeleteror DataSeleteror { get; }

		public ProductDataProvider(DbDataSeleteror dataSeleteror)
		{
			if (dataSeleteror == null) throw new ArgumentNullException(nameof(dataSeleteror));

			this.DataSeleteror = dataSeleteror;
		}

		public List<Product> GetProducts()
		{
			var query = new DbQuery<Product>(@"", new object[] { }, r =>
			{
				return default(Product);
			});

			// No cache
			//return this.DataSeleteror.QueryData<Product>(query);

			// Use cache
			return this.DataSeleteror.GetData<Product>(query);
		}
	}


	public sealed class DisplayProductsScreen
	{
		public DisplayProductsViewModel ViewModel { get; } = new DisplayProductsViewModel(new ProductDataProvider(DbDataSeleteror.DataSelector));

		public void Load()
		{
			this.ViewModel.Load();
		}
	}

	public sealed class DisplayProductsViewModel
	{
		public IProductDataProvider DataProvider { get; }
		public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();

		public DisplayProductsViewModel(IProductDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void Load()
		{
			this.Products.Clear();
			foreach (var product in this.DataProvider.GetProducts())
			{
				this.Products.Add(new ProductViewModel(product));
			}
		}
	}


	public sealed class CreateProductViewModel
	{
		public IProductCreateDataProvider DataProvider { get; }

		public CreateProductViewModel(IProductCreateDataProvider dataProvider)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.DataProvider = dataProvider;
		}

		public void Create(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var exists = this.DataProvider.Exists(name);
			if (exists)
			{
				// TODO : Display message
				return;
			}
			this.DataProvider.Create(name);
		}
	}

	public interface IProductCreateDataProvider
	{
		bool Exists(string name);
		void Create(string name);
	}

	public sealed class OrderProductItem
	{
		public Product Product { get; }
		public int Quantity { get; }

		public OrderProductItem(Product product, int quantity)
		{
			if (product == null) throw new ArgumentNullException(nameof(product));
			Product = product;
			Quantity = quantity;
		}
	}

	//public sealed class Order
	//{
	//	public long Id { get; set; }
	//	public long UserId { get; set; }
	//	public DateTime CreatedAt { get; set; }
	//	public List<OrderProductItem> Items { get; set; } = new List<OrderProductItem>();
	//}


}