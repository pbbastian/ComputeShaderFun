static const float EPSILON = 1e-6;

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

float3 MultiplyPoint(float4x4 mat, float3 p)
{
	float4 result = mul(mat, float4(p, 1));
	return result.xyz / result.w;
}

struct Triangle
{
	float3 a;
	float3 b;
	float3 c;
};

Triangle MakeTriangle(float3 a, float3 b, float3 c)
{
	Triangle tri;
	tri.a = a;
	tri.b = b;
	tri.c = c;
	return tri;
}

struct Ray
{
	float3 origin;
	float3 direction;
};

Ray MakeRay(float3 origin, float3 direction)
{
	Ray ray;
	ray.origin = origin;
	ray.direction = direction;
	return ray;
}

// Möller–Trumbore intersection algorithm
float IntersectTriangle(Triangle tri, Ray ray, out float3 coordinates)
{
	float3 e1 = tri.b - tri.a;
	float3 e2 = tri.c - tri.a;
	float3 P = cross(ray.direction, e2);
	float det = dot(e1, P);
	if (det > -EPSILON && det < EPSILON)
		return 0;
	float invDet = 1.0 / det;

	float3 T = ray.origin - tri.a;

	float u = dot(T, P) * invDet;
	if (u < 0 || u > 1)
		return 0;

	float3 Q = cross(T, e1);

	float v = dot(ray.direction, Q) * invDet;
	if (v < 0 || u + v > 1)
		return 0;

	float t = dot(e2, Q) * invDet;
    coordinates = float3(u, v, 1 - u - v);
	return max(t, 0);
}

Ray CameraRay(float4x4 inverseCameraMatrix, float3 origin, float2 screenSize, int2 pixelCoordinates)
{
	float2 position = float2(pixelCoordinates.x * 2 / screenSize.x - 1, pixelCoordinates.y * 2 / screenSize.y - 1);
	float3 direction = normalize(MultiplyPoint(inverseCameraMatrix, float3(position, 1)) - origin);
	return MakeRay(origin, direction);
}