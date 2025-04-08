namespace Dommel.Bulk.Extensions;

public static class EnumerableExtensions
{
#if NETSTANDARD2_0
    public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
          if (source == null)
              throw new ArgumentNullException(nameof(source));
          if (size < 1)
              throw new ArgumentOutOfRangeException(nameof(size));
          return ChunkIterator(source, size);
        }

        #nullable disable
        private static IEnumerable<TSource[]> ChunkIterator<TSource>(
          IEnumerable<TSource> source,
          int size)
        {
          using (IEnumerator<TSource> e = source.GetEnumerator())
          {
            if (e.MoveNext())
            {
              int arraySize = Math.Min(size, 4);
              int i;
              do
              {
                TSource[] array = new TSource[arraySize];
                array[0] = e.Current;
                i = 1;
                if (size != array.Length)
                {
                  for (; i < size && e.MoveNext(); ++i)
                  {
                    if (i >= array.Length)
                    {
                      arraySize = (int) Math.Min((uint) size, (uint) (2 * array.Length));
                      Array.Resize<TSource>(ref array, arraySize);
                    }
                    array[i] = e.Current;
                  }
                }
                else
                {
                  for (TSource[] sourceArray = array; (uint) i < (uint) sourceArray.Length && e.MoveNext(); ++i)
                    sourceArray[i] = e.Current;
                }
                if (i != array.Length)
                  Array.Resize<TSource>(ref array, i);
                yield return array;
              }
              while (i >= size && e.MoveNext());
            }
          }
        }

        #nullable enable

  #endif
}