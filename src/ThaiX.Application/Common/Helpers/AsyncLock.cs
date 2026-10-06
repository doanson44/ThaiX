namespace ThaiX.Application.Common.Helpers;

// Asynchronous locking based on a string key.
// Inspired by: https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key
// Original: https://github.com/VirtoCommerce/vc-platform/blob/fb7de30881fb5008d64b3be684dba992029a0be9/src/VirtoCommerce.Platform.Core/Common/AsyncLock.cs
public sealed class AsyncLock
{
    private readonly string _key;

    private AsyncLock(string key)
    {
        _key = key;
    }

    private static readonly Dictionary<string, RefCounted<SemaphoreSlim>> _semaphoreSlims = new();

    private static SemaphoreSlim GetOrCreate(string key)
    {
        RefCounted<SemaphoreSlim> item;
        lock (_semaphoreSlims)
        {
            if (_semaphoreSlims.TryGetValue(key, out item!))
            {
                ++item.RefCount;
            }
            else
            {
                item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
                _semaphoreSlims[key] = item;
            }
        }
        return item.Value;
    }

    public static AsyncLock GetLockByKey(string key) => new(key);

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        await GetOrCreate(_key).WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(_key);
    }

    public readonly struct Releaser : IDisposable
    {
        private readonly string _key;

        public Releaser(string key)
        {
            _key = key;
        }

        public void Dispose()
        {
            RefCounted<SemaphoreSlim> item;
            lock (_semaphoreSlims)
            {
                item = _semaphoreSlims[_key];
                --item.RefCount;
                if (item.RefCount == 0)
                {
                    _semaphoreSlims.Remove(_key);
                }
            }
            item.Value.Release();
        }
    }

    private sealed class RefCounted<T>
    {
        public RefCounted(T value)
        {
            RefCount = 1;
            Value = value;
        }

        public int RefCount { get; set; }
        public T Value { get; }
    }
}
