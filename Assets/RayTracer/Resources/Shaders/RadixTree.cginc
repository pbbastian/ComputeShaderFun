namespace RadixTree
{
	struct InternalNode
	{
		int left;
		int right;
	};

	bool IsLeafNode(int index)
	{
		return index < 0;
	}

	int DecodeLeafNodeIndex(int index)
	{
		return -index - 1;
	}

	int EncodeLeafNodeIndex(int index)
	{
		return -(index + 1);
	}
};