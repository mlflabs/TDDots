namespace BovineLabs.Event.Containers
{
    using System;
    using System.Diagnostics;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe partial struct NativeEventStream
    {
        /// <summary>
        /// </summary>
        [NativeContainer]
        [NativeContainerIsAtomicWriteOnly]
        public struct ThreadWriter : IStreamWriter
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle _mSafety;
#endif

            private UnsafeEventStream.ThreadWriter _writer;

            internal ThreadWriter(ref NativeEventStream stream)
            {
                this._writer = stream._stream.AsThreadWriter();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                _mSafety = stream.m_Safety;
#endif
            }

            /// <summary>
            ///
            /// </summary>
            public int ForEachCount => UnsafeEventStream.ThreadWriter.ForEachCount;

            /// <summary>
            /// Write data.
            /// </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <param name="value"></param>
            public void Write<T>(T value)
                where T : struct
            {
                ref T dst = ref this.Allocate<T>();
                dst = value;
            }

            /// <summary>
            /// Allocate space for data.
            /// </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <returns></returns>
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

            /// <summary>
            /// Allocate space for data.
            /// </summary>
            /// <param name="size">Size in bytes.</param>
            /// <returns></returns>
            public byte* Allocate(int size)
            {
                this.CheckAllocateSize(size);
                return this._writer.Allocate(size);
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void CheckAllocateSize(int size)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(this._mSafety);

                if (size > UnsafeEventStreamBlockData.AllocationSize - sizeof(void*))
                {
                    throw new ArgumentException("Allocation size is too large");
                }
#endif
            }
        }
    }
}