int CommonPrefix(int i, int j)
{
	return firstbithigh(i ^ j);
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