using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ECSPhysics2D
{
  /// <summary>
  /// Generic 2-tier event buffer that never drops events.
  /// 
  /// Tier 1 (Hot): Pre-allocated fixed array - zero allocation fast path
  /// Tier 2 (Overflow): Growable list - handles burst beyond hot capacity
  /// 
  /// Design: Tier 2 grows unbounded to guarantee no event loss.
  /// If overflow is consistently used, increase hot buffer size.
  /// </summary>
  public struct PhysicsEventBuffer<T> : IDisposable where T : unmanaged
  {
    // Tier 1: Hot buffer (always allocated, fixed size)
    [NativeDisableContainerSafetyRestriction]
    private NativeArray<T> _hotBuffer;
    private int _hotCount;
    private int _hotCapacity;

    // Tier 2: Overflow buffer (allocated on demand, growable)
    [NativeDisableContainerSafetyRestriction]
    private NativeList<T> _overflowBuffer;
    private bool _overflowActive;
    private int _overflowInitialCapacity;

    // Statistics
    private int _peakCount;
    private bool _overflowUsedThisFrame;

    /// <summary>
    /// Total number of events currently stored.
    /// </summary>
    public int Count => _hotCount + (_overflowActive ? _overflowBuffer.Length : 0);

    /// <summary>
    /// Whether overflow buffer was used this frame.
    /// </summary>
    public bool OverflowUsed => _overflowUsedThisFrame;

    /// <summary>
    /// Peak event count across all frames (for tuning hot buffer size).
    /// </summary>
    public int PeakCount => _peakCount;

    /// <summary>
    /// Hot buffer capacity.
    /// </summary>
    public int HotCapacity => _hotCapacity;

    /// <summary>
    /// Create a new event buffer with specified capacities.
    /// </summary>
    public static PhysicsEventBuffer<T> Create(
        int hotCapacity,
        int overflowInitialCapacity = 1024)
    {
      return new PhysicsEventBuffer<T>
      {
        _hotBuffer = new NativeArray<T>(hotCapacity, Allocator.Persistent),
        _hotCount = 0,
        _hotCapacity = hotCapacity,
        _overflowBuffer = default,
        _overflowActive = false,
        _overflowInitialCapacity = overflowInitialCapacity,
        _peakCount = 0,
        _overflowUsedThisFrame = false
      };
    }

    /// <summary>
    /// Add an event to the buffer. Never fails, never drops.
    /// </summary>
    public void Add(T evt)
    {
      if (_hotCount < _hotCapacity) {
        // Fast path: use hot buffer
        _hotBuffer[_hotCount++] = evt;
      } else {
        // Overflow path
        AddToOverflow(evt);
      }
    }

    private void AddToOverflow(T evt)
    {
      if (!_overflowActive) {
        // First overflow: allocate buffer
        _overflowBuffer = new NativeList<T>(_overflowInitialCapacity, Allocator.Persistent);
        _overflowActive = true;
      }

      _overflowBuffer.Add(evt);
      _overflowUsedThisFrame = true;
    }

    /// <summary>
    /// Get event at index (unified across hot and overflow).
    /// </summary>
    public T this[int index] {
      get {
        if (index < _hotCount) {
          return _hotBuffer[index];
        } else if (_overflowActive) {
          return _overflowBuffer[index - _hotCount];
        }
        throw new IndexOutOfRangeException($"Event index {index} out of range (count: {Count})");
      }
    }

    /// <summary>
    /// Clear all events for next frame.
    /// Does not deallocate overflow (keeps it warm for next burst).
    /// </summary>
    public void Clear()
    {
      // Update peak tracking
      int currentCount = Count;
      if (currentCount > _peakCount) {
        _peakCount = currentCount;
      }

      // Reset hot buffer
      _hotCount = 0;

      // Clear overflow (but keep allocated for next frame)
      if (_overflowActive) {
        _overflowBuffer.Clear();
      }

      _overflowUsedThisFrame = false;
    }

    /// <summary>
    /// Release all memory. Call on system shutdown.
    /// </summary>
    public void Dispose()
    {
      if (_hotBuffer.IsCreated) {
        _hotBuffer.Dispose();
      }

      if (_overflowActive && _overflowBuffer.IsCreated) {
        _overflowBuffer.Dispose();
      }

      _hotCount = 0;
      _overflowActive = false;
    }

    /// <summary>
    /// Shrink overflow buffer if it's significantly oversized.
    /// Call periodically (e.g., every N frames) to reclaim memory.
    /// </summary>
    public void TrimExcess(int targetCapacity)
    {
      if (_overflowActive && _overflowBuffer.IsCreated) {
        if (_overflowBuffer.Capacity > targetCapacity * 4) {
          // Reallocate to smaller size
          _overflowBuffer.TrimExcess();
        }
      }
    }

    /// <summary>
    /// Get enumerator for iterating all events.
    /// </summary>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    /// <summary>
    /// Enumerator for iterating events across hot and overflow buffers.
    /// </summary>
    public struct Enumerator
    {
      private PhysicsEventBuffer<T> _buffer;
      private int _index;

      public Enumerator(PhysicsEventBuffer<T> buffer)
      {
        _buffer = buffer;
        _index = -1;
      }

      public T Current => _buffer[_index];

      public bool MoveNext()
      {
        _index++;
        return _index < _buffer.Count;
      }

      public void Reset()
      {
        _index = -1;
      }
    }

    /// <summary>
    /// Copy all events to a NativeArray for parallel processing.
    /// Allocates with specified allocator.
    /// </summary>
    public NativeArray<T> ToNativeArray(Allocator allocator)
    {
      int count = Count;
      if (count == 0) {
        return new NativeArray<T>(0, allocator);
      }

      var array = new NativeArray<T>(count, allocator);

      // Copy hot buffer
      if (_hotCount > 0) {
        NativeArray<T>.Copy(_hotBuffer, 0, array, 0, _hotCount);
      }

      // Copy overflow
      if (_overflowActive && _overflowBuffer.Length > 0) {
        NativeArray<T>.Copy(_overflowBuffer.AsArray(), 0, array, _hotCount, _overflowBuffer.Length);
      }

      return array;
    }
  }
}