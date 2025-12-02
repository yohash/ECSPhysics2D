using Unity.Mathematics;

namespace ECSPhysics2D
{
  /// <summary>
  /// Motor curve library for common motion patterns.
  /// </summary>
  public static class MotorCurves
  {
    /// <summary>
    /// Linear acceleration/deceleration ramp.
    /// </summary>
    public static float LinearRamp(float time, float rampDuration, float targetSpeed)
    {
      if (rampDuration <= 0f)
        return targetSpeed;
      float t = math.saturate(time / rampDuration);
      return targetSpeed * t;
    }

    /// <summary>
    /// Smooth acceleration using ease-in-out curve.
    /// </summary>
    public static float SmoothStep(float time, float duration, float targetSpeed)
    {
      if (duration <= 0f)
        return targetSpeed;
      float t = math.saturate(time / duration);
      t = t * t * (3f - 2f * t); // Hermite interpolation
      return targetSpeed * t;
    }

    /// <summary>
    /// Oscillating motion (pendulum, reciprocating).
    /// </summary>
    public static float Oscillate(float time, float frequency, float amplitude)
    {
      return amplitude * math.sin(2f * math.PI * frequency * time);
    }

    /// <summary>
    /// Pulsed motion with on/off periods.
    /// </summary>
    public static float Pulse(float time, float period, float dutyCycle, float speed)
    {
      float phase = (time % period) / period;
      return phase < dutyCycle ? speed : 0f;
    }

    /// <summary>
    /// Sawtooth wave for repeating linear motion.
    /// </summary>
    public static float Sawtooth(float time, float period, float speed)
    {
      float phase = (time % period) / period;
      return speed * (2f * phase - 1f);
    }

    /// <summary>
    /// Triangle wave for back-and-forth motion.
    /// </summary>
    public static float Triangle(float time, float period, float speed)
    {
      float phase = (time % period) / period;
      return speed * (phase < 0.5f ? 4f * phase - 1f : 3f - 4f * phase);
    }

    /// <summary>
    /// Exponential acceleration curve.
    /// </summary>
    public static float ExponentialRamp(float time, float timeConstant, float targetSpeed)
    {
      if (timeConstant <= 0f)
        return targetSpeed;
      return targetSpeed * (1f - math.exp(-time / timeConstant));
    }

    /// <summary>
    /// Spring-damped approach to target speed.
    /// </summary>
    public static float SpringDamped(float currentSpeed, float targetSpeed, float frequency,
        float damping, float deltaTime)
    {
      float omega = 2f * math.PI * frequency;
      float exp = math.exp(-damping * omega * deltaTime);
      float cos = math.cos(omega * deltaTime);
      float sin = math.sin(omega * deltaTime);

      float newSpeed = exp * (currentSpeed * cos +
          ((currentSpeed - targetSpeed) * damping + currentSpeed / omega) * sin) +
          targetSpeed * (1f - exp);

      return newSpeed;
    }

    /// <summary>
    /// Custom bezier curve for precise control.
    /// </summary>
    public static float BezierCurve(float t, float p0, float p1, float p2, float p3)
    {
      t = math.saturate(t);
      float oneMinusT = 1f - t;
      float oneMinusT2 = oneMinusT * oneMinusT;
      float oneMinusT3 = oneMinusT2 * oneMinusT;
      float t2 = t * t;
      float t3 = t2 * t;

      return oneMinusT3 * p0 +
             3f * oneMinusT2 * t * p1 +
             3f * oneMinusT * t2 * p2 +
             t3 * p3;
    }

    /// <summary>
    /// Stepped motion for discrete positions.
    /// </summary>
    public static float Stepped(float time, float stepDuration, float stepSize, int maxSteps)
    {
      int step = (int)(time / stepDuration);
      step = math.min(step, maxSteps);
      return step * stepSize;
    }
  }
}