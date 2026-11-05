using Microsoft.AspNetCore.Http;

namespace RaceDay_POE_API.Tests.Helpers
{
    // A simple in-memory session so controllers can be tested without a web server
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => "test-session";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_store.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = Array.Empty<byte>();
            return false;
        }
    }
}