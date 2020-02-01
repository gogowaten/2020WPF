using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Windows.Media;


namespace _20200201_減色SIMD処理速度
{
    public class Cube
    {
        private enum MaxSideColor
        {
            Red = 2, Green = 1, Blue = 0, None = -1
        }


        #region sonota
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }
        public byte[] RedArray { get; private set; }
        public byte[] GreArray { get; private set; }
        public byte[] BluArray { get; private set; }
        public int ColorCount { get; private set; }
        public List<Cube> Cubes { get; private set; }
        #endregion
        private MaxSideColor KeyColor;//最大辺長の色(RGBのどれか)
        private int MaxSideLength;//最大辺長
        private int Median;//最大長辺の中央値

        public Cube(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            ColorCount = redArray.Length;
            Initialize(redArray, greenArray, blueArray);
        }

        public Cube(byte[] pixels)
        {
            ColorCount = pixels.Length / 4;
            var redArray = new byte[ColorCount];
            var greenArray = new byte[ColorCount];
            var blueArray = new byte[ColorCount];
            for (int i = 0; i < ColorCount; i++)
            {
                redArray[i] = pixels[i * 4 + 2];
                greenArray[i] = pixels[i * 4 + 1];
                blueArray[i] = pixels[i * 4];
            }

            Initialize(redArray, greenArray, blueArray);
        }

        #region 初期化処理
        private void Initialize(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            RedArray = redArray;
            GreArray = greenArray;
            BluArray = blueArray;
            SetMinMax();
            Cubes = new List<Cube>();
            Cubes.Add(this);
            SetKeyColorAndMaxSideLength();
        }
        private void SetMinMax()
        {
            var simdLength = Vector<byte>.Count;
            if (simdLength > ColorCount)
            {
                SetMinMax2();
                return;
            }

            var vMaxR = new Vector<byte>(byte.MinValue);
            var vMaxG = new Vector<byte>(byte.MinValue);
            var vMaxB = new Vector<byte>(byte.MinValue);
            var vMinR = new Vector<byte>(byte.MaxValue);
            var vMinG = new Vector<byte>(byte.MaxValue);
            var vMinB = new Vector<byte>(byte.MaxValue);

            int myLast = ColorCount - simdLength;
            for (int i = 0; i < myLast; i += simdLength)
            {
                vMaxR = System.Numerics.Vector.Max(vMaxR, new Vector<byte>(RedArray, i));
                vMaxG = System.Numerics.Vector.Max(vMaxG, new Vector<byte>(GreArray, i));
                vMaxB = System.Numerics.Vector.Max(vMaxB, new Vector<byte>(BluArray, i));
                vMinR = System.Numerics.Vector.Min(vMinR, new Vector<byte>(RedArray, i));
                vMinG = System.Numerics.Vector.Min(vMinG, new Vector<byte>(GreArray, i));
                vMinB = System.Numerics.Vector.Min(vMinB, new Vector<byte>(BluArray, i));
            }

            MaxR = vMaxR[0];
            MaxG = vMaxG[0];
            MaxB = vMaxB[0];
            MinR = vMinR[0];
            MinG = vMinG[0];
            MinB = vMinB[0];
            for (int i = 1; i < simdLength; i++)
            {
                if (MaxR < vMaxR[i]) MaxR = vMaxR[i];
                if (MaxG < vMaxG[i]) MaxG = vMaxG[i];
                if (MaxB < vMaxB[i]) MaxB = vMaxB[i];
                if (MinR > vMinR[i]) MinR = vMinR[i];
                if (MinG > vMinG[i]) MinG = vMinG[i];
                if (MinB > vMinB[i]) MinB = vMinB[i];
            }

            for (int i = myLast; i < ColorCount; i++)
            {
                MaxR = Math.Max(MaxR, RedArray[i]);
                MaxG = Math.Max(MaxG, GreArray[i]);
                MaxB = Math.Max(MaxB, BluArray[i]);
                MinR = Math.Min(MinR, RedArray[i]);
                MinG = Math.Min(MinG, GreArray[i]);
                MinB = Math.Min(MinB, BluArray[i]);
            }
        }
        private void SetMinMax2()
        {
            for (int i = 0; i < ColorCount; i++)
            {
                MaxR = Math.Max(MaxR, RedArray[i]);
                MaxG = Math.Max(MaxG, GreArray[i]);
                MaxB = Math.Max(MaxB, BluArray[i]);
                MinR = Math.Min(MinR, RedArray[i]);
                MinG = Math.Min(MinG, GreArray[i]);
                MinB = Math.Min(MinB, BluArray[i]);
            }
        }




        //最大辺長とその色と中央値を設定
        private void SetKeyColorAndMaxSideLength()
        {
            int r = MaxR - MinR;
            int g = MaxG - MinG;
            int b = MaxB - MinB;
            if (r > g)
            {
                if (r > b)
                {
                    MaxSideLength = r;
                    KeyColor = MaxSideColor.Red;
                    Median = (MaxR + MinR) / 2;
                }
                else
                {
                    MaxSideLength = b;
                    KeyColor = MaxSideColor.Blue;
                    Median = (MaxB + MinB) / 2;
                }
            }
            else if (g > b)
            {
                MaxSideLength = g;
                KeyColor = MaxSideColor.Green;
                Median = (MaxG + MinG) / 2;
            }

            else
            {
                MaxSideLength = b;
                KeyColor = MaxSideColor.Blue;
                Median = (MaxB + MinB) / 2;
            }
        }


        #endregion


        //分割
        public List<Cube> Split(int spritCount)
        {
            Cube selectedCube = Select();
            List<byte> vs1R = new List<byte>();
            List<byte> vs1G = new List<byte>();
            List<byte> vs1B = new List<byte>();
            List<byte> vs2R = new List<byte>();
            List<byte> vs2G = new List<byte>();
            List<byte> vs2B = new List<byte>();

            //最大長辺の色の配列を取得
            byte[] keyArray;
            if (selectedCube.KeyColor == MaxSideColor.Red)
            {
                keyArray = selectedCube.RedArray;
            }
            else if (selectedCube.KeyColor == MaxSideColor.Green)
            {
                keyArray = selectedCube.GreArray;
            }
            else
            {
                keyArray = selectedCube.BluArray;
            }
            //中央値で分割
            for (int i = 0; i < selectedCube.ColorCount; i++)
            {
                if (selectedCube.Median > keyArray[i])
                {
                    vs1R.Add(selectedCube.RedArray[i]);
                    vs1G.Add(selectedCube.GreArray[i]);
                    vs1B.Add(selectedCube.BluArray[i]);

                }
                else
                {
                    vs2R.Add(selectedCube.RedArray[i]);
                    vs2G.Add(selectedCube.GreArray[i]);
                    vs2B.Add(selectedCube.BluArray[i]);
                }
            }
            //2つに分割できなかったら、
            if (vs1B.Count == 0 || vs2B.Count == 0)
            {
                //今のCubeを諦めて別のCubeで試行？諦める？
                return Cubes;//諦め
            }
            //2つに分割できたら
            else
            {
                //分割した元Cubeはリストから削除、元Cubeから分割した2つのCubeをリストに追加
                Cubes.Remove(selectedCube);
                Cubes.Add(new Cube(vs1R.ToArray(), vs1G.ToArray(), vs1B.ToArray()));
                Cubes.Add(new Cube(vs2R.ToArray(), vs2G.ToArray(), vs2B.ToArray()));
                //指定分割数になるまで繰り返す
                if (Cubes.Count < spritCount) Split(spritCount);
            }
            return Cubes;
        }


        //最大辺のcubeを選択
        private Cube Select()
        {
            if (Cubes.Count == 0) return null;
            Cube selectedCube = Cubes[0];
            if (Cubes.Count == 1) return selectedCube;

            for (int i = 1; i < Cubes.Count; i++)
            {
                if (selectedCube.MaxSideLength < Cubes[i].MaxSideLength)
                {
                    selectedCube = Cubes[i];
                }
            }
            return selectedCube;
        }


        //色取得
        public List<Color> GetColors()
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i < Cubes.Count; i++)
            {
                colors.Add(MakeColorAverage(Cubes[i]));
            }
            return colors;
        }
        //Cubeから色の取得は平均色
        private Color MakeColorAverage(Cube cube)
        {
            //red[n]++++++.../red.count
            int simdLength = Vector<int>.Count;
            int lastIndex = cube.ColorCount - simdLength;
            var r = new Vector<int>(cube.RedArray., 0);
            var g = new Vector<byte>(cube.GreArray, 0);
            var b = new Vector<byte>(cube.BluArray, 0);

            for (int i = 1; i < lastIndex; i += simdLength)
            {
                r = Vector.Add(r, new Vector<byte>(cube.RedArray, i));
                g = Vector.Add(r, new Vector<byte>(cube.GreArray, i));
                b = Vector.Add(r, new Vector<byte>(cube.BluArray, i));
            }
            int rAve = 0; int gAve = 0; int bAve = 0;
            for (int i = 0; i < simdLength; i++)
            {
                rAve += r[i]; gAve += g[i]; bAve += b[i];
            }
            for (int i = lastIndex; i < cube.ColorCount; i++)
            {
                rAve += cube.RedArray[i]; gAve += cube.GreArray[i]; bAve += cube.BluArray[i];
            }
            rAve /= ColorCount; gAve /= ColorCount; bAve /= ColorCount;
            return Color.FromArgb(255, (byte)rAve, (byte)gAve, (byte)bAve);
        }



    }
}
