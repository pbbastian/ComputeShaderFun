Shader "Hidden/BvhShadows"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "Math.cginc"
#include "Bvh.cginc"

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
};

v2f vert (appdata v)
{
	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = v.uv;
	return o;
}

#define STACK_SIZE 64

static const int _entrypointSentinel = 2147483647;
static const bool _anyHit = true;

sampler2D _MainTex;
sampler2D _CameraGBufferTexture2;
sampler2D_float _CameraDepthTexture;
float4x4 _InverseView;
float4x4 _projection;
float3 _light;
StructuredBuffer<Bvh::Node> _nodes;
StructuredBuffer<IndexedTriangle> _triangles;
StructuredBuffer<float4> _vertices;

int EncodeLeaf(int nodeIndex, bool isLeaf)
{
	if (isLeaf)
		nodeIndex = -(nodeIndex + 1);
	return nodeIndex;
}

int DecodeLeaf(int nodeIndex)
{
	return (-nodeIndex) - 1;
}

fixed4 frag (v2f i) : SV_Target
{
	float vz = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
    float2 p11_22 = float2(_projection._11, _projection._22);
    float3 vpos = float3((i.uv * 2 - 1) / p11_22, -1) * vz;
    float4 wpos = mul(_InverseView, float4(vpos, 1));
    float3 normal = normalize(tex2D(_CameraGBufferTexture2, i.uv) * 2 - 1);
    float3 direction = normalize(-_light);
    Ray r = MakeRay(wpos + normal*1e-2, direction);

	int traversalStack[STACK_SIZE];
	traversalStack[0] = _entrypointSentinel;
	int nodeIndex = 0;
	int stackIndex = 0;

	// float3 idir = 1.0 / r.direction;
	float t = 100000;

	int boxIntersections = 0;
	int triangleIntersections = 0;
	int pushes = 0;
	int pops = 0;

	if (dot(direction, normal) <= 1e-3)
	{
		nodeIndex = _entrypointSentinel;
		t = 1;
	}

	while (nodeIndex != _entrypointSentinel)
	{
		while (nodeIndex >= 0 && nodeIndex != _entrypointSentinel)
		{
			Bvh::Node node = _nodes[nodeIndex];
			FloatMinMax tLeft = IntersectAabb(node.leftBounds, r);
			FloatMinMax tRight = IntersectAabb(node.rightBounds, r);
			bool traverseLeft = tLeft.min <= tLeft.max    && tLeft.min  < t;
			bool traverseRight = tRight.min <= tRight.max && tRight.min  < t;

			int encodedLeft = EncodeLeaf(node.left, node.isLeftLeaf);
			int encodedRight = EncodeLeaf(node.right, node.isRightLeaf);
			nodeIndex = encodedLeft;

			if (traverseLeft != traverseRight)
			{
				boxIntersections++;
				// If only a single child was intersected we simply go to that one
				if (traverseRight)
					nodeIndex = encodedRight;
			}
			else
			{
				if (!traverseLeft)
				{
					// If neither child was intersected we pop the stack
					nodeIndex = traversalStack[stackIndex];
					stackIndex--;
					pops++;
				}
				else
				{
					boxIntersections += 2;
					// If both children were intersected we push one onto the stack
					nodeIndex = encodedLeft;
					int postponeIndex = encodedRight;
					if (tRight.min < tLeft.min)
					{
						nodeIndex = encodedRight;
						postponeIndex = encodedLeft;
					}

					stackIndex++;
					traversalStack[stackIndex] = postponeIndex;
					pushes++;
				}
			}
		}

		if (nodeIndex < 0)
		{
			nodeIndex = DecodeLeaf(nodeIndex);
			IndexedTriangle indices = _triangles[nodeIndex];
			Triangle tri = MakeTriangle(_vertices[indices.v1].xyz, _vertices[indices.v2].xyz, _vertices[indices.v3].xyz);

			// intersect triangle
			float3 candidateCoordinates;
			float candidate_t = IntersectTriangle(tri, r, candidateCoordinates);
			triangleIntersections++;
			if (candidate_t > 0 && candidate_t < t)
			{
				t = candidate_t;
				break;
				// nodeIndex = _entrypointSentinel;

				// if (_anyHit)
				// 	break;
			}
			
			nodeIndex = traversalStack[stackIndex];
			stackIndex--;
		}
	}

	// just invert the colors
	//col = 1 - col;

	// fixed4(HeatMap(100, boxIntersections), 1);
	fixed4 col = tex2D(_MainTex, i.uv);
	if (t < 99999)
		col *= 0.5;
	else
		col *= 1.0;
	// if (boxIntersections > 50-1)
	// 	col = fixed4(1, 0, 0, 1);
	return col;// float4(i.uv, 0, 1);
}
			ENDCG
		}
	}
}
