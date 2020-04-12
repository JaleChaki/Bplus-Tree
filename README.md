# B+ Tree

B+ tree data structure

### Usage

```c#
// as usual collection
BplusTree<int> tree = new BplusTree<int>();
tree.Add(4);
tree.Add(7);
tree.Add(-15);

foreach (item in tree) {
	Console.WriteLine(item);
}

tree.Remove(4);
tree.Clear();

```