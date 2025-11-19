using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.Utilities
{
    public interface IArithmetic<T>
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, float scalar);
        T Negate(T a);
        T Zero { get; }
        T One { get; }
    }
    
    /// <summary>
    /// Static registry/cacher for IArithmetic<T> implementations.
    /// By default it is null and you should register known implementations (done below).
    /// Users can also register custom adapters for their types.
    /// </summary>
    public static class Arithmetic<T>
    {
        private static IArithmetic<T> _operator;
        public static IArithmetic<T> Operator
        {
            get
            {
                if (_operator == null)
                    throw new InvalidOperationException($"No IArithmetic<{typeof(T).Name}> registered. Call Arithmetic.Register or ensure default adapter is provided.");
                return _operator;
            }
        }

        public static void Register(IArithmetic<T> op)
        {
            _operator = op ?? throw new ArgumentNullException(nameof(op));
        }

        /// <summary>
        /// Try register only if not already registered.
        /// Useful for library code to set defaults without overriding user registrations.
        /// </summary>
        public static void RegisterIfAbsent(IArithmetic<T> op)
        {
            if (_operator == null)
                _operator = op ?? throw new ArgumentNullException(nameof(op));
        }
    }
    
        // float
    public class FloatArithmetic : IArithmetic<float>
    {
        public float Add(float a, float b) => a + b;
        public float Subtract(float a, float b) => a - b;
        public float Multiply(float a, float scalar) => a * scalar;
        public float Negate(float a) => -a;
        public float Zero => 0f;
        public float One => 1f;
    }

    // double (note Multiply uses float scalar; cast accordingly)
    public class DoubleArithmetic : IArithmetic<double>
    {
        public double Add(double a, double b) => a + b;
        public double Subtract(double a, double b) => a - b;
        public double Multiply(double a, float scalar) => a * (double)scalar;
        public double Negate(double a) => -a;
        public double Zero => 0.0;
        public double One => 1.0;
    }

    // int (multiplying with float will be rounded/truncated — choose behaviour)
    public class IntArithmetic : IArithmetic<int>
    {
        public int Add(int a, int b) => a + b;
        public int Subtract(int a, int b) => a - b;
        public int Multiply(int a, float scalar) => (int)(a * scalar); // truncation; you can change to Mathf.RoundToInt
        public int Negate(int a) => -a;
        public int Zero => 0;
        public int One => 1;
    }

    // UnityEngine.Vector2
    public class Vector2Arithmetic : IArithmetic<Vector2>
    {
        public Vector2 Add(Vector2 a, Vector2 b) => a + b;
        public Vector2 Subtract(Vector2 a, Vector2 b) => a - b;
        public Vector2 Multiply(Vector2 a, float scalar) => a * scalar;
        public Vector2 Negate(Vector2 a) => -a;
        public Vector2 Zero => Vector2.zero;
        public Vector2 One => Vector2.one; // element-wise one
    }

    // UnityEngine.Vector3
    public class Vector3Arithmetic : IArithmetic<Vector3>
    {
        public Vector3 Add(Vector3 a, Vector3 b) => a + b;
        public Vector3 Subtract(Vector3 a, Vector3 b) => a - b;
        public Vector3 Multiply(Vector3 a, float scalar) => a * scalar;
        public Vector3 Negate(Vector3 a) => -a;
        public Vector3 Zero => Vector3.zero;
        public Vector3 One => Vector3.one;
    }
    
    
    public static class ArithmeticDefaultsInitializer
    {
        // This attribute runs the method when the game loads in player/editor
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Register defaults; use RegisterIfAbsent so user can override before first use if desired.
            Arithmetic<float>.RegisterIfAbsent(new FloatArithmetic());
            Arithmetic<double>.RegisterIfAbsent(new DoubleArithmetic());
            Arithmetic<int>.RegisterIfAbsent(new IntArithmetic());
            Arithmetic<UnityEngine.Vector2>.RegisterIfAbsent(new Vector2Arithmetic());
            Arithmetic<UnityEngine.Vector3>.RegisterIfAbsent(new Vector3Arithmetic());
        }
    }
    
    public static class GenericMath
    {
        /// <summary>
        /// Blend (linear interpolation) between a and b by p in [0,1]:
        /// result = a * (1 - p) + b * p
        /// T must have registered IArithmetic<T>.
        /// </summary>
        public static T Blend<T>(T a, T b, float p)
        {
            var op = Arithmetic<T>.Operator;
            var oneMinusP = 1f - p;
            // b - a
            T diff = op.Subtract(b, a);
            // diff * p
            T scaled = op.Multiply(diff, p);
            // a + scaled
            return op.Add(a, scaled);
        }

        /// <summary>
        /// Example: a + b * scalar
        /// </summary>
        public static T AddScalarMul<T>(T a, T b, float scalar)
        {
            var op = Arithmetic<T>.Operator;
            return op.Add(a, op.Multiply(b, scalar));
        }
        
        
        
        // 
        /// <summary>
        /// 弹簧趋近方式：将差分形式转化为解析形式
        /// </summary>
        /// <param name="currValue">当前值</param>
        /// <param name="targValue">目标值</param>
        /// <param name="k">弹簧系数：k介于0~1，k越大则越快趋近目标，否则越慢趋近目标</param>
        /// <param name="step">步长：1为标准步长</param>
        /// <typeparam name="T">模板类型</typeparam>
        /// <returns></returns>
        public static T SpringApproach<T>(T currValue, T targValue, float k, float step)
        {
            k = Mathf.Clamp01(k);
            float t = 1 - Mathf.Pow(1 - k, step);
            return Blend(currValue, targValue, t);
        }
    }
    
    public static class BitCast
    {
        // 将 uint 的 32bit 原样重解释成 float
        public static float UIntToFloat(uint value)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        // 将 float 的 32bit 原样重解释成 uint
        public static uint FloatToUInt(float value)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }
    }
}
