namespace Bvh
{
	struct AlignedAABB
	{
		AABB aabb;
		int padding[2];
	};

	AlignedAABB MakeAlignedAABB(AABB aabb)
	{
		AlignedAABB alignedAabb;
		alignedAabb.aabb = aabb;
		alignedAabb.padding[0] = 0;
		alignedAabb.padding[1] = 0;
		return alignedAabb;
	}

	AABB EmptyBounds()
	{
		AABB bounds;
		bounds.min = 0;
		bounds.max = 0;
		return bounds;
	}

	AABB MergeBounds(AABB left, AABB right)
	{
		AABB bounds;
		bounds.min = min(left.min, right.min);
		bounds.max = max(left.max, right.max);
		return bounds;
	}

	struct Node
	{
		AABB leftBounds;
		AABB rightBounds;
		int left;
		int right;
		bool isLeftLeaf;
		bool isRightLeaf;

		AABB GetBounds()
		{
			return MergeBounds(leftBounds, rightBounds);
		}
	};
};