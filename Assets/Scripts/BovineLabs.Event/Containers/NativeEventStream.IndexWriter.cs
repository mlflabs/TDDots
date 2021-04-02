namespace BovineLabs.Event.Containers
{
    using System;
    using System.Diagnostics;
    using Unity.Assertions;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe partial struct NativeEventStream
    {
        /// <summary> The writer instance. </summary> // TODO
        [NativeContainer]
        [NativeContainerSupportsMinMaxWriteRestriction]
        public struct IndexWriter : IStreamWriter
        {
            private UnsafeEventStream.IndexWriter _writer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            private AtomicSafetyHandle _mSafety;
#pragma warning disable CS0414 // warning CS0414: The field 'NativeEventStream.Writer.m_Length' is assigned but its value is never used
            private int _mLength;
#pragma warning restore CS0414
            private int _mMinIndex;
            private int _mMaxIndex;

            [NativeDisableUnsafePtrRestriction]
            private void* _mPassByRefCheck;
#endif

            internal IndexWriter(ref NativeEventStream stream)
            {
                this._writer = stream._stream.AsIndexWriter();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                this._mSafety = stream.m_Safety;
                this._mLength = int.MaxValue;
                this._mMinIndex = int.MinValue;
                this._mMaxIndex = int.MinValue;
                this._mPassByRefCheck = null;
#endif
            }

            /// <summary> Gets the number of streams the container can use. </summary>
            public int ForEachCount => this._writer.ForEachCount;

            /// <summary> Begin reading data at the iteration index. </summary>
            /// <param name="foreachIndex"> The index to work on. </param>
            /// <remarks><para> BeginForEachIndex must always be called balanced by a EndForEachIndex. </para></remarks>
            public void BeginForEachIndex(int foreachIndex)
            {
                this.CheckBeginForEachIndex(foreachIndex);
                this._writer.BeginForEachIndex(foreachIndex);
            }

            /// <summary> Ensures that all data has been read for the active iteration index. </summary>
            /// <remarks><para> EndForEachIndex must always be called balanced by a BeginForEachIndex. </para></remarks>
            public void EndForEachIndex()
            {
                this.CheckEndForEachIndex();
                this._writer.EndForEachIndex();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                this._writer.MForeachIndex = int.MinValue;
#endif
            }

            /// <summary> Write data. </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <param name="value"> The data to write. </param>
            public void Write<T>(T value)
                where T : struct
            {
                ref T dst = ref this.Allocate<T>();
                dst = value;
            }

            /// <summary> Allocate space for data. </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <returns> Reference for the allocated space. </returns>
            public ref T Allocate<T>()
                where T : struct
            {
                CollectionHelper.CheckIsUnmanaged<T>();
                int size = UnsafeUtility.SizeOf<T>();
#if UNITY_COLLECTIONS_0_14_OR_NEWER
                return ref UnsafeUtility.AsRef<T>(this.Allocate(size));
#else
                return ref UnsafeUtilityEx.AsRef<T>(this.Allocate(size));
#endif
            }

            /// <summary> Allocate space for data. </summary>
            /// <param name="size">Size in bytes.</param>
            /// <returns> Reference for the allocated space. </returns>
            public byte* Allocate(int size)
            {
                this.CheckAllocateSize(size);
                return this._writer.Allocate(size);
            }

            internal void PatchMinMaxRange(int foreEachIndex)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                this._mMinIndex = foreEachIndex;
                this._mMaxIndex = foreEachIndex;
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckBeginForEachIndex(int foreachIndex)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(this._mSafety);

                if (this._mPassByRefCheck == null)
                {
                    this._mPassByRefCheck = UnsafeUtility.AddressOf(ref this);
                }

                if (foreachIndex < this._mMinIndex || foreachIndex > this._mMaxIndex)
                {
                    // When the code is not running through the job system no ParallelForRange patching will occur
                    // We can't grab m_BlockStream->RangeCount on creation of the writer because the RangeCount can be initialized
                    // in a job after creation of the writer
                    if (this._mMinIndex == int.MinValue && this._mMaxIndex == int.MinValue)
                    {
                        this._mMinIndex = 0;
                        this._mMaxIndex = this._writer.MBlockStream->RangeCount - 1;
                    }

                    if (foreachIndex < this._mMinIndex || foreachIndex > this._mMaxIndex)
                    {
                        throw new ArgumentException($"Index {foreachIndex} is out of restricted IJobParallelFor range [{this._mMinIndex}...{this._mMaxIndex}] in NativeEventStream.");
                    }
                }

                if (this._writer.MForeachIndex != int.MinValue)
                {
                    throw new ArgumentException($"BeginForEachIndex must always be balanced by a EndForEachIndex call");
                }

                if (this._writer.MBlockStream->Ranges[foreachIndex].ElementCount != 0)
                {
                    throw new ArgumentException($"BeginForEachIndex can only be called once for the same index ({foreachIndex}).");
                }

                Assert.IsTrue(foreachIndex >= 0 && foreachIndex < this._writer.MBlockStream->RangeCount);
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckEndForEachIndex()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(this._mSafety);

                if (this._writer.MForeachIndex == int.MinValue)
                {
                    throw new System.ArgumentException("EndForEachIndex must always be called balanced by a BeginForEachIndex or AppendForEachIndex call");
                }
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckAllocateSize(int size)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(this._mSafety);

                if (this._mPassByRefCheck != UnsafeUtility.AddressOf(ref this))
                {
                    throw new ArgumentException("NativeEventStream.Writer must be passed by ref once it is in use");
                }

                if (this._writer.MForeachIndex == int.MinValue)
                {
                    throw new ArgumentException("Allocate must be called within BeginForEachIndex / EndForEachIndex");
                }

                if (size > UnsafeEventStreamBlockData.AllocationSize - sizeof(void*))
                {
                    throw new ArgumentException("Allocation size is too large");
                }
#endif
            }
        }
    }
}