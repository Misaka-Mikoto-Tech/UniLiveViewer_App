using System.Collections.Generic;
using UnityEngine;

namespace NanaCiel
{
    //拡張メソッド
    public static class ExtensionMethods
    {
        //水平方向ベクトルを取得する
        public static Vector3 GetHorizontalDirection(this Vector3 originalVector)
        {
            return new Vector3(originalVector.x, 0.0f, originalVector.z);
        }

        //垂直方向ベクトルを取得する
        public static Vector3 GetVerticalDirection(this Vector3 originalVector)
        {
            return new Vector3(0.0f, originalVector.y, originalVector.z);
        }

        public static Vector3 RandomQuake(this Vector3 maxMagnitude)
        {
            maxMagnitude.x = Random.Range(-maxMagnitude.x, maxMagnitude.x);
            maxMagnitude.y = Random.Range(-maxMagnitude.y, maxMagnitude.y);
            maxMagnitude.z = Random.Range(-maxMagnitude.z, maxMagnitude.z);
            return maxMagnitude;
        }

        //フォントサイズをいい感じに調整
        public static int FontSizeMatch(this string str, int coeffic, int min, int max)
        {
            int result = coeffic / str.Length;
            result = Mathf.Clamp(result, min, max);
            return result;
        }

        //blendshapeが設定されているSkinnedMeshRendererのみ抽出
        public static SkinnedMeshRenderer[] GetMorphSkinnedMeshRenderer(this SkinnedMeshRenderer[] skin)
        {
            List<SkinnedMeshRenderer> result = new List<SkinnedMeshRenderer>();
            foreach (var e in skin)
            {
                if(e.sharedMesh.blendShapeCount > 0)
                {
                    result.Add(e);
                }
            }
            return result.ToArray();
        }
    }

}