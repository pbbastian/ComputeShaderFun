# BVH construction steps from scratch
1. Find all meshes
2. Create vertex, normal and triangle buffers with enough size to fit meshes
3. Upload data into buffers
4. Dispatch compute shader updating vertices and normals using transforms
5. Sort triangles using morton codes (maybe put morton code in separate buffer first)