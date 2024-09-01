namespace Saltworks.Extensions {

    public static class SaltworksExtensions {

        public static void ForEach<T>(
            this IEnumerable<T> sequence,
            Action<T> action) {
            // argument null checking omitted
            foreach (T item in sequence)
                action(item);
        }
    }
}