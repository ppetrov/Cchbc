using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI.ArticlesModule.ViewModel
{
	//public sealed class OutletsViewModel
	//{
	//	public readonly ObservableCollection<OutletViewItem> ViewItems = new ObservableCollection<OutletViewItem>();

	//	private ContactPersonAdapter ContactPersonAdapter { get; }
	//	private OutletsModule OutletsModule { get; }
	//	private FeatureManager FeatureManager { get; }
	//	private ILogger Logger { get; }

	//	public OutletsViewModel(ILogger logger, ModifyDataQueryHelper queryHelper, FeatureManager featureManager)
	//	{
	//		if (logger == null) throw new ArgumentNullException(nameof(logger));
	//		if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
	//		if (featureManager == null) throw new ArgumentNullException(nameof(featureManager));

	//		this.Logger = logger;
	//		this.FeatureManager = featureManager;
	//		this.ContactPersonAdapter = new ContactPersonAdapter(queryHelper);

	//		var sorter = new Sorter<OutletViewItem>(new[]
	//		{
	//			new SortOption<OutletViewItem>(@"Name", (x,y)=>
	//			{
	//				var cmp = string.Compare(x.Item.Name, y.Item.Name, StringComparison.Ordinal);
	//				if (cmp == 0)
	//				{
	//					cmp = x.Item.Id.CompareTo(y.Item.Id);
	//				}
	//				return cmp;
	//			}),
	//			new SortOption<OutletViewItem>(@"City", (x, y) =>
	//			{
	//				var cmp = string.Compare(x.Item.City.Name, y.Item.City.Name, StringComparison.Ordinal);
	//				if (cmp == 0)
	//				{
	//					cmp = x.Item.Id.CompareTo(y.Item.Id);
	//				}
	//				return cmp;
	//			}),
	//		});
	//		var searcher = new Searcher<OutletViewItem>(new[]
	//		{
	//			new SearchOption<OutletViewItem>(@"RED", v => v.IsRed, true),
	//			new SearchOption<OutletViewItem>(@"With Equipments", v => v.HasEquipments),
	//		}, (v, s) => v.Item.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
	//		             v.Item.Address.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
	//		             v.Item.City.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);

	//		var filterOptions = new[]
	//		{
	//			new FilterOption<OutletViewItem>(@"Suppressed", v => v.Item.IsSuppressed, true),
	//			new FilterOption<OutletViewItem>(@"In territory", v => true),
	//		};


	//		this.OutletsModule = new OutletsModule(sorter, searcher, this.ContactPersonAdapter, filterOptions);
	//		this.OutletsModule.ContactPersonManager.ItemInserted += ContactPersonManagerOnItemInserted;
	//		this.OutletsModule.ContactPersonManager.ItemDeleted += ContactPersonManagerOnItemDeleted;
	//		this.OutletsModule.ContactPersonManager.OperationStart += (sender, args) =>
	//		{
	//			// TODO : Show progress
	//		};
	//		this.OutletsModule.ContactPersonManager.OperationEnd += (sender, args) =>
	//		{
	//			// TODO : Hide progress
	//			this.FeatureManager.Add(this.Logger.Context, args.Feature);
	//		};
	//		this.OutletsModule.ContactPersonManager.OperationError += (sender, args) =>
	//		{
	//			this.Logger.Error(args.Exception.ToString());
	//		};
	//	}

	//	private void ContactPersonManagerOnItemInserted(object sender, ObjectEventArgs<ContactPersonViewItem> args)
	//	{
	//		var outlet = args.Item.Item.Outlet;
	//		foreach (var viewItem in this.ViewItems)
	//		{
	//			if (viewItem.Item.Id == outlet)
	//			{
	//				this.OutletsModule.ContactPersonManager.Insert(viewItem.Contacts, args.Item, string.Empty, null);
	//				break;
	//			}
	//		}
	//	}

	//	private void ContactPersonManagerOnItemDeleted(object sender, ObjectEventArgs<ContactPersonViewItem> args)
	//	{
	//		var outlet = args.Item.Item.Outlet;
	//		foreach (var viewItem in this.ViewItems)
	//		{
	//			if (viewItem.Item.Id == outlet)
	//			{
	//				this.OutletsModule.ContactPersonManager.Delete(viewItem.Contacts, args.Item);
	//				break;
	//			}
	//		}
	//	}

	//	public async Task LoadAsync()
	//	{
	//		// TODO : !!! Load data
	//		var viewItems = new OutletViewItem[0];

	//		var contacts = (await this.ContactPersonAdapter.GetAllAsync());

	//		this.OutletsModule.LoadData(viewItems);

	//		this.ViewItems.Clear();
	//		foreach (var viewItem in this.OutletsModule.Search(string.Empty, this.OutletsModule.Searcher.CurrentOption))
	//		{
	//			FillContacts(viewItem, contacts);

	//			this.ViewItems.Add(viewItem);
	//		}
	//	}

	//	// Data loader ???

	//	private static void FillContacts(OutletViewItem viewItem, IEnumerable<ContactPerson> contacts)
	//	{
	//		var outlet = viewItem.Item.Id;
	//		var contactsViewItems = viewItem.Contacts;

	//		contactsViewItems.Clear();
	//		foreach (var contact in contacts)
	//		{
	//			if (contact.Outlet == outlet)
	//			{
	//				contactsViewItems.Add(new ContactPersonViewItem(contact));
	//			}
	//		}
	//	}

	//	public async Task AddContactAsync(ModalDialog dialog, OutletViewItem outletViewItem, ContactPerson contact)
	//	{
	//		if (outletViewItem == null) throw new ArgumentNullException(nameof(outletViewItem));
	//		if (contact == null) throw new ArgumentNullException(nameof(contact));
	//		if (dialog == null) throw new ArgumentNullException(nameof(dialog));

	//		contact.Outlet = outletViewItem.Item.Id;
	//		contact.IsPrimary = false;

	//		await this.OutletsModule.ContactPersonManager.AddAsync(new ContactPersonViewItem(contact), dialog, new Feature(@"Add Contact for Outlet"));
	//	}

	//	public async Task DeleteContactAsync(ModalDialog dialog, ContactPersonViewItem viewItem)
	//	{
	//		if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));
	//		if (dialog == null) throw new ArgumentNullException(nameof(dialog));

	//		await this.OutletsModule.ContactPersonManager.DeleteAsync(viewItem, dialog, new Feature(@"Delete Contact for Outlet"));
	//	}
	//}
}