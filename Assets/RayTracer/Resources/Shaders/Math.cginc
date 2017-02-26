int CommonPrefix(int x, int y)
{
	return 31 - floor(log2(x ^ y));
	// return firstbithigh(x ^ y);
}

int AugmentedCommonPrefix(int x, int y, int i, int j)
{
	if (x == y)
		return 32 + CommonPrefix(i, j);
	else
		return CommonPrefix(x, y);
}

int Div2(int x)
{
	return x >> 1;
}

int CeilDiv2(int x)
{
	return Div2(x + 1);
	// return 1 + Div2(x - 1);
}