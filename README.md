# Adaptive Clothing System for Unty

https://github.com/user-attachments/assets/ee987bcd-852d-49ef-aa6a-9c9906dc99bf

> **Note:** The repository includes the rigged character model and sample clothing shown in the demo video.

---

## How to Use

1. Attach the `AdaptiveClothing.cs` script to any GameObject in your scene.
2. Assign the **Under Garment** (the mesh to shrink) and the **Over Garment** (the outer layer) in the Inspector.
3. Click the **three dots context menu (`⋮`)** on the script component and select **Adapt Clothing (Bind Pose)**.
4. A new adapted GameObject will be created with the generated `SkinnedMeshRenderer`.

---

## Notes

* **SkinnedMeshRenderer Required:** Both garment objects must have a `SkinnedMeshRenderer` component attached with valid rigging data.
* **Bind Pose Evaluation:** Collision and deformation calculations are evaluated against the asset's rest/bind pose (`sharedMesh`).
* **Parameter Tuning:** Increase or decrease **Shrink Amount** and **Ray Distance** depending on the thickness of the clothing assets and unit scale. If the specified range is too restrictive, feel free to change it in the script.
