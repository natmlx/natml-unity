/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Types {

    using System;

    /// <summary>
    /// ML image feature type.
    /// </summary>
    public class MLImageType : MLArrayType {

        #region --Client API--
        /// <summary>
        /// Image width.
        /// </summary>
        public virtual int width => shape?[interleaved ? 2 : 3] ?? 0;

        /// <summary>
        /// Image height.
        /// </summary>
        public virtual int height => shape?[interleaved ? 1 : 2] ?? 0;

        /// <summary>
        /// Image channels.
        /// </summary>
        public virtual int channels => shape?[interleaved ? 3 : 1] ?? 0;

        /// <summary>
        /// Whether the image is interleaved or planar.
        /// </summary>
        public virtual bool interleaved { get; protected set; }

        /// <summary>
        /// Create an image feature type.
        /// This constructor assumes interleaved pixel buffers.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="channels">Image channels.</param>
        public MLImageType (int width, int height, int channels) : this(width, height, channels, typeof(byte)) { }

        /// <summary>
        /// Create an image feature type.
        /// This constructor assumes interleaved pixel buffers.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="channels">Image channels.</param>
        /// <param name="type">Image data type.</param>
        public MLImageType (int width, int height, int channels, Type type) : this(new [] { 1, height, width, channels }, type) { }

        /// <summary>
        /// Create an image feature type.
        /// </summary>
        /// <param name="shape">Image feature shape.</param>
        /// <param name="type">Image data type.</param>
        /// <param name="name">Feature name.</param>
        public MLImageType (int[] shape, Type type, string name = null) : base(shape, type, name) => this.interleaved = shape == null || shape[1] > shape[3];

        /// <summary>
        /// Get the corresponding image type for a given feature type.
        /// </summary>
        /// <param name="type">Input type.</param>
        /// <returns>Corresponding image type or `null` if input type is not an image type.</returns>
        public static MLImageType FromType (MLFeatureType type) {
            switch (type) {
                case MLImageType imageType: return imageType;
                case MLArrayType arrayType when arrayType.dims == 4: return new MLImageType(arrayType.shape, arrayType.dataType, arrayType.name);
                default: return null;
            }
        }
        #endregion
    }
}