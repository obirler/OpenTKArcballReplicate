using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace OpenTKArcballReplicate
{
    public class BoundingBox
    {
        // Token: 0x060018D6 RID: 6358 RVA: 0x000AFA25 File Offset: 0x000ADC25
        public BoundingBox(float xmin, float xmax, float ymin, float ymax, float zmin, float zmax)
        {
            this.Xmin = xmin;
            this.Xmax = xmax;
            this.Ymin = ymin;
            this.Ymax = ymax;
            this.Zmin = zmin;
            this.Zmax = zmax;
        }

        // Token: 0x060018D7 RID: 6359 RVA: 0x000AFA5C File Offset: 0x000ADC5C
        public BoundingBox()
        {
            this.Xmin = float.MaxValue;
            this.Xmax = float.MinValue;
            this.Ymin = float.MaxValue;
            this.Ymax = float.MinValue;
            this.Zmin = float.MaxValue;
            this.Zmax = float.MinValue;
        }

        // Token: 0x17000609 RID: 1545
        // (get) Token: 0x060018D9 RID: 6361 RVA: 0x000AFBB8 File Offset: 0x000ADDB8
        public float XLength
        {
            get
            {
                return this.Xmax - this.Xmin;
            }
        }

        // Token: 0x1700060A RID: 1546
        // (get) Token: 0x060018DA RID: 6362 RVA: 0x000AFBD8 File Offset: 0x000ADDD8
        public float YLength
        {
            get
            {
                return this.Ymax - this.Ymin;
            }
        }

        // Token: 0x1700060B RID: 1547
        // (get) Token: 0x060018DB RID: 6363 RVA: 0x000AFBF8 File Offset: 0x000ADDF8
        public float ZLength
        {
            get
            {
                return this.Zmax - this.Zmin;
            }
        }

        // Token: 0x1700060C RID: 1548
        // (get) Token: 0x060018DC RID: 6364 RVA: 0x000AFC18 File Offset: 0x000ADE18
        public Vector3 Middle
        {
            get
            {
                return new Vector3((this.Xmax + this.Xmin) / 2.0f, (this.Ymax + this.Ymin) / 2.0f, (this.Zmax + this.Zmin) / 2.0f);
            }
        }

        // Token: 0x060018DD RID: 6365 RVA: 0x000AFC74 File Offset: 0x000ADE74
        public float DiagonalLength()
        {
            return MathF.Sqrt(MathF.Pow(this.Xmax - this.Xmin, 2.0f) + MathF.Pow(this.Ymax - this.Ymin, 2.0f) + MathF.Pow(this.Zmax - this.Zmin, 2.0f));
        }


        // Token: 0x060018E1 RID: 6369 RVA: 0x000AFF24 File Offset: 0x000AE124
        public void Expand(float x, float y, float z)
        {
            this.Xmin = ((x < this.Xmin) ? x : this.Xmin);
            this.Xmax = ((x > this.Xmax) ? x : this.Xmax);
            this.Ymin = ((y < this.Ymin) ? y : this.Ymin);
            this.Ymax = ((y > this.Ymax) ? y : this.Ymax);
            this.Zmin = ((z < this.Zmin) ? z : this.Zmin);
            this.Zmax = ((z > this.Zmax) ? z : this.Zmax);
        }

        // Token: 0x04000A6B RID: 2667
        public float Xmin;

        // Token: 0x04000A6C RID: 2668
        public float Xmax;

        // Token: 0x04000A6D RID: 2669
        public float Ymin;

        // Token: 0x04000A6E RID: 2670
        public float Ymax;

        // Token: 0x04000A6F RID: 2671
        public float Zmin;

        // Token: 0x04000A70 RID: 2672
        public float Zmax;
    }
}
