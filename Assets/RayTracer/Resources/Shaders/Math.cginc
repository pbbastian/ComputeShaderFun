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

struct IndexedTriangle 
{
	uint v1;
	uint v2;
	uint v3;
};

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

Ray CameraRay(float4x4 inverseCameraMatrix, float3 origin, int2 pixelCoordinates)
{
	// float2 position = float2(pixelCoordinates.x * 2 / screenSize.x - 1, pixelCoordinates.y * 2 / screenSize.y - 1);
	float2 position = pixelCoordinates;
	float3 direction = normalize(MultiplyPoint(inverseCameraMatrix, float3(position, 1)) - origin);
	return MakeRay(origin, direction);
}

float3 Hue(float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

// http://chilliant.blogspot.dk/2010/11/rgbhsv-in-hlsl.html
float3 HSVtoRGB(in float3 HSV)
{
    return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
}

float3 HeatMap(float3 bands, float value)
{
	float v = clamp(value, 0.0, bands.x)/bands.x;
	float h = clamp(value - bands.x, 0.0, bands.y)*0.85/bands.y;
	float s = 1.0 - clamp(value - bands.x - bands.y, 0.0, bands.z)/bands.z;
	return HSVtoRGB(float3(h, s, v));
}