namespace Landis.Extension.Succession.ForC
{
    public interface ITimeCollection<T> where T : ITimeInput
    {
        /// <summary>
        /// Returns true if there is an ITimeInput in the collection with the same Year, false otherwise.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Contains(T value);

        /// <summary>
        /// Adds an ITimeInput object to the collection.  If there is already an object
        /// for the given ITimeInput.Year in the collection, the existing object is
        /// removed and replaced by this one.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        void Add(T value);

        /// <summary>
        /// Returns True with the given ITimeInput object if found a 'matching' ITimeInput object in the collection.
        /// </summary>
        /// <param name="nYear"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue(int nYear, out T value);
    }
}
