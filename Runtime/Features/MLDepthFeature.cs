/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

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
        public virtual int width => (type as MLImageType).width;

        /// <summary>
        /// Depth map height.
        /// </summary>
        public virtual int height => (type as MLImageType).height;
        #endregion


        #region --Sampling--
        /// <summary>
        /// Sample the depth value at a normalized pixel location.
        /// The (x, y) coordinates are in range [0, 1].
        /// </summary>
        /// <returns>Depth in meters.</returns>
        public abstract float Sample (float x, float y);
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
        protected MLDepthFeature (MLImageType type) => this.type = type;
        #endregion
    }
}