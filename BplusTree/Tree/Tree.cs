using System.Collections;
using System.Collections.Generic;

namespace MyProject.Trees {
	public abstract class Tree<T> : ICollection<T> {

		protected interface INode {

			bool IsLeaf { get; }

		}

        public bool IsReadOnly { get; set; }

        public int Count { get; }

        public abstract void Add(T item);

        public abstract bool Remove(T item);

        public abstract bool Contains(T item);

        public abstract void Clear();

        public abstract void CopyTo(T[] array, int cap);

        public abstract IEnumerator<T> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
