using System;
using System.Collections.Generic;
using System.Data;

namespace ConsoleClient
{
//	public sealed class DocumentType
//	{
//		public long Id { get; }
//		public string Name { get; }
//		public string Description { get; }

//		public DocumentType(long id, string name, string description)
//		{
//			if (name == null) throw new ArgumentNullException(nameof(name));
//			if (description == null) throw new ArgumentNullException(nameof(description));

//			this.Id = id;
//			this.Name = name;
//			this.Description = description;
//		}
//	}

//	public sealed class Document
//	{
//		public long Id { get; }
//		public string Name { get; }
//		public string Description { get; }
//		public List<DocumentTag> Tags { get; } = new List<DocumentTag>();
//		public DocumentType Type { get; }
//		public DateTime ValidFrom { get; } = DateTime.MinValue;
//		public DateTime ValidTo { get; } = DateTime.MaxValue;
//		public List<DocumentTradeChannel> TradeChannels { get; } = new List<DocumentTradeChannel>();
//		public List<DocumentSubTradeChannel> SybTradeChannels { get; } = new List<DocumentSubTradeChannel>();
//		public List<DocumentCcafSegment> CcafSegments { get; } = new List<DocumentCcafSegment>();
//		public List<DocumentCpl> Cpls { get; } = new List<DocumentCpl>();
//		public DocumentHierarchyLevel HierarchyLevel { get; }
//		public List<DocumentOutlet> Outlets { get; } = new List<DocumentOutlet>();
//		public List<DocumentOutletCluster> Clusters { get; } = new List<DocumentOutletCluster>();
//		public List<DocumentArticle> Articles { get; } = new List<DocumentArticle>();
//		public List<DocumentPromotion> Promotions { get; } = new List<DocumentPromotion>();
//		public List<DocumentImage> Images { get; } = new List<DocumentImage>();
//	}

//	public sealed class DocumentTag
//	{
//		public string Value { get; }

//		public DocumentTag(string value)
//		{
//			if (value == null) throw new ArgumentNullException(nameof(value));

//			this.Value = value;
//		}
//	}

//	public sealed class DocumentTradeChannel
//	{

//	}

//	public sealed class DocumentSubTradeChannel
//	{

//	}

//	public sealed class DocumentCcafSegment
//	{

//	}

//	public sealed class DocumentCpl
//	{

//	}

//	public sealed class DocumentHierarchyLevel
//	{

//	}

//	public sealed class DocumentOutlet
//	{

//	}

//	public sealed class DocumentOutletCluster
//	{

//	}

//	public sealed class DocumentArticle
//	{

//	}

//	public sealed class DocumentPromotion
//	{

//	}

//	public sealed class DocumentImage
//	{
//		public string Guid { get; }
//		public string Name { get; }
//		public string Description { get; }
//		public int Sequence { get; }
//		public byte[] Data { get; }

//		public DocumentImage(string guid, string name, string description, int sequence, byte[] data)
//		{
//			if (guid == null) throw new ArgumentNullException(nameof(guid));
//			if (name == null) throw new ArgumentNullException(nameof(name));
//			if (description == null) throw new ArgumentNullException(nameof(description));
//			if (data == null) throw new ArgumentNullException(nameof(data));

//			this.Guid = guid;
//			this.Name = name;
//			this.Description = description;
//			this.Sequence = sequence;
//			this.Data = data;
//		}
//	}



//	public sealed class DocumentsAdapter
//	{
//		public Func<IDbConnection> ConnectionProvider { get; }

//		public DocumentsAdapter(Func<IDbConnection> connectionProvider)
//		{
//			if (connectionProvider == null) throw new ArgumentNullException(nameof(connectionProvider));

//			this.ConnectionProvider = connectionProvider;
//		}

//		public void CreateSchema()
//		{
//			var queries = new[]
//			{
//				@"
//CREATE TABLE [DocumentTypes] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[Name] nvarchar(256) NOT NULL, 
//	[Local_Name] nvarchar(256) NOT NULL, 
//	[Description] nvarchar(1024) NOT NULL
//)",
//				@"
//CREATE TABLE [Documents] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[Name] nvarchar NOT NULL, 
//	[Description] nvarchar NOT NULL, 
//	[Tags] nvarchar NOT NULL, 
//	[Document_Type_Id] bigint NOT NULL, 
//	[Valid_From] datetime, 
//	[Valid_To] datetime, 
//	[Trade_Channels] nvarchar NOT NULL, 
//	[Sub_Trade_Channels] nvarchar NOT NULL, 
//	[Ccaf_Segments] nvarchar NOT NULL, 
//	[Cpls] nvarchar NOT NULL, 
//	[Customer_Hierarchy_Level6] nvarchar NOT NULL, 
//	[Customers] nvarchar NOT NULL, 
//	[CustomerClusters] nvarchar NOT NULL, 
//	[Article_Numbers] nvarchar NOT NULL, 
//	[Promotion_Ids] nvarchar NOT NULL, 
//	FOREIGN KEY ([Document_Type_Id])
//		REFERENCES [DocumentTypes] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE
//)",
//				@"
//CREATE TABLE [DocumentImages] (
//	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
//	[Guid] nvarchar(254) NOT NULL, 
//	[Name] nvarchar(254) NOT NULL, 
//	[Description] nvarchar(254) NOT NULL, 
//	[Is_Default] integer NOT NULL, 
//	[Image_Data] blob NOT NULL, 
//	[Document_Id] bigint NOT NULL, 
//	[Sequence] integer NOT NULL, 
//	FOREIGN KEY ([Document_Id])
//		REFERENCES [Documents] ([Id])
//		ON UPDATE CASCADE ON DELETE CASCADE
//)"
//			};

//			using (var cn = this.ConnectionProvider())
//			{
//				cn.Open();

//				//var queryHelper = new QueryHelper(cn);
//				//foreach (var query in queries)
//				//{
//				//	queryHelper.ExecuteQuery(query);
//				//}
//			}
//		}
//	}
}