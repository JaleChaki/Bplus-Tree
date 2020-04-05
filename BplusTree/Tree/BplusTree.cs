using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Trees {

	public class BplusTree<T> : Tree<T> {

		/// <summary>
		/// B+ tree branching ratio
		/// </summary>
		public int BranchingRatio { get; }

		/// <summary>
		/// root node
		/// </summary>
		protected Node Root { get; set; }

		/// <summary>
		/// class, which contains node information
		/// </summary>
		protected class Node : INode {

			public int KeyCount => Keys.Count;

			public IList<int> Keys { get; set; }

			public Node Parent { get; set; }

			public IList<Node> Childs { get; private set; }

			public Node LeftBrother { get; private set; }

			public Node RightBrother { get; private set; }

			public IList<T> Pointers { get; set; }

			public bool IsRoot => Parent == null;

			public bool IsLeaf => Pointers.Count != 0 || (IsRoot && Childs.Count == 0);

			public Node() {
				Keys = new List<int>();
				Childs = new List<Node>();
				Pointers = new List<T>();
			}

			public virtual bool ContainsKey(int key) {
				return Keys.Contains(key);
			}

			public void AddChild(int key, Node child, bool force = false) {
				if (IsLeaf && !force) {
					throw new ArgumentException("Can't adding child to leaf node");
				}
				if (ContainsKey(key)) {
					throw new ArgumentException("Key already added");
				}
				if (child == null) {
					throw new ArgumentException("Child not valid");
				}
				Childs.Add(child);
				Keys.Add(key);
			}

			public void AddPointer(int key, T data) {
				if (!IsLeaf) {
					throw new NotImplementedException("Can't adding data to leaf node");
				}
				if (ContainsKey(key)) {
					throw new ArgumentException("Key already added");
				}
				Pointers.Add(data);
				Keys.Add(key);
			}

			public int InsertKeyPointer(int key, T data) {
				if (KeyCount == 0) {
					Keys.Add(key);
					Pointers.Add(data);
					return 0;
				}
				Keys.Add(int.MaxValue);
				Pointers.Add(Pointers[Pointers.Count - 1]);
				int i;
				for (i = KeyCount - 1; i > 0 && Keys[i - 1] >= key; --i) {
					Keys[i] = Keys[i - 1];
					Pointers[i] = Pointers[i - 1];
				}
				Keys[i] = key;
				Pointers[i] = data;
				return i;
			}

			public int InsertKeyChild(int key, Node child) {
				if (KeyCount == 0) {
					Keys.Add(key);
					Childs.Add(child);
					return 0;
				}
				Keys.Add(int.MaxValue);
				Childs.Add(null);
				int i;
				for (i = KeyCount - 1; i > 0 && Keys[i - 1] >= key; --i) {
					Keys[i] = Keys[i - 1];
					Childs[i] = Childs[i - 1];
				}
				Keys[i] = key;
				Childs[i] = child;
				return i;
			}

			public void SetLeftBrother(Node l) {
				LeftBrother = l;
			}

			public void SetRightBrother(Node r) {
				RightBrother = r;
			}

		}

		public class BplusTreeEnumerator : IEnumerator<T> {
			public T Current => current;

			object IEnumerator.Current => current;

			private T current;

			protected BplusTreeEnumerator(T first) {
				current = first;
			}

			public void Dispose() {
				throw new NotImplementedException();
			}

			public bool MoveNext() {
				throw new NotImplementedException();
			}

			public void Reset() {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// calculate element's key
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		protected static int GetKey(T obj) {
			return obj.GetHashCode();
		}

		/// <summary>
		/// find leaf, which should contains the key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected virtual Node FindLeaf(int key) {
			var CurrentNode = Root;
			while (!CurrentNode.IsLeaf) {
				for (int i = 0; i < CurrentNode.KeyCount; ++i) {
					if (key >= CurrentNode.Keys[i]) {
						CurrentNode = CurrentNode.Childs[i];
						break;
					}
				}
			}
			return CurrentNode;
		}

		/// <summary>
		/// add key and data
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		protected virtual bool InsertKey(int key, T data) {
			var targetLeaf = FindLeaf(key);
			if (targetLeaf.ContainsKey(key)) {
				return false;
			}

			int keyNewPosition = 0;
			while (keyNewPosition < targetLeaf.KeyCount && targetLeaf.Keys[keyNewPosition] < key) {
				++keyNewPosition;
			}

			/*for (int i = targetLeaf.KeyCount; i > key; --i) {
				targetLeaf.Keys[i] = targetLeaf.Keys[i - 1];
				targetLeaf.Pointers[i] = targetLeaf.Pointers[i - 1];
			}
			targetLeaf.Keys[keyNewPosition] = key;
			targetLeaf.Pointers[keyNewPosition] = data;
			*/
			targetLeaf.InsertKeyPointer(key, data);

			if (targetLeaf.KeyCount == BranchingRatio * 2) {
				Split(targetLeaf);
			}
			return true;
		}

		/// <summary>
		/// split node, create new leafs and recursively rebuild tree, if needed
		/// </summary>
		/// <param name="n"></param>
		protected virtual void Split(Node n) {
			var createdNode = new Node();

			bool isLeaf = n.IsLeaf;

			createdNode.SetRightBrother(n.RightBrother);
			n.RightBrother?.SetLeftBrother(n.LeftBrother);
			n.SetRightBrother(createdNode);
			createdNode.SetLeftBrother(n);

			int middleKey = n.Keys[BranchingRatio];

			int newNodeKeyCount = BranchingRatio - 1;
			int oldNodeKeyCount = BranchingRatio;

			IList<int> movedKeys = new List<int>();
			IList<Node> movedChilds = new List<Node>();
			IList<T> movedPointers = new List<T>();
			for (int i = 0; i < n.KeyCount; ++i) {
				movedKeys.Add(n.Keys[i]);
				if (n.IsLeaf) {
					movedPointers.Add(n.Pointers[i]);
				} else {
					movedChilds.Add(n.Childs[i]);
				}
			}
			n.Keys.Clear();
			n.Pointers.Clear();
			n.Childs.Clear();
			// t keys in previous node
			for (int i = 0; i < oldNodeKeyCount; ++i) {
				if (isLeaf) {
					n.AddPointer(movedKeys[i], movedPointers[i]);
				} else {
					n.AddChild(movedKeys[i], movedChilds[i]);
				}
			}
			// t-1 keys in new node
			for (int i = BranchingRatio + 1; i < movedKeys.Count; ++i) {
				if (isLeaf) {
					createdNode.AddPointer(movedKeys[i], movedPointers[i]);
				} else {
					createdNode.AddChild(movedKeys[i], movedChilds[i]);
				}
			}

			if (isLeaf) {
				createdNode.InsertKeyPointer(middleKey, movedPointers[BranchingRatio]);
			}

			if (n.IsRoot) {
				Root = new Node();
				Root.AddChild(int.MinValue, n, force: true);
				Root.AddChild(middleKey, createdNode, force: true);
				n.Parent = Root;
				createdNode.Parent = Root;
			} else {
				createdNode.Parent = n.Parent;
				var parent = n.Parent;

				parent.InsertKeyChild(middleKey, createdNode);

				if (parent.KeyCount == 2 * BranchingRatio) {
					Split(parent);
				}

			}
		}

		/// <summary>
		/// add element to tree
		/// </summary>
		/// <param name="item"></param>
		public override void Add(T item) {
			InsertKey(GetKey(item), item);
		}

		/// <summary>
		/// remove element from tree
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool Remove(T item) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// check elements exists
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool Contains(T item) {
			int key = GetKey(item);
			return FindLeaf(key).ContainsKey(key);
		}

		/// <summary>
		/// clear all elements in tree
		/// </summary>
		public override void Clear() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// copy elements to array from this tree
		/// </summary>
		/// <param name="array"></param>
		/// <param name="cap"></param>
		public override void CopyTo(T[] array, int cap) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// get begin enumerator
		/// </summary>
		/// <returns></returns>
		public override IEnumerator<T> GetEnumerator() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="t"> branching ratio </param>
		public BplusTree(int t = 3) {
			if (t <= 0) {
				throw new ArgumentException("incorrect branching ratio");
			}
			BranchingRatio = t;
			Root = new Node();
		}

	}
}
