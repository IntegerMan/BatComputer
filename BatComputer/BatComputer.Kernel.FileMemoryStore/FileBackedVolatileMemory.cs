using Microsoft.SemanticKernel.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;

namespace MattEland.BatComputer.Kernel.FileMemoryStore;

/// <summary>
/// WARNING: This is not a serious memory store to use in production. This is designed to be a "good enough" concept of a store
/// for light usage during a demo or workshop that doesn't require a database, but still has a good chance of persisting small datasets correctly.
/// It's performance and reliability are bound to be abysmal. I also suspect it to have thread safety issues. Seriously, don't use this in production.
/// </summary>
public class FileBackedMemory : IMemoryStore, IEnumerable<MemoryRecordCollection>
{
    private readonly ConcurrentDictionary<string, MemoryRecordCollection> _collections = new();
    private readonly string _filePath;
    private readonly object _fileLock = new();

    public FileBackedMemory(string filePath)
    {
        _filePath = filePath;

        if (File.Exists(filePath))
        {
            LoadFromFile();
        }
        else
        {
            File.Create(filePath).Dispose();
        }
    }

    private void LoadFromFile()
    {
        lock (_fileLock)
        {
            // Load the file into memory
            string json = File.ReadAllText(_filePath);

            _collections.Clear();
            List<MemoryRecordCollection> collections = System.Text.Json.JsonSerializer.Deserialize<List<MemoryRecordCollection>>(json)!;

            // Add them to the store
            foreach (MemoryRecordCollection collection in collections)
            {
                _collections.TryAdd(collection.Collection, collection);
            }
        }
    }

    private void SaveToFile()
    {
        lock (_fileLock)
        {
            ICollection<MemoryRecordCollection> collections = _collections.Values;
            string json = System.Text.Json.JsonSerializer.Serialize(collections);
            File.WriteAllText(_filePath, json);
        }
    }

    public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (!_collections.ContainsKey(collectionName))
        {
            MemoryRecordCollection collection = new()
            {
                Collection = collectionName
            };
            _collections.TryAdd(collectionName, collection);

            SaveToFile();
        }

        return Task.CompletedTask;
    }

    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (_collections.TryRemove(collectionName, out _))
        {
            SaveToFile();
        }

        return Task.CompletedTask;
    }

    public Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
        => Task.FromResult(_collections.ContainsKey(collectionName));

    public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        _collections.TryGetValue(collectionName, out MemoryRecordCollection? collection);

        if (collection == null)
        {
            return Task.FromResult<MemoryRecord?>(null);
        }

        MemoryRecord? match = collection.Records.FirstOrDefault(r => r.Key == key);
        if (!withEmbedding && match != null)
        {
            match = MemoryRecord.FromMetadata(match.Metadata, ReadOnlyMemory<float>.Empty, match.Key, match.Timestamp);
        }

        return Task.FromResult(match);
    }

    public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (string key in keys)
        {
            MemoryRecord? memoryRecord = await GetAsync(collectionName, key, withEmbeddings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            if (memoryRecord != null)
            {
                yield return memoryRecord;
            }
        }
    }

    public IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken = default)
        => _collections.Keys.ToAsyncEnumerable();

    public IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collectionName, ReadOnlyMemory<float> embedding, int limit, double minRelevanceScore = 0.0, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        _collections.TryGetValue(collectionName, out MemoryRecordCollection? collection);
        if (collection == null || collection.Records.Count == 0 || limit <= 0)
        {
            return AsyncEnumerable.Empty<(MemoryRecord, double)>();
        }

        List<ScoredMemoryRecord> top = new(limit);
        foreach (MemoryRecord record in collection.Records)
        {
            if (record == null)
                continue;

            double score = TensorPrimitives.CosineSimilarity(embedding.Span, record.Embedding.Span);

            if (score >= minRelevanceScore)
            {
                MemoryRecord item = withEmbeddings
                    ? record
                    : MemoryRecord.FromMetadata(record.Metadata, ReadOnlyMemory<float>.Empty, record.Key, record.Timestamp);

                top.Add(new ScoredMemoryRecord(item, score));
            }
        }

        top = top.OrderBy(r => r.Score).Take(limit).ToList();

        return top.Select((x) => (x.Record, x.Score)).ToAsyncEnumerable();
    }

    public async Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore = 0.0, bool withEmbedding = false, CancellationToken cancellationToken = default)
        => await GetNearestMatchesAsync(collectionName, embedding, 1, minRelevanceScore, withEmbedding, cancellationToken).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

    public Task RemoveAsync(string collectionName, string key, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out MemoryRecordCollection? collection))
        {
            collection.Records.RemoveAll(r => r.Key == key);
            SaveToFile();
        }

        return Task.CompletedTask;
    }

    public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out MemoryRecordCollection? collection))
        {
            collection.Records.RemoveAll(r => keys.Contains(r.Key));
            SaveToFile();
        }

        return Task.CompletedTask;
    }

    public Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        if (!_collections.TryGetValue(collectionName, out MemoryRecordCollection? collection))
        {
            collection = new MemoryRecordCollection
            {
                Collection = collectionName
            };
            _collections.TryAdd(collectionName, collection);
        }

        collection.Records.RemoveAll(r => r.Key == record.Key);
        collection.Records.Add(record);

        SaveToFile();

        return Task.FromResult(record.Key);
    }

    public IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default)
    {
        if (!_collections.TryGetValue(collectionName, out MemoryRecordCollection? collection))
        {
            collection = new MemoryRecordCollection
            {
                Collection = collectionName
            };
            _collections.TryAdd(collectionName, collection);
        }

        foreach (var record in records)
        {
            collection.Records.RemoveAll(r => r.Key == record.Key);
            collection.Records.Add(record);
        }

        SaveToFile();

        return records.Select(r => r.Key).ToAsyncEnumerable();
    }

    public IEnumerator<MemoryRecordCollection> GetEnumerator()
    {
        foreach (MemoryRecordCollection collection in _collections.Values)
        {
            yield return collection;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
