using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Helpers;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.App.OrderModule
{
	public sealed class OrderType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

		public OrderType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class OrderTypeAdapter : IReadOnlyAdapter<OrderType>
	{
		public void Fill(ITransactionContext context, Dictionary<long, OrderType> items, Func<OrderType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			items.Add(1, new OrderType(1, @"ZOR"));
		}
	}

	public sealed class OrderTypeHelper : Helper<OrderType>
	{

	}





	public sealed class DeliveryAddress : IDbObject
	{
		public long Id { get; set; }
		public Outlet Outlet { get; }
		public string Name { get; }
		public bool IsPrimary { get; }

		public DeliveryAddress(long id, Outlet outlet, string name, bool isPrimary)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Outlet = outlet;
			this.Name = name;
			this.IsPrimary = isPrimary;
		}
	}

	public sealed class DeliveryAddressAdapter : IModifiableAdapter<DeliveryAddress>
	{
		public Task<List<DeliveryAddress>> GetByOutletAsync(Outlet outlet)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			var addresses = new List<DeliveryAddress>();

			addresses.Add(new DeliveryAddress(1, outlet, @"Sofia", true));

			return Task.FromResult(addresses);
		}

		public Task InsertAsync(ITransactionContext context, DeliveryAddress item)
		{
			//public long Id { get; set; }
			//public Outlet Outlet { get; }
			//public string Name { get; }
			//public bool IsPrimary { get; }

			throw new NotImplementedException();
		}

		public Task UpdateAsync(ITransactionContext context, DeliveryAddress item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(ITransactionContext context, DeliveryAddress item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class DeliveryAddressModule : Module<DeliveryAddress, DeliveryAddressViewModel>
	{
		public DeliveryAddressModule(ITransactionContextCreator contextCreator, IModifiableAdapter<DeliveryAddress> adapter,
			Sorter<DeliveryAddressViewModel> sorter,
			Searcher<DeliveryAddressViewModel> searcher, FilterOption<DeliveryAddressViewModel>[] filterOptions = null) : base(contextCreator, adapter, sorter, searcher, filterOptions)
		{
		}

		public override IEnumerable<ValidationResult> ValidateProperties(DeliveryAddressViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanInsertAsync(DeliveryAddressViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(DeliveryAddressViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(DeliveryAddressViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class DeliveryAddressViewModel : ViewModel<DeliveryAddress>
	{
		public string Name { get; }

		public DeliveryAddressViewModel(DeliveryAddress model) : base(model)
		{
			this.Name = model.Name;
		}
	}




	public sealed class Vendor : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

		public Vendor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class VendorViewModel : ViewModel<Vendor>
	{
		public string Name { get; }

		public VendorViewModel(Vendor model) : base(model)
		{
			this.Name = model.Name;
		}
	}

	public sealed class VendorAdapter : IReadOnlyAdapter<Vendor>
	{
		public void Fill(ITransactionContext context, Dictionary<long, Vendor> items, Func<Vendor, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			items.Add(1, new Vendor(1, @"Cchbc"));
		}
	}

	public sealed class VendorHelper : Helper<Vendor>
	{

	}





	public sealed class Wholesaler : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }

		public Wholesaler(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class WholesalerViewModel : ViewModel<Wholesaler>
	{
		public string Name { get; }

		public WholesalerViewModel(Wholesaler model) : base(model)
		{
			this.Name = model.Name;
		}
	}

	public sealed class WholesalerAdapter : IReadOnlyAdapter<Wholesaler>
	{
		public void Fill(ITransactionContext context, Dictionary<long, Wholesaler> items, Func<Wholesaler, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			items.Add(1, new Wholesaler(1, @"Metro"));
		}
	}

	public sealed class WholesalerHelper : Helper<Wholesaler>
	{

	}




	public sealed class Outlet : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class Activity : IDbObject
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
	}







	public sealed class OrderHeaderAdapter : IModifiableAdapter<OrderHeader>
	{
		public Task<OrderHeader> GetByIdAsync(ITransactionContextCreator contextCreator, Activity activity, Dictionary<long, OrderType> orderTypes, Dictionary<long, Vendor> vendors, Dictionary<long, Wholesaler> wholesalers)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));
			if (orderTypes == null) throw new ArgumentNullException(nameof(orderTypes));
			if (vendors == null) throw new ArgumentNullException(nameof(vendors));
			if (wholesalers == null) throw new ArgumentNullException(nameof(wholesalers));

			//var headers = await this.QueryHelper.ExecuteAsync(new Query<OrderHeader>(@"", r =>
			//{
			//	var orderHeader = new OrderHeader(1, activity);
			//	var a = orderTypes[0];
			//	var b = vendors[0];
			//	var d = wholesalers[0];
			//	return orderHeader;
			//}));
			//return headers[0];
			var oh = new OrderHeader(1, activity);

			oh.Type = new OrderType(1, string.Empty);
			oh.Vendor = new Vendor(1, string.Empty);
			oh.Wholesaler = new Wholesaler(1, string.Empty);
			oh.Address = new DeliveryAddress(1, activity.Outlet, string.Empty, false);

			return Task.FromResult(oh);
		}

		public Task InsertAsync(ITransactionContext context, OrderHeader item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ITransactionContext context, OrderHeader item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(ITransactionContext context, OrderHeader item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderTypeViewModel : ViewModel<OrderType>
	{
		public OrderTypeViewModel(OrderType model) : base(model)
		{
		}
	}


	public sealed class OrderHeader : IDbObject
	{
		public long Id { get; set; }
		public Activity Activity { get; }

		public OrderType Type { get; set; }
		public Vendor Vendor { get; set; }
		public Wholesaler Wholesaler { get; set; }
		public DeliveryAddress Address { get; set; }

		public OrderHeader(long id, Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			this.Id = id;
			this.Activity = activity;
		}
	}

	public sealed class Order
	{
		public OrderHeader OrderHeader { get; set; }
		public List<OrderDetail> Details { get; } = new List<OrderDetail>();
	}

	public sealed class ContextCreator : ITransactionContextCreator
	{
		public ITransactionContext Create()
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderManager
	{
		public Core Core { get; }
		public Activity Activity { get; }
		public Order Order { get; } = new Order();
		public ObservableCollection<OrderTypeViewModel> OrderTypes = new ObservableCollection<OrderTypeViewModel>();
		public ObservableCollection<VendorViewModel> Vendors = new ObservableCollection<VendorViewModel>();
		public ObservableCollection<WholesalerViewModel> Wholesalers = new ObservableCollection<WholesalerViewModel>();
		public ObservableCollection<DeliveryAddressViewModel> Addresses = new ObservableCollection<DeliveryAddressViewModel>();
		public ObservableCollection<OrderNoteViewModel> Notes = new ObservableCollection<OrderNoteViewModel>();
		public ObservableCollection<AssortmentViewModel> Assortments = new ObservableCollection<AssortmentViewModel>();

		private DeliveryAddressModule DeliveryAddressModule { get; }
		private OrderNoteModule OrderNoteModule { get; }
		private AssortmentModule AssortmentModule { get; }

		public OrderManager(Core core, Activity activity)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			this.Core = core;
			this.Activity = activity;

			var deliveryAddressSorter = new Sorter<DeliveryAddressViewModel>(new[]
			{
				new SortOption<DeliveryAddressViewModel>(@"Default",(x,y)=>
				{
					var cmp = x.Model.IsPrimary.CompareTo(y.Model.IsPrimary);
					if (cmp == 0)
					{
						cmp = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
					}
					return cmp;
				}),
			});
			var deliveryAddressSearcher = new Searcher<DeliveryAddressViewModel>(new[]
			{
				new SearchOption<DeliveryAddressViewModel>(@"All", v=> true),
				new SearchOption<DeliveryAddressViewModel>(@"Primary", v=> v.Model.IsPrimary),
			}, (vi, s) => vi.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
			this.DeliveryAddressModule = new DeliveryAddressModule(core.ContextCreator, new DeliveryAddressAdapter(), deliveryAddressSorter, deliveryAddressSearcher);


			var orderNoteSorter = new Sorter<OrderNoteViewModel>(new[]
			{
				new SortOption<OrderNoteViewModel>(@"Default", (x,y)=> string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)),
			});
			var orderNoteSearcher =
				new Searcher<OrderNoteViewModel>((vi, s) => vi.Type.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
														   vi.Contents.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
			this.OrderNoteModule = new OrderNoteModule(core.ContextCreator, new OrderNoteAdapter(), orderNoteSorter, orderNoteSearcher);

			var sorter = new Sorter<AssortmentViewModel>(new[]
			{
				new SortOption<AssortmentViewModel>(@"Number", (x,y)=> x.Model.Article.Id.CompareTo(y.Model.Article.Id)),
			});
			var searcher = new Searcher<AssortmentViewModel>(new[]
			{
				new SearchOption<AssortmentViewModel>(@"All", v => true),
				new SearchOption<AssortmentViewModel>(@"Empties", v => v.Model.Article.Brand == Brand.Empty),
			},
				(vi, s) => vi.Number.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
						   vi.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
			this.AssortmentModule = new AssortmentModule(core.ContextCreator, new AssortmentAdapter(), sorter, searcher);
		}

		public async Task LoadDataAsync()
		{
			var cache = this.Core.DataCache;

			var featureManager = this.Core.FeatureManager;
			var feature = featureManager.StartNew(nameof(OrderManager), nameof(LoadDataAsync));

			feature.AddStep(nameof(LoadOrderTypes));
			this.LoadOrderTypes(cache);

			feature.AddStep(nameof(LoadVendors));
			this.LoadVendors(cache);

			feature.AddStep(nameof(LoadWholesaler));
			this.LoadWholesaler(cache);

			feature.AddStep(nameof(LoadAddresses));
			await this.LoadAddresses();

			feature.AddStep(nameof(LoadAssortments));
			await this.LoadAssortments();

			feature.AddStep(nameof(LoadOrderHeader));
			await this.LoadOrderHeader();

			feature.AddStep(nameof(LoadOrderNotes));
			await this.LoadOrderNotes();

			feature.AddStep(nameof(LoadOrderDetails));
			await this.LoadOrderDetails();

			featureManager.StopAsync(feature);
		}

		private async Task LoadOrderDetails()
		{
			foreach (var detail in await new OrderDetailAdapter().GetByOrderAsync(this.Order.OrderHeader))
			{
				var articleId = detail.Article;

				// Find assortment
				var viewModel = default(AssortmentViewModel);

				foreach (var assortment in this.Assortments)
				{
					var article = assortment.Model.Article;
					if (article.Id == articleId)
					{
						viewModel = assortment;

						break;
					}
				}

				// No assortment found
				if (viewModel == null)
				{
					// Create new assortment
					var articleHelper = this.Core.DataCache.Get<Article>();
					var article = articleHelper.Items[articleId];
					var assortment = new Assortment(article);
					viewModel = new AssortmentViewModel(assortment);

					// Add to assortments
					this.Assortments.Add(viewModel);
				}

				// Set quantity
				viewModel.Quantity = detail.Quantity;
			}
		}

		private async Task LoadOrderHeader()
		{
			var cache = this.Core.DataCache;
			var orderHeader = await new OrderHeaderAdapter().GetByIdAsync(this.Core.ContextCreator, this.Activity,
				cache.Get<OrderType>().Items,
				cache.Get<Vendor>().Items,
				cache.Get<Wholesaler>().Items);

			orderHeader.Type = this.Core.DataCache.Get<OrderType>().Items[orderHeader.Type.Id];
			orderHeader.Vendor = this.Core.DataCache.Get<Vendor>().Items[orderHeader.Vendor.Id];
			orderHeader.Wholesaler = this.Core.DataCache.Get<Wholesaler>().Items[orderHeader.Wholesaler.Id];

			var addressId = orderHeader.Address.Id;
			foreach (var vi in this.Addresses)
			{
				var address = vi.Model;
				if (address.Id == addressId)
				{
					orderHeader.Address = address;
					break;
				}
			}

			this.Order.OrderHeader = orderHeader;
		}

		private async Task LoadAssortments()
		{
			var assortments = await new AssortmentAdapter().GetByOutletAsync(this.Activity.Outlet);
			var assortmentViewModels = new AssortmentViewModel[assortments.Count];
			for (var i = 0; i < assortments.Count; i++)
			{
				assortmentViewModels[i] = new AssortmentViewModel(assortments[i]);
			}
			this.AssortmentModule.SetupViewModels(assortmentViewModels);

			this.Assortments.Clear();
			foreach (var viewModel in this.AssortmentModule.ViewModels)
			{
				this.Assortments.Add(viewModel);
			}
		}

		private async Task LoadAddresses()
		{
			var addresses = await new DeliveryAddressAdapter().GetByOutletAsync(this.Activity.Outlet);

			var addressViewModels = new DeliveryAddressViewModel[addresses.Count];
			for (var i = 0; i < addresses.Count; i++)
			{
				addressViewModels[i] = new DeliveryAddressViewModel(addresses[i]);
			}
			this.DeliveryAddressModule.SetupViewModels(addressViewModels);

			this.Addresses.Clear();
			foreach (var viewModel in this.DeliveryAddressModule.ViewModels)
			{
				this.Addresses.Add(viewModel);
			}
		}

		private async Task LoadOrderNotes()
		{
			var adapter = new OrderNoteAdapter();
			var orderNotes = await adapter.GetByOutletAsync(this.Core.ContextCreator, this.Order.OrderHeader, this.Core.DataCache.Get<OrderNoteType>().Items);
			var orderNotesViewModels = new OrderNoteViewModel[orderNotes.Count];
			for (var i = 0; i < orderNotes.Count; i++)
			{
				orderNotesViewModels[i] = new OrderNoteViewModel(orderNotes[i]);
			}
			this.OrderNoteModule.SetupViewModels(orderNotesViewModels);

			this.Notes.Clear();
			foreach (var viewModel in this.OrderNoteModule.ViewModels)
			{
				this.Notes.Add(viewModel);
			}
		}

		private void LoadOrderTypes(DataCache cache)
		{
			var orderTypeHelper = cache.Get<OrderType>();
			this.OrderTypes.Clear();
			foreach (var orderType in orderTypeHelper.Items.Values)
			{
				this.OrderTypes.Add(new OrderTypeViewModel(orderType));
			}
		}

		private void LoadVendors(DataCache cache)
		{
			var vendorHelper = cache.Get<Vendor>();
			this.Vendors.Clear();
			foreach (var vendor in vendorHelper.Items.Values)
			{
				this.Vendors.Add(new VendorViewModel(vendor));
			}
		}

		private void LoadWholesaler(DataCache cache)
		{
			var wholesalerHelper = cache.Get<Wholesaler>();
			this.Wholesalers.Clear();
			foreach (var wholesaler in wholesalerHelper.Items.Values)
			{
				this.Wholesalers.Add(new WholesalerViewModel(wholesaler));
			}
		}
	}

	public sealed class OrderNoteType : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; }

		public OrderNoteType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class OrderNoteTypeAdapter : IReadOnlyAdapter<OrderNoteType>
	{
		public void Fill(ITransactionContext context, Dictionary<long, OrderNoteType> items, Func<OrderNoteType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			items.Add(1, new OrderNoteType(1, @"Driver"));
		}
	}

	public sealed class OrderNoteTypeHelper : Helper<OrderNoteType>
	{

	}

	public sealed class OrderNote : IDbObject
	{
		public long Id { get; set; }
		public OrderNoteType Type { get; set; }
		public string Contents { get; set; }

		public OrderNote(long id, OrderNoteType type, string contents)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			this.Id = id;
			this.Type = type;
			this.Contents = contents;
		}
	}

	public sealed class OrderNoteAdapter : IModifiableAdapter<OrderNote>
	{
		public Task<List<OrderNote>> GetByOutletAsync(ITransactionContextCreator contextCreator, OrderHeader orderHeader, Dictionary<long, OrderNoteType> types)
		{
			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));
			if (types == null) throw new ArgumentNullException(nameof(types));

			return Task.FromResult(new List<OrderNote>());
		}

		public Task InsertAsync(ITransactionContext context, OrderNote item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ITransactionContext context, OrderNote item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(ITransactionContext context, OrderNote item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderNoteModule : Module<OrderNote, OrderNoteViewModel>
	{
		public OrderNoteModule(ITransactionContextCreator contextCreator, IModifiableAdapter<OrderNote> adapter,
			Sorter<OrderNoteViewModel> sorter,
			Searcher<OrderNoteViewModel> searcher,
			FilterOption<OrderNoteViewModel>[] filterOptions = null) : base(contextCreator, adapter, sorter, searcher, filterOptions)
		{
		}

		public override IEnumerable<ValidationResult> ValidateProperties(OrderNoteViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanInsertAsync(OrderNoteViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(OrderNoteViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(OrderNoteViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderNoteViewModel : ViewModel<OrderNote>
	{
		public string Type { get; }
		public string Contents { get; }

		public OrderNoteViewModel(OrderNote model) : base(model)
		{
			this.Type = model.Type.Name;
			this.Contents = model.Contents;
		}
	}

	public sealed class OrderDetail : IDbObject
	{
		public long Id { get; set; }
		public long Article { get; set; }
		public long Quantity { get; set; }
	}

	public sealed class OrderDetailAdapter : IModifiableAdapter<OrderDetail>
	{
		public Task<List<OrderDetail>> GetByOrderAsync(OrderHeader orderHeader)
		{
			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));

			return Task.FromResult(new List<OrderDetail>());
		}

		public Task InsertAsync(ITransactionContext context, OrderDetail item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ITransactionContext context, OrderDetail item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(ITransactionContext context, OrderDetail item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class Assortment : IDbObject
	{
		public long Id { get; set; }
		public Article Article { get; }

		public Assortment(Article article)
		{
			if (article == null) throw new ArgumentNullException(nameof(article));

			this.Article = article;
		}

		public Assortment(long id, Article article)
		{
			if (article == null) throw new ArgumentNullException(nameof(article));

			this.Id = id;
			this.Article = article;
		}
	}

	public sealed class AssortmentAdapter : IModifiableAdapter<Assortment>
	{
		public Task<List<Assortment>> GetByOutletAsync(Outlet outlet)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			var assortments = new List<Assortment> { new Assortment(1, new Article(1, @"Coca Cola", Brand.Empty, Flavor.Empty)) };

			return Task.FromResult(assortments);
		}

		public Task InsertAsync(ITransactionContext context, Assortment item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(ITransactionContext context, Assortment item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(ITransactionContext context, Assortment item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AssortmentModule : Module<Assortment, AssortmentViewModel>
	{
		public AssortmentModule(ITransactionContextCreator contextCreator, IModifiableAdapter<Assortment> adapter,
			Sorter<AssortmentViewModel> sorter,
			Searcher<AssortmentViewModel> searcher,
			FilterOption<AssortmentViewModel>[] filterOptions = null) : base(contextCreator, adapter, sorter, searcher, filterOptions)
		{
		}

		public override IEnumerable<ValidationResult> ValidateProperties(AssortmentViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanInsertAsync(AssortmentViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(AssortmentViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(AssortmentViewModel viewModel, Feature feature)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AssortmentViewModel : ViewModel<Assortment>
	{
		public string Number { get; }
		public string Name { get; }
		private long _quantity;
		public long Quantity
		{
			get { return _quantity; }
			set { this.SetField(ref _quantity, value); }
		}

		public AssortmentViewModel(Assortment model) : base(model)
		{
			this.Number = model.Article.Id.ToString();
			this.Name = model.Article.Name;
		}
	}
}