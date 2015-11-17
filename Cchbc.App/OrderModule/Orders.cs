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
		public Task FillAsync(Dictionary<long, OrderType> items, Func<OrderType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			items.Add(1, new OrderType(1, @"ZOR"));

			return Task.FromResult(true);
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

		public Task InsertAsync(DeliveryAddress item)
		{
			//public long Id { get; set; }
			//public Outlet Outlet { get; }
			//public string Name { get; }
			//public bool IsPrimary { get; }

			throw new NotImplementedException();
		}

		public Task UpdateAsync(DeliveryAddress item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(DeliveryAddress item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class DeliveryAddressManager : Manager<DeliveryAddress, DeliveryAddressViewItem>
	{
		public DeliveryAddressManager(IModifiableAdapter<DeliveryAddress> adapter,
			Sorter<DeliveryAddressViewItem> sorter,
			Searcher<DeliveryAddressViewItem> searcher, FilterOption<DeliveryAddressViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		{
		}

		public override ValidationResult[] ValidateProperties(DeliveryAddressViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanInsertAsync(DeliveryAddressViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(DeliveryAddressViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(DeliveryAddressViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class DeliveryAddressViewItem : ViewItem<DeliveryAddress>
	{
		public string Name { get; }

		public DeliveryAddressViewItem(DeliveryAddress item) : base(item)
		{
			this.Name = item.Name;
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

	public sealed class VendorViewItem : ViewItem<Vendor>
	{
		public string Name { get; }

		public VendorViewItem(Vendor item) : base(item)
		{
			this.Name = item.Name;
		}
	}

	public sealed class VendorAdapter : IReadOnlyAdapter<Vendor>
	{
		public Task FillAsync(Dictionary<long, Vendor> items, Func<Vendor, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			items.Add(1, new Vendor(1, @"Cchbc"));

			return Task.FromResult(true);
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

	public sealed class WholesalerViewItem : ViewItem<Wholesaler>
	{
		public string Name { get; }

		public WholesalerViewItem(Wholesaler item) : base(item)
		{
			this.Name = item.Name;
		}
	}

	public sealed class WholesalerAdapter : IReadOnlyAdapter<Wholesaler>
	{
		public Task FillAsync(Dictionary<long, Wholesaler> items, Func<Wholesaler, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			items.Add(1, new Wholesaler(1, @"Metro"));

			return Task.FromResult(true);
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
		public QueryHelper QueryHelper { get; }

		public OrderHeaderAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task<OrderHeader> GetByIdAsync(Activity activity, Dictionary<long, OrderType> orderTypes, Dictionary<long, Vendor> vendors, Dictionary<long, Wholesaler> wholesalers)
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

		public Task InsertAsync(OrderHeader item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(OrderHeader item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(OrderHeader item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderTypeViewItem : ViewItem<OrderType>
	{
		public OrderTypeViewItem(OrderType item) : base(item)
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

	public sealed class OrderManager
	{
		public Core Core { get; }
		public Activity Activity { get; }
		public Order Order { get; } = new Order();
		public ObservableCollection<OrderTypeViewItem> OrderTypes = new ObservableCollection<OrderTypeViewItem>();
		public ObservableCollection<VendorViewItem> Vendors = new ObservableCollection<VendorViewItem>();
		public ObservableCollection<WholesalerViewItem> Wholesalers = new ObservableCollection<WholesalerViewItem>();
		public ObservableCollection<DeliveryAddressViewItem> Addresses = new ObservableCollection<DeliveryAddressViewItem>();
		public ObservableCollection<OrderNoteViewItem> Notes = new ObservableCollection<OrderNoteViewItem>();
		public ObservableCollection<AssortmentViewItem> Assortments = new ObservableCollection<AssortmentViewItem>();

		private DeliveryAddressManager DeliveryAddressManager { get; }
		private OrderNoteManager OrderNoteManager { get; }
		private AssortmentManager AssortmentManager { get; }

		public OrderManager(Core core, Activity activity)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			this.Core = core;
			this.Activity = activity;

			var deliveryAddressSorter = new Sorter<DeliveryAddressViewItem>(new[]
			{
				new SortOption<DeliveryAddressViewItem>(@"Default",(x,y)=>
				{
					var cmp = x.Item.IsPrimary.CompareTo(y.Item.IsPrimary);
					if (cmp == 0)
					{
						cmp = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
					}
					return cmp;
				}),
			});
			var deliveryAddressSearcher = new Searcher<DeliveryAddressViewItem>(new[]
			{
				new SearchOption<DeliveryAddressViewItem>(@"All", v=> true),
				new SearchOption<DeliveryAddressViewItem>(@"Primary", v=> v.Item.IsPrimary),
			}, (vi, s) => vi.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
			this.DeliveryAddressManager = new DeliveryAddressManager(new DeliveryAddressAdapter(), deliveryAddressSorter, deliveryAddressSearcher);


			var orderNoteSorter = new Sorter<OrderNoteViewItem>(new[]
			{
				new SortOption<OrderNoteViewItem>(@"Default", (x,y)=> string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase)),
			});
			var orderNoteSearcher =
				new Searcher<OrderNoteViewItem>((vi, s) => vi.Type.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
														   vi.Contents.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
			this.OrderNoteManager = new OrderNoteManager(new OrderNoteAdapter(this.Core.QueryHelper), orderNoteSorter, orderNoteSearcher);

			var sorter = new Sorter<AssortmentViewItem>(new[]
			{
				new SortOption<AssortmentViewItem>(@"Number", (x,y)=> x.Item.Article.Id.CompareTo(y.Item.Article.Id)),
			});
			var searcher = new Searcher<AssortmentViewItem>(new[]
			{
				new SearchOption<AssortmentViewItem>(@"All", v => true),
				new SearchOption<AssortmentViewItem>(@"Empties", v => v.Item.Article.Brand == Brand.Empty),
			},
				(vi, s) => vi.Number.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
						   vi.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
			this.AssortmentManager = new AssortmentManager(new AssortmentAdapter(), sorter, searcher);
		}

		public async Task LoadDataAsync()
		{
			var cache = this.Core.DataCache;

			var feature = Feature.StartNew(nameof(OrderManager), nameof(LoadDataAsync));

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

			var featureManager = this.Core.FeatureManager;
			featureManager.Stop(feature);
		}

		private async Task LoadOrderDetails()
		{
			foreach (var detail in await new OrderDetailAdapter().GetByOrderAsync(this.Order.OrderHeader))
			{
				var articleId = detail.Article;

				// Find assortment
				var viewItem = default(AssortmentViewItem);

				foreach (var assortment in this.Assortments)
				{
					var article = assortment.Item.Article;
					if (article.Id == articleId)
					{
						viewItem = assortment;

						break;
					}
				}

				// No assortment found
				if (viewItem == null)
				{
					// Create new assortment
					var articleHelper = this.Core.DataCache.GetHelper<Article>();
					var article = articleHelper.Items[articleId];
					var assortment = new Assortment(article);
					viewItem = new AssortmentViewItem(assortment);

					// Add to assortments
					this.Assortments.Add(viewItem);
				}

				// Set quantity
				viewItem.Quantity = detail.Quantity;
			}
		}

		private async Task LoadOrderHeader()
		{
			var cache = this.Core.DataCache;
			var orderHeader = await new OrderHeaderAdapter(this.Core.QueryHelper).GetByIdAsync(this.Activity,
				cache.GetHelper<OrderType>().Items,
				cache.GetHelper<Vendor>().Items,
				cache.GetHelper<Wholesaler>().Items);

			orderHeader.Type = this.Core.DataCache.GetHelper<OrderType>().Items[orderHeader.Type.Id];
			orderHeader.Vendor = this.Core.DataCache.GetHelper<Vendor>().Items[orderHeader.Vendor.Id];
			orderHeader.Wholesaler = this.Core.DataCache.GetHelper<Wholesaler>().Items[orderHeader.Wholesaler.Id];

			var addressId = orderHeader.Address.Id;
			foreach (var vi in this.Addresses)
			{
				var address = vi.Item;
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
			var assortmentViewItems = new AssortmentViewItem[assortments.Count];
			for (var i = 0; i < assortments.Count; i++)
			{
				assortmentViewItems[i] = new AssortmentViewItem(assortments[i]);
			}
			this.AssortmentManager.SetupData(assortmentViewItems);

			this.Assortments.Clear();
			foreach (var viewItem in this.AssortmentManager.ViewItems)
			{
				this.Assortments.Add(viewItem);
			}
		}

		private async Task LoadAddresses()
		{
			var addresses = await new DeliveryAddressAdapter().GetByOutletAsync(this.Activity.Outlet);

			var addressViewItems = new DeliveryAddressViewItem[addresses.Count];
			for (var i = 0; i < addresses.Count; i++)
			{
				addressViewItems[i] = new DeliveryAddressViewItem(addresses[i]);
			}
			this.DeliveryAddressManager.SetupData(addressViewItems);

			this.Addresses.Clear();
			foreach (var viewItem in this.DeliveryAddressManager.ViewItems)
			{
				this.Addresses.Add(viewItem);
			}
		}

		private async Task LoadOrderNotes()
		{
			var adapter = new OrderNoteAdapter(this.Core.QueryHelper);
			var orderNotes = await adapter.GetByOutletAsync(this.Order.OrderHeader, this.Core.DataCache.GetHelper<OrderNoteType>().Items);
			var orderNotesViewItems = new OrderNoteViewItem[orderNotes.Count];
			for (var i = 0; i < orderNotes.Count; i++)
			{
				orderNotesViewItems[i] = new OrderNoteViewItem(orderNotes[i]);
			}
			this.OrderNoteManager.SetupData(orderNotesViewItems);

			this.Notes.Clear();
			foreach (var viewItem in this.OrderNoteManager.ViewItems)
			{
				this.Notes.Add(viewItem);
			}
		}

		private void LoadOrderTypes(DataCache cache)
		{
			var orderTypeHelper = cache.GetHelper<OrderType>();
			this.OrderTypes.Clear();
			foreach (var orderType in orderTypeHelper.Items.Values)
			{
				this.OrderTypes.Add(new OrderTypeViewItem(orderType));
			}
		}

		private void LoadVendors(DataCache cache)
		{
			var vendorHelper = cache.GetHelper<Vendor>();
			this.Vendors.Clear();
			foreach (var vendor in vendorHelper.Items.Values)
			{
				this.Vendors.Add(new VendorViewItem(vendor));
			}
		}

		private void LoadWholesaler(DataCache cache)
		{
			var wholesalerHelper = cache.GetHelper<Wholesaler>();
			this.Wholesalers.Clear();
			foreach (var wholesaler in wholesalerHelper.Items.Values)
			{
				this.Wholesalers.Add(new WholesalerViewItem(wholesaler));
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
		public Task FillAsync(Dictionary<long, OrderNoteType> items, Func<OrderNoteType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			items.Add(1, new OrderNoteType(1, @"Driver"));

			return Task.FromResult(true);
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
		public QueryHelper QueryHelper { get; }

		public OrderNoteAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task<List<OrderNote>> GetByOutletAsync(OrderHeader orderHeader, Dictionary<long, OrderNoteType> types)
		{
			if (orderHeader == null) throw new ArgumentNullException(nameof(orderHeader));
			if (types == null) throw new ArgumentNullException(nameof(types));

			return Task.FromResult(new List<OrderNote>());
			//return this.QueryHelper.ExecuteAsync(new Query<OrderNote>(@"SELECT * FROM ORDER_NOTES", r =>
			//{
			//	return new OrderNote(-1, types[0], string.Empty);
			//}));
		}

		public Task InsertAsync(OrderNote item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(OrderNote item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(OrderNote item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderNoteManager : Manager<OrderNote, OrderNoteViewItem>
	{
		public OrderNoteManager(IModifiableAdapter<OrderNote> adapter, Sorter<OrderNoteViewItem> sorter, Searcher<OrderNoteViewItem> searcher, FilterOption<OrderNoteViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		{
		}

		public override ValidationResult[] ValidateProperties(OrderNoteViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanInsertAsync(OrderNoteViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(OrderNoteViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(OrderNoteViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class OrderNoteViewItem : ViewItem<OrderNote>
	{
		public string Type { get; }
		public string Contents { get; }

		public OrderNoteViewItem(OrderNote item) : base(item)
		{
			this.Type = item.Type.Name;
			this.Contents = item.Contents;
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

		public Task InsertAsync(OrderDetail item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(OrderDetail item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(OrderDetail item)
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

		public Task InsertAsync(Assortment item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(Assortment item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(Assortment item)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AssortmentManager : Manager<Assortment, AssortmentViewItem>
	{
		public AssortmentManager(IModifiableAdapter<Assortment> adapter, Sorter<AssortmentViewItem> sorter, Searcher<AssortmentViewItem> searcher, FilterOption<AssortmentViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		{
		}

		public override ValidationResult[] ValidateProperties(AssortmentViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanInsertAsync(AssortmentViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanUpdateAsync(AssortmentViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}

		public override Task<PermissionResult> CanDeleteAsync(AssortmentViewItem viewItem, Feature feature)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AssortmentViewItem : ViewItem<Assortment>
	{
		public string Number { get; }
		public string Name { get; }
		private long _quantity;
		public long Quantity
		{
			get { return _quantity; }
			set { this.SetField(ref _quantity, value); }
		}

		public AssortmentViewItem(Assortment item) : base(item)
		{
			this.Number = item.Article.Id.ToString();
			this.Name = item.Article.Name;
		}
	}
}