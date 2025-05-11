using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace GameFramework.Rendering
{
    public class ShaderProgram : IDisposable
    {
        public int Handle { get; private set; }
        private bool _disposedValue;

        private readonly Dictionary<string, int> _uniformLocations;

        public ShaderProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            Handle = ShaderHelper.CreateProgram(vertexShaderPath, fragmentShaderPath);
            _uniformLocations = new Dictionary<string, int>();
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public int GetUniformLocation(string uniformName)
        {
            if (_uniformLocations.TryGetValue(uniformName, out int location))
            {
                return location;
            }

            location = GL.GetUniformLocation(Handle, uniformName);
            if (location == -1)
            {
                // Console.WriteLine($"Warning: Uniform '{uniformName}' not found in shader program {Handle}.");
            }
            _uniformLocations[uniformName] = location;
            return location;
        }

        // Uniform setters
        public void SetUniform(string name, int value)
        {
            GL.Uniform1(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, float value)
        {
            GL.Uniform1(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Vector2 value)
        {
            GL.Uniform2(GetUniformLocation(name), value);
        }
        
        public void SetUniform(string name, Vector3 value)
        {
            GL.Uniform3(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Vector4 value)
        {
            GL.Uniform4(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Matrix4 value)
        {
            GL.UniformMatrix4(GetUniformLocation(name), false, ref value);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                }

                GL.DeleteProgram(Handle);
                _disposedValue = true;
            }
        }

        ~ShaderProgram()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
