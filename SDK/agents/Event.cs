using Saffron.Runtime;

public static class EventAgent
{
    public async static Task EmitEvent(List<EmitEventArgs> events)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = new EmitEventRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
        };
        req.Events.AddRange(events.Select(e => e.ToProto()));

        await client.EmitEventAsync(req);
    }

    public async static Task<SearchEventResponse> SearchEvent(SearchEventRequest search)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = search.ToProto(Global.TaskKey);
        var resp = await client.SearchEventAsync(req);

        return new SearchEventResponse(resp);
    }

    public async static Task<SearchEventWithPatternResponse> SearchEventWithPattern(SearchEventWithPatternRequest search)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = search.ToProto(Global.TaskKey);
        var resp = await client.SearchEventWithPatternAsync(req);

        return new SearchEventWithPatternResponse(resp);
    }
}

public class EmitEventArgs
{
    public string LabelName { get; set; }
    public string SourceDigitalIdentity { get; set; }
    public string TargetDigitalIdentity { get; set; }
    public string Meta { get; set; }
    public string Type { get; set; }

    public EmitEventArgs(
        string labelName,
        string sourceDigitalIdentity,
        string targetDigitalIdentity,
        string meta,
        string type
    )
    {
        this.LabelName = labelName;
        this.SourceDigitalIdentity = sourceDigitalIdentity;
        this.TargetDigitalIdentity = targetDigitalIdentity;
        this.Meta = meta;
        this.Type = type;
    }

    public EmitEventRequest.Types.EventArgs ToProto()
    {
        return new EmitEventRequest.Types.EventArgs
        {
            LabelName = LabelName,
            SourceDigitalIdentity = SourceDigitalIdentity,
            TargetDigitalIdentity = TargetDigitalIdentity,
            Meta = Meta,
            Type = Type,
        };
    }
}

public class Query
{
    public string Field { get; set; }
    public IQueryType QueryType { get; set; }

    public interface IQueryType { }

    public class Match : IQueryType
    {
        public string Value { get; set; }

        public Match(string Value)
        {
            this.Value = Value;
        }
    }

    public class MatchPhrase : IQueryType
    {
        public string Value { get; set; }

        public MatchPhrase(string Value)
        {
            this.Value = Value;
        }
    }

    public class Term : IQueryType
    {
        public string Value { get; set; }

        public Term(string value)
        {
            this.Value = value;
        }
    }

    public Query(string field, IQueryType queryType)
    {
        this.Field = field;
        this.QueryType = queryType;
    }

    public Saffron.Event.Query ToProto()
    {
        var query = new Saffron.Event.Query
        {
            Field = Field,
        };

        switch (QueryType)
        {
            case Match match:
                query.Match = new Saffron.Event.Query.Types.Match
                {
                    Value = match.Value,
                };
                break;

            case MatchPhrase matchPhrase:
                query.MatchPhrase = new Saffron.Event.Query.Types.MatchPhrase
                {
                    Value = matchPhrase.Value,
                };
                break;

            case Term term:
                query.Term = new Saffron.Event.Query.Types.Term
                {
                    Value = term.Value,
                };
                break;
        }

        return query;
    }
}

public class Filter
{
    public string Field { get; set; }
    public IQuery Query { get; set; }

    public interface IQuery { }

    public class Range : IQuery
    {
        public ulong? Gte { get; set; }
        public ulong? Lte { get; set; }
    }

    public class Wildcard : IQuery
    {
        public string Value { get; set; }

        public Wildcard(string value)
        {
            this.Value = value;
        }
    }

    public Filter(string field, IQuery query)
    {
        this.Field = field;
        this.Query = query;
    }

    public Saffron.Event.Filter ToProto()
    {
        var filter = new Saffron.Event.Filter
        {
            Field = Field,
        };

        switch (Query)
        {
            case Range range:
                filter.Range = new Saffron.Event.Filter.Types.Range
                {
                    Gte = range.Gte ?? 0,
                    Lte = range.Lte ?? 0,
                };
                break;

            case Wildcard wildcard:
                filter.Wildcard = new Saffron.Event.Filter.Types.Wildcard
                {
                    Value = wildcard.Value,
                };
                break;
        }

        return filter;
    }
}

public enum SortOrder
{
    SortOrderUnspecified = 0,
    SortOrderAsc = 1,
    SortOrderDesc = 2
}

public class Sort
{
    public string Field { get; set; }
    public SortOrder Order { get; set; }

    public Sort(string field, SortOrder order)
    {
        this.Field = field;
        this.Order = order;
    }

    public Saffron.Event.Sort ToProto()
    {
        return new Saffron.Event.Sort
        {
            Field = Field,
            Order = Order switch
            {
                SortOrder.SortOrderAsc => Saffron.Event.SortOrder.Asc,
                SortOrder.SortOrderDesc => Saffron.Event.SortOrder.Desc,
                _ => Saffron.Event.SortOrder.Unspecified,
            },
        };
    }
}

public class Aggregation
{
    public List<Query> Queries { get; set; } = new List<Query>();
    public ulong? Size { get; set; }
    public Dictionary<string, string> After { get; set; } = new Dictionary<string, string>();

    public class Query
    {
        public string Field { get; set; }
        public IAggregation AggregationType { get; set; }

        public interface IAggregation { }

        public class Terms : IAggregation
        {
            public SortOrder? Order { get; set; }
        }

        public class DateHistogram : IAggregation
        {
            public string Interval { get; set; }
            public SortOrder? Order { get; set; }

            public DateHistogram(string Interval)
            {
                this.Interval = Interval;
            }

            public DateHistogram(string interval, SortOrder order)
            {
                this.Interval = interval;
                this.Order = order;
            }
        }

        public Query(string field, IAggregation aggregationType)
        {
            this.Field = field;
            this.AggregationType = aggregationType;
        }

        public Saffron.Event.Aggregation.Types.Query ToProto()
        {
            var query = new Saffron.Event.Aggregation.Types.Query
            {
                Field = Field,
            };

            switch (AggregationType)
            {
                case Terms terms:
                    query.Terms = new Saffron.Event.Aggregation.Types.Query.Types.Terms
                    {
                        Order = terms.Order switch
                        {
                            SortOrder.SortOrderAsc => Saffron.Event.SortOrder.Asc,
                            SortOrder.SortOrderDesc => Saffron.Event.SortOrder.Desc,
                            _ => Saffron.Event.SortOrder.Unspecified,
                        },
                    };
                    break;

                case DateHistogram dateHistogram:
                    query.DateHistogram = new Saffron.Event.Aggregation.Types.Query.Types.DateHistogram
                    {
                        Interval = dateHistogram.Interval,
                        Order = dateHistogram.Order switch
                        {
                            SortOrder.SortOrderAsc => Saffron.Event.SortOrder.Asc,
                            SortOrder.SortOrderDesc => Saffron.Event.SortOrder.Desc,
                            _ => Saffron.Event.SortOrder.Unspecified,
                        },
                    };
                    break;
            }

            return query;
        }
    }

    public Aggregation(
        List<Query> Queries,
        ulong? Size,
        Dictionary<string, string> After
    )
    {
        this.Queries = Queries;
        this.Size = Size;
        this.After = After;
    }

    public Saffron.Event.Aggregation ToProto()
    {
        var aggregation = new Saffron.Event.Aggregation
        {
            Size = Size ?? 0,
            After = { After },
        };
        aggregation.Queries.AddRange(Queries.Select(q => q.ToProto()).ToList());

        return aggregation;
    }
}

public class SearchEventRequest
{
    public List<Query> Queries { get; set; }
    public List<Query> Excludes { get; set; }
    public List<Filter> Filters { get; set; }
    public List<Sort> Sorts { get; set; }
    public Aggregation Aggregation { get; set; }
    public ulong From { get; set; }
    public ulong Size { get; set; }

    public SearchEventRequest(
        List<Query> queries,
        List<Query> excludes,
        List<Filter> filters,
        List<Sort> sorts,
        Aggregation aggregation,
        ulong from,
        ulong size
    )
    {
        this.Queries = queries;
        this.Excludes = excludes;
        this.Filters = filters;
        this.Sorts = sorts;
        this.Aggregation = aggregation;
        this.From = from;
        this.Size = size;
    }

    public Saffron.Runtime.SearchEventRequest ToProto(TaskKey taskKey)
    {
        var request = new Saffron.Runtime.SearchEventRequest
        {
            TaskKey = taskKey.ToProto(),
            Aggregation = Aggregation.ToProto(),
            From = From,
            Size = Size,
        };
        request.Queries.AddRange(Queries.Select(q => q.ToProto()).ToList());
        request.Excludes.AddRange(Excludes.Select(q => q.ToProto()).ToList());
        request.Filters.AddRange(Filters.Select(q => q.ToProto()).ToList());
        request.Sorts.AddRange(Sorts.Select(q => q.ToProto()).ToList());

        return request;
    }
}

public class Event
{
    public VersionedIdentityContext DataProcessIdentityContext { get; set; }
    public VersionedIdentityContext LogicIdentityContext { get; set; }
    public string ExecutionId { get; set; }
    public string TaskId { get; set; }
    public DateTime Timestamp { get; set; }
    public string SourceDigitalIdentity { get; set; }
    public string TargetDigitalIdentity { get; set; }
    public string LabelId { get; set; }
    public string LabelName { get; set; }
    public ulong Sequence { get; set; }
    public string Type { get; set; }
    public string Meta { get; set; }

    public Event(
        VersionedIdentityContext dataProcessIdentityContext,
        VersionedIdentityContext logicIdentityContext,
        string executionId,
        string taskId,
        DateTime timestamp,
        string sourceDigitalIdentity,
        string targetDigitalIdentity,
        string labelId,
        string labelName,
        ulong sequence,
        string type,
        string meta
    )
    {
        this.DataProcessIdentityContext = dataProcessIdentityContext;
        this.LogicIdentityContext = logicIdentityContext;
        this.ExecutionId = executionId;
        this.TaskId = taskId;
        this.Timestamp = timestamp;
        this.SourceDigitalIdentity = sourceDigitalIdentity;
        this.TargetDigitalIdentity = targetDigitalIdentity;
        this.LabelId = labelId;
        this.LabelName = labelName;
        this.Sequence = sequence;
        this.Type = type;
        this.Meta = meta;
    }

    public Event(Saffron.Event.Event proto)
    {
        this.DataProcessIdentityContext = new VersionedIdentityContext(proto.DataProcessIdentityContext);
        this.LogicIdentityContext = new VersionedIdentityContext(proto.LogicIdentityContext);
        this.ExecutionId = proto.ExecutionId;
        this.TaskId = proto.TaskId;
        this.Timestamp = proto.Timestamp.ToDateTime();
        this.SourceDigitalIdentity = proto.SourceDigitalIdentity;
        this.TargetDigitalIdentity = proto.TargetDigitalIdentity;
        this.LabelId = proto.LabelId;
        this.LabelName = proto.LabelName;
        this.Sequence = proto.Sequence;
        this.Type = proto.Type;
        this.Meta = proto.Meta;
    }
}

public class AggregationResult
{
    public List<Dictionary<string, string>> Buckets { get; set; } = new List<Dictionary<string, string>>();
    public Dictionary<string, string> After { get; set; } = new Dictionary<string, string>();

    public AggregationResult(
        List<Dictionary<string, string>> buckets,
        Dictionary<string, string> after
    )
    {
        Buckets = buckets;
        After = after;
    }

    public AggregationResult(Saffron.Event.AggregationResult proto)
    {
        this.Buckets = proto.Buckets.Select(b => b.Key.ToDictionary(k => k.Key, k => k.Value)).ToList();
        this.After = proto.After.ToDictionary(k => k.Key, k => k.Value);
    }
}

public class SearchEventResponse
{
    public ulong Took { get; set; }
    public ulong Count { get; set; }
    public ulong Total { get; set; }
    public List<Event> Events { get; set; }
    public AggregationResult Aggregation { get; set; }

    public SearchEventResponse(
        ulong took,
        ulong count,
        ulong total,
        List<Event> events,
        AggregationResult aggregation
    )
    {
        Took = took;
        Count = count;
        Total = total;
        Events = events;
        Aggregation = aggregation;
    }

    public SearchEventResponse(Saffron.Runtime.SearchEventResponse proto)
    {
        Took = proto.Took;
        Count = proto.Count;
        Total = proto.Total;
        Events = proto.Events.Select(e => new Event(e)).ToList();
        Aggregation = new AggregationResult(proto.Aggregation);
    }
}



// message SearchEventWithPatternRequest {
//   repeated saffron.event.Sequence sequences = 2;
//   optional string max_span = 3;
//   optional saffron.event.Filter filter = 4;
// }

// message SearchEventWithPatternResponse {
//   message SequenceResult {
//     repeated string join_keys = 1;
//     repeated saffron.event.Event events = 2;
//   }

//   uint64 took = 1;
//   uint64 count = 2;
//   uint64 total = 3;
//   repeated SequenceResult sequences = 4;
// }

// message Sequence {
//   message Condition {
//     enum Op {
//       OP_UNSPECIFIED = 0;
//       OP_EQ = 1;
//       OP_NE = 2;
//       OP_GT = 3;
//       OP_LT = 4;
//       OP_GTE = 5;
//       OP_LTE = 6;
//     }
//     Op op = 1;
//     string field = 2;
//     string value = 3;
//   }
//   repeated Condition conditions = 1;
//   repeated string shared_fields = 2;
//   optional string type = 3;
// }

public class Condition
{
    public string Field { get; set; }
    public string Value { get; set; }
    public string Op { get; set; }

    public Condition(
        string field,
        string value,
        string op
    )
    {
        this.Field = field;
        this.Value = value;
        this.Op = op;
    }

    public Saffron.Event.Sequence.Types.Condition ToProto()
    {
        return new Saffron.Event.Sequence.Types.Condition
        {
            Field = Field,
            Value = Value,
            Op = Op switch
            {
                "OP_UNSPECIFIED" => Saffron.Event.Sequence.Types.Condition.Types.Op.Unspecified,
                "OP_EQ" => Saffron.Event.Sequence.Types.Condition.Types.Op.Eq,
                "OP_NE" => Saffron.Event.Sequence.Types.Condition.Types.Op.Ne,
                "OP_GT" => Saffron.Event.Sequence.Types.Condition.Types.Op.Gt,
                "OP_LT" => Saffron.Event.Sequence.Types.Condition.Types.Op.Lt,
                "OP_GTE" => Saffron.Event.Sequence.Types.Condition.Types.Op.Gte,
                "OP_LTE" => Saffron.Event.Sequence.Types.Condition.Types.Op.Lte,
                _ => Saffron.Event.Sequence.Types.Condition.Types.Op.Unspecified,
            },
        };
    }
}

public class Sequence
{
    public List<Condition> Conditions { get; set; }
    public List<string> SharedFields { get; set; }
    public string Type { get; set; }

    public Sequence(
        List<Condition> conditions,
        List<string> sharedFields,
        string type
    )
    {
        this.Conditions = conditions;
        this.SharedFields = sharedFields;
        this.Type = type;
    }

    public Saffron.Event.Sequence ToProto()
    {
        var sequence = new Saffron.Event.Sequence
        {
            Type = Type,
        };
        sequence.Conditions.AddRange(Conditions.Select(c => c.ToProto()).ToList());
        sequence.SharedFields.AddRange(SharedFields);

        return sequence;
    }
}

public class SearchEventWithPatternRequest
{
    public List<Sequence> Sequences { get; set; }
    public string MaxSpan { get; set; }
    public Filter Filter { get; set; }

    public SearchEventWithPatternRequest(
        List<Sequence> sequences,
        string maxSpan,
        Filter filter
    )
    {
        this.Sequences = sequences;
        this.MaxSpan = maxSpan;
        this.Filter = filter;
    }

    public Saffron.Runtime.SearchEventWithPatternRequest ToProto(TaskKey taskKey)
    {
        var request = new Saffron.Runtime.SearchEventWithPatternRequest
        {
            TaskKey = taskKey.ToProto(),
            MaxSpan = MaxSpan,
            Filter = Filter.ToProto(),
        };
        request.Sequences.AddRange(Sequences.Select(s => s.ToProto()).ToList());

        return request;
    }
}

public class SequenceResult
{
    public List<string> JoinKeys { get; set; }
    public List<Event> Events { get; set; }

    public SequenceResult(
        List<string> joinKeys,
        List<Event> events
    )
    {
        this.JoinKeys = joinKeys;
        this.Events = events;
    }

    public SequenceResult(Saffron.Runtime.SearchEventWithPatternResponse.Types.SequenceResult proto)
    {
        this.JoinKeys = proto.JoinKeys.ToList();
        this.Events = proto.Events.Select(e => new Event(e)).ToList();
    }
}

public class SearchEventWithPatternResponse
{
    public List<SequenceResult> Sequences { get; set; }

    public SearchEventWithPatternResponse(
        List<SequenceResult> sequences
    )
    {
        this.Sequences = sequences;
    }

    public SearchEventWithPatternResponse(Saffron.Runtime.SearchEventWithPatternResponse proto)
    {
        this.Sequences = proto.Sequences.Select(s => new SequenceResult(s)).ToList();
    }
}