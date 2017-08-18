using UnityEngine;
using Verse;

namespace FuelBurning
{
    class Graphic_LinkedCornerComplement : Graphic_Linked
    {
        protected Material cornerMat;
        protected string compTexPath;

        private readonly Vector2 size = new Vector2(0.2f, 0.2f);

        private const float slope = 0f;
        private readonly float distCenterCorner;
        private readonly float coverOffsetDist;

        private readonly Vector2[][] compUVs;
        private readonly Vector3[][] vecs;
        private readonly int[] tris;
        private readonly Color32[] colors;
        
        public Graphic_LinkedCornerComplement()
        {
            this.distCenterCorner = new Vector2(0.5f, 0.5f).magnitude;
            this.coverOffsetDist = distCenterCorner - size.magnitude * 0.5f;
            this.vecs = new Vector3[][]
            {
                new Vector3[] {
                    new Vector3(0.5f * size.x, 0f, -0.5f * size.y),
                    new Vector3(-0.5f * size.x, 0f, -0.5f * size.y),
                    new Vector3(-0.5f * size.x, slope, 0.5f * size.y)
                    
                },
                new Vector3[]
                {
                    new Vector3(-0.5f * size.x, 0f, -0.5f * size.y),
                    new Vector3(-0.5f * size.x, slope, 0.5f * size.y),
                    new Vector3(0.5f * size.x, slope, 0.5f * size.y)
                },
                new Vector3[]
                {
                    new Vector3(-0.5f * size.x, slope, 0.5f * size.y),
                    new Vector3(0.5f * size.x, slope, 0.5f * size.y),
                    new Vector3(0.5f * size.x, 0f, -0.5f * size.y)
                },
                new Vector3[]
                {
                    new Vector3(0.5f * size.x, slope, 0.5f * size.y),
                    new Vector3(0.5f * size.x, 0f, -0.5f * size.y),
                    new Vector3(-0.5f * size.x, 0f, -0.5f * size.y)
                }
            };
            this.compUVs = new Vector2[][]
            {
                new Vector2[] {
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f)
                },
                 new Vector2[] {
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 1f),
                    new Vector2(1f, 1f)
                },
                new Vector2[] {
                    new Vector2(0f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 0f)
                },
                new Vector2[] {
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                    new Vector2(0.5f, 0f)
                }
            };
            this.tris = new int[] { 0, 1, 2 };
            this.colors = new Color32[]
            {
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255)
            };
        }
        public override void Init(GraphicRequest req)
        {
            this.path = req.path;
            base.subGraphic = GraphicDatabase.Get<Graphic_Single>(req.path, ShaderDatabase.Transparent);
            compTexPath = req.path + "_Comp";
            this.cornerMat = this.LoadCompMaterial(this.compTexPath);
        }

        public override void Print(SectionLayer layer, Thing thing)
        {
            base.Print(layer, thing);
            IntVec3 position = thing.Position;
            for (int i = 0; i < 4; i++)
            {
                IntVec3 c = thing.Position + GenAdj.DiagonalDirectionsAround[i];
                if (!this.ShouldLinkWith(c, thing))
                {
                    Vector3 center = thing.DrawPos + GenAdj.DiagonalDirectionsAround[i].ToVector3().normalized * this.coverOffsetDist;
                    if (this.CheckLinkedFourWays(position, thing, i))
                    {
                        Printer_Mesh.PrintMesh(layer, center, this.GetCornerMesh(compUVs[i], i), this.cornerMat);
                    }
                }
            }
        }

        protected bool CheckLinkedFourWays(IntVec3 position, Thing thing, int i)
        {
            if (i != 0 || (this.ShouldLinkWith(position + IntVec3.West, thing) && this.ShouldLinkWith(position + IntVec3.South, thing)))
            {
                if (i != 1 || (this.ShouldLinkWith(position + IntVec3.West, thing) && this.ShouldLinkWith(position + IntVec3.North, thing)))
                {
                    if (i != 2 || (this.ShouldLinkWith(position + IntVec3.East, thing) && this.ShouldLinkWith(position + IntVec3.North, thing)))
                    {
                        if (i != 3 || (this.ShouldLinkWith(position + IntVec3.East, thing) && this.ShouldLinkWith(position + IntVec3.South, thing)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected Material LoadCompMaterial(string path)
        {
            Material mat = MaterialPool.MatFrom(path, ShaderDatabase.Transparent);
            if(mat == null)
            {
                Log.Error(DebugLog.Sign() + "Could not found the texture. path \"" + this.compTexPath + "\"");
                mat = BaseContent.BadMat;
            }
            return mat;
        }
        protected Mesh GetCornerMesh(Vector2[] uvs, int direction)
        {
            Mesh mesh = new Mesh();
            mesh.name = "ComplementMesh";
            mesh.vertices = this.vecs[direction];
            mesh.uv = uvs;
            mesh.colors32 = this.colors;
            mesh.SetTriangles(this.tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}