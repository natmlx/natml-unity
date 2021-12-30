/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Extensions {

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using Features;
    using Types;

    public static partial class MLPredictorExtensions {

        #region --Client API--
        /// <summary>
        /// Get the image size for a feature.
        /// </summary>
        /// <param name="width">Output image width.</param>
        /// <param name="height">Output image height.</param>
        /// <return>Whether the feature is an image and has a valid image size.</returns>
        [Obsolete(@"Deprecated in NatML 1.0.6. Use MLImageType::FromType instead.", false)]
        public static bool GetImageSize (this MLFeature feature, out int width, out int height) {
            switch (feature.type) {
                case MLImageType imageType:
                    (width, height) = (imageType.width, imageType.height);
                    return true;
                case MLArrayType arrayType when arrayType.dims == 4:
                    (width, height) = (arrayType.shape[3], arrayType.shape[2]);
                    return true;
                default:
                    (width, height) = (0, 0);
                    return false;
            }
        }

        /// <summary>
        /// Perform non-max suppression on a set of candidate boxes.
        /// </summary>
        /// <param name="rects">Candidate boxes.</param>
        /// <param name="scores">Candidate scores.</param>
        /// <param name="maxIoU">Maximum IoU for preserving overlapping boxes.</param>
        /// <returns>Indices of boxes to keep.</returns>
        public static int[] NonMaxSuppression (IReadOnlyList<Rect> rects, IReadOnlyList<float> scores, in float maxIoU) {
            var discard = new bool[rects.Count];
            for (var i = 0; i < rects.Count - 1; ++i) {
                if (discard[i])
                    continue;
                for (var j = i + 1; j < rects.Count; ++j) {
                    if (discard[j])
                        continue;
                    var iou = IoU(rects[i], rects[j]);
                    if (iou < maxIoU)
                        continue;
                    if (scores[i] > scores[j])
                        discard[j] = true;
                    else {
                        discard[i] = true;
                        break;
                    }
                }
            }
            var result = new List<int>();
            for (var i = 0; i < rects.Count; ++i)
                if (!discard[i])
                    result.Add(i);
            return result.ToArray();
        }

        /// <summary>
        /// Rectify a detection rectangle for a given aspect mode.
        /// </summary>
        /// <param name="rect">Input rectangle in normalized coordinates.</param>
        /// <param name="featureType">Feature image type used for aspect scaling.</param>
        /// <param name="imageWidth">Original image width before aspect scaling.</param>
        /// <param name="imageHeight">Original image height before aspect scaling.</param>
        /// <param name="aspectMode">Aspect mode used for scaling.</param>
        /// <returns>Rectified detection rectangle in the original image space.</returns>
        public static Rect RectifyAspect (
            this in Rect rect,
            in MLImageType featureType,
            in int imageWidth,
            in int imageHeight,
            in MLImageFeature.AspectMode aspectMode
        ) {
            // Shortcut
            if (aspectMode == MLImageFeature.AspectMode.ScaleToFit)
                return rect;
            // Get normalizing factor
            var scaleFactor = 1f;
            if (aspectMode == MLImageFeature.AspectMode.AspectFit)
                scaleFactor = Mathf.Min((float)featureType.width / imageWidth, (float)featureType.height / imageHeight);
            if (aspectMode == MLImageFeature.AspectMode.AspectFill)
                scaleFactor = Mathf.Max((float)featureType.width / imageWidth, (float)featureType.height / imageHeight);
            // Compute crop rect
            var width = scaleFactor * imageWidth / featureType.width;
            var height = scaleFactor * imageHeight / featureType.height;
            var x = 0.5f * (1f - width);
            var y = 0.5f * (1f - height);
            // Scale
            var result = Rect.MinMaxRect(
                (rect.x - x) / width,
                (rect.y - y) / height,
                (rect.xMax - x) / width,
                (rect.yMax - y) / height
            );
            return result;
        }
        #endregion


        #region --Operations--

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float IoU (in Rect a, in Rect b) {
            var areaA = a.width * a.height;
            var areaB = b.width * b.height;
            var c = Rect.MinMaxRect(
                Mathf.Max(a.xMin, b.xMin),
                Mathf.Max(a.yMin, b.yMin),
                Mathf.Min(a.xMax, b.xMax),
                Mathf.Min(a.yMax, b.yMax)
            );
            var areaC = Mathf.Max(c.width, 0) * Mathf.Max(c.height, 0);
            return areaC / (areaA + areaB - areaC);
        }
        #endregion
    }
}