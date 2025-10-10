using System.Collections.Generic;

namespace Landis.Extension.Succession.ForC
{
    public class TimeCollection<T> : ITimeCollection<T> where T : ITimeInput
    {
        protected SortedList<int, T> m_listValues = new SortedList<int, T>();

        /// <summary>
        /// Returns true if there is an ITimeInput (or derived) in the collection with the same Year, false otherwise.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return m_listValues.ContainsKey(value.Year);
        }

        /// <summary>
        /// Adds an ITimeInput object (or derived) to the collection.  If there is already an object
        /// for the given ITimeInput.Year in the collection, the existing object is
        /// removed and replaced by this one.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Add(T value)
        {
            if (Contains(value))
                m_listValues.Remove(value.Year);
            m_listValues.Add(value.Year, value);
        }

        /// <summary>
        /// Tries to get the matching ITimeInput (or derived) object for the given year.
        /// </summary>
        /// <param name="nYear"></param>
        /// <param name="value">When True is returned, value is set to the ITimeInput (or derived) object.</param>
        /// <returns>True - If there is an ITimeInput (or derived) object that 'matches' the given year.
        /// False otherwise.</returns>
        /// <remarks>Note that a match is the object which has the same or closest Year value.</remarks>
        public bool TryGetValue(int nYear, out T value)
        {
            value = default(T); // null
            // Iterate the list in reverse order, returning the ITimeInput object with a Year <= value.Year
            for (int n = m_listValues.Count - 1; n >= 0; n--)
            {
                if (m_listValues.Values[n].Year <= nYear)
                {
                    value = m_listValues.Values[n];
                    return true;
                }
            }
            return false;
        }
    }
}
