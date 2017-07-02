The project is divided into 3 folders: RayTracer, ShadowRenderPipeline and BenchmarkPipeline. (Inside the Assets folder.)
Note that the project is only suitable for running on AMD GPUs. Minor changes have to be made in places in order to support the 32-wide SIMDs of NVIDIA GPUs.

# RayTracer
This folder contains the initial implementation of the project implementated as an image effect. Note that the traversal shader used in this code is not the latest version.
It also contains the shaders for BVH construction as well as tests for them.
Shaders can be found in "Assets\RayTracer\Resources\Shaders". Please see the tests in "Assets\RayTracer\Editor\Tests" for how they are used.
The code for BVH construction is in "Assets\RayTracer\Runtime\BvhUtil.cs".

# ShadowRenderPipeline
This folder contains the render pipeline implementation of the project. Note that this still uses the BVH construction from the `RayTracer` folder.
Shaders can be found in "Assets\ShadowRenderPipeline\Resources".
Most important entrypoint files for exploring the code is:
- "Assets\ShadowRenderPipeline\ShadowRenderPipeline"
- "Assets\ShadowRenderPipeline\Editor\ShadowRenderPipelineInspector"

# BenchmarkPipeline
This folder contains a render pipeline for running compute shaders in the render loop, such that they can be timed for performance investigations. Not used for report.
