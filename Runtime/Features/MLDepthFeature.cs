/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using UnityEngine;
    using Types;

    /// <summary>
    /// ML depth feature.
    /// The depth feature is used to provide depth data to predictors that require such data.
    /// Implementers can derive from this class and provide custom logic for sampling depth given a pixel location.
    /// </summary>
    public abstract class MLDepthFeature : MLFeature {

        #region --Inspection--
        /// <summary>
        /// Depth map width.
        /// </summary>
        public int width => (type as MLImageType).width;

        /// <summary>
        /// Depth map height.
        /// </summary>
        public int height => (type as MLImageType).height;
        #endregion


        #region --Sampling--
        /// <summary>
        /// Sample the depth feature at a given point.
        /// </summary>
        /// <param name="point">Point to sample in normalized coordinates.</param>
        /// <returns>Depth in meters.</returns>
        public abstract float Sample (Vector2 point);

        /// <summary>
        /// Unproject a 2D point into 3D world space using depth data.
        /// </summary>
        /// <param name="point">Normalized point to transform in range [0., 1.].</param>
        /// <returns>Unprojected point in 3D world space.</param>
        public abstract Vector3 Unproject (Vector2 point);
        #endregion


        #region --Operations--
        /// <summary>
        /// Initialize the depth feature with the depth map dimensions.
        /// </summary>
        /// <param name="width">Depth map width.</param>
        /// <param name="height">Depth map height.</param>
        protected MLDepthFeature (int width, int height) : this(new MLImageType(width, height, 1, typeof(float))) { }

        /// <summary>
        /// Initialize the depth feature with the depth map feature type.
        /// </summary>
        /// <param name="type">Depth map feature type.</param>
        protected MLDepthFeature (MLImageType type) : base(type) { }
        #endregion


        #region --DEPRECATED--
        [Obsolete(@"Deprecated in NatML 1.0.16. Use `ViewportToWorldPoint` method instead.", false)]
        public virtual Vector3 TransformPoint (Vector2 point) => ViewportToWorldPoint(point);

        [Obsolete(@"Deprecated in NatML 1.1.4. Use `Unproject` method instead.", false)]
        public virtual Vector3 ViewportToWorldPoint (Vector2 point) => Unproject(point);
        #endregion
    }
}