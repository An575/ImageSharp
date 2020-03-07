// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Processing.Processors.Quantization
{
    /// <summary>
    /// Gets the closest color to the supplied color based upon the Eucladean distance.
    /// TODO: Expose this somehow.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal readonly struct KdTreePixelMap<TPixel> : IPixelMap<TPixel>, IEquatable<KdTreePixelMap<TPixel>>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly KdTree tree;

        /// <summary>
        /// Initializes a new instance of the <see cref="EuclideanPixelMap{TPixel}"/> struct.
        /// </summary>
        /// <param name="palette">The color palette to map from.</param>
        public KdTreePixelMap(ReadOnlyMemory<TPixel> palette)
        {
            Guard.MustBeGreaterThan(palette.Length, 0, nameof(palette));

            this.Palette = palette;
            ReadOnlySpan<TPixel> paletteSpan = this.Palette.Span;

            var points = new List<KdTree.ColorWithIndex>();
            for (int i = 0; i < paletteSpan.Length; i++)
            {
                points.Add(new KdTree.ColorWithIndex(paletteSpan[i].ToScaledVector4(), i));
            }

            this.tree = new KdTree(points);
            var tmp = this.tree.GetPointList();
        }

        /// <inheritdoc/>
        public ReadOnlyMemory<TPixel> Palette { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is EuclideanPixelMap<TPixel> map && this.Equals(map);

        /// <inheritdoc/>
        public bool Equals(KdTreePixelMap<TPixel> other)
            => this.Palette.Equals(other.Palette);

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public int GetClosestColor(TPixel color, out TPixel match)
        {
            ReadOnlySpan<TPixel> paletteSpan = this.Palette.Span;
            Vector4 vector = color.ToScaledVector4();
            KdTree.Node node = KdTree.FindNearestNeighbour(vector, this.tree.Root, out KdTree.Node snnNode);
            match = paletteSpan[node.Point.Index];
            return node.Point.Index;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => this.tree.GetHashCode();
    }
}
