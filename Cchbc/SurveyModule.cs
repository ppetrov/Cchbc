using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Helpers;
using Cchbc.Objects;

namespace Cchbc
{
	public sealed class Activity : IDbObject
	{
		public long Id { get; set; }
		public List<Survey> Surveys { get; set; }
		public List<NodeValue> Values { get; set; }
		public List<SurveyActivityHeader> Headers { get; set; }
	}

	public sealed class NodeValue : IDbObject
	{
		public long Id { get; set; }
		public Node Node { get; set; }
		public string Value { get; set; } = string.Empty;
	}

	public sealed class SurveyActivityHeader : IDbObject
	{
		public long Id { get; set; }
		public Activity Activity { get; set; }
		public Survey Survey { get; set; }
		public int Completed { get; set; }
	}

	public sealed class Survey : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public List<Node> Nodes { get; set; } = new List<Node>();
	}

	public sealed class Node : IDbObject
	{
		public long Id { get; set; }
		public NodeType Type { get; set; }
		public string Description { get; set; } = string.Empty;
	}

	public sealed class NodeType : IDbObject
	{
		public long Id { get; set; }
		public NodeValueType Type { get; set; }
		public string Name { get; set; }
		public List<NodeChoice> Choices { get; set; } = new List<NodeChoice>();
	}

	public sealed class NodeChoice : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public sealed class NodeChoiceAdapter : IReadOnlyAdapter<NodeChoice>
	{
		private readonly ReadDataQueryHelper _queryHelper;

		public NodeChoiceAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, NodeChoice> items)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));

			return _queryHelper.FillAsync(null, items);
		}
	}

	public sealed class NodeChoiceHelper : Helper<NodeChoice>
	{

	}

	public enum NodeValueType
	{
		Integer,
		Decimal,
		DateTime,
		String,
		ComboBox,
		List
	}

	public class SurveyModule
	{
		public async Task LoadAsync(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			// Load nodes choices
			NodeChoiceHelper ncHelper = new NodeChoiceHelper();
			await ncHelper.LoadAsync(new NodeChoiceAdapter(queryHelper));


			// Load surveys


		}
	}
}