using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyProject.Trees;

namespace BplusTreeTest {
	[TestClass]
	public class BplusTreeTests {

		[TestMethod]
		public void BplusTreeCreateChecker() {
			BplusTree<string> firstTree = new BplusTree<string>();
			Assert.IsNotNull(firstTree);

			BplusTree<int> secondTree = new BplusTree<int>(6);
			Assert.AreEqual(secondTree.BranchingRatio, 6);
		}

		[TestMethod]
		public void BplusTreeAddChecker() {
			int addCap = 5;
			BplusTree<int> testableTree = new BplusTree<int>();
			for (int i = 0; i < addCap; ++i) {
				testableTree.Add(i);
			}
			for (int i = 0; i < addCap; ++i) {
				Assert.IsTrue(testableTree.Contains(i));
			}
		}
	}
}
